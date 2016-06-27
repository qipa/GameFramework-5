using System;

namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOEBaseComponent
	{
		protected bool mEnable = true;
		protected bool mRealtimeUpdate = false;
		protected bool mLifeOver = false;
        protected long mGUID = 0;
        protected System.Object mOwner = null;
        static long msGUID = 0;
        public static int mDebugCount = 0;
		public GOEBaseComponent()
		{
            mGUID = ++msGUID;
            mDebugCount++;
		}

        ~GOEBaseComponent()
        {
            --mDebugCount;
        }

        internal System.Object Owner
        {
            set { mOwner = value; }
            get { return mOwner; }
        }

        internal long GUID
        {
            get { return mGUID; }
        }

		internal bool Enable
		{
			set{ this.SetEnable(value);}
			get{ return mEnable; }
		}

        internal bool LifeOver
		{
			set { mLifeOver = value;}
			get { return mLifeOver; }
		}
		
		internal virtual void Update()
		{
			
		}

        internal virtual void Start()
        {

        }

        internal virtual void OnDestroy()
		{
			
		}
		
		private void SetEnable( bool bEnable )
		{
			if ( mEnable == bEnable )
			{
				return;
			}
			
			mEnable = bEnable;
			
			if ( mEnable )
			{
				this.OnEnabled();
			}
			else
			{
				this.OnDisabled();
			}
		}
		
		protected virtual void OnEnabled()
		{
			
		}
		
		protected virtual void OnDisabled()
		{
			
		}
	}
}

