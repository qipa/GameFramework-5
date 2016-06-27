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
        class ResourceLoader : WWWLoader
    {
        public enum MultiLoaderState
        {
            None,
            Preload,
            Load,
            Done,
        }

        internal static int MaxLoadTime = 30;
        private float loadScale = 1f;
        private WWW loadWWW;
        private float mLastProgress = 0;

        private MultiLoaderState mState = MultiLoaderState.None;

        public bool IsFromWeb = false;
        public bool IsFromStream = false;
        HashSet<string> pendingDependencies;

        public bool IsLoading { get { return mState == MultiLoaderState.Load; } }

        public bool IsRawResource { get; set; }

        public bool IsDestroyed { get; set; }

        public bool IsDone
        {
            get
            {
                return this.IsFinished();
            }
        }

        public float Progress
        {
            get
            {
                return this.GetProgerss();
            }
        }

        public WWW WWW
        {
            get
            {
                if (!IsDone)
                {
                    Logger.GetFile(LogFile.Res).LogError("www Get where !IsDone");
                }

                return loadWWW;
            }
        }

        public string Text
        {
            get
            {
                if ( !IsDone )
                {
                    Logger.GetFile( LogFile.Res ).LogError( "Text.Get where !IsDone" );
                }

                return loadWWW.text;
            }
        }

        public AudioClip AudioClip
        {
            get
            {
                if ( !IsDone )
                {
                    Logger.GetFile( LogFile.Res ).LogError( "AudioClip.Get where !IsDone" );
                }

                return loadWWW.audioClip;
            }
        }

        public byte[] Byte
        {
            get 
            {
                if ( !IsDone )
                {
                    Logger.GetFile( LogFile.Res ).LogError( "Byte.Get where !IsDone" );
                }

                return loadWWW.bytes;
            }
        }
        AssetBundle bundle;

        public AssetBundle AssetBundle
        {
            get
            {
                if ( !IsDone )
                {
                    Logger.GetFile( LogFile.Res ).LogError( "AssetBundle.Get where !IsDone" );
                }
                //bundle只能被获取一次
                if (bundle == null)
                    bundle = loadWWW.assetBundle;
                return bundle; 
            }
        }

        private bool IsFinished()
        {
            if ( mState != MultiLoaderState.Done )
            {
                return false;
            }

            return true;
        }

        private float fakeValue = 0;
        private float GetProgerss()
        {
            if (IsFinished())
                return 1;
            float loadCount = 0;
            if (loadWWW != null)
                loadCount = loadWWW.progress;
            return loadCount;
        }

        public void BeginLoad()
        {
            if (mWWWType == GOEngine.WWWType.AssetBundle)
                loadScale = 0.9f;

            mRealName = mName;            
#if DEBUG_BUNDLE_CACHE
            ResourceMgr.Instance().SampleLoadEvent(mName, 2);
#endif     
            GOEBundleMapItem item = GetFinalBundle(mName, IsFromWeb);
            mRealName = item.FinalName;
            string name = mRealName;

            loadWWW = WWWUtil.CreateWWW(name, IsRawResource, IsFromStream, IsFromWeb);
            mBeginLoadTime = 0;

        }

        public static GOEBundleMapItem GetFinalBundle(string name, bool fromWeb = false)
        {
            GOEBundleMapItem item = null;
            if(!fromWeb)
                item = ResourceMgr.Instance().BundleRename.GetBundleItemFromOriginalName( name );
            if ( null == item )
            {
                item = new GOEBundleMapItem();
                item.FinalName = name;
            }
            
            return item; 
        }


        private bool IsAllDone()
        {
            return loadWWW != null && loadWWW.isDone;
        }

        void BeginPreload()
        {
            GOEBundle bundle = ResourceMgr.Instance().GetBundle(mName);
            pendingDependencies = null;
            if (bundle != null)
            {
                foreach (var dep in bundle.DependsOn)
                {
                    if (pendingDependencies == null)
                        pendingDependencies = new HashSet<string>();
                    pendingDependencies.Add(dep);
                    GOERoot.ResMgr.PreloadBundle(dep, OnPreloadBundle, Priority, true);
                }
            }
        }

        void OnPreloadBundle(string name, AssetBundle bundle)
        {
            if (pendingDependencies != null)
                pendingDependencies.Remove(name);
        }

        public void Update()
        {
            if ( mState == MultiLoaderState.None )
            {
                BeginPreload();
                NextState();
            }
            else if (mState == MultiLoaderState.Preload)
            {
                if (pendingDependencies == null || pendingDependencies.Count == 0)
                {
                    BeginLoad();
                    NextState();
                }
            }
            if ( mState == MultiLoaderState.Load )
            {
                if (IsAllDone())
                {                   
#if DEBUG_BUNDLE_CACHE
                    ResourceMgr.Instance().SampleLoadEvent(mName, 3);
#endif     
                    NextState();
                }
                float pro = Progress;

                if ( pro != mLastProgress )
                {
                    mLastProgress = pro;
                    mBeginLoadTime = 0;
                }
                else
                {
                    mBeginLoadTime += Time.deltaTime;
                }

                if (mBeginLoadTime >= MaxLoadTime)
                {
                    mState = MultiLoaderState.None;

                    Logger.GetFile( LogFile.Res ).LogError( "dead link: " + mName );
                }
            }
        }        

        protected void NextState()
        {
            mState += 1;
        }

        public void Destroy()
        {
            if(WWWType != GOEngine.WWWType.WWW && loadWWW != null)
            {
                loadWWW.Dispose();
                loadWWW = null;
            }
            mState = MultiLoaderState.None;
            IsDestroyed = true;
        }
    }
}
