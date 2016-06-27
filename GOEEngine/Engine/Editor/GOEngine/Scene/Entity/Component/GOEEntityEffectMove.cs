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
 class GOEEntityEffectMove : GOEEntityComponent
    {
        private string mEffectName = string.Empty;
        private float mTime = 0;
        private float mCurTime = 0;
        private Vector3 mStartPos;
        private Vector3 mEndPos;
        private IEntity mEffect;

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

        internal string EffectName
        {
            set { mEffectName = value; }
        }

        internal float MaxTime
        {
            set { mTime = value; }
            get { return mTime; }
        }

        internal float DestructionDelay
        {
            get;
            set;
        }

        private float mHRatio = 0f;
        internal float HeightRatio
        {
            get { return mHRatio; }
            set { mHRatio = value; }
        }

        private float mHSpeed = 0;
        private float mHAccelerate = 0;
        internal void StartMove(float speed = 1)
        {
            mEffect = this.Entity.Scene.AddEffect(mEffectName, mStartPos, Quaternion.identity, 1, MaxTime);
            mEffect.Position = mStartPos;
            if(speed != 1)
            {
                mTime = mTime / speed;
                mEffect.SetSpeed(speed);
            }
            float height = Vector3.Distance(mEndPos, mStartPos) * mHRatio;
            mHAccelerate = -height * 2 * 4 / (mTime * mTime);
            mHSpeed = -mHAccelerate * mTime / 2;
            mEffect.Load();
        }

        internal void Pause()
        {
            Enable = false;
            if (mEffect != null)
                mEffect.Pause();
        }

        internal void Restart()
        {
            Enable = true;
            if (mEffect != null)
                mEffect.Restart();
        }

        internal override void Update()
        {
            base.Update();

            float time = mCurTime;

            mCurTime += Time.deltaTime;
            if (time > mTime)
            {
                if (time > (mTime + DestructionDelay))
                    this.LifeOver = true;
               
                time = mTime;
            }

            if (mStartPos.Equals(mEndPos))
                return;
            if (mEffect != null)
            {

                Vector3 dir = (mEndPos - mStartPos).normalized;

                Vector3 pos = mStartPos + dir * (time / mTime) * (mEndPos - mStartPos).magnitude;

                if( float.IsNaN(pos.x) || float.IsNaN(pos.y) || float.IsNaN(pos.z))
                {
                    LifeOver = true;
                    pos = Vector3.zero;
                }
                if(mHRatio != 0)
                {
                    pos.y = mStartPos.y + mHSpeed * time + mHAccelerate * time * time / 2;
                }
                if (mEffect.Position.EqualsVector3(ref pos))
                    return;
                mEffect.Orientation = (pos - mEffect.Position);
                mEffect.Position = pos;
            }
        }

        

        internal override void OnDestroy()
        {
            base.OnDestroy();

            if (mEffect != null)
            {
                (mEffect as Entity).Destroy();
                mEffect = null;
            }
        }


    }
}
