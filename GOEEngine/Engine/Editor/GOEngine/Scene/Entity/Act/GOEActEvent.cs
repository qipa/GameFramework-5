using System;
using System.Collections.Generic;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.Event)]
    [DisplayName("触发事件")]
    [System.Reflection.Obfuscation(Exclude = true)]
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class GOEActEvent : GOEActComponent
    {
        static public Dictionary<string, string> arrayEvent = new Dictionary<string, string>() { { "ActEvent", "ActEvent" }, { "GeneralEvent", "GeneralEvent" }, { "ChainSkillEvent", "触发下一个连续技" }, { "Chat", "对话气泡" }, { "Pause", "暂停战斗" }, { "BossCamera", "切换到Boss视角" } };

        private string mEventName;
        private string mEventPara;

        [JsonField(JsonFieldTypes.ActEvent)]
        [DisplayName("事件名称")]
        public string EventName
        {
            set { mEventName = value; }
            get { return mEventName; }
        }
        [DisplayName("事件参数")]
        public string EventParam
        {
            set { mEventPara = value; }
            get { return mEventPara; }
        }
        internal override void OnTrigger()
        {
            base.OnTrigger();
            if (EngineDelegate.OnActEvent != null)
                EngineDelegate.OnActEvent(this.Entity, mEventName, mEventPara);
            this.Enable = false;
        }

        internal override void GetResAsset(System.Collections.Generic.HashSet<string> setRes)
        {

        }

        internal override void PreLoad()
        {

        }
    }
}

