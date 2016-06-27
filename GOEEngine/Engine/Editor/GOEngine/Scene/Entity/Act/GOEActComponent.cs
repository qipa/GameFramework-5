using System;
using System.Collections.Generic;

namespace GOEngine.Implement
{
    [System.Reflection.Obfuscation(Exclude = true)]
#if UNITY_EDITOR
    public
#else
    internal
#endif
 abstract class GOEActComponent : GOEBaseComponent
    {
        private float mfTriggerTimeLocal = 0.0f;
        private float mLocalTime = 0.0f;
        protected GOEEntityAct mEntityAct = null;
        public bool IsIgored = false;
        public static bool IsEditorMode = false;

        [DisplayName("触发时间")]
        public float TriggerTime
        {
            set { mfTriggerTimeLocal = value; }
            get { return mfTriggerTimeLocal; }
        }

        [DisplayName("是否仅在编辑器中使用")]
        public bool EditorOnly { get; set; }

        internal float LocalTime
        {
            set { mLocalTime = value; }
            get { return mLocalTime; }
        }

        internal GOEEntityAct EntityAct
        {
            set { mEntityAct = value; }
            get { return mEntityAct; }
        }

        public Entity Entity
        {
            get { return mEntityAct.Entity; }
        }

        internal bool hasTriggered = false;
        internal override void Start()
        {
            base.Start();
            Enable = false;
            hasTriggered = false;
            IsIgored = false;
            LifeOver = false;
        }

        internal void CheckTrigger()
        {
            if (IsIgored || LifeOver || (EditorOnly && !IsEditorMode))
                return;
            if (LocalTime >= TriggerTime)
            {
                if (!hasTriggered)
                {
                    Enable = true;
                    hasTriggered = true;
                    this.OnTrigger();
                }
            }
        }

        internal virtual void OnTrigger()
        {

        }

        internal virtual void Pause()
        {

        }

        internal virtual void Restart()
        {

        }

        internal abstract void PreLoad();

        internal virtual void GetRes(HashSet<string> setRes)
        {
            HashSet<string> tempRes = new HashSet<string>();

            this.GetResAsset(tempRes);

            foreach (string res in tempRes)
            {
                string bundlename = GOERoot.ResMgrImp.GetBundleName(res);
                if (bundlename == string.Empty)
                {
                    Logger.GetFile(LogFile.Res).LogWarning(res + " not find bundlename");
                }
                else
                {
                    setRes.Add(bundlename);
                }
            }
        }

        internal abstract void GetResAsset(HashSet<string> setRes);

        internal override void OnDestroy()
        {
            base.OnDestroy();
            Enable = false;
            LocalTime = 0f;
            hasTriggered = false;
        }
    }
}

