using System;
using UnityEngine;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.SceneEffect)]
    [System.Reflection.Obfuscation(Exclude = true)]
    [DisplayName("场景特效")]
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class GOEActSceneEffect : GOEActComponent
    {

        private string mEffectName = string.Empty;				//特效名称//
        private Vector3 mOffSet;					//偏移//
        private Vector3 mRot;							//旋转//
        private bool mbOffsetFollow = true;		//默认跟随//
        private bool mbRotFollow = false;		//默认不跟随//
        float time;

        [DisplayName("特效名称")]
        public string EffectName
        {
            get { return mEffectName; }
            set { mEffectName = value; }
        }

        [DisplayName("偏移")]
        public Vector3 Offset
        {
            get { return mOffSet; }
            set { mOffSet = value; }
        }
        [DisplayName("最大播放时间")]
        public float MaxTime { get; set; }
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

        [DisplayName("是否在目标Actor播放")]
        public bool IsTargetPosition { get; set; }

        public GOEActSceneEffect()
        {
        }

        private Entity effect;
        internal override void OnTrigger()
        {
            base.OnTrigger();
            time = 0;
            Entity e = this.Entity;
            Vector3 v = Offset;
            if (OffsetFollow)
            {
                Matrix4x4 mat = Matrix4x4.TRS(Vector3.zero, e.Rotation, new Vector3(1, 1, 1));
                v = mat * v;
            }

            Vector3 pos = IsTargetPosition ? EntityAct.TargetPos : e.Position;

            pos.y = this.Entity.Scene.GetHeight(pos.x, pos.z, pos.y);

            float maxTime = MaxTime > 0 ? MaxTime : -1;
            effect = this.Entity.Scene.AddEffect(this.EffectName,
                pos + v,
                this.RotFollow ? e.Rotation * Quaternion.Euler(Rot) : Quaternion.Euler(Rot), 1, maxTime) as Entity;//m_Rotation);
            effect.Priority = LoadPriority.PostLoad;
            effect.Load();
        }
        internal override void Start()
        {
            base.Start();
            time = 0;
        }
        internal override void Pause()
        {
            base.Pause();
            if (effect != null)
            {
                effect.Pause();
            }

            Enable = false;
        }

        internal override void Restart()
        {
            base.Pause();
            if (effect != null)
            {
                effect.Restart();
            }

            Enable = true;
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();
            if (effect != null && MaxTime <= 0)
                this.Entity.Scene.DelEntity(effect);
            effect = null;
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

        internal override void Update()
        {
            base.Update();
            time += Time.deltaTime;
            if (time > MaxTime && MaxTime > 0)
            {
                LifeOver = true;
            }
        }
    }
}

