using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.EffectToPos)]
    [System.Reflection.Obfuscation(Exclude = true)]
    [DisplayName("弹道（飞向目标点）")]
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOEActEffectToPos : GOEActComponent
    {

        [DisplayName("特效名")]
        public string EffectName
        {
            set { mEffectName = value; }
            get { return mEffectName; }
        }

        [DisplayName("发出方绑点")]
        [JsonFieldAttribute(JsonFieldTypes.BindPoint)]
        public string SenderAttachNode
        {
            set { mSenderAttachNode = value; }
            get { return mSenderAttachNode; }
        }

        [DisplayName("飞行时间")]
        public float Time
        {
            set { mTime = value; }
            get { return mTime; }
        }

        [DisplayName("是否按照指定速度飞行")]
        public bool FlyBySpeed
        {
            get;
            set;
        }

        [DisplayName("飞行速度")]
        public float Speed
        {
            get { return mSpeed; }
            set { mSpeed = value; }
        }

        [DisplayName("销毁延迟")]
        public float DestructionDelay
        {
            get { return mDDelay; }
            set { mDDelay = value; }
        }

        [DisplayName("平飞")]
        public bool KeepHeight
        {
            get;
            set;
        }
        [DisplayName("高度系数")]
        public float HeightRatio
        {
            get;
            set;
        }
        private string mEffectName = string.Empty;
        private string mSenderAttachNode = string.Empty;
        private float mSpeed = 0;
        private float mTime = 0.3f;
        private float mDDelay = 0;

        GOEEntityEffectMove effect;
        internal override void OnTrigger()
        {
            base.OnTrigger();

            effect = this.Entity.AddComponent<GOEEntityEffectMove>();
            effect.StartPos = Entity.GetBindPoint(mSenderAttachNode).position;
            UnityEngine.Vector3 pos = this.EntityAct.TargetPos;
            if (KeepHeight)
                pos.y = effect.StartPos.y;
            effect.EndPos = pos;
            if (FlyBySpeed)
            {
                effect.MaxTime = (effect.EndPos - effect.StartPos).magnitude / mSpeed;
            }
            else
            {
                effect.MaxTime = mTime;
            }
            effect.HeightRatio = HeightRatio;
            effect.DestructionDelay = mDDelay;
            effect.EffectName = mEffectName;
            effect.StartMove(EntityAct.ActSpeed);
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();
            if(effect != null)
            {
                effect.OnDestroy();
                effect = null;
            }
        }
        internal override void Pause()
        {
            base.Pause();
            if (effect != null)
            {
                effect.Pause();
            }
        }

        internal override void Restart()
        {
            base.Restart();
            if (effect != null)
            {
                effect.Restart();
            }
        }

        internal override void GetResAsset(HashSet<string> setRes)
        {
            if ( mEffectName != string.Empty )
            {
                setRes.Add( mEffectName );
            }
        }

        internal override void PreLoad()
        {
            GOERoot.ResMgrImp.GetAsset( mEffectName, null );
        }

    }
}
