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
        class GOEGameScene : GOEScene, IGOEGameScene
    {
        LightFaceEffect lfe;
        public float GetHeight( float x, float z, float refY )
        {
            Vector3 ori = new Vector3( x, refY + 100f, z );
            Vector3 dir = new Vector3( 0, -1, 0 );
            Ray ray = new Ray( ori, dir );
            Vector3 pos = new Vector3();
            Vector3 normal = new Vector3();
            getIntersectTerrainPos(ray, ref pos, ref normal);

            if ( pos.Equals( Vector3.zero ) )
            {
                pos.y = refY;
            }
            return pos.y;
        }

        public float GetHeightByNavmesh(IDetourSender sender, Vector3 pos)
        {
            DetourMgr detour = this.GetComponent<DetourMgr>();
            return detour.GetHeight(sender, pos);
        }

        //public bool CheckPositonByHeightMap(ref Vector3 pos)
        //{
        //    float y = this.GetComponent<ClientHeightMap>().GetHeight(pos.x, pos.z);
        //    if (y == 0)
        //        return false;
        //    pos.y = y;
        //    return true;
        //}

        public bool CheckPositonByRayCast(ref Vector3 pos)
        {
            Vector3 normal = new Vector3();
            return CheckPositonByRayCast(ref pos, ref normal);
        }

        public bool CheckPositonByRayCast(ref Vector3 pos,ref Vector3 normal)
        {
            Vector3 ori = new Vector3(pos.x, pos.y + 2000f, pos.z);
            Vector3 dir = new Vector3(0, -1, 0);
            Ray ray = new Ray(ori, dir);
            return getIntersectTerrainPos(ray, ref pos, ref normal);
        }

        public bool GetIntersectTerrainPos(Vector3 screenPos, ref Vector3 terrainPos)
        {
            Ray ray = this.Camera.Camera.ScreenPointToRay(screenPos);
            Vector3 normal = new Vector3();
            return getIntersectTerrainPos(ray, ref terrainPos, ref normal);
        }

        internal int GetPath(IDetourSender actor, Vector3 start, Vector3 end, out List<Vector3> path)
        {
            DetourMgr detour = this.GetComponent<DetourMgr>();
            return detour.FindStraightPath(actor, start, end, out path);
        }

        public int GetPath(Vector3 start, Vector3 end, out List<Vector3> path, IDetourSender actor, float d = 0f)
        {
            if (!CheckPositonByRayCast(ref end))
            {
                Vector3 ori = new Vector3(end.x, end.y + 100f, end.z);
                Vector3 dir = new Vector3(0, -1, 0);
                Ray ray = new Ray(ori, dir);
                float dis = (start.y - ray.origin.y) / ray.direction.y;
                Vector3 pos = ray.origin + ray.direction * dis;
                getNearestPoint(pos, out end, actor);
            }

            if (GetPath(actor, start, end, out path) == 0)
            {
                if (path.Count > 0)
                    d -= (end - path[path.Count - 1]).magnitude;
                else
                    d = 0f;
                if (d > float.Epsilon)
                {
                    Vector3 dir;
                    int i = path.Count - 1;
                    Vector3 End = path[i];
                    for (; i > 0; i--)
                    {
                        End = path[i];
                        dir = End - path[i - 1];
                        float dis = dir.magnitude;
                        dir.Normalize();
                        if (dis > d)
                        {
                            End -= d * dir;
                            path[i] = End;
                            d = 0f;
                            break;
                        }
                        else
                        {
                            path.RemoveAt(i);
                            d -= dis;
                        }
                    }
                    if (d > float.Epsilon)
                    {
                        path.Clear();
                        dir = End - start;
                        if (dir.magnitude > d)
                        {
                            dir.Normalize();
                            End -= dir * d;
                            path.Add(end);
                        }
                        else
                            End = start;
                    }
                }
            }
            return 0;
        }


        private List<Vector3> dirs = new List<Vector3>();
        public void getNearestPoint(Vector3 end, out Vector3 pos, IDetourSender actor)
        {
            GOEActorEntity Actor = actor as GOEActorEntity;
            if (dirs.Count == 0)
            {
                for(int i = 0; i < 24; i++)
                {
                    double an = Math.PI * 15 * i / 180;
                    Vector3 dir = Vector3.zero;
                    dir.x = (float)Math.Cos(an) * 5;
                    dir.z = (float)Math.Sin(an) * 5;
                    dirs.Add(dir);
                }
            }
            pos = Vector3.zero;

            for (int i = 0; i < 24; i++)
            {
                Vector3 dir = dirs[i];
                //Vector3 start = new Vector3(end.x - dir.x, end.y, end.z - dir.z);
                Vector3 tem;
                GetRaycast(Actor, end - dir, end, out tem);
                if (Vector3.Distance(end, tem) < Vector3.Distance(pos, end))
                    pos = tem;
                if (Vector3.Distance(end, tem) <= 2)
                    break;
            }
        }

        public int GetRaycast(IDetourSender actor, Vector3 start, Vector3 end, out Vector3 cast)
        {
            DetourMgr detour = this.GetComponent<DetourMgr>();
            return detour.Raycast(actor, start, end, out cast);
        }

        public bool IsPositionWalkable(IDetourSender sender, Vector3 point)
        {
            DetourMgr detour = this.GetComponent<DetourMgr>();
            return detour.IsPositionWalkable(sender, point);
        }

        public bool IsDetourAStar()
        {
            DetourMgr detour = this.GetComponent<DetourMgr>();
            return detour.IsDetourAStar();
        }

        public void ClearTileData()
        {
            GOESceneRandomTile ranComp = this.GetComponent<GOESceneRandomTile>();
            ranComp.Clear();
        }
        public void AddTileData(TileData tile)
        {
            GOESceneRandomTile ranComp = this.GetComponent<GOESceneRandomTile>();
            ranComp.AddTile(tile);
        }

        public IEntity AddEffect(string name, Vector3 pos, Quaternion rot, float scale = 1, float time = -1 )
        {
            if (scale == 0)
                scale = 1;
            Entity entity = AddEntity<Entity>(name);
            entity.Position = pos;
            entity.Rotation = rot;
            entity.Scale = new Vector3(scale, scale, scale);
            if (time > 0)
                entity.AutoDestory(time);
            return entity;
        }

        public IEntity AddEntity(string name)
        {
            return AddEntity<Entity>(name);
        }

        public IGOEActorEntity AddActor(string name)
        {
            return AddEntity<GOEActorEntity>(name);
        }

        public IEntity AddEntity()
        {
            return AddEntity<Entity>();
        }

        public IGOEActorEntity AddActor()
        {
            return AddEntity<GOEActorEntity>();
        }

        public void AddEntity(IEntity obj)
        {
            (obj as Entity).Scene = this;
            GOESceneEntityContainer container = this.GetComponent<GOESceneEntityContainer>();
            container.AddEntity(obj as Entity);
        }

        public Entity Hero;
        public void SetHero(IEntity obj)
        {
            //Debug.Log("--------------call SetHero1");
            Hero = obj as Entity;
            Hero.OnLoadResource -= SetHeroFunc;
            Hero.OnLoadResource += SetHeroFunc;
            if (Hero.ResStatus == ResStatus.OK)
                SetHeroFunc();
        }

        public void RemoveHeroLoadListener()
        {
            Hero.OnLoadResource -= SetHeroFunc;
        }
        public void AddHeroLoadListener()
        {
            Hero.OnLoadResource -= SetHeroFunc;
            Hero.OnLoadResource += SetHeroFunc;
        }
        private void SetHeroFunc()
        {
            //Debug.Log("-------------------call SetHeroFunc2");
            LightFaceEffect lf = GameObject.FindObjectOfType<LightFaceEffect>();
            if (lf != null)
            {
                //Debug.Log("----------------set hero func lfe.lookattarget = " + Hero.GameObject.GetHashCode());
                lf.LookAtTarget = Hero.GameObject.transform;
            }
                
            MainLightFace mlf = GameObject.FindObjectOfType<MainLightFace>();
            if (mlf != null)
                mlf.target = Hero.GameObject.transform;
            GOERoot.GOECamera.LookAt(Hero);

            GOESceneCameraTransitionAreaMgr areaMgr = GetComponent<GOESceneCameraTransitionAreaMgr>() as GOESceneCameraTransitionAreaMgr;
            areaMgr.SetHero(Hero.GameObject.transform);

            TerrainBehaviour terrain = GameObject.FindObjectOfType<TerrainBehaviour>() as TerrainBehaviour;
            if (terrain != null)
            {
                //Debug.Log("------------set hero func terrainbehavior.mhro = " + Hero.GameObject.GetHashCode());
                terrain.Hero = Hero.GameObject.transform;
            }
            else
            {
                //Debug.Log("-------------set hero func terrainbehavior = null");
            }
        }

        internal T AddEntity<T>() where T : IEntity, new()
        {
            T obj = new T();
            AddEntity(obj);
            return obj;
        }

        internal T AddEntity<T>(string name) where T : IEntity, new()
        {
            T obj = AddEntity<T>();
            obj.Name = name;
            return obj;
        }
        // 		
        public IEntity GetEntityByID( string id )
        {
            GOESceneEntityContainer container = this.GetComponent<GOESceneEntityContainer>();
            return container.GetEntityByID( id );
        }

        // 		
        public void DelEntity(string id)
        {
            GOESceneEntityContainer container = this.GetComponent<GOESceneEntityContainer>();
            container.DelEntity( id );
        }

        public void DelEntity(IEntity obj)
        {
            GOESceneEntityContainer container = this.GetComponent<GOESceneEntityContainer>();
            container.DelEntity(obj as Entity);
        }

        public Vector3 GetBornPoint(int ind = 0)
        {
            if (this.Terrain != null)
            {
                SceneConfig config = Terrain.GetComponent<SceneConfig>();
                if (config != null)
                {
                    CheckPositonByRayCast(ref config.BornPoint);
                    return config.BornPoint;
                }
            }
            return Vector3.zero;
        }

        private AudioSource _backMusic;
        private string _sourceName;
        public void AddBackgroundMusic(string name)
        {
            if (_sourceName == name)
                return;
            _sourceName = name;
            _backMusic = GOERoot.GOEAudioMgr.AddSound(name, GOERoot.Context, true, true, AudioType.AUDIO_TYPE_BGM);
        }

        public void RemoveBackgroundMusic()
        {
            if(_backMusic != null)
            {
                GOERoot.GOEAudioMgr.RemoveSound(_backMusic);
                _backMusic = null;
                _sourceName = null;
            }
        }

        public void OpenFlash()
        {
        }

        public void CloseFlash()
        {
        }
        public void OpenRain()
        {
        }

        public void CloseRain()
        {
        }

        public void SetLight(float light)
        {
            LightFaceEffect lfe = GetLightFaceEffect();
            if (lfe != null)
            {
                //lfe.Light = light;
            }
        }

        public bool FlashEnabled {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public bool RainEnabled
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public float LightBrightness
        {
            get
            {
                 LightFaceEffect lfe = GetLightFaceEffect();
                 if (lfe != null)
                 {
                     return 0;// return lfe.mLight;
                 }
                 else
                     return 0;
            }
            set
            {
                SetLight(value);
            }
        }

        public float MoonLightScale
        {
            get
            {
                LightFaceEffect lfe = GetLightFaceEffect();
                if (lfe != null)
                {
                    return 1;// return lfe.MoonlightScale;
                }
                else
                    return 1;
            }
            set
            {
                LightFaceEffect lfe = GetLightFaceEffect();
                if (lfe != null)
                {
                    //lfe.MoonlightScale = value;
                }
            }
        }

        LightFaceEffect GetLightFaceEffect()
        {
            if (!lfe)
                lfe = GameObject.FindObjectOfType<LightFaceEffect>() as LightFaceEffect;

            return lfe;
        }
    }
}
