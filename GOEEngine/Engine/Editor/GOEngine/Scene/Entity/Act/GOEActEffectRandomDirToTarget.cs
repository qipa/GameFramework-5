using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GOEngine.Implement
{
    [ActTypeName(ActType.EffectRandomToTarget)]
    [System.Reflection.Obfuscation(Exclude = true)]
#if UNITY_EDITOR
    public
#else
    internal
#endif
        class GOEActEffectRandomDirToTarget : GOEActComponent
	{
        public string EffectName
        {
            set { mEffectName = value; }
            get { return mEffectName; }
        }

        [JsonFieldAttribute(JsonFieldTypes.BindPoint)]
        public string TargetAttachNode
        {
            set { mTargetAttachNode = value; }
            get { return mTargetAttachNode; }
        }

        public float Time
        {
            set { mTime = value; }
            get { return mTime; }
        }

        private string mEffectName = string.Empty;
        private string mTargetAttachNode = "t_chesthit";
        private float mTime = 0.15f;
        private float mDistance = 5.0f;
        private List<GOEEntityEffectMove> effects = new List<GOEEntityEffectMove>();

        internal override void OnTrigger()
        {
            base.OnTrigger();

            List<Entity> listTarget = this.EntityAct.TargetList;
            foreach (Entity obj in listTarget)
            {
                if (obj != null && obj.ResStatus == ResStatus.OK)
                {

                    Transform tm = obj.GetBindPoint(mTargetAttachNode);
                    if (tm == null)
                    {
                        continue;
                    }

                    int x = UnityEngine.Random.Range(-100, 100);
                    int z = UnityEngine.Random.Range(-100, 100);
                    if (x == 0 && z == 0)
                    {
                        x = 1;
                    }
                    Vector3 ori = new Vector3(x, 0, z);
                    ori = ori.normalized;

                    Vector3 startPos = tm.position + ori * mDistance;

                    GOEEntityEffectMove move = obj.AddComponent<GOEEntityEffectMove>();
                    move.EffectName = mEffectName;
                    move.MaxTime = mTime;
                    move.StartPos = startPos;
                    move.EndPos = tm.position;

                    move.StartMove(EntityAct.ActSpeed);

                    effects.Add(move);
                }
            }
        }
        internal override void OnDestroy()
        {
            base.OnDestroy();
            for (int i = 0; i < effects.Count; i++)
            {
                GOEEntityEffectMove effect = effects[i];
                effect.OnDestroy();
            }
            effects.Clear();
        }

        internal override void Pause()
        {
            base.Pause();
            for (int i = 0; i < effects.Count; i++)
            {
                GOEEntityEffectMove effect = effects[i];
                effect.Pause();
            }
        }

        internal override void Restart()
        {
            base.Restart();
            for (int i = 0; i < effects.Count; i++)
            {
                GOEEntityEffectMove effect = effects[i];
                effect.Restart();
            }
        }
        internal override void GetResAsset(HashSet<string> setRes)
        {
            if ( mEffectName != string.Empty)
            {
                setRes.Add( mEffectName );
            }
        }

        internal override void PreLoad()
        {
            GOERoot.ResMgrImp.GetAsset( mEffectName, null );
        }
	}
}
