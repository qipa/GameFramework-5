using System;

namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOESceneComponent : GOEBaseComponent
	{
		private object _fireScene = null;

        internal virtual void OnEnter()
        {

        }

        internal virtual void OnLeave()
        {

        }
             
		
	}
}

