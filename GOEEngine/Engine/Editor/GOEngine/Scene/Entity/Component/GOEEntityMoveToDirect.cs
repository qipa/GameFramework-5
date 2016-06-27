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
 class GOEEntityMoveToDirect : GOEEntityComponent
    {
        private float mTime = 0;
        private float mCurTime = 0;
        private Vector3 mStartPos;
        private Vector3 mEndPos;
        internal Vector3 StartPos
        {
            set { mStartPos = value; }
            get { return mStartPos; }
        }

        internal Vector3 EndPos
        {
            set { mEndPos = value; }
            get { return mEndPos; }
        }

        internal float MaxTime
        {
            set { mTime = value; }
            get { return mTime; }
        }

        internal void Pause()
        {
            Enable = false;
        }

        internal void Restart()
        {
            Enable = true;
        }

        internal override void Update()
        {
            base.Update();
            float time = mCurTime;

            mCurTime += Time.deltaTime;

            if (time > mTime)
            {
                if (time > mTime)
                    this.LifeOver = true;
                //D.log("Effect hit:" + UnityEngine.Time.time);

                time = mTime;
            }

            if (mStartPos.Equals(mEndPos))
                return;

            if (LifeOver)
                return;
            Vector3 dir = (mEndPos - mStartPos).normalized;

            Vector3 pos = mStartPos + dir * (time / mTime) * (mEndPos - mStartPos).magnitude;
            Entity.Position = pos;

            dir.y = 0;

        }

        public void StopMove()
        {
            LifeOver = true;
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();
            Enable = false;
            Vector3 rot = Entity.Rotation.eulerAngles;
            rot.x = 0;
            rot.z = 0;
            Entity.Rotation = Quaternion.Euler(rot);
        }
    }
}
