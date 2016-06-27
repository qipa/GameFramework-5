#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace GOEngine.Implement
{
    public partial class GOECurvKeyFrame
    {
        const string EditorGameObjectPrefab = "Assets/GOEditor/Editor/CurvAnim/GOECurvAnimGO.prefab";
        static GameObject editorGameObjectPrefabGO;
        GOECurvKeyFrameGO kfObj;
        
        [JsonFieldAttribute(JsonFieldTypes.UnEditable)]
        public GameObject EditorGameObject { get; set; }

        public void EnsureEditorGO(GameObject curvGO)
        {
            if (!EditorGameObject)
            {
                if (!editorGameObjectPrefabGO)
                {
                    editorGameObjectPrefabGO = AssetDatabase.LoadAssetAtPath(EditorGameObjectPrefab, typeof(GameObject)) as GameObject;
                }
                EditorGameObject = GameObject.Instantiate(editorGameObjectPrefabGO) as GameObject;
                kfObj = EditorGameObject.GetComponent<GOECurvKeyFrameGO>();
                kfObj.Keyframe = this;
            }

            EditorGameObject.transform.parent = curvGO.transform;
            EditorGameObject.transform.localPosition = Position;
            EditorGameObject.transform.localRotation = Rotation;
            EditorGameObject.transform.localScale = Scale;
        }

        public void DisposeEditorGO()
        {
            if (EditorGameObject)
            {
                GameObject.Destroy(EditorGameObject);
                EditorGameObject = null;
                kfObj = null;
            }
        }
    }
}
#endif