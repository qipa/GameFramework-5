using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class GOEEntityFollowTransform : GOEEntityComponent
    {
        float curTime;
        internal float MaxTime { get; set; }
        internal Transform Target
        {
            get;
            set;
        }

        internal void Pause()
        {
            Enable = false;
        }

        internal void Restart()
        {
            Enable = true;
        }

        internal override void Update()
        {
            base.Update();
            curTime += Time.deltaTime;
            if (curTime > MaxTime)
            {
                LifeOver = true;
                return;
            }
            if (Target)
            {
                Entity.Position = Target.position;
                Entity.Rotation = Target.rotation;
            }
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();
            Enable = false;
            Target = null;
        }
    }
}
