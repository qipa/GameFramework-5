using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Threading;

namespace GOEngine
{
    public enum LoaderState
    {
        None,
        Wait,
        Loading,
        Over,
    }

    public enum WWWType
    {
        AssetBundle,
        Text,
        Audio,
        Byte,
        WWW,
    }

    public enum LoadPriority
    {
        Default = 0,
        MostPrior = 20,
        Prior = 10,
        HighPrior = 15,
        PostLoad = -10,
    }
}
namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class WWWLoader
    {
        protected string mName = string.Empty;
        protected float mBeginLoadTime = 0;
        private LoaderState mState = LoaderState.None;
        protected WWWType mWWWType = WWWType.AssetBundle;

        private Resource mResource = null;

        private LoadPriority mPriority = LoadPriority.Default;
        
        protected string mRealName = string.Empty;

        public int Size
        {           
            get { return mResource.Size; }
        }

        public LoaderState State
        {
            set { mState = value; }
            get { return mState; }
        }

        public WWWType WWWType
        {
            set { mWWWType = value; }
            get { return mWWWType; }
        }

        public LoadPriority Priority
        {
            set { mPriority = value; }
            get { return mPriority; }
        }

        public Resource Resource
        {
            set { mResource = value; }
            get { return mResource; }
        }

        public string Name
        {
            set { mName = value; }
            get { return mName; }
        }

        public float BeginLoadTime
        {
            get { return mBeginLoadTime; }
        }
    }
}
namespace GOEngine
{
    public class WWWUtil
    {
        /// <summary>
        /// 禁用Unity的Cache，pc上等需要双开的环境需要禁用
        /// </summary>
        public static bool DisableLoadCache { get; set; }
        public static WWW CreateWWW(string name, bool isRawResource, bool inStream = false, bool fromWeb = false)
        {
            string path = null;
            if (fromWeb)
            {
                path = SysConf.GAME_RES_URL + "/" + name;
                return createWWW(path, true);
            }
            if (EngineDelegate.DynamicResource)
            {
                path = Application.persistentDataPath + "/" + name;
                if (File.Exists(path))
                {
                    path = "file:///" + path;
                    return createWWW(path, true);
                }
            }
            if (inStream || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                path = Application.streamingAssetsPath + "/" + name;
                if (Application.platform != RuntimePlatform.Android)
                    path = "file:///" + path;
            }
            else
            {
                if (!GOERoot.IsEditor)
                {
                    if (!EngineDelegate.TestMobile)
                        path = "file:///" + Application.dataPath + "/../../bin/res/" + name;
                    else
                        path = "file:///" + Application.dataPath + "/StreamingAssets/" + name;
                }
                else
                {
                    path = "file:///" + Application.dataPath + "/../../../client/bin/res/" + name;
                }
            }
            return createWWW(path, isRawResource);
        }

        private static WWW createWWW(string path, bool isRaw)
        {
            WWW www = isRaw || DisableLoadCache ? new WWW(path) : WWW.LoadFromCacheOrDownload(path, 1);
            if (Implement.ResourceMgr.Instance().LowAsyncLoadPriority)
                www.threadPriority = UnityEngine.ThreadPriority.Low;
            else
                www.threadPriority = UnityEngine.ThreadPriority.High;
            return www;
        }

        public static WWW CreateWWWForDynamic(string name)
        {
            string path = SysConf.GAME_RES_URL + "/" + name;
            return createWWW(path, true);
        }
    }
}
