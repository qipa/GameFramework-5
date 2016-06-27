using System;
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
        class GOEBaseScene : ComponentObject
	{
        private GOECamera _camera = null;

        public GOEBaseScene()
        {
			_camera = new GOECamera();
            this.AddComponent<GOESceneEntityContainer>();
            this.AddComponent<GOESceneBigTerrainX>();
            this.AddComponent<GOESceneRandomTile>();
            GOESceneCameraTransitionAreaMgr areaMgr = this.AddComponent<GOESceneCameraTransitionAreaMgr>() as GOESceneCameraTransitionAreaMgr;
        }

        public GOECamera Camera
        {
            get { return _camera; }
        }

        internal virtual void OnEnter()
        {
            GOESceneComponent comp;
            for (int i = 0; i < mListComponent.Count; i++)
            {
                comp = mListComponent[i] as GOESceneComponent;
                comp.OnEnter();
            }
        }

        internal virtual void OnLeave()
        {
            GOESceneComponent comp;
            for (int i = 0; i < mListComponent.Count; i++)
            {
                comp = mListComponent[i] as GOESceneComponent;
                comp.OnLeave();
            }
        }

	}
}
