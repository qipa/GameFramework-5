﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
    class ResList : ResComponent
    {
        protected List<string> mListRes = new List<string>();
        protected List<string> mListAsset = new List<string>();

        internal void AddBundle(string name, string asset = null)
        {
            foreach (string res in mListRes)
            {
                if (res == name)
                {
                    return;
                }
            }

            if (name.Length == 0)
            {
                Logger.GetFile(LogFile.Res).LogError(" ResList:AddBundle name can't empty");
                return;
            }
            GOEBundle bundle = ResourceMgr.Instance().GetBundle(name);
            if (bundle == null)
                UnityEngine.Debug.LogError("Cannot find bundle " + name);
            mListRes.Add(name);
            
            if (asset == null)
            {
                asset = bundle.Files.ElementAt(bundle.Files.Count - 1);
            }
            mListAsset.Add(asset);
            foreach (var i in bundle.DependsOn)
            {
                GOEBundle dep = ResourceMgr.Instance().GetBundle(i);
                mListRes.Add(i);
                mListAsset.Add(dep.FirstAsset);
            }
        }

        internal void AddWWW(string name)
        {
            foreach (string res in mListRes)
            {
                if (res == name)
                {
                    return;
                }
            }

            if (name.Length == 0)
            {
                Logger.GetFile(LogFile.Res).LogError(" ResList:AddBundle name can't empty");
                return;
            }

            mListRes.Add(name);
            GOEBundleMapItem bundle = ResourceLoader.GetFinalBundle(name);
            if (bundle == null)
                UnityEngine.Debug.LogError("Cannot find bundle " + name);
            mListAsset.Add(bundle.FinalName);
        }
        

        internal void AddAsset(string name)
        {
            string bundleName = ResourceMgr.Instance().GetBundleName( name );
            if ( bundleName != string.Empty )
            {
                this.AddBundle( bundleName, name );
            }
        }
    }
}
