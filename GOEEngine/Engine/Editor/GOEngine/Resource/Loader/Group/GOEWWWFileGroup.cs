﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class WWWFileGroup : ResTypeGroup
    {
        protected Dictionary<string, System.Object> mDicObject = new Dictionary<string, System.Object>();

        internal void GetTextFile(string name, Action<string, string> func, bool fromStream = false)
        {
            Resource res = loadFile(name, func, false, fromStream);
            if (res != null)
            {
                res.Loader.WWWType = WWWType.Text;
                res.AddGotTextCallback(func);
            }
        }

        internal void GetByteFile(string name, Action<string, byte[]> func, bool fromStream = false)
        {
            Resource res = loadFile(name, func, false, fromStream);
            if (res != null)
            {
                res.Loader.WWWType = WWWType.Text;
                res.AddGotBytesCallback(func);
            }
        }

        internal void getWWWFileFromServer(string name, Action<string, WWW> func)
        {
            Resource res = loadFile(name, func, true);
            if (res != null)
            {
                res.Loader.WWWType = WWWType.WWW;
                res.AddGotWWWCallback(func);
            }
        }

        private Resource loadFile<T>(string name, Action<string, T> func, bool fromWeb = false, bool fromStream = false)
        {
            if (name == null || name == string.Empty)
            {
                return null;
            }

            if (HasLoaded(name))
            {
                System.Object www = this.GetObject(name);
                if (func != null)
                {
                    func(name, (T)www);
                }
                return null;
            }

            Resource res = this.GetDownloadResource(name);
            if (res == null)
            {
                res = this.CreateResource(name, LoadPriority.Default);
                res.Loader.IsFromWeb = fromWeb;
                res.Loader.IsFromStream = fromStream;
                res.LoadRes();
            }
            return res;
        }

        private System.Object GetObject(string name)
        {
            System.Object www;
            if (mDicObject.TryGetValue(name, out www))
            {
                return www;
            }

            return null;
        }

        protected override bool HasLoaded(string name)
        {
            return mDicObject.ContainsKey(name);
        }

        internal override void Clear()
        {
            base.Clear();

            mDicObject.Clear();
        }

        protected override void OnLoadWWW(string name, WWW www)
        {
            //base.OnLoadWWW(name, www);
        }

        protected virtual void OnLoadObject(string name, WWW bundle)
        {
            if (bundle == null || bundle.bytes.Length == 0 || (!string.IsNullOrEmpty(bundle.error)))
            {
                RemoveResource(name);
                return;
            }
            System.Object old;
            if (mDicObject.TryGetValue(name, out old))
            {
                Logger.GetFile(LogFile.Res).LogError(name + " already in mDicWWW");
            }
            Resource res = this.GetDownloadResource(name);
            if (res.Loader.WWWType == WWWType.Text)
                mDicObject.Add(name, bundle.text);
            else
                mDicObject.Add(name, bundle.bytes);
            if (res.Loader.WWWType != WWWType.WWW)
                RemoveResource(name);
        }

        protected override Resource CreateResource(string name, LoadPriority priority)
        {
            Resource res = base.CreateResource(name, priority);

            res.AddGotWWWCallback(this.OnLoadObject);

            return res;
        }

        internal override bool RemoveAsset(string name, bool force = true)
        {
            System.Object obj;
            if (mDicObject.TryGetValue(name, out obj))
            {
                mDicObject.Remove(name);
                return true;
            }
            return false;
        }
    }
}
