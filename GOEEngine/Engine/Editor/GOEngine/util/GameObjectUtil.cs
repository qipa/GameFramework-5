using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using GOEngine.Implement;

namespace GOEngine
{
    public class GameObjectUtil
	{
        public static GameObject GetGameObjectByName(GameObject obj, string name)
        {
            Transform[] tms = obj.GetComponentsInChildren<Transform>(true);
            foreach (Transform tm in tms)
            {
                if (tm.gameObject.name == name)
                {
                    return tm.gameObject;
                }
            }

            return null;
        }

        static public void SetLayer(GameObject go, int layer, bool enforceSet = false)
        {
            go.layer = layer;
            Transform[] trans = go.GetComponentsInChildren<Transform>(true);
            foreach (Transform tran in trans)
            {
                impGameObjectlayer(tran.gameObject, layer, enforceSet);
            }
        }

        private static void impGameObjectlayer(GameObject obj, int layer, bool force)
        {
            if (!force)
            {
                int oldLayer = obj.layer;
                if (oldLayer == LayerDef.LIGHT_FACE)
                    return;
                if (oldLayer == LayerDef.Effect)
                    return;
            }
            obj.layer = layer;
        }

        public static Transform GetBindPoint(GameObject obj, string name)
        {
            if (name == "self")
            {
                return obj.transform;
            }

            Transform[] bones = obj.GetComponentsInChildren<Transform>(true);
            if (bones == null)
            {
                return null;
            }
            Transform tempBone = null;

           foreach (Transform bone in bones)
            {
                if (bone.name == name)
                {
                    tempBone = bone;
                    break;
                }
            }
            return tempBone;
        }

        static public Bounds GetGameObjectBounds(GameObject obj)
        {
            Bounds bd = new Bounds(Vector3.zero, Vector3.zero);
            foreach (Renderer r in obj.GetComponentsInChildren<Renderer>(true))
            {
                if (r == null)
                {
                    continue;
                }
                if (!(r is MeshRenderer) && !(r is SkinnedMeshRenderer))
                {
                    continue;
                }

                Bounds rbd = r.bounds;
                if (MathUtil.IsBoundsInvalide(bd))
                {
                    bd = rbd;
                }
                else
                {
                    bd.Encapsulate(rbd);
                }
            }

            bd.center = bd.center - obj.transform.position;

            return bd;
        }
	}
}
