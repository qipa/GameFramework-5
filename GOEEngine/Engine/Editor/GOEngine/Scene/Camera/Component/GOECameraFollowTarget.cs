//using System;
//using UnityEngine;

//namespace GOEngine.Implement
//{
//#if UNITY_EDITOR
//    public
//#else
//    internal
//#endif
//        class GOECameraFollowTarget : GOECameraComponent
//    {

//        private Vector3 mTrsShift;
//        private float mCurTrsTime = 0;
//        private float mTrsTimeLength = 0;
//        private bool mBeginTrs = false;

//        private Entity target = null;

//        private Vector3 mShift = new Vector3(6.21f, 9.13f, -7.2f);

//        private float mOrgRadius;
//        private float mOrgY;
//        private float mOrgAngle;

//        SmoothFollowerObj mPosFollow;

//        internal Vector3 Shift
//        {
//            set { mShift = value; }
//            get { return mShift; }
//        }


//        internal float OrgRadius
//        {
//            get { return mOrgRadius; }
//        }

//        internal float OrgY
//        {
//            get { return mOrgY; }
//        }

//        internal float OrgAngle
//        {
//            get { return mOrgAngle; }
//        } 


//        public void SetShift(float radius, float y, float angle)
//        {
//            mOrgRadius = radius;
//            mOrgY = y;
//            mOrgAngle = angle;


//            float x = radius * Mathf.Sin(angle) * Mathf.Cos(0);
//            float z = radius * Mathf.Cos(angle);
//            Shift = new Vector3(x, y, z);
//        }

//        internal override void Start()
//        {
//            base.Start();

//            this.Enable = false;

//            initPos();
//        }

//        internal Entity Target
//        {
//            get { return target; }
//            set { target = value; }
//        }

//        internal override void EnterScene()
//        {
//            base.EnterScene();
//            this.InitProp();
//            initPos();
//        }
		
//        private void initPos()
//        {
//            mPosFollow = new SmoothFollowerObj(0.5f, 0.5f);
//            mPosFollow.Update(GOECamera.Position, 0, true);
//        }

//        private Vector3 _tarPos;
//        internal override void Update()
//        {
//            if(target == null)
//                return;
//            //if (_tarPos != null && _tarPos.x == target.Position.x && _tarPos.z == target.Position.z)
//                //return;

//            if (mBeginTrs)
//            {
//                mCurTrsTime += Time.deltaTime;
//                float temp = mCurTrsTime / mTrsTimeLength;
//                mShift.x = Mathf.Lerp(mShift.x, mTrsShift.x, temp);
//                mShift.y = Mathf.Lerp(mShift.y, mTrsShift.y, temp);
//                mShift.z = Mathf.Lerp(mShift.z, mTrsShift.z, temp);

//                if (mCurTrsTime >= mTrsTimeLength)
//                {
//                    mBeginTrs = false;
//                } 
//            }



//            _tarPos = target.Position;
//            if(target.GameObject != null)
//                GOECamera.LookAt(target.GameObject.transform);
//            GOECamera.Position = mPosFollow.Update(_tarPos + mShift, Time.deltaTime);

//            //GOECamera.Position = _tarPos + mShift;
			
//            base.Update();
//        }

//        private void InitProp()
//        {
//            GOECamera.Fov = GOEngine.SysConf.CAMERA_FOV;
//            GOECamera.Far = GOEngine.SysConf.CAMERA_FAR;
//            GOECamera.Near = GOEngine.SysConf.CAMERA_NEAR;
//        }

		
//        public void SmoothTransition(float radius, float y, float angle, float time)
//        {
//            float x = radius * Mathf.Sin(angle) * Mathf.Cos(0);
//            float z = radius * Mathf.Cos(angle);
//            mTrsShift = new Vector3(x, y, z);

//            mTrsTimeLength = time;
//            mCurTrsTime = 0;
//            mBeginTrs = true;
//        }


//    }
//}

