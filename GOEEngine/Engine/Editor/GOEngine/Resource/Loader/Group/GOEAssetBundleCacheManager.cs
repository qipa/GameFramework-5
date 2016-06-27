using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GOEngine.Implement
{
    internal class GOEAssetBundleCacheManager
    {
        class AssetRequestInfo
        {
            public UnityEngine.Object Asset;
            public AssetBundleRequest Request;
            public string AssetName;
        }
        class AssetBundleAssetLoadingInfo
        {
            public string BundleName;
            public bool IsAsync;
            public AssetBundle AssetBundle;
            public List<AssetRequestInfo> Requests;
            public bool IsNull
            {
                get
                {
                    return BundleName == null;
                }
            }

            public bool AllDone
            {
                get
                {
                    if (!IsAsync)
                        return true;
                    bool allDone = true;
                    for (int j = 0; j < Requests.Count; j++)
                    {
                        if (!Requests[j].Request.isDone)
                        {
                            allDone = false;
                            break;
                        }
                    }
                    return allDone;
                }
            }
        }
        public GOEAssetBundleCacheManager(AssetBundleGroup group)
        {
            this.group = group;
        }

        GOEAssetBundleCache cache = new GOEAssetBundleCache();
        AssetBundleGroup group;

        const int MaximalLoadingAssetBundles = 1;

        List<AssetBundleAssetLoadingInfo> assetLoading = new List<AssetBundleAssetLoadingInfo>();
        Queue<AssetBundleAssetLoadingInfo> pendingBundle = new Queue<AssetBundleAssetLoadingInfo>();
        Dictionary<string, LinkedList<Action<string, UnityEngine.Object>>> assetLoadedCallbacks = new Dictionary<string, LinkedList<Action<string, UnityEngine.Object>>>();
        Dictionary<string, Action<string, AssetBundle>> bundleLoadedCallbacks = new Dictionary<string, Action<string, AssetBundle>>();

        public GOEAssetBundleCache Cache { get { return cache; } }

        public bool HasLoadingInfo(string bundleName)
        {
            foreach (var i in pendingBundle)
            {
                if (i.BundleName == bundleName)
                    return true;
            }
            for (int i = 0; i < assetLoading.Count; i++)
            {
                var res = assetLoading[i];
                if (res.BundleName == bundleName)
                {
                    return true;
                }
            }
            return false;
        }
        internal void Update()
        {
#if DEBUG_BUNDLE_CACHE
            Profiler.BeginSample("GOEAssetBundleCacheManager.Update");
#endif
            while (assetLoading.Count < MaximalLoadingAssetBundles && pendingBundle.Count > 0)
            {
                var info = pendingBundle.Dequeue();

                StartLoadAssets(info);
            }
            for (int i = 0; i < assetLoading.Count; i++)
            {
                AssetBundleAssetLoadingInfo info = assetLoading[i];
                
                if (info.AllDone)
                {
#if DEBUG_BUNDLE_CACHE
                    ResourceMgr.Instance().SampleLoadEvent(info.BundleName, 6);
#endif     
                    GOEBundleResource res = new GOEBundleResource();
                    res.AssetBundle = info.AssetBundle;
                    res.BundleInfo = ResourceMgr.Instance().GetBundle(info.BundleName);
                    cache.CacheAssetBundle(res);
                    bool isAsync = info.IsAsync;
                    for (int j = 0; j < info.Requests.Count; j++)
                    {
                        AssetRequestInfo req = info.Requests[j];
                        UnityEngine.Object obj = isAsync ? req.Request.asset : req.Asset;
                        if (obj)
                        {
                            string assetName = req.AssetName;
                            var asset = OnGotAsset(assetName, obj, info.BundleName);
                            res.AddAsset(assetName, asset);
                            InvokeAssetLoaded(asset);
                        }
                    }
                    InvokeBundleLoaded(info);
                    MarkBundleDependency(res);

                    assetLoading.RemoveAt(i);
#if DEBUG_BUNDLE_CACHE
                    ResourceMgr.Instance().SampleLoadEvent(info.BundleName, 7);
                    Profiler.EndSample();
#endif     
                    return;//一个Update只删除一个已完成Bundle
                }
            }

            //不在加载资源时GC
            bool hasLoading = assetLoading.Count > 0 || pendingBundle.Count > 0;
            if (!hasLoading)
                cache.DoGC();
#if DEBUG_BUNDLE_CACHE
            Profiler.EndSample();
#endif
        }

        internal void RemoveAssetLoadCallback(string name, Action<string, UnityEngine.Object> func)
        {
            LinkedList<Action<string, UnityEngine.Object>> lst;
            if (assetLoadedCallbacks.TryGetValue(name, out lst))
            {
                var cur = lst.First;
                while (cur != null)
                {
                    if (cur.Value == func)
                    {
                        lst.Remove(cur);
                        break;
                    }
                    cur = cur.Next;
                }
            }
        }

        void StartLoadAssets(AssetBundleAssetLoadingInfo info)
        {
            try
            {
                GOEBundle gBundle = ResourceMgr.Instance().GetBundle(info.BundleName);
                var bundle = info.AssetBundle;
                if (!bundle)
                {
                    Debug.LogError(string.Format("AssetBundle {0} is null", gBundle.mName));
                    return;
                }
                info.Requests = new List<AssetRequestInfo>();
                bool shouldAsync = true;
                info.IsAsync = shouldAsync;
#if DEBUG_BUNDLE_CACHE
                ResourceMgr.Instance().SampleLoadEvent(info.BundleName, 5);
#endif     
                foreach (string str in gBundle.Files)
                {
                    AssetRequestInfo ar = new AssetRequestInfo();
                    ar.AssetName = str;
                    if (shouldAsync)
                    {
                        ar.Request = bundle.LoadAssetAsync(str);
                    }
                    else
                    {
                        ar.Asset = bundle.LoadAsset(str);
                    }
                    info.Requests.Add(ar);
                }
                assetLoading.Add(info);
            }
            catch
            {
                Debug.LogError("Cannot load assetbundle:" + info.BundleName);
            }
        }

        void InvokeBundleLoaded(AssetBundleAssetLoadingInfo info)
        {
            Action<string, AssetBundle> cb = null;
            if (bundleLoadedCallbacks.TryGetValue(info.BundleName, out cb))
            {
                try
                {
                    cb(info.BundleName, info.AssetBundle);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.ToString());
                }
                bundleLoadedCallbacks.Remove(info.BundleName);
            }
        }

        void InvokeAssetLoaded(GOEAsset asset)
        {
            LinkedList<Action<string, UnityEngine.Object>> list = null;
            if (assetLoadedCallbacks.TryGetValue(asset.Name, out list))
            {
                LinkedListNode<Action<string, UnityEngine.Object>> cur = list.First;
                while (cur != null)
                {
                    try
                    {
                        asset.GetAsset(cur.Value);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.ToString());
                    }
                    cur = cur.Next;
                }
                assetLoadedCallbacks.Remove(asset.Name);
            }
        }

        void MarkBundleDependency(GOEBundleResource res)
        {
            foreach (var i in res.BundleInfo.DependsOn)
            {
                var bundle = cache[i];
                if (bundle != null)
                {
                    res.AddDependency(bundle);
                }
                else
                {
                    Debug.LogError(string.Format("Cannot find dependency bundle {0} for bundle {1}.", i, res.Name));
                }
            }
        }

        public void ReleaseAssetbundleOnLevelLoad()
        {
            cache.DoGCOnLoadLevel();

            //取消所有进行中的资源加载
            foreach (var i in pendingBundle)
            {
                if (i.AssetBundle)
                    i.AssetBundle.Unload(true);
            }
            pendingBundle.Clear();
            foreach (var i in assetLoading)
            {
                if (i.AssetBundle)
                    i.AssetBundle.Unload(true);
            }
            assetLoading.Clear();

            assetLoadedCallbacks.Clear();
            bundleLoadedCallbacks.Clear();
        }

        public void OnLoadAssetBundle(string name, AssetBundle bundle, bool dependencyResource)
        {
#if DEBUG_BUNDLE_CACHE
            ResourceMgr.Instance().SampleLoadEvent(name, 4);
#endif     
            AssetBundleAssetLoadingInfo info = new AssetBundleAssetLoadingInfo();
            info.AssetBundle = bundle;
            info.BundleName = name;
            if (!dependencyResource)
                pendingBundle.Enqueue(info);
            else
            {
                GOEBundleResource res = new GOEBundleResource();
                res.AssetBundle = info.AssetBundle;
                res.BundleInfo = ResourceMgr.Instance().GetBundle(info.BundleName);
                cache.CacheAssetBundle(res);
                InvokeBundleLoaded(info);
                MarkBundleDependency(res);
            }
        }

        private GOEAsset OnGotAsset(string name, UnityEngine.Object obj, string bundleName)
        {
            GOEAsset asset = new GOEAsset(obj, name, bundleName);
            return asset;
        }

        public void AddCallbackToDic(string name, Action<string, UnityEngine.Object> action)
        {
            LinkedList<Action<string, UnityEngine.Object>> old = null;
            if (!assetLoadedCallbacks.TryGetValue(name, out old))
            {
                old = new LinkedList<Action<string, UnityEngine.Object>>();
                assetLoadedCallbacks[name] = old;
            }
            old.AddLast(action);
        }

        public void AddCallbackToDic(string name, Action<string, AssetBundle> action)
        {
            if (action == null)
                return;
            Action<string, AssetBundle> old = null;
            Action<string, AssetBundle> newDel = action;
            if (bundleLoadedCallbacks.TryGetValue(name, out old))
            {
                newDel = old + action;
            }
            
            bundleLoadedCallbacks[name] = newDel;

        }
    }

#if DEBUG_BUNDLE_CACHE
    class GOEAssetBundleCacheProfiler
    {
        Dictionary<string, GOEAssetBundleProfileInfo> info = new Dictionary<string, GOEAssetBundleProfileInfo>();

        public void SampleLoadEvent(string bn, int eventID)
        {
            GOEAssetBundleProfileInfo pi = null;
            float time = Time.realtimeSinceStartup;
            if (info.TryGetValue(bn, out pi))
            {
                switch (eventID)
                {
                    case 2:
                        pi.WWWStartTime = time;
                        break;
                    case 3:
                        pi.WWWFinishTime = time;
                        break;
                    case 4:
                        pi.ResourceReadyTime = time;
                        break;
                    case 5:
                        pi.AssetLoadStartTime = time;
                        break;
                    case 6:
                        pi.AssetLoadFinishTime = time;
                        break;
                    case 7:
                        pi.EndTime = time;
                        break;
                }
            }
            else if (eventID == 1)
            {
                pi = new GOEAssetBundleProfileInfo();
                pi.BundleName = bn;
                GOEBundle bundle = ResourceMgr.Instance().GetBundle(bn);
                pi.BundleSize = bundle.Size;
                pi.StartTime = time;
                info[bn] = pi;
            }
        }

        public void Reset()
        {
            info.Clear();
        }

        public string DumpCacheStatistics()
        {
            StringBuilder sb = new StringBuilder();
            int totalSize = 0;
            int cnt = 0;
            float totalTime = 0, totalWWWTime = 0, totalAssetTime = 0, totalWaitTime = 0;

            foreach (var i in info)
            {
                sb.Append(i.Key);
                sb.Append(":\t\t\t\tStartTime:");
                sb.Append(i.Value.StartTime.ToString("0.##"));
                sb.Append("\tTotalLoadTime:");
                sb.Append(i.Value.TotalLoadTime.ToString("0.##"));
                sb.Append("\tWWWLoadTime:");
                sb.Append(i.Value.WWWLoadTime.ToString("0.##"));
                sb.Append("\tAssetLoadTime:");
                sb.Append(i.Value.AssetLoadTime.ToString("0.##"));
                sb.Append("\tWaitTime:");
                sb.AppendLine(i.Value.WaitTime.ToString("0.##"));

                totalSize += i.Value.BundleSize;
                totalTime += i.Value.TotalLoadTime;
                totalWWWTime += i.Value.WWWLoadTime;
                totalAssetTime += i.Value.AssetLoadTime;
                totalWaitTime += i.Value.WaitTime;
                cnt++;
            }

            sb.Append("TotalCount:");
            sb.Append(cnt);
            sb.Append(" TotalTime:");
            sb.Append(totalTime.ToString("0.##"));
            sb.Append(" TotalSize:");
            sb.Append((totalSize / 1024 / 1024f).ToString("0.##MB"));
            sb.Append(" TotalWWWTime:");
            sb.Append(totalWWWTime.ToString("0.##"));
            sb.Append(" TotalAssetTime:");
            sb.Append(totalAssetTime.ToString("0.##"));
            sb.Append(" TotalWaitTime:");
            sb.Append(totalWaitTime.ToString("0.##"));

            return sb.ToString();
        }
    }

    class GOEAssetBundleProfileInfo
    {
        public string BundleName { get; set; }
        public int BundleSize { get; set; }
        public float StartTime { get; set; }
        public float WWWStartTime { get; set; }
        public float WWWFinishTime { get; set; }
        public float ResourceReadyTime { get; set; }
        public float AssetLoadStartTime { get; set; }

        public float AssetLoadFinishTime { get; set; }
        public float EndTime { get; set; }

        float ValidEndTime{get{return EndTime > 0 ? EndTime : (ResourceReadyTime > 0 ? ResourceReadyTime : StartTime);}}

        public float TotalLoadTime { get { return WWWLoadTime + AssetLoadTime; } }

        public float WWWLoadTime { get { return ResourceReadyTime > 0 ? ResourceReadyTime - WWWStartTime : 0; } }
        public float AssetLoadTime { get { return AssetLoadStartTime > 0 ? ValidEndTime - AssetLoadStartTime : 0; } }

        public float WaitTime { get { return ValidEndTime - StartTime - WWWLoadTime - AssetLoadTime; } }
    }
#endif
}
