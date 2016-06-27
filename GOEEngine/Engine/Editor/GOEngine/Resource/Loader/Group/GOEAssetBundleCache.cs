using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GOEngine.Implement
{
    internal class GOEAssetBundleCache
    {
        Dictionary<string, GOEBundleResource> cachedBundles = new Dictionary<string, GOEBundleResource>();
        Dictionary<string, GOEAsset> addedAssets = new Dictionary<string, GOEAsset>();

        int cachedSize = 0;

        int preferedCacheSize = 16 * 1024 * 1024;

        float minimalGCInterval = 5f;
        float lastGCTime = 0;
        bool needGC = false;

        /// <summary>
        /// 当前缓存量，bundle的字节数，不包含解压后的资源尺寸
        /// </summary>
        public int CachedSize { get { return cachedSize; } }

        /// <summary>
        /// 希望的缓存尺寸，超出后在GC时会尝试释放暂时不用的资源到该尺寸
        /// </summary>
        public int PreferedCacheSize { get { return preferedCacheSize; } set { preferedCacheSize = value; } }

        /// <summary>
        /// 最小GC间隔，避免太频繁GC压力过高
        /// </summary>
        public float MinimalGCInterval { get { return minimalGCInterval; } set { minimalGCInterval = value; } }

        public int CachedBundleCount { get { return cachedBundles.Count; } }
        /// <summary>
        /// 根据指定名称获取缓存的bundle，如果没缓存该bundle则返回null
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GOEBundleResource this[string name]
        {
            get
            {
                GOEBundleResource res;
                if (cachedBundles.TryGetValue(name, out res))
                    return res;
                else
                    return null;
            }
        }

        public bool HasLoadedAsset(string asset)
        {
            var bName = ResourceMgr.Instance().GetBundleName(asset);
            GOEBundleResource b;
            if (cachedBundles.TryGetValue(bName, out b))
            {
                return b.ContainsAsset(asset);
            }
            else
                return addedAssets.ContainsKey(asset);
        }
        public void GetAssetInCache(string name, Action<string, UnityEngine.Object> callback)
        {
            var bName = ResourceMgr.Instance().GetBundleName(name);
            GOEBundleResource b;
            if (cachedBundles.TryGetValue(bName, out b))
            {
                var asset = b[name];
                b.Touch();
                if (asset != null)
                    asset.GetAsset(callback);
                else
                {
                    if (addedAssets.TryGetValue(name, out asset))
                        asset.GetAsset(callback);
                }
            }
            else
            {
                GOEAsset asset;
                if (addedAssets.TryGetValue(name, out asset))
                    asset.GetAsset(callback);
            }
        }

        public void AddAsset(string name, GOEAsset asset)
        {
            GOEAsset old;
            if (addedAssets.TryGetValue(name, out old))
                old.ReleaseAsset(true);
            addedAssets[name] = asset;
        }
        public bool ReleaseAssetReference(string name, UnityEngine.Object obj)
        {
            var bName = ResourceMgr.Instance().GetBundleName(name);
            GOEBundleResource b;
            if (cachedBundles.TryGetValue(bName, out b))
            {
                var asset = b[name];
                if (asset != null)
                {
                    asset.ReleaseAssetReference(obj);
                    return true;
                }
            }
            return false;
        }

        public bool CacheAssetBundle(GOEBundleResource bundle)
        {
            if (!cachedBundles.ContainsKey(bundle.Name))
            {
                cachedBundles[bundle.Name] = bundle;
                cachedSize += bundle.Size;
                bundle.Touch();
                /*#if DEBUG_BUNDLE_CACHE
                                Debug.Log(string.Format("{2:0.##}:Bundle {0} cached, new CacheSize={1:0.##}MB", bundle.Name, cachedSize / 1024f / 1024f, Time.realtimeSinceStartup));
                #endif*/
                needGC = true;
                return true;
            }
            return false;
        }

        public void CacheSceneBundleDependency(GOEBundle bundle)
        {
            GOEBundleResource newRes = new GOEBundleResource();
            newRes.BundleInfo = bundle;
            newRes.IsSceneBundle = true;
            foreach (var i in bundle.DependsOn)
            {
                GOEBundleResource res;
                if (cachedBundles.TryGetValue(i, out res))
                {
                    newRes.AddDependency(res);
                }
            }
            CacheAssetBundle(newRes);
        }

        /// <summary>
        /// 尝试进行垃圾回收
        /// </summary>
        /// <param name="suppress">是否忽视最佳缓存尺寸释放所有可释放资源</param>
        public void DoGC(bool suppress = false, bool force = false)
        {
#if DEBUG_BUNDLE_CACHE
            Profiler.BeginSample("GOEAssetBundleCache.DoGC");
#endif
            if (needGC && ResourceMgr.Instance().GOELoaderMgr.IsFree)
            {
                bool shouldDoGC = cachedSize > preferedCacheSize || suppress;

                if (Time.realtimeSinceStartup - lastGCTime > minimalGCInterval)
                {
#if DEBUG_BUNDLE_CACHE
                    int oldCacheSize = cachedSize;
                    int oldCacheCount = CachedBundleCount;
#endif
                    bool needUnloadUnused = false;
                    List<GOEBundleResource> bundleCanRelease = null;
                    do
                    {
                        bundleCanRelease = null;
                        foreach (var i in cachedBundles)
                        {
                            var bundle = i.Value;
                            /*#if DEBUG_BUNDLE_CACHE
                                                    bool hasBundle = bundle.AssetBundle;
                            #endif*/
                            bundle.ReleaseAssetBundle();
                            /*#if DEBUG_BUNDLE_CACHE
                                                    if (hasBundle && !bundle.AssetBundle)
                                                        Debug.LogWarning(string.Format("{2:0.##}:Bundle {0} released webstream, new CacheSize={1:0.##}MB", bundle.Name, cachedSize / 1024f / 1024f, Time.realtimeSinceStartup));
                            #endif*/
                            if (shouldDoGC && bundle.CanRelease)
                            {
                                if (bundleCanRelease == null)
                                    bundleCanRelease = new List<GOEBundleResource>();
                                bundleCanRelease.Add(bundle);
                            }
                        }
                        if (!shouldDoGC)
                        {
                            needGC = false;
#if DEBUG_BUNDLE_CACHE
                        Profiler.EndSample();
#endif
                            return;
                        }
                        if (bundleCanRelease != null)
                        {
                            if (bundleCanRelease.Count > 0)
                            {
                                bundleCanRelease.Sort((a, b) => Mathf.CeilToInt((a.HitTime - b.HitTime) * 10));
                                HashSet<string> bundleRemoved = new HashSet<string>();
                                for (int i = 0; i < bundleCanRelease.Count; i++)
                                {
                                    var bundle = bundleCanRelease[i];
                                    bundle.Release(false);
                                    if (bundle.Released)
                                    {
                                        needUnloadUnused = true;
                                        cachedSize -= bundle.Size;
                                        /*#if DEBUG_BUNDLE_CACHE
                                                                            Debug.LogWarningFormat("{2:0.##}:Bundle {0} released, new CacheSize={1:0.##}MB", bundle.Name, cachedSize / 1024f / 1024f, Time.realtimeSinceStartup);
                                        #endif*/
                                        bundleRemoved.Add(bundle.Name);
                                    }
                                    if (cachedSize < preferedCacheSize * 0.8f && !force)
                                    {
                                        bundleCanRelease = null;
                                        break;
                                    }
                                }
                                foreach (var i in bundleRemoved)
                                {
                                    cachedBundles.Remove(i);
                                }
                                //GC.Collect();
                            }
                        }
                    }
                    while (bundleCanRelease != null && bundleCanRelease.Count > 0);
                    if (needUnloadUnused)
                    {
#if DEBUG_BUNDLE_CACHE
                        Profiler.BeginSample("Resources.UnloadUnusedAssets");
#endif
                        Resources.UnloadUnusedAssets();
#if DEBUG_BUNDLE_CACHE
                        Profiler.EndSample();
#endif
                    }
                    needGC = false;
                    lastGCTime = Time.realtimeSinceStartup;
#if DEBUG_BUNDLE_CACHE
                    Debug.LogFormat("{0:0.##}:Cache GC, released {1} bundles, freed {2:0.##} MB", Time.realtimeSinceStartup, oldCacheCount - CachedBundleCount, (oldCacheSize - cachedSize) / 1024f / 1024f);
#endif
                }
            }
#if DEBUG_BUNDLE_CACHE
            Profiler.EndSample();
#endif
        }

        /// <summary>
        /// 彻底整理GC
        /// </summary>
        public void DoGCOnLoadLevel()
        {
            bool released = false;
            do
            {
                released = ReleaseCacheHasNoReference();
            } while (released);
        }

        bool ReleaseCacheHasNoReference()
        {
            HashSet<string> bundleRemoved = null;
            List<GOEBundleResource> bundleCanRelease = null;
            bool shouldGC = cachedSize >= preferedCacheSize * 0.8f;
            foreach (var i in cachedBundles)
            {
                var b = i.Value;
                if (b.IsSceneBundle)
                {
                    b.Release(true);
                    if (b.Released)
                    {
                        cachedSize -= b.Size;
                        if (bundleRemoved == null)
                            bundleRemoved = new HashSet<string>();
                        bundleRemoved.Add(i.Key);
                    }
                }
                else if (shouldGC && !b.HasReference && b.CanReleaseOnLevel)
                {
                    if (bundleCanRelease == null)
                        bundleCanRelease = new List<GOEBundleResource>();
                    bundleCanRelease.Add(b);
                }

                if (b.CanReleaseOnLevel)
                {
                    b.ReleaseInstances();                    
                }
            }

            if (bundleCanRelease != null)
            {
                if (bundleCanRelease.Count > 0)
                {
                    bundleCanRelease.Sort((a, b) => Mathf.CeilToInt((a.HitTime - b.HitTime) * 10));
                    for (int i = 0; i < bundleCanRelease.Count; i++)
                    {
                        var bundle = bundleCanRelease[i];
                        bundle.Release(true);
                        if (bundle.Released)
                        {
                            cachedSize -= bundle.Size;
                            bundleRemoved.Add(bundle.Name);
                        }
                        if (cachedSize < preferedCacheSize * 0.8f)
                            break;
                    }
                }
            }

            if (bundleRemoved != null)
            {
                foreach (var i in bundleRemoved)
                {
                    cachedBundles.Remove(i);
                }
                return bundleRemoved.Count > 0;
            }
            else
                return false;
        }

        public string DumpCacheInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Current Cache:" + (cachedSize / 1024f / 1024f).ToString("0.##MB"));
            sb.AppendLine("Cached Bundles:" + CachedBundleCount);
            List<GOEBundleResource> bundles = new List<GOEBundleResource>();
            foreach (var i in cachedBundles)
            {
                bundles.Add(i.Value);
            }
            bundles.Sort((a, b) => b.Size - a.Size);
            foreach (var bundle in bundles)
            {
                sb.Append("AssetBundle:");
                sb.Append(bundle.Name);
                sb.Append(" Size:");
                sb.Append((bundle.Size / 1024f / 1024f).ToString("0.##MB"));
                sb.Append(" CanRelease:");
                sb.Append(bundle.CanRelease);
                sb.Append(" AssetCount:");
                sb.Append(bundle.AssetCount);
                sb.Append(" HasAssetBundle:");
                sb.Append((bool)bundle.AssetBundle);
                sb.Append(" ReferenceCount:");
                sb.Append(bundle.ReferencedBy.Count);
                sb.Append(" SurvivalTimeExpired:");
                sb.Append(bundle.SurvivalTimeExpired);
                sb.Append(" IsSceneBundle:");
                sb.AppendLine(bundle.IsSceneBundle.ToString());
                foreach (var j in bundle.ReferencedBy)
                {
                    sb.Append("    -");
                    sb.Append(j.Name);
                    sb.Append("  ");
                    sb.AppendLine((bundle.Size / 1024f / 1024f).ToString("0.##MB"));
                }
            }

            return sb.ToString();
        }

        public void ForceGC()
        {
            needGC = true;
            DoGC(true, true);
        }
    }
}
