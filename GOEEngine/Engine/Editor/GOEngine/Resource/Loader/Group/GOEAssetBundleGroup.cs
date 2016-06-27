using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GOEngine.Implement
{
    internal class AssetBundleGroup : ResTypeGroup
    {
        GOEAssetBundleCacheManager cacheMgr;

        public AssetBundleGroup()
        {
            cacheMgr = new GOEAssetBundleCacheManager(this);
        }
        internal void EnsureDependencies(string name)
        {
            string bundleName = ResourceMgr.GetBundleName(name);
            if (!string.IsNullOrEmpty(bundleName))
            {
                GOEBundle bundle = ResourceMgr.GetBundle(bundleName);
                if (bundle != null)
                {
                    foreach (var dep in bundle.DependsOn)
                    {
                        GOEBundle depBundle = ResourceMgr.GetBundle(dep);
                        if (depBundle == null)
                            continue;
                        if (!HasLoaded(depBundle.FirstAsset))
                        {
                            GOERoot.ResMgr.PreloadBundle(dep, null);
                        }
                    }
                }
            }
        }

        internal void GetAsset(string name, Action<string, UnityEngine.Object> callback, LoadPriority priority = LoadPriority.Default)
        {
            if ( name == null || name == string.Empty )
            {
                return;
            }
            if (HasLoaded(name))
            {
                if (callback != null)
                {
                    GetAssetInCache(name, callback);
                }
                return;
            }
#if UNITY_EDITOR
            UnityEngine.Object obj = LoadFromPrefab(name, typeof(UnityEngine.Object));
            if (obj != null)
            {
                SetAsset(name, obj).Reference = 1;
                if (callback != null)
                    callback(name, GOEAsset.InstanceAsset(obj, name));
                return;
            }
#endif
            string bundleName = ResourceMgr.GetBundleName(name);
            if ( bundleName == string.Empty )
            {
                Logger.GetFile( LogFile.Res ).LogError( "can not find asset: " + name );
                return;
            }
            if (!cacheMgr.HasLoadingInfo(bundleName))
            {
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
            }
            cacheMgr.AddCallbackToDic(name, callback);
            
        }

        internal ResourceState PreloadBundle(string bundleName, Action<string, AssetBundle> callback, LoadPriority priority = LoadPriority.Default, bool dependencyResource = false)
        {
            GOEBundle bundle = ResourceMgr.Instance().GetBundle(bundleName);
            if (cacheMgr.Cache[bundleName] != null)
            {
                if (callback != null)
                {
                    callback(bundleName, null);
                }
                return ResourceState.OK;
            }
            Resource res = this.GetDownloadResource(bundleName);
            if (res == null)
            {
                if (!cacheMgr.HasLoadingInfo(bundleName))
                {
                    res = this.CreateResource(bundleName, priority);
                    res.DependencyResource = dependencyResource;
                    res.LoadRes();
                }
                cacheMgr.AddCallbackToDic(bundleName, callback);
            }
            else if (res.ResOK)
            {
                if (callback != null)
                {
                    callback(bundleName, res.Loader.AssetBundle);
                }
                return ResourceState.OK;
            }
            else
            {
                cacheMgr.AddCallbackToDic(bundleName, callback);
                if (res.Loader.Priority < priority)
                {
                    this.ResourceMgr.GOELoaderMgr.SetLoaderPriority(res.Loader, priority);
                }
            }
            return ResourceState.Wait;
        }
        
        internal override void Update()
        {
            if (!ResourceMgr.IsLoadingScene)
                cacheMgr.Update();
        }

        protected override bool HasLoaded(string name)
        {
            return cacheMgr.Cache.HasLoadedAsset(name);
        }
        private void GetAssetInCache(string name, Action<string, UnityEngine.Object> callback)
        {
            cacheMgr.Cache.GetAssetInCache(name, callback);
        }        
        
        internal override void ReleaseAssetbundle()
        {
            cacheMgr.ReleaseAssetbundleOnLevelLoad();
        }

        protected virtual void OnLoadAssetBundle(string name, AssetBundle bundle, bool dependencyResource)
        {
            cacheMgr.OnLoadAssetBundle(name, bundle, dependencyResource);
        }

        internal override bool ReleaseAssetReference(string name, UnityEngine.Object obj)
        {
            return cacheMgr.Cache.ReleaseAssetReference(name, obj);
        }
        internal override GOEAsset SetAsset(string name, UnityEngine.Object obj, string bundleName = null)
        {
            GOEAsset asset = new GOEAsset(obj, name, bundleName);
            cacheMgr.Cache.AddAsset(name, asset);
            return asset;
        }

        internal void CacheSceneBundleDependency(GOEBundle bundle)
        {
            cacheMgr.Cache.CacheSceneBundleDependency(bundle);
        }

        internal bool IsBundleCached(string name)
        {
            return cacheMgr.Cache[name] != null;
        }
        internal void ReleaseAssetCallback(string name, Action<string, UnityEngine.Object> func)
        {
            cacheMgr.RemoveAssetLoadCallback(name, func);
        }

        protected override Resource CreateResource(string name, LoadPriority priority)
        {
            Resource res =  base.CreateResource( name, priority );
#if DEBUG_BUNDLE_CACHE
            ResourceMgr.Instance().SampleLoadEvent(name, 1);
#endif                  
            res.AddGotBundleCallback( this.OnLoadAssetBundle );

            return res;

        }

        public int GetCacheSize()
        {
            return cacheMgr.Cache.CachedSize;
        }

        public int GetPreferedCacheSize()
        {
            return cacheMgr.Cache.PreferedCacheSize;
        }

        public void SetPreferedCacheSize(int size)
        {
            cacheMgr.Cache.PreferedCacheSize = size;
        }

        public int GetCacheCount()
        {
            return cacheMgr.Cache.CachedBundleCount;
        }

        public string DumpCacheInfo()
        {
            return cacheMgr.Cache.DumpCacheInfo();
        }

        public void ForceGCCache()
        {
            cacheMgr.Cache.ForceGC();
        }
    }
}
