using System;
using UnityEngine;
using GOEngine.Implement;

namespace GOEngine
{
    [System.Reflection.Obfuscation(Exclude = true)]
    public class GOERoot : MonoBehaviour
    {
		public static long FrameNum { set; get; }
        static private GOERoot _mSystem = null;
        static public void Start(GameObject context, Action onReady = null, bool bEditor = false)
        {
            JsonUtil.InitJson();
            GameObject.DontDestroyOnLoad(context);
            if (context.GetComponent<AudioListener>() == null)
                context.AddComponent<AudioListener>();
            _mSystem = context.AddComponent<GOERoot>();
            _mSystem._mbEditor = bEditor;
            _mSystem._context = context;
            _mSystem._onEngineReady += onReady;
            _mSystem.Init();
        }

        public static GOEngine.IProjectResource ResMgr
        {
            get { return _mSystem._mResMgr; }
        }

        public static IRealtime RealTime
        {
            get { return _mSystem._realTime; }
        }

        public static GOEngine.IGOELog GetLogger(string logname)
        {
            return Logger.GetFile(logname);
        }

        public static IGOEGameScene Scene
        {
            get { return _mSystem._mScene; }
        }

        public static IGOECamera GOECamera
        {
            get { return _mSystem._mScene.Camera; }
        }

        public static IGOEAudioMgr GOEAudioMgr
        {
            get { return _mSystem._audioMgr; }
        }

        public static IGOEQualityMgr GOEQualityMgr
        {
            get { return _mSystem._qualityMgr; }
        }

        static public bool IsEditor
        {
            get { return _mSystem._mbEditor; }
        }

        static public bool IsRunning
        {
            get { return null != _mSystem; }
        }

        //======================== internal api begin ==========================//
        #region

#if UNITY_EDITOR
	public
#else
        internal
#endif
 static ProjectResource ResMgrImp
        {
            get { return _mSystem._mResMgr; }
        }

#if UNITY_EDITOR
	public
#else
        internal
#endif
 static GOEGameScene SceneImp
        {
            get { return _mSystem._mScene; }
        }

#if UNITY_EDITOR
	public
#else
        internal
#endif
 static GOEAudioMgr GOEAudioMgrImp
        {
            get { return _mSystem._audioMgr; }
        }


        internal static GameObject Context
        {
            get { return _mSystem._context; }
        }

        #endregion
        //======================== internal api end ==========================//

        private ProjectResource _mResMgr = null;
        private Realtime _realTime = null;
        private GOEGameScene _mScene = null;
        private bool _mbEditor = false;
        private GOEAudioMgr _audioMgr;
        private Action _onEngineReady;
        private GOEQualityMgr _qualityMgr;
        private GameObject _context;
        private void Init()
        {
			FrameNum = 0;
            Logger.Start(false);
            if (_mbEditor)
            {
#if UNITY_EDITOR
				_mResMgr = new EditorResource();
#endif
            }
            else
            {
                _mResMgr = new ProjectResource();
            }
            if (_mResMgr != null)
                _mResMgr.Start();

            _realTime = new Realtime();

            _mScene = new GOEGameScene();

            _audioMgr = new GOEAudioMgr();

            _qualityMgr = new GOEQualityMgr();

        }

        void OnDestory()
        {
            _mSystem.Destroy();
            _mSystem = null;
        }
        void Update()
        {
			++FrameNum;
            if (_realTime != null)
                _realTime.Update();

            if (_mResMgr != null)
                _mResMgr.Update();
            if (_mScene != null)
            {
                _mScene.Update();
                _mScene.Camera.Update();
            }
            if (_mSystem._mResMgr != null && _mSystem._mResMgr.IsReady && _onEngineReady != null)
            {
                _onEngineReady();
                _onEngineReady = null;
            }
            _mSystem._audioMgr.Update();
        }

        void OnLevelWasLoaded()
        {
            //Scene.OnLevelWasLoaded();
            _mSystem._mResMgr.OnEnterScene();
            _mSystem._mScene.Camera.EnterScene();
            _mSystem._mScene.OnEnter();
        }

        private void Destroy()
        {
            _mScene.DeInit();
            _mScene = null;

            _mResMgr.Shutdown();
            _mResMgr = null;

            Logger.Shutdown();
        }
    }
}

