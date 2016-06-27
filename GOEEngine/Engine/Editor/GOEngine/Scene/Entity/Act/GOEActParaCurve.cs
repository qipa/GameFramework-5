using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.ParaCurve)]
    [System.Reflection.Obfuscation(Exclude = true)]
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOEActParaCurve : GOEActComponent
	{
		
		private string mEffectName = "gui_huoqujineng_guiji.prefab";					//特效名称//
		private float mFlyTime = 1;
		private int mParaCurveNum = 1;
		private int mTheta = 0;
		
		private Vector3 mOri = Vector3.zero;
		private Vector3 mPos = Vector3.zero;

        private List<Entity> mParaCurveList = new List<Entity>();
		
		public string EffectName
		{
			get { return mEffectName;}
			set { mEffectName = value;}
		}
		
		public float FlyTime
		{
			get { return mFlyTime; }
			set { mFlyTime = value; }
		}
		
		public Vector3 Ori
		{
			set { mOri = value;}
			get { return mOri;}
		}
		
		public Vector3 Pos
		{
			set { mPos = value; }
			get { return mPos; }
		}
		
		
		public GOEActParaCurve ()
		{

		}
		
		private Vector2 vxz = Vector2.zero;
		private float v0y = 0;
		private float ay = 0;
		private Vector3 mStartPos = Vector3.zero;

        internal override void OnTrigger()
		{
			base.OnTrigger ();
		
			for ( int i = 0; i < mParaCurveNum; ++i )
			{
                Entity soul = this.Entity.Scene.AddEntity(mEffectName) as Entity;
				soul.Name = mEffectName;
				soul.Position = this.Entity.Position + this.Entity.Bound.size.y * Vector3.up;
				mStartPos = soul.Position;
				
				Vector3 dis = mPos - soul.Position;
				mOri = dis.normalized;
				Vector2 disXZ = Vector2.zero;
				disXZ.x = dis.x;
				disXZ.y = dis.z;
				
				vxz = disXZ / FlyTime;
				float rateY = Mathf.Tan ( (float)mTheta / 180 * Mathf.PI );
				v0y = vxz.magnitude;
				v0y *= rateY;
				
				float disY= dis.y;
				ay = 2 * (disY - v0y* FlyTime ) / (FlyTime * FlyTime );
				
				soul.Load();
				
				mParaCurveList.Add(soul);
			}
			
			
		}

        internal override void Update()
		{
			base.Update ();
			
			float curTime = LocalTime - TriggerTime;
			if ( curTime < FlyTime )
			{
				for ( int i = 0; i< mParaCurveList.Count; ++i )
				{
                    Entity soul = mParaCurveList[i];
					
					Vector2 moveDis = vxz * curTime;
					float moveY = ay * curTime * curTime / 2 + v0y * curTime;
					
					soul.Position = mStartPos + new Vector3 ( moveDis.x, moveY, moveDis.y );
					
				}
			}
			else
			{
				for ( int i = 0; i< mParaCurveList.Count; ++i )
				{
                    Entity soul = mParaCurveList[i];
					
					soul.Destroy();
					
				}
				mParaCurveList.Clear();
			}			
		}

        internal override void OnDestroy()
        {
            base.OnDestroy();

            for ( int i = 0; i < mParaCurveList.Count; ++i )
            {
                Entity soul = mParaCurveList[i];

                soul.Destroy();

            }
            mParaCurveList.Clear();
        }

        internal override void GetResAsset(HashSet<string> setRes)
        {
            
        }

        internal override void PreLoad()
        {
            
        }

	}
}




