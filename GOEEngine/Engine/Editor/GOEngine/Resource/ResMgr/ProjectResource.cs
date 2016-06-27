using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

namespace GOEngine.Implement
{
    public class ProjectResource : IProjectResource
    {

        public Dictionary<string, GOEBundle> mBundleFileList = new Dictionary<string, GOEBundle>();
        private ResourceMgr resourceMgr;
        internal virtual void Start() 
        {
            resourceMgr = ResourceMgr.Instance();
            resourceMgr.Start();
        }

        internal virtual void Shutdown()
        {

        }

        internal bool IsReady
        {
           get{ return resourceMgr.HasInitialized;}
        }

        internal virtual void Update()
        {
            resourceMgr.Update();
        }

        internal void OnEnterScene()
        {
        }

        internal void OnLeaveScene()
        {
            resourceMgr.OnLeaveScene();
        }
        
        public int MaxLoaderCount
        {
            set { resourceMgr.GOELoaderMgr.MaxLoader = value; }
        }

        public int ScenePoolCount
        {
            set { resourceMgr.SceneBundleGroup.PoolCount = value; }
        }

        public int LoadMaxTime
        {
            set { ResourceLoader.MaxLoadTime = value; }
        }

        public string GetBundleName(string assetname)
        {
            return resourceMgr.GetBundleName(assetname);
        }
        
        public void AddLanguageResource(string text)
        {
            resourceMgr.LocaleResource.AddLanguageResource(text);
        }

        public string GetStringByKey(string key)
        {
            return resourceMgr.LocaleResource.GetStringByKey(key);
        }

        /// <summary>
        /// 从assetbundle中加载asset
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <param name="priority"></param>
        public virtual void GetAsset(string name, Action<string, UnityEngine.Object> callback, LoadPriority priority = LoadPriority.Default)
        {
           
            resourceMgr.AssetBundleGroup.GetAsset(name, callback, priority);
        }

        public virtual void EnsureDependencies(string name)
        {
            resourceMgr.AssetBundleGroup.EnsureDependencies(name);
        }

        /// <summary>
        /// 预加载assetbundle
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <param name="priority"></param>
        public virtual void PreloadBundle(string name, Action<string, AssetBundle> callback, LoadPriority priority = LoadPriority.Default, bool dependencyResouece = false)
        {
            resourceMgr.AssetBundleGroup.PreloadBundle(name, callback, priority, dependencyResouece);
        }

        public virtual void SetProgress(string[] bundleNames, string[] assetNames, Action<float> handler, Action onEnd = null)
        {
            resourceMgr.SetProgress(bundleNames, assetNames, handler, onEnd);
        }


        public virtual void SetProgress(string[] wwwNames, Action<float> handler, Action onEnd = null)
        {
            resourceMgr.SetProgress(wwwNames, handler, onEnd);
        }

        public virtual void RegisterAsset(string name, UnityEngine.Object obj)
        {
            resourceMgr.AssetBundleGroup.SetAsset(name, obj);
        }

        //���س���
        public virtual void GetScene(string name, Action callBack = null, LoadPriority priority = LoadPriority.MostPrior)
        {
            resourceMgr.SceneBundleGroup.GetScene(name, callBack, priority);           
        }

        /// <summary>
        /// 从远程服务器上加载文件，用于热更新
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        internal void getWWWFromServer(string name, Action<string, WWW> callback)
        {
            resourceMgr.WWWFileGroup.getWWWFileFromServer(name, callback);
        }


        /// <summary>
        /// 从本地加载二进制文件
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        public void GetBytes(string name, Action<string, byte[]> callback, bool fromStream = false)
        {
            resourceMgr.WWWFileGroup.GetByteFile(name, callback, fromStream);
        }

        /// <summary>
        /// 从文本文件中读取字符串
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        public void GetText(string name, Action<string, string> callback, bool fromStream = false)
        {
            resourceMgr.WWWFileGroup.GetTextFile(name, callback, fromStream);
        }

        /// <summary>
        /// 从assetbundle中读取字符串
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public virtual void GetString(string name, Action<string, string> callback)
        {
            resourceMgr.GOEStringDelegate.GetString(name, callback);
        }

        public void LoadBundleMap(Action onLoad)
        {
            resourceMgr.LoadBundleMap(onLoad);
        }

        //ɾ����Դ����
        public void ReleaseAsset(string name, UnityEngine.Object obj)
        {
            resourceMgr.ReleaseAsset(name, obj);
        }

        public void RemoveAsset(string name, bool force = true)
        {
            resourceMgr.RemoveAsset(name, force);
        }

        public void RemoveAllAsset()
        {
            resourceMgr.RemoveAllAsset();
        }

        //ɾ����Դ����
        public void ReleaseAssetCallback(string name, Action<string, UnityEngine.Object> func)
        {
            resourceMgr.ReleaseAssetCallback(name, func);
        }


        public int GetCurrentCacheSize()
        {
            return resourceMgr.AssetBundleGroup.GetCacheSize();
        }

        public int GetCacheCount()
        {
            return resourceMgr.AssetBundleGroup.GetCacheCount();
        }

        public int GetPreferedCacheSize()
        {
            return resourceMgr.AssetBundleGroup.GetPreferedCacheSize();
        }

        public void SetPreferedCacheSize(int size)
        {
            resourceMgr.AssetBundleGroup.SetPreferedCacheSize(size);
        }

        public void SetLowAsyncLoadPriority(bool value)
        {
            resourceMgr.LowAsyncLoadPriority = value;
            if (value)
            {
                Application.backgroundLoadingPriority = ThreadPriority.Low;
            }
            else
            {
                Application.backgroundLoadingPriority = ThreadPriority.High;
            }
        }

        public string DumpAssetBundleCacheInfo()
        {
            return resourceMgr.AssetBundleGroup.DumpCacheInfo();
        }

        public void SetAssetBundlePersistent(string bundleName)
        {
            var bundle = resourceMgr.GetBundle(bundleName);
            bundle.BundlePersistent = true;
        }
        public void SetAssetPersistent(string bundleName)
        {
            var bundle = resourceMgr.GetBundle(bundleName);
            bundle.AssetPersistent = true;
        }
        public void ForceGCCache()
        {
            resourceMgr.AssetBundleGroup.ForceGCCache();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetCacheProfiler()
        {
#if DEBUG_BUNDLE_CACHE
            resourceMgr.ResetCacheProfiler();
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string DumpCacheProfilerInfo()
        {
#if DEBUG_BUNDLE_CACHE
            return resourceMgr.DumpCacheProfilerInfo();
#else
            return "Profiler Off";
#endif
        }
    }
}
