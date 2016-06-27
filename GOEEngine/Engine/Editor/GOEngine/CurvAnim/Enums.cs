using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GOEngine.Implement
{
    public enum GOECurveFindIndexResults
    {
        FailedNoKeyFrame,
        BeforeAllKeyFrame,
        AfterAllKeyFrame,
        ExactMatchFrame,
        Between2Frames,
    }
}