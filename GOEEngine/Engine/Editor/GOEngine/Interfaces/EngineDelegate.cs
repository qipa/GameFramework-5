using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOEngine
{
    public class EngineDelegate
    {
        public static Func<UnityEngine.GameObject, bool> PrefabUnInstantiateRule;
        public static Action<IEntity, string, string> OnActEvent;
        public static Action<string[]> OnUpdateResource;
        public static bool TestMobile = false;
        public static bool DynamicResource = false;
        public static float DefaultMoveSpeed = 8;
        public static float MaxSoundVolume = 0.8f;
        public static float MaxBGMVolume = 0.8f;
        public static float VolumeDuration = 1;
    }
}
