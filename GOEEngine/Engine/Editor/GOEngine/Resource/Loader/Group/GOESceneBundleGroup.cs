using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GOEngine.Implement
{
    internal class SceneBundleGroup : AssetBundleGroup
    {
        private string mCurScene = string.Empty;
        bool loading = false;
        public bool IsLoading
        {
            get
            {
                return loading;
            }
        }
        internal void GetScene(string name, Action callBack, LoadPriority priority = LoadPriority.Default)
        {
            if (GOERoot.IsEditor || HasLoaded(name) || ResourceMgr.IsBundleCached(name + ".bundle"))
            {
                if (mCurScene != name)
                {
                    LoadScene(name);
                    if (callBack != null)
                        callBack();
                    return;
                }
                else
                {
                    removeBundle(name);
                }
            }
            string bundleName = this.ResourceMgr.GetBundleName(name);
            if (string.IsNullOrEmpty(bundleName))
                bundleName = this.ResourceMgr.GetBundleName(name + ".unity");
            if (bundleName == string.Empty)
            {
                Logger.GetFile(LogFile.Res).LogError("can not find scene: " + name);
                return;
            }

            mCurScene = name;

            Resource res = this.GetDownloadResource(bundleName);
            if (res == null)
            {
                res = this.CreateResource(bundleName, priority);
                res.LoadRes();
            }

            //逻辑加载时，提高优先级//
            if (res.Loader.Priority < priority)
            {
                this.ResourceMgr.GOELoaderMgr.SetLoaderPriority(res.Loader, priority);
            }
            res.AddGotSceneCallback(callBack);
        }

        private void removeBundle(string name)
        {
            int index = cachedNames.IndexOf(name);
            if (index != -1)
            {
                cachedNames.RemoveAt(index);
                AssetBundle bundle = cachedBundles[index];                
                cachedBundles.RemoveAt(index);
                bundle.Unload(false);
            }
        }

        protected override void OnLoadAssetBundle(string name, AssetBundle www, bool dependencyResource)
        {
            if (www == null)
            {
                Logger.GetFile(LogFile.Res).LogError(name + " assetBundel == null");
                return;
            }
            string sceneName = GetSceneName(name);
            cachedBundles.Add(www);
            cachedNames.Add(sceneName);
            GOEBundle bundle = ResourceMgr.GetBundle(name);
            ResourceMgr.CacheSceneBundleDependency(bundle);
            if (sceneName == mCurScene)
            {
                this.LoadScene(sceneName);
            }
        }

        private string GetSceneName(string bname)
        {
            return bname.Replace(".bundle", "");
        }

        private void LoadScene(string name)
        {
            mCurScene = name;
            loading = true;
            Application.LoadLevelAsync(name);
            GOERoot.SceneImp.OnEnterScene += OnLevelLoaded;
            if (PoolCount == 0)
                return;
            int index = cachedNames.IndexOf(name);
            if (index != cachedNames.Count - 1)
            {
                cachedNames.RemoveAt(index);
                cachedNames.Add(name);
                AssetBundle www = cachedBundles[index];
                cachedBundles.RemoveAt(index);
                cachedBundles.Add(www);
            }
        }

        private void OnLevelLoaded()
        {
            GOERoot.SceneImp.OnEnterScene -= OnLevelLoaded;
            while (cachedBundles.Count > PoolCount)
            {
                cachedBundles[0].Unload(false);
                cachedBundles.RemoveAt(0);
                cachedNames.RemoveAt(0);
            }
            loading = false;
        }

        private List<AssetBundle> cachedBundles = new List<AssetBundle>();
        private List<string> cachedNames = new List<string>();

        public int PoolCount = 2;

        protected override bool HasLoaded(string name)
        {
            if (cachedNames.Contains(name))
                return true;
            return false;
        }


        protected override Resource CreateResource(string name, LoadPriority priority)
        {
            Resource res = base.CreateResource(name, priority);

            res.AddGotBundleCallback(this.OnLoadAssetBundle);

            return res;
        }
    }
}
