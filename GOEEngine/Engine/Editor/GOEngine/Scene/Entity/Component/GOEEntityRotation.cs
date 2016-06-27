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
 class GOEEntityRotation : GOEEntityComponent
   {
       private float mTotalTime = 0.0f;
       private float mCurTime = 0.0f;
       private Quaternion mTarget = Quaternion.identity;
       private Quaternion mStart = Quaternion.identity;
       private bool m_bBegin;
       public void StartRotation(Quaternion target, float total)
       {
           mStart     = this.Entity.Rotation;
           mTarget    = target;
           mTotalTime = IsRotationSpeedUniform ? total * Quaternion.Angle(mStart, mTarget) / 180.0f : total;
           mCurTime   = 0.0f;
           m_bBegin   = true;
       }

        /// <summary>
        /// 是否匀速旋转
        /// </summary>
       public bool IsRotationSpeedUniform { get; set; }

       public bool IsBegin
       {
           get { return m_bBegin; }
       }

       public Quaternion Target
       {
           get { return mTarget; }
       }

       public void InterruptRotation()
       {
           m_bBegin = false;
       }

       internal override void Update()
       { 
           if (m_bBegin)
           {
               mCurTime += GOERoot.RealTime.DeltaTime;
               if (mCurTime < mTotalTime)
               {
                   this.Entity.Rotation = Quaternion.Slerp(mStart, mTarget, mCurTime / mTotalTime);
               }
               else
               {
                   this.Entity.Rotation = mTarget;
                   m_bBegin = false;
               }
           }
       }

       public void Abort()
       {
           InterruptRotation();
           this.Entity.Rotation = mTarget;
       }

    }
}
