using System;

namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOECameraComponent : GOEBaseComponent
	{
        internal GOECamera GOECamera
		{
			get { return this.Owner as GOECamera;}
		}

        internal virtual void EnterScene()
		{
			
		}
	}
}

