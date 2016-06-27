using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOEngine.Implement
{
    public class GOEBundleResource
    {
        HashSet<GOEBundleResource> referencedBy = new HashSet<GOEBundleResource>();
        List<GOEBundleResource> dependsOn = new List<GOEBundleResource>();
        Dictionary<string, GOEAsset> loadedAssets = new Dictionary<string, GOEAsset>();
        List<GOEAsset> assetList = new List<GOEAsset>();
        bool released = false;
        float hitTime = Time.realtimeSinceStartup;

        /// <summary>
        /// 最少存活时间，避免依赖包刚加载上了发现没用就被卸载了或者短时间内反复需要
        /// </summary>
        const float MinimalSurvivalTime = 15f;
        public GOEBundle BundleInfo { get; set; }

        public AssetBundle AssetBundle { get; set; }

        public bool HasAssetLoaded { get; set; }

        public string Name { get { return BundleInfo.mName; } }

        public bool HasReference { get { return referencedBy.Count > 0; } }

        public int Size { get { return BundleInfo.Size; } }

        public bool Released { get { return released; } }

        public float HitTime { get { return hitTime; } }

        public bool IsSceneBundle { get; set; }

        public int AssetCount { get { return assetList.Count; } }

        public HashSet<GOEBundleResource> ReferencedBy { get { return referencedBy; } }

        public bool CanRelease
        {
            get
            {
                if (IsSceneBundle || BundleInfo.BundlePersistent || BundleInfo.AssetPersistent)
                    return false;
                for (int i = 0; i < assetList.Count; i++)
                {
                    if (!assetList[i].CanRelease(false))
                        return false;
                }
                return !HasReference && SurvivalTimeExpired;
            }
        }

        public bool SurvivalTimeExpired
        {
            get
            {
                return (Time.realtimeSinceStartup - hitTime > MinimalSurvivalTime);
            }
        }

        public bool CanReleaseOnLevel
        {
            get
            {
                if (BundleInfo.BundlePersistent || BundleInfo.AssetPersistent)
                    return false;
                for (int i = 0; i < assetList.Count; i++)
                {
                    if (!assetList[i].CanRelease(true))
                        return false;
                }
                return true;
            }
        }

        public GOEAsset this[string idx]
        {
            get
            {
                GOEAsset res;
                if (loadedAssets.TryGetValue(idx, out res))
                    return res;
                else
                    return null;
            }
        }

        public void AddDependency(GOEBundleResource bundle)
        {
            dependsOn.Add(bundle);
            bundle.referencedBy.Add(this);
            bundle.BundleInfo.IsDependency = true;//被依赖的包永不能卸载AssetBundle
        }

        public void AddAsset(string name, GOEAsset asset)
        {
            if (!loadedAssets.ContainsKey(name))
            {
                loadedAssets[name] = asset;
                assetList.Add(asset);
            }
        }

        public void Touch()
        {
            hitTime = Time.realtimeSinceStartup;
        }

        public bool ContainsAsset(string name)
        {
            return loadedAssets.ContainsKey(name);
        }

        /// <summary>
        /// 尝试释放Unity的AssetBundle
        /// </summary>
        public void ReleaseAssetBundle(bool forceRelease = false)
        {
            if (!BundleInfo.BundlePersistent && !BundleInfo.IsDependency && AssetBundle)
            {
                //只有没被引用才能卸载AssetBundle，
                if ((!HasReference && SurvivalTimeExpired) || forceRelease)
                {
                    AssetBundle.Unload(false);
                    AssetBundle = null;
                }
            }
        }

        /// <summary>
        /// 释放Bundle以及其包含的Asset的资源
        /// </summary>
        /// <param name="forceRelease">无视Asset引用状况强制释放，用于场景切换</param>
        public void Release(bool forceRelease = false)
        {
            ReleaseAssetBundle(forceRelease);
            if (!released)
            {
                if ((CanRelease && !forceRelease) || (forceRelease && CanReleaseOnLevel))
                {
                    for (int i = 0; i < assetList.Count; i++)
                    {
                        assetList[i].ReleaseAsset(true);
                    }
                    assetList.Clear();
                    loadedAssets.Clear();

                    for (int i = 0; i < dependsOn.Count; i++)
                    {
                        dependsOn[i].referencedBy.Remove(this);
                    }
                    dependsOn.Clear();

                    if (HasReference)
                    {
                        Debug.LogWarning(string.Format("Releasing bundle {0} while still referenced by {1} other bundles", Name, referencedBy.Count));
                    }
                    referencedBy.Clear();
                    released = true;
                    if (AssetBundle)
                    {
                        //如果Assetbundle未被卸载，说明是依赖的bundle，需手动删除
                        AssetBundle.Unload(false);
                        AssetBundle = null;
                    }
                }
            }
        }

        public void ReleaseInstances()
        {
            for (int i = 0; i < assetList.Count; i++)
            {
                if (assetList[i].NeedInstance)
                    assetList[i].Reference = 0;
            }
        }
    }
}
