using System;
using UnityEngine;

namespace GOEngine.Implement
{
    public class Realtime : IRealtime
    {
		private float realTime = 0;
        private float deltaTime = 0;

        internal void Start()
        {
            realTime = UnityEngine.Time.realtimeSinceStartup;
        }

        public float DeltaTime
		{
            get { return deltaTime; }
		}

        public float RealTime
        {
            get { return realTime; }
        }

        public long RealTimeMill
        {
            get { return Convert.ToInt64(realTime * 1000); }
        }
		
        internal void Update()
        {
            deltaTime = UnityEngine.Time.realtimeSinceStartup - realTime;
            realTime = UnityEngine.Time.realtimeSinceStartup;
        }
    }
}