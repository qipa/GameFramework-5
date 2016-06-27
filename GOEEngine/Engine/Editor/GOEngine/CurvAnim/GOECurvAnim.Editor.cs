#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GOEngine.Implement
{
    public partial class GOECurvAnim : GOEBaseComponent
    {
        public void DisposeEditorGameObject()
        {
            foreach (GOECurvController i in mControllers)
                i.DisposeEditorGameObject();            
        }
    }
}
#endif