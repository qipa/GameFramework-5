using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GOEngine.Implement
{
    public class GOEAsset
    {
        private int mReference = 0;
        private UnityEngine.Object Asset;
        string mName, mBundleName;
        public string Name
        {
            get { return mName; }
        }
        public string BundleName
        {
            get { return mBundleName; }
            
        }
        internal int Reference
        {
            set 
            {
                mReference = value;
            }
            get { return mReference; }
        }

        internal bool HasNoReference { get { return mReference <= 0; } }

        internal bool AssetValid { get { return Asset; } }

#if DEBUG_BUNDLE_CACHE
        string getAssetName;
        String GetAssetName
        {
            get
            {
                if (getAssetName == null)
                    getAssetName = string.Format("GOEAsset.GetAsset:{0}.{1}", BundleName, Name);
                return getAssetName;
            }
        }
#endif
        internal void GetAsset(Action<string, UnityEngine.Object> callback)
        {
#if DEBUG_BUNDLE_CACHE
            Profiler.BeginSample(GetAssetName);
#endif
            var res = GetAsset();
#if DEBUG_BUNDLE_CACHE
            Profiler.EndSample();
#endif

            if (callback != null)
            {   
#if DEBUG_BUNDLE_CACHE
                Profiler.BeginSample(callback.Method.ToString());
#endif
                callback(Name, res);
#if DEBUG_BUNDLE_CACHE
                Profiler.EndSample();
#endif
            }
        }

        private MemoryPool<UnityEngine.Object> objectPool = new MemoryPool<UnityEngine.Object>(30);
        internal UnityEngine.Object GetAsset()
        {
            if (AssetValid)
            {
                Reference++;
                UnityEngine.Object obj = objectPool.Alloc();
                if (obj != null)
                {
                    if (obj is GameObject)
                        (obj as GameObject).SetActive(true);
                    return obj;
                }
                return InstanceAsset(Asset, Name);
            }
            else
                return null;
        }

        internal bool NeedInstance
        {
            get
            {
                return needInstance(Asset);
            }
        }
        /// <summary>
        /// 对于取的东西是否instantiate，可以自己设定规则
        /// </summary>
        internal static UnityEngine.Object InstanceAsset(UnityEngine.Object obj, string name)
        {
            if( name.IndexOf(".exr") >= 0)
            {
                return obj;
            }

            if (needInstance(obj))
            {
                return GameObject.Instantiate(obj);
            }
            return obj;
        }

        private static bool needInstance(UnityEngine.Object obj)
        {

            if (obj is GameObject)
            {
                if (EngineDelegate.PrefabUnInstantiateRule != null && EngineDelegate.PrefabUnInstantiateRule(obj as GameObject))
                    return false;
                return true;
            }
            return false
;
        }

        internal void ReleaseAssetReference(UnityEngine.Object obj)
        {
            Reference--;
            if (obj is GameObject)
            {
                (obj as GameObject).SetActive(false);
                (obj as GameObject).transform.SetParent(null);
            }
            if (needInstance(obj) )
            {
                if(objectPool.Free(obj))
                {
                    return;
                }
                UnityEngine.Object.Destroy(obj);
            }
        }

        internal GOEAsset(UnityEngine.Object obj, string name, string bundleName)
        {
            mName = name;
            mBundleName = bundleName;
            Asset = obj;
        }

        private bool gotConfig;
        ResConfig config = null;
        internal bool CanRelease(bool onLevel)
        {
            if (!Asset)
                return true;
            if(!gotConfig)
            {
                if (Asset is GameObject)
                {
                    if ((Asset as GameObject).activeSelf)
                        config = (Asset as GameObject).GetComponent<ResConfig>();
                    else
                    {
                        ResConfig[] configs = (Asset as GameObject).GetComponentsInChildren<ResConfig>(true);
                        if (configs.Length > 0)
                            config = configs[0];
                    }
                }
                gotConfig = true;
            }
            if (Asset is Font)
                return false;
            if (onLevel)
            {
                return (config == null || config.ReleaseOnLevelLoaded);
            }
            else
            {
                return (config == null || config.ReleaseOnLevelLoaded) && HasNoReference;
            }
        }

        internal void ReleaseAsset(bool withAsset)
        {
            while (true)
            {
                UnityEngine.Object obj = objectPool.Alloc();
                if (obj != null)
                {
                    UnityEngine.Object.Destroy(obj);
                }
                else
                {
                    break;
                }
            }
            bool isIneditor = false;
            if (!withAsset || isIneditor)
                return;
            if (Asset)
                GameObject.DestroyImmediate(Asset, true);
            objectPool.Dispose();
            Asset = null;
        }
    }
}
