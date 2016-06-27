using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.AttachVisible)]
    [System.Reflection.Obfuscation(Exclude = true)]
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOEActAttachVisible : GOEActComponent
    {
        private string attName = string.Empty;
        private bool mVisible = false;

        public string AttachName
        {
            set { attName = value; }
            get { return attName; }
        }

        public bool Visible
        {
            set { mVisible = value; }
            get { return mVisible; }
        }

        internal override void OnTrigger()
        {
            base.OnTrigger();
            Entity att = this.Entity.GetAttchByName(attName) as Entity;
            if (att != null)
                att.Visible = Visible;
            Enable = false;
        }

        internal override void GetResAsset(HashSet<string> setRes)
        {
            
        }

        internal override void PreLoad()
        {
            
        }
    }
}
