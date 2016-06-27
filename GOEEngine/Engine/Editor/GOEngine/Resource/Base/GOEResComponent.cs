﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class ResComponent : GOEBaseComponent
	{
        internal ResourceMgr ResourceMgr
        {
            get { return Owner as ResourceMgr; }
        }

        internal virtual void OnLeaveScene()
        {

        }
	}
}
