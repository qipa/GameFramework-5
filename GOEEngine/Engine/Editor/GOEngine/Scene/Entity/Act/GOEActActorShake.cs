using System;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.ActorShake)]
    [System.Reflection.Obfuscation(Exclude = true)]
    [DisplayName("角色震动")]
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class GOEActActorShake : GOEActComponent
    {

        private float mForce = 1024;
        private float mSpring = 256;
        private float mAttenuation = 8;
        private float mLifeTime = 1;
        float initialSpeed = 5;
        float gravity = 9;

        private GOEEntityShake shakeComp;
      
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

        [DisplayName("重力模式")]
        public bool GravityMode
        {
            get;
            set;
        }

        [DisplayName("初始速度")]
        public float InitialSpeed { get { return initialSpeed; } set { initialSpeed = value; } }

        [DisplayName("重力加速度")]
        public float Gravity { get { return gravity; } set { gravity = value; } }

        [DisplayName("生存时间")]
        public float LifeTime
        {
            set { mLifeTime = value; }
            get { return mLifeTime; }
        }

        internal override void OnTrigger()
        {
            base.OnTrigger();

            shakeComp = Entity.AddComponent<GOEEntityShake>();
            if (shakeComp != null)
            {
                //shakeComp.realTime = mRealTime;
                shakeComp.Force = Force;
                shakeComp.Spring = Spring;
                shakeComp.Attenuation = Attenuation;
                shakeComp.ExistTime = LifeTime;
                shakeComp.GravityMode = GravityMode;
                shakeComp.InitialSpeed = initialSpeed;
                shakeComp.Gravity = gravity;
                shakeComp.StartShake();
            }
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();
            if (shakeComp != null)
                shakeComp.OnDestroy();
            shakeComp = null;
        }

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

        internal override void PreLoad()
        {
            
        }

        internal override void GetResAsset(System.Collections.Generic.HashSet<string> setRes)
        {
            
        }
    }
}

