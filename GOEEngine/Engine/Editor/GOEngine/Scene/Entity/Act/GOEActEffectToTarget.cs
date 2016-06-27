using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.EffectToTarget)]
    [System.Reflection.Obfuscation(Exclude = true)]
    [DisplayName("弹道（飞向目标角色）")]
        
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOEActEffectToTarget : GOEActComponent
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

        [DisplayName("目标绑点")]
        [JsonFieldAttribute(JsonFieldTypes.BindPoint)]
        public string TargetAttachNode
        {
            set { mTargetAttachNode = value; }
            get { return mTargetAttachNode; }
        }

        [DisplayName("飞行时间")]
        public float Time
        {
            set { mTime = value; }
            get { return mTime; }
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

        [DisplayName("是否按照指定速度飞行")]
        public bool FlyBySpeed
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
        private string mTargetAttachNode = string.Empty;
        private float mTime = 0.3f;
        private float mDDelay = 0;
        private float mSpeed = 0;
        private List<GOEEntityEffectMove> effects = new List<GOEEntityEffectMove>();
        
        internal override void OnTrigger()
        {
            base.OnTrigger();

            List<Entity> listTarget = this.EntityAct.TargetList;
            foreach ( Entity ent in listTarget )
            {
                if (ent.ResStatus != ResStatus.OK)
                    return;
                GOEEntityEffectMove effect = this.Entity.AddComponent<GOEEntityEffectMove>();
                effect.StartPos = Entity.GetBindPoint(mSenderAttachNode).position;
                if (ent != null)
                {
                    effect.EndPos = ent.GetBindPoint(mTargetAttachNode).position;
                }
                if (FlyBySpeed)
                {
                    effect.MaxTime = (ent.Position - Entity.Position).magnitude / mSpeed;            
                }
                else
                {
                    effect.MaxTime = mTime;
                }
                effect.HeightRatio = HeightRatio;
                effect.DestructionDelay = mDDelay;
                effect.EffectName = mEffectName;
                effect.StartMove(EntityAct.ActSpeed);
                effects.Add(effect);
            }
        }
        internal override void OnDestroy()
        {
            base.OnDestroy();
            for (int i = 0; i < effects.Count; i++)
            {
                GOEEntityEffectMove effect = effects[i];
                effect.OnDestroy();
            }
            effects.Clear();
        }

        internal override void Pause()
        {
            base.Pause();
            for (int i = 0; i < effects.Count; i++)
            {
                GOEEntityEffectMove effect = effects[i];
                effect.Pause();
            }
        }

        internal override void Restart()
        {
            base.Restart();
            for (int i = 0; i < effects.Count; i++)
            {
                GOEEntityEffectMove effect = effects[i];
                effect.Restart();
            }
        }

        internal override void GetResAsset(HashSet<string> setRes)
        {
            if (mEffectName != string.Empty)
            {
                setRes.Add(mEffectName);
            }
        }

        internal override void PreLoad()
        {
            GOERoot.ResMgrImp.GetAsset( mEffectName, null );
        }
    }
}
