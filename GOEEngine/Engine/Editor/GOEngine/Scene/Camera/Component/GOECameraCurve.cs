using System;
using UnityEngine;

namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOECameraCurve : GOECameraComponent
	{
		private bool mInited = false;
        public CurveHolderNew mCurveHolder;
        private float mCurveTime = 0.0f;
        private Vector3 startPos;
		private Quaternion startQuat = Quaternion.identity;

		public float CurveTime
		{
			get { return mCurveTime; }
		}

        public static GOECameraCurve AddCameraCurve()
        {
            if ((GOERoot.GOECamera as GOECamera).GetComponent<GOECameraCurve>() == null)
                return ComponentObject.Add<GOECameraCurve>(GOERoot.GOECamera as GOECamera);
            return null;
        }

		private static string ASSET_NAME = ".asset";
		
		public GOECameraCurve ()
		{
			this.Enable = false;
		}

        void OnLoadCurveAsset(string curveName, UnityEngine.Object obj)
        {
			Start ( obj as CurveHolderNew );
        }

        public void Play(string curveName)
        {
            Play(curveName, 0);
        }

        public void Play(string curveName, float time)
        {
			Init ( curveName, time );
			GOECamera.DisableMoveCameras ();
			Enable = true;
		}
        internal void Init(string curveName)
		{
			Init ( curveName, 0 );
		}
        internal void Init(string curveName, float time)
		{
			time /= 1000;
			if ( !curveName.Contains ( ASSET_NAME ) )
				curveName += ASSET_NAME;
			mCurveTime = time;
			GOERoot.ResMgrImp.GetAsset( curveName, this.OnLoadCurveAsset );
		}

		private void Start ( CurveHolderNew ch )
        {
            Init(ch);
			mInited = true;
        }

        internal void Stop()
		{
            this.LifeOver = true;
			this.Enable = false;
            GOERoot.GOECamera.ResetToDefault();
		}

        internal void Init(CurveHolderNew ch)
		{
            GOERoot.GOECamera.DisableMoveCameras();
            mCurveHolder = ch;
			mCurveTime += Time.deltaTime;
            Transform cpos = GOECamera.Camera.transform;
            startPos = new Vector3(cpos.position.x, cpos.position.y, cpos.position.z);
            startQuat = cpos.localRotation;
		}

        internal override void Update()
	    {
	        if ( mInited )
			{
                if (mCurveHolder.TimeOut(mCurveTime))
                    Stop();
                else
                {
                    Vector3 position = mCurveHolder.GetPositionByType(mCurveTime);
                    Quaternion rotation = mCurveHolder.GetRotationByType(mCurveTime);
                    if (mCurveHolder.isLocal)
                    {
                        position += startPos;
						rotation = rotation * startQuat;
                    }
                    GOECamera.Camera.transform.position = position;
                    GOECamera.Camera.transform.localRotation = rotation;
					
					mCurveTime += Time.deltaTime;
                }
                
			}
	    }
	}
}

