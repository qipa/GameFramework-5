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
 class GOESceneCameraTransitionAreaMgr : GOESceneComponent
    {
        private List<CameraTransitionArea> mListArea = new List<CameraTransitionArea>();
        // private Entity mHero = null;
        // private GOECamera mCamera = null;
        private CameraTransitionArea mLastArea = null;
        public void SetHero( Transform tf )
        {
            for (int i = 0; i < mListArea.Count; ++i)
            {
                mListArea[i].target = tf;
            }
        }

        //public GOECamera Camera
        //{
        //    set { mCamera = value; }
        //}

        private void AddTransitionArea(CameraTransitionArea area)
        {
            mListArea.Add(area);
        }


        internal override void OnEnter()
        {
            // 进入场景，收集所有的CameraTransitionArea对象 //
            CameraTransitionArea[] areas = GameObject.FindObjectsOfType(typeof(CameraTransitionArea)) as CameraTransitionArea[];
            for (int i = 0; i < areas.Length; ++i)
            {
                AddTransitionArea(areas[i]);
            }
        }


        //internal override void Update()
        //{
        //    base.Update();

        //    if (mHero == null || mCamera == null)
        //        return;

        //    if (mLastArea != null)
        //    {
        //        if (mLastArea.IsInArea(mHero.Position))
        //        {
        //            return;
        //        }
        //        else
        //        {
        //            mCamera.SmoothTransition(mCamera.GetOrgRadius(), mCamera.GetOrgY(), mCamera.GetOrgAngle(), mLastArea.trsTime);
        //            mLastArea = null;
        //        }
        //    }

        //    for (int i = 0; i < mListArea.Count; ++i)
        //    {
        //        if (mListArea[i].IsInArea(mHero.Position))
        //        {
        //            if (mLastArea == mListArea[i])
        //            {
        //                break;
        //            }
        //            else
        //            {
        //                mCamera.SmoothTransition(mListArea[i].trsRadius, mListArea[i].trsY, mListArea[i].trsAngle, mListArea[i].trsTime);
        //                mLastArea = mListArea[i];
        //                break;
        //            }
        //        }
        //    }
        //}

        internal void Clear()
        {
            mListArea.Clear();
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();
            mListArea.Clear();
            mListArea = null;
            //mHero = null;
            //mCamera = null;
        }
    }
}
