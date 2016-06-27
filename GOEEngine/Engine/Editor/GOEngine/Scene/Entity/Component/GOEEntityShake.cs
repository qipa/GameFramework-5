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
 class GOEEntityShake : GOEEntityComponent
    {
        float force = 60;
        float spring = 250;
        float attenuation = 30;
        float timescale = 1;

        public float Force { get { return force; } set { force = value; } }

        public float Spring { get { return spring; } set { spring = value; } }

        public float Attenuation { get { return attenuation; } set { attenuation = value; } }

        public float Timescale { get { return timescale; } set { timescale = value; } }

        public bool GravityMode { get; set; }

        public float InitialSpeed { get; set; }

        public float Gravity { get; set; }

        private float pull;
        private float velociy;
        private float accel;

        private float existTime = 1;
        private float time;

        private const float fMaxDT = 1.0f / 33;

        public float ExistTime { get { return existTime; } set { existTime = value; } }
       
        private Vector3 mStartPos = Vector3.zero;
        private float mLastShiftY = 0f;
        
        internal Transform Target
        {
            get;
            set;
        }

        internal void Pause()
        {
            Enable = false;
        }

        internal void Restart()
        {
            Enable = true;
        }
        public void StartShake()
        {
            pull = velociy = accel = 0;
            time = 0;
            accel = force;

            mStartPos = Entity.Position;
            mLastShiftY = 0f;
        }
        internal override void Update()
        {
            if (time > existTime)
            {
                LifeOver = true;
                return;
            }


            //float dt = App.Clock.DeltaTimeSec*timescale;
            float dt = Time.deltaTime * timescale;
            if (dt > fMaxDT) dt = fMaxDT;

            time += dt;

            if (GravityMode)
                DoGravityMode(time);
            else
            {
               

                velociy += accel * dt;
                pull += velociy * dt;
                accel = -pull * spring;

                float resistance = 1 - (dt * attenuation);
                if (resistance < 0) resistance = 0;
                velociy *= resistance;

                // affect camera
                Vector3 pos = Entity.Position;
                pos.y -= mLastShiftY;

                float shiftY = pull;
                pos.y += shiftY;
                mLastShiftY = shiftY;
                Entity.Position = pos;
            }
        }

        void DoGravityMode(float time)
        {
            float s = InitialSpeed * time + -0.5f * Gravity * time * time;
            if (s < 0)
                s = 0;
            Vector3 pos = Entity.Position;
            pos.y = mStartPos.y + s;
            Entity.Position = pos;
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();
            LifeOver = true;
            Enable = false;
            Vector3 pos = Entity.Position;
            pos.y = mStartPos.y;
            Entity.Position = pos;
        }
    }
}
