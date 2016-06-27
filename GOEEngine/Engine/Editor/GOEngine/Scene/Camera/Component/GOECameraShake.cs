using System;
using UnityEngine;

namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class GOECameraShake : GOECameraComponent
    {
        //public bool realTime = true;
        public float force = 60;
        public float spring = 250;
        public float attenuation = 30;
        public float timescale = 1;
        float lastRemainingTime = 0;

        private float pull;
        private float velociy;
        private float accel;

        private float existTime = 1;
        private float time;

        private const float FrameRate = 1.0f / 60;

        public float ExistTime { get { return existTime; } set { existTime = value; } }
        private float GetPull() { return pull; }

        private Vector3 mStartCamPos = Vector3.zero;
        private float mLastShiftY = 0f;
        float offset;
        private Vector3 mMyLastPos = Vector3.zero;

        public static GOECameraShake AddCameraShake()
        {
            var shake = (GOERoot.GOECamera as GOECamera).GetComponent<GOECameraShake>();
            if (shake == null)
                return ComponentObject.Add<GOECameraShake>(GOERoot.GOECamera as GOECamera);
            return shake;
        }

        protected override void OnEnabled()
        {
            offset = 0;
            pull = velociy = accel = 0;
            time = 0;
            lastRemainingTime = 0;
            accel = force;

            mStartCamPos = GOECamera.Position;
            mMyLastPos = GOECamera.Position;
            mLastShiftY = 0f;
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            time = 0;
            lastRemainingTime = 0;
            offset = 0;
            GOERoot.GOECamera.YOffset = 0;
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();
            Enable = false;
            LifeOver = true;
        }

        internal override void Update()
        {
            if (time > existTime)
            {
                OnDestroy();
                return;
            }

            //float dt = App.Clock.DeltaTimeSec*timescale;
            float dt = Time.deltaTime * timescale;
            lastRemainingTime += dt;
            while (lastRemainingTime >= FrameRate)
            {
                UpdateOnce();
                lastRemainingTime -= FrameRate;
            }
        }

        void UpdateOnce()
        {
            time += FrameRate;

            velociy += accel * FrameRate;
            pull += velociy * FrameRate;
            accel = -pull * spring;

            float resistance = 1 - (FrameRate * attenuation);
            if (resistance < 0) resistance = 0;
            velociy *= resistance;

            // affect camera
            offset -= mLastShiftY;

            float shiftY = GetPull();
            offset += shiftY;
            mLastShiftY = shiftY;
            GOECamera.YOffset = offset;
        }
    }
}

