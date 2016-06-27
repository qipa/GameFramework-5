using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.AttachTarget)]
    [DisplayName("绑定目标到绑点")]
    [System.Reflection.Obfuscation(Exclude = true)]
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOEActAttachTarget : GOEActComponent
    {
        private string mAttachNode = string.Empty;

        GOEEntityFollowTransform follow;
        
        [DisplayName("绑点")]
        [JsonFieldAttribute(JsonFieldTypes.BindPoint)]
        public string AttachNode
        {
            set { mAttachNode = value; }
            get { return mAttachNode; }
        }

        [DisplayName("绑定时间")]
        public float MaxTime
        {
            get;
            set;
        }

        internal override void OnTrigger()
        {
            base.OnTrigger();
            UnityEngine.Transform attachNode = Entity.GetBindPoint(mAttachNode);
            List<Entity> listTarget = this.EntityAct.TargetList;
            if (attachNode != null && listTarget.Count > 0)
            {
                follow = listTarget[0].AddComponent<GOEEntityFollowTransform>();
                follow.MaxTime = MaxTime;
                follow.Target = attachNode;
            }
        }

        internal override void GetResAsset(HashSet<string> setRes)
        {
            
        }

        internal override void PreLoad()
        {
            
        }

        internal override void Pause()
        {
            base.Pause();
            if (follow != null)
                follow.Pause();
        }

        internal override void Restart()
        {
            base.Restart();
            if (follow != null)
                follow.Restart();
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();
            if (Entity != null && follow != null)
            {
                Entity.DelComponent(follow);
                follow.OnDestroy();
                follow = null;
            }
        }
    }
}
