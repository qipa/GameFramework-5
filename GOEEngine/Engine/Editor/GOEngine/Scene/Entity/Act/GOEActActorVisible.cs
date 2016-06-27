using System;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.ActorVisible)]
    [System.Reflection.Obfuscation(Exclude = true)]
    [DisplayName("Actor可见性")]        
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class GOEActActorVisible : GOEActComponent
    {


        private bool mVisible = false;

        public bool Visible
        {
            set { mVisible = value; }
            get { return mVisible; }
        }

        internal override void OnTrigger()
        {
            base.OnTrigger();
            EnableRenderer(Visible);
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();

            //Logger.GetFile( LogFile.EnterLeave ).LogError ( "actor visible des " + Visible.ToString() + " " + this.Entity.LogicID );
            EnableRenderer(true);
        }

        private void EnableRenderer(bool b)
        {
            if (b)
                Entity.Visible = b;

            if (null != Entity.GameObject && null != Entity.GameObject.GetComponent<UnityEngine.Renderer>())
            {
                Entity.GameObject.GetComponent<UnityEngine.Renderer>().enabled = b;
            }
            UnityEngine.Renderer[] rs = this.Entity.GetUnityComponentsInChildren<UnityEngine.Renderer>(true);
            if (null != rs)
            {
                foreach (UnityEngine.Renderer r in rs)
                {
                    r.enabled = b;
                }
            }
        }

        internal override void GetResAsset(System.Collections.Generic.HashSet<string> setRes)
        {

        }

        internal override void PreLoad()
        {

        }
    }
}

