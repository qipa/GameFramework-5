using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Collections;

namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class ResTypeGroup : ResComponent
    {
        protected Dictionary<string, GOEAsset> mDicAsset = new Dictionary<string, GOEAsset>();
        protected List<Resource> mDownResource = new List<Resource>();

        internal virtual void Clear()
        {
            mDicAsset.Clear();
            for (int i = mDownResource.Count - 1; i >= 0; --i)
            {
                Resource res = mDownResource[i];
                if (res.ResOK)
                {
                    mDownResource.RemoveAt(i);
                }
            }
        }

        internal Resource GetDownloadResource(string name)
        {
            for (int i = 0; i < mDownResource.Count; i++)
            {
                var res = mDownResource[i];
                if (res.Name == name)
                {
                    return res;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        Dictionary<string, string> fileMap;
        protected UnityEngine.Object LoadFromPrefab(string name, Type type)
        {
            if (fileMap == null)
            {
                fileMap = new Dictionary<string, string>();
                string[] paths = UnityEditor.AssetDatabase.GetAllAssetPaths();
                foreach (string file in paths)
                {
                    if (file.Contains("/generate/"))
                        continue;
                    fileMap[Path.GetFileName(file)] = file;
                }
            }
            string path;
            if (fileMap.TryGetValue(name, out path))
            {
                UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath(path, type);
                if (obj != null)
                {
                    return obj;
                }
            }

            return null;
        }
#endif

        internal virtual GOEAsset SetAsset(string name, UnityEngine.Object obj, string bundleName = null)
        {
            GOEAsset asset = new GOEAsset(obj, name, bundleName);
            GOEAsset oldAsset;
            if (mDicAsset.TryGetValue(name, out oldAsset))
                oldAsset.ReleaseAsset(true);
            mDicAsset[name] = asset;
            return asset;
        }

        protected virtual UnityEngine.Object GetAsset(string name)
        {
            GOEAsset old;
            if (mDicAsset.TryGetValue(name, out old))
                return old.GetAsset();
            return null;
        }

        protected virtual bool HasLoaded(string name)
        {
            return mDicAsset.ContainsKey(name);
        }

        protected virtual Resource CreateResource(string name, LoadPriority priority)
        {
            Resource res = new Resource();
            res.Name = name;
            res.Loader.Priority = priority;
            mDownResource.Add( res );

            GOEBundle bundle = ResourceMgr.GetBundle( name );
            if ( bundle != null )
            {
                res.Size = bundle.Size;
            }

            res.AddGotWWWCallback( this.OnLoadWWW );

            return res;
        }

        protected virtual void OnLoadWWW( string name, WWW www )
        {
            RemoveResource(name);
        }

        protected void RemoveResource( string name )
        {
            for ( int i = 0; i < mDownResource.Count; ++i )
            {
                Resource res = mDownResource[i];
                if ( res.Name == name )
                {
                    mDownResource[i].Destroy();
                    mDownResource.RemoveAt( i );
                    return;
                }
            }
        }

        internal void RemoveAllResource()
        {
            for ( int i = 0; i < mDownResource.Count; ++i )
            {
                Resource res = mDownResource[i];
                res.Destroy();
            }
            mDownResource.Clear();
        }

        internal virtual void ReleaseAssetbundle()
        {

        }

        internal virtual bool IsResCached(string name)
        {
            return HasLoaded(name);
        }

        internal virtual bool ReleaseAssetReference(string name, UnityEngine.Object obj)
        {
            GOEAsset old;
            if (mDicAsset.TryGetValue(name, out old))
            {
                old.ReleaseAssetReference(obj);
                return true;
            }
            return false;
        }

        internal virtual void RemoveAssetOnLevel()
        {
            RemoveAllAsset(true);
        }

        internal virtual void RemoveAllAsset(bool onlevel)
        {
            List<string> delKeys = new List<string>();
            if (mDicAsset.Count > 0)
            {
                Dictionary<string, GOEAsset>.Enumerator e = mDicAsset.GetEnumerator();
                while (e.MoveNext())
                {
                    bool releaseAsset = e.Current.Value.CanRelease(onlevel);
                    e.Current.Value.ReleaseAsset(releaseAsset);
                    if (releaseAsset)
                    {
                        delKeys.Add(e.Current.Key);
                    }
                }
                foreach (string key in delKeys)
                {
                    mDicAsset.Remove(key);
                }
                delKeys.Clear();
                //UnityEngine.Resources.UnloadUnusedAssets();
            }
        }

        internal virtual bool RemoveAsset(string name, bool force = true)
        {
            GOEAsset old;
            if (mDicAsset.TryGetValue(name, out old))
            {
                bool releaseAsset = old.CanRelease(false);
                if(!releaseAsset)
                {
                    if (!force)
                        return true;
                    //Logger.GetFile(LogFile.Res).LogError(name + " is referenced by others");
                }
                old.ReleaseAsset(releaseAsset);
                mDicAsset.Remove(name);
                //UnityEngine.Resources.UnloadUnusedAssets();
                return true;
            }
            return false;
        }

    }
}
