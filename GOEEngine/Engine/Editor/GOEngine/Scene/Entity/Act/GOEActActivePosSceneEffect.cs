using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.ActivePosSceneEffect)]
    [System.Reflection.Obfuscation(Exclude = true)]
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class GOEActActivePosSceneEffect : GOEActComponent
    {

        private string mEffectName;				//特效名称//
        private Vector3 mOffSet;					//偏移//
        private Vector3 mRot;							//旋转//
        private bool mbOffsetFollow = true;		//默认跟随//
        private bool mbRotFollow = false;		//默认不跟随//

        public string EffectName
        {
            get { return mEffectName; }
            set { mEffectName = value; }
        }

        public Vector3 Offset
        {
            get { return mOffSet; }
            set { mOffSet = value; }
        }

        public bool OffsetFollow
        {
            set { mbOffsetFollow = value; }
            get { return mbOffsetFollow; }
        }

        public Vector3 Rot
        {
            get { return mRot; }
            set { mRot = value; }
        }

        public bool RotFollow
        {
            get { return mbRotFollow; }
            set { mbRotFollow = value; }
        }

        private Vector3 mPos = Vector3.one;
        private Vector3 mOffset = Vector3.zero;
        private Quaternion mRoattion = Quaternion.identity;


        internal override void Start()
        {
            base.Start();

            Vector3 pos = this.Entity.Position;
            pos.y = this.Entity.Scene.GetHeight(pos.x, pos.z, pos.y);

            Entity e = this.Entity;
            Vector3 v = Offset;
            if (OffsetFollow)
            {
                Matrix4x4 mat = Matrix4x4.TRS(Vector3.zero, e.Rotation, new Vector3(1, 1, 1));
                v = mat * v;
            }

            mPos = pos + v;

            mRoattion = this.RotFollow ? e.Rotation * Quaternion.Euler(Rot) : Quaternion.Euler(Rot);
        }

        internal override void OnTrigger()
        {
            base.OnTrigger();


            IEntity effect = this.Entity.Scene.AddEffect(this.EffectName, mPos, mRoattion);
            effect.Priority = LoadPriority.PostLoad;
            effect.Load();
            this.Enable = false;
        }

        internal override void GetResAsset(System.Collections.Generic.HashSet<string> setRes)
        {
            if (mEffectName != string.Empty)
            {
                setRes.Add(mEffectName);
            }
        }

        internal override void PreLoad()
        {
            GOERoot.ResMgrImp.GetAsset(mEffectName, null, LoadPriority.PostLoad);
        }

    }
}
