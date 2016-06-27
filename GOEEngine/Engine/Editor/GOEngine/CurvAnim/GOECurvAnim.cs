using System;
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
        partial class GOECurvAnim : GOEBaseComponent
    {
        public const int FramePerSecond = 60;

        int frameCnt = 300;
        List<GOECurvController> mControllers = new List<GOECurvController>();
        public bool RestrictInScene { get; set; }
        public string SceneName { get; set; }

        public int FrameCount { get { return frameCnt; } set { frameCnt = value; } }
        [JsonFieldAttribute(JsonFieldTypes.HasChildren)]
        public List<GOECurvController> Controllers { get { return mControllers; } }

        public void AddController()
        {
            mControllers.Add(new GOECurvController());
        }

        public void RemoveController(int idx)
        {
            mControllers.RemoveAt(idx);
        }
    }
}
