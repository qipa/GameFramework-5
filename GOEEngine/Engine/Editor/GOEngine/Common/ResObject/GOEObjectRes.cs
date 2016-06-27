using System;
using UnityEngine;
namespace GOEngine
{
    public enum ResStatus
    {
        NONE,
        OK,
        ERROR,
        WAIT,
    }
}
namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class ObjectRes : ComponentObject
    {
        protected string mName = string.Empty;
        protected ResStatus mResStatus = ResStatus.NONE;
        protected UnityEngine.Object mObject = null;
        protected float mDelayDestroyTime = 0.0f;
        private bool mAsyncLoad = true;
        private LoadPriority mLoadPriority = LoadPriority.Default;

        public Action OnResLoaded = null;

        internal ResStatus ResStatus
        {
            get{ return mResStatus; }
            set { mResStatus = value; }
        }
        internal UnityEngine.Object Object
        {
            get { return mObject; }
        }

        public string Name
        {
            set 
            {
                if (mName == value)
                    return;
                DestoryObject();
                if (mResStatus != ResStatus.NONE)
                    mResStatus = ResStatus.NONE;
                mName = value;
            }
            get { return mName;}
        }

        public LoadPriority Priority
        {
            set { mLoadPriority = value; }
            get { return mLoadPriority; }
        }

        public void Reload()
        {
            DestoryObject();
            if (mResStatus != ResStatus.NONE)
                mResStatus = ResStatus.NONE;
            Load();
        }

        
        public void Load()
        {
            if ( mName.Length < 1 )
            {
                return;
            }
            if ( mResStatus != ResStatus.NONE )
            {
                return;
            }
            
            mResStatus = ResStatus.WAIT;

            LoadRes( mName, OnLoadRes );
            
        }

        protected virtual void LoadRes(string name, Action<string, UnityEngine.Object> callback)
        {
            GOERoot.ResMgrImp.GetAsset( name, callback, mLoadPriority );
        }

        internal virtual void Destroy()
        {
            DestoryObject();
            base.OnDestroy();
        }

        protected virtual void DestoryObject()
        {
            if (this.ResStatus == GOEngine.ResStatus.OK || this.ResStatus == GOEngine.ResStatus.ERROR)
            {
                GOERoot.ResMgrImp.ReleaseAsset(Name, mObject);
            }
            else if (this.ResStatus == GOEngine.ResStatus.WAIT)
            {
                GOERoot.ResMgrImp.ReleaseAssetCallback(Name, OnLoadRes);
            }
            if (mObject != null)
            {
                mObject = null;
            }

            mResStatus = ResStatus.NONE;
        }

        private void OnLoadRes( string name, UnityEngine.Object obj )
        {
            mResStatus = ResStatus.OK;

            mObject = obj;

            if (null == mObject)
            {
                mResStatus = ResStatus.ERROR;
            }

            this.OnLoadRes( name );
            
            if (OnResLoaded != null )
            {
                OnResLoaded();
            }
        }
        
        protected virtual void OnLoadRes( string name )
        {
            
        }
    }
}

