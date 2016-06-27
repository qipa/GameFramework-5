using System;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.CameraShake)]
    [System.Reflection.Obfuscation(Exclude = true)]
    [DisplayName("震屏")]
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class GOEActCameraShake : GOEActComponent
    {

        private bool mRealTime = true;
        private float mForce = 1024;
        private float mSpring = 256;
        private float mAttenuation = 8;
        private float mLifeTime = 1;
        private bool mSelfActive = false;

        internal bool RealTime
        {
            get { return mRealTime; }
            set { mRealTime = value; }
        }
        [DisplayName("是否只对主角有效")]
        public bool SelfActive
        {
            get { return mSelfActive; }
            set { mSelfActive = value; }
        }

        [DisplayName("力度")]
        public float Force
        {
            get { return mForce; }
            set { mForce = value; }
        }
        [DisplayName("反弹频率")]
        public float Spring
        {
            get { return mSpring; }
            set { mSpring = value; }
        }

        [DisplayName("衰减")]
        public float Attenuation
        {
            set { mAttenuation = value; }
            get { return mAttenuation; }
        }

        [DisplayName("生存时间")]
        public float LifeTime
        {
            set { mLifeTime = value; }
            get { return mLifeTime; }
        }

        internal override void OnTrigger()
        {
            base.OnTrigger();
            if (SelfActive)
            {
                if (GOERoot.SceneImp == null || this.Entity != GOERoot.SceneImp.Hero)
                {
                    this.Enable = false;
                    return;
                }
            }
            shakeComp = GOECameraShake.AddCameraShake();
            if (shakeComp != null)
            {
                shakeComp.Enable = false;
                //shakeComp.realTime = mRealTime;
                shakeComp.force = Force;
                shakeComp.spring = Spring;
                shakeComp.attenuation = Attenuation;
                shakeComp.ExistTime = LifeTime;
                shakeComp.Enable = true;
            }
            else
                UnityEngine.Debug.LogWarning("Shake component == null");
            this.Enable = false;
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();
            if (shakeComp != null)
                shakeComp.OnDestroy();
            shakeComp = null;
        }

        private GOECameraShake shakeComp;
        internal override void Pause()
        {
            base.Pause();
            if (shakeComp != null)
            {
                shakeComp.Enable = false;
            }
        }

        internal override void Restart()
        {
            base.Pause();
            if (shakeComp != null && !shakeComp.LifeOver)
            {
                shakeComp.Enable = true;
            }
        }

        internal override void GetResAsset(System.Collections.Generic.HashSet<string> setRes)
        {

        }

        internal override void PreLoad()
        {

        }

    }
}

