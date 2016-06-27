using System;
using UnityEngine;
using System.Collections.Generic;
namespace GOEngine.Implement
{
#if UNITY_EDITOR
    public
#else
    internal
#endif
 class GOEEntityMoveTo : GOEEntityComponent
    {
        protected bool isMoving = false;
        protected float mTime = 0.0f;
        bool mMoveByTime = false;
        bool isPathfinding = false;
        internal GOEActorEntity Actor
        {
            get { return Entity as GOEActorEntity; }
        }

        public GOEEntityMoveTo()
        {
            mEnable = false;
        }


        ~GOEEntityMoveTo()
        {
        }

        internal bool IsMoving
        {
            get { return isMoving; }
        }

        private bool _paused = false;
        public void PauseMove()
        {
            _paused = true;
        }

        public void RestartMove()
        {
            _paused = false;
        }

        private bool EqualsPos(Vector3 start, Vector3 end)
        {
            return Vector3.Distance(start, end) < 0.01;
        }

        private float _lastDegree = -1000;
        private const float LIMITMOVE = 10;
        private bool _atEdge = false;
        private Matrix4x4 _matDir = new Matrix4x4();
        private Vector3 _rockDir = new Vector3();
        private bool _lastBarrier;
        private float _lastMoveTime = 0.0f;

        private List<bool> _leftHistory = new List<bool>(new bool[5] { false, false, false, false, false });
        internal void MoveDir(float degree, float shift = 0)
        {
            if (!_lastBarrier
                && Math.Abs(_lastDegree - degree) < LIMITMOVE
                && shift == 0)
            {
                if (_path.Count > 0
                    && Vector3.Distance(_path[_path.Count - 1], Entity.Position) < 0.1f)
                {
                }else
                {
                    return;
                }
            }

            _lastMoveTime = GOERoot.RealTime.RealTime;
            _lastBarrier = false;
            _matDir.SetTRS(Vector3.zero, Quaternion.Euler(0, degree + shift, 0), Vector3.one);
            _rockDir = _matDir.MultiplyVector(GOERoot.GOECamera.Camera.transform.forward);
            _rockDir.y = 0;
            _rockDir.Normalize();

            bool canmove = true;
            Vector3 cast;
            if (!Actor.ClientDetour)
            {
                cast = Entity.Position + _rockDir * 1;
            }
            else
            {
                Vector3 end = Entity.Position + _rockDir * 1000;
                canmove = Entity.Scene.GetRaycast(Actor, Actor.Position, end, out cast) == DetourState.DT_SUCCESS;
                //Debug.DrawLine(Actor.Position, end, Color.red);

                if (canmove)
                {
                    Entity.Scene.CheckPositonByRayCast(ref cast);
                    if (EqualsPos(cast, Entity.Position))
                    {
                        _lastBarrier = true;
                        _matDir.SetTRS(Vector3.zero, Quaternion.Euler(0, degree - 15, 0), Vector3.one);
                        Vector3 leftdir = _matDir.MultiplyVector(GOERoot.GOECamera.Camera.transform.forward);
                        leftdir.y = 0;
                        leftdir.Normalize();

                        _matDir.SetTRS(Vector3.zero, Quaternion.Euler(0, degree + 15, 0), Vector3.one);
                        Vector3 rightdir = _matDir.MultiplyVector(GOERoot.GOECamera.Camera.transform.forward);
                        rightdir.y = 0;
                        rightdir.Normalize();

                        Vector3 tempstart = this.Entity.Position - _rockDir * 0.3f;
                        //Entity.Scene.GetRaycast(Actor, cast, tempstart, out tempstart);

                        Vector3 templeftend = tempstart + leftdir * 1000;
                        Vector3 temprightend = tempstart + rightdir * 1000;
                        Vector3 tempcast1 = Vector3.zero;
                        Vector3 tempcast2 = Vector3.zero;
                        float dis1 = float.MaxValue;
                        float dis2 = float.MaxValue;
                        bool leftcanmove = Entity.Scene.GetRaycast(Actor, tempstart, templeftend, out tempcast1) == DetourState.DT_SUCCESS;
                        //Debug.DrawLine(tempstart, templeftend, Color.blue);
                        if (!EqualsPos(tempstart, tempcast1))
                        {
                            dis1 = (end - tempcast1).magnitude;
                        }
						bool rightCanmove = Entity.Scene.GetRaycast(Actor, tempstart, temprightend, out tempcast2) == DetourState.DT_SUCCESS;
                        //Debug.DrawLine(tempstart, temprightend, Color.green);
                        bool chooseLeft = false;
                        if (!EqualsPos(tempstart, tempcast2))
                        {
                            dis2 = (end - tempcast2).magnitude;
                        }
                        if( Math.Abs( dis1 - dis2) < 0.01f)
                        {
                            canmove = false;
                            PlayMoveAnim();
                            return;
                        }
                        else if (dis1 < dis2)
                        {
                            cast = tempcast1;
                            chooseLeft = true;
                        }
                        else
                        {
                            cast = tempcast2;
                            chooseLeft = false;
                        }
                        bool slide = true;
                        for (int i = 0; i < 4; ++i )
                        {
                            _leftHistory[i] = _leftHistory[i + 1];
                        }
                        if (_leftHistory[4] != chooseLeft && Math.Abs(_lastDegree - degree) < LIMITMOVE)
                        {
                            canmove = false;
                            PlayMoveAnim();
                            return;
                        }
                        _leftHistory[4] = chooseLeft;
                        string leftStr = "";
                        foreach (bool b in _leftHistory)
                        {
                            leftStr += b.ToString();
                        }
                        //Debug.LogError(leftStr);
                        for (int i = 0; i < 4; ++i)
                        {
                            bool tempb = (_leftHistory[i] == _leftHistory[i + 1]);
                            slide = slide && tempb;
                        }
                        if (!slide)
                        {
                            canmove = false;
                            PlayMoveAnim();
                            return;
                        }
                        if (_leftHistory[4] != _leftHistory[3])
                        {
                            canmove = false;
                            PlayMoveAnim();
                            return;
                        }
                    }
                    if (canmove)
                    {
                        isPathfinding = false;
                        if (checkCollision())
                        {
                            PlayMoveAnim();
                            return;
                        }
                    }
                }
            }
            if (canmove)
            {
                Entity.Scene.CheckPositonByRayCast(ref cast);
                if (EqualsPos(cast, Entity.Position))
                {
                    //Debug.Log("cast = " + cast.ToString());
                    _lastDegree = degree;
                    // @xl 加上碰到墙继续播放动作的功能，这代码。。。 //
                    //_currentTarget = cast;
                    //endMoveByNextMove();
                    PlayMoveAnim();
                    //MoveDir(degree, shift > 0 ? -shift : -shift + 20);
                    return;
                }
                //Debug.Log(cast.ToString());
                if (shift > 0 && Vector3.Distance(cast, Entity.Position) > 0.2)
                {
                    cast = Entity.Position + _rockDir * 0.2f;
                }
                endMoveByNextMove();
                _atEdge = shift != 0;
                moveStraight(cast);
                _lastDegree = degree;
            }
            else if (!isMoving)
            {
                _lastDegree = -1000;
            }
        }

        // @xl 添加与其他玩家碰撞的功能 //
        private bool checkCollision()
        {
            //此处主角应该不为0
            if (this.Entity.CollisionRadius < 0.001f)
            {
                return false;
            }
            GOESceneEntityContainer con = Entity.Scene.GetComponent<GOESceneEntityContainer>();
            for (int i = 0; i < con.GetEntityCount(); ++i)
            {
                Entity ent = con.GetEntityByIndex(i);
                if (ent != null)
                {
                    if (ent.GameObject != null)
                    {
                        //Debug.Log(ent.GameObject.name + "----" + ent.GUID.ToString());
                    }
                    if (ent.CollisionRadius < 0.01f)
                    {
                        continue;
                    }
                    if (isPathfinding && ent.IgnorePathFindingCollision)
                        continue;
                    if (!ent.Visible)
                    {
                        continue;
                    }
                    if (ent.GUID == this.Entity.GUID)
                    {
                        //Debug.Log(ent.GUID.ToString() + "continue");
                        continue;
                    }
                    if (!(ent is IGOEActorEntity))
                    {
                        continue;
                    }
                    Vector3 posvec = (ent.Position - Entity.Position);
                    posvec.y = 0;
                    if (posvec.magnitude < (ent.CollisionRadius + Entity.CollisionRadius))
                    {
                        //Debug.Log("zhuangshang =  " + posvec.magnitude.ToString());
                        Vector3 tempdir = _rockDir;
                        tempdir.y = 0;
                        Debug.DrawRay(Entity.Position, tempdir, Color.yellow);
                        tempdir.Normalize();
                        posvec.Normalize();

                        float dotrst = Vector3.Dot(tempdir, posvec);
                        //Debug.Log(dotrst.ToString());
                        //Debug.Log(Entity.Position.ToString() + "---" + ent.Position.ToString());
                        if (ent.GameObject != null)
                        {
                            //Debug.Log(Entity.GameObject.name);
                        }
                        Debug.DrawLine(Entity.Position, ent.Position, Color.red);
                        if (dotrst > 0.3f && dotrst < 1f)
                        {
                            //_currentTarget = end;
                            //endMoveByNextMove();
                            endMove(false);
                            if (Actor.OnCollision != null)
                                Actor.OnCollision(ent);
                            //PlayMoveAnim();
                            _lastDegree = -1000;
                            //Debug.Log("zhuangshang ");
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        private List<Vector3> _path = new List<Vector3>();
        private Action _onNearTarget;
        internal List<Vector3> MoveTo(float x, float y, Action onNear)
        {
            Vector3 end = new Vector3();
            Vector3 scp = new Vector3(x, y, 0);
            if (!Entity.Scene.GetIntersectTerrainPos(scp, ref end))
            {
                Ray ray = GOERoot.GOECamera.Camera.ScreenPointToRay(scp);
                float dis = (Actor.Position.y - ray.origin.y) / ray.direction.y;
                end = ray.origin + ray.direction * dis;
                Actor.Scene.getNearestPoint(end, out end, Actor);
            }

            return MoveTo(end, onNear);
        }

        internal void MoveTo(List<Vector3> path, Action onReach)
        {
            endMoveByNextMove();
            isPathfinding = true;
            _path.Clear();
            _path.AddRange(path);
            _onNearTarget += onReach;
            beginMove();
        }

        internal List<Vector3> MoveTo(Vector3 end, Action onNear, float dis = 0f)
        {
            ////Debug.Log ("want move to " + end.ToString ());
            bool canMove = false;
            endMoveByNextMove();
            isPathfinding = true;
            if (Actor.ClientDetour)
            {
                if (!Entity.Scene.CheckPositonByRayCast(ref end))
                {
                    Actor.Scene.getNearestPoint(end, out end, Actor);
                }
				if (Entity.Scene.GetPath(Actor, Actor.Position, end, out _path) == DetourState.DT_SUCCESS)
                {
                    canMove = true;
                    resetEnd(dis);
                    if (_path.Count > 0)
                        beginMove();
                }
            }
            else
            {
                canMove = true;
                moveStraight(end);
            }
            if (isMoving)
                _onNearTarget += onNear;
            else if (canMove && onNear != null)
                onNear();
            return _path;
        }

        private void resetEnd(float dis)
        {
            if (dis <= 0)
                return;
            int count = _path.Count - 1;
            Vector3 end = _path[count];
            Vector3 start = Actor.Position;
            if (_path.Count > 1)
                start = _path[count - 1];

            if (start.Equals(end))
                return;
            if (Vector3.Distance(start, end) > dis)
            {
                Vector3 dir = (end - start);
                dir.Normalize();
                end = end - dir * dis;
                _path[count] = end;
            }
            else
            {
                _path.RemoveAt(count);
                if (_path.Count == 1 && _path[0].EqualsVector3(ref start))
                    _path.Clear();
            }
        }

        internal List<Vector3> MoveClose(Vector3 pos, Action onClose, float dis = 1.0f)
        {
            return MoveTo(pos, onClose, dis);
        }

        internal void StopMove()
        {
            endMove(false);
        }

        private void endMoveByNextMove()
        {
            _onNearTarget = null;
            bool old = isMoving;
            clearData();
            isMoving = old;
        }

        private float speed
        {
            get
            {
                if (_tempSpeed > 0)
                    return _tempSpeed;
                return Actor.MoveSpeed;
            }
        }

        private float _tempSpeed = -1;
        private bool _keepFace;
        private bool _keepAni;
        internal void MoveStraight(Vector3 end, float speed = -1, bool keeFace = false, bool keepAni = false, Action onReach = null)
        {
            endMoveByNextMove();
            isPathfinding = true;
            _tempSpeed = speed;
            _keepFace = keeFace;
            _keepAni = keepAni;
            _onNearTarget += onReach;
            moveStraight(end);
        }

        internal void MoveStraightByTime(Vector3 end, float time, bool keeFace, bool keepAni)
        {
            endMoveByNextMove();
            isPathfinding = true;
            _keepFace = keeFace;
            _keepAni = keepAni;
            mMoveByTime = true;
            moveStraight(end);
            _distance = time * speed;
        }

        private void moveStraight(Vector3 end)
        {
            if (float.IsInfinity(end.x))
                return;
            _path.Add(end);
            beginMove();
        }

        internal override void Update()
        {
            base.Update();

            if (!isMoving || _paused)
            {
                return;
            }
            mTime += GOERoot.RealTime.DeltaTime;
            UpdatePathNode();
            UpdateForward();
        }

        private Vector3 _currentTarget;
        private int _nodeIndex;
        private void UpdatePathNode()
        {
            if (_nodeIndex < _path.Count)
            {
                if (_distance <= 0)
                {
                    _currentTarget = _path[_nodeIndex];
                    if (Actor.OnUserMoveTo != null)
                        Actor.OnUserMoveTo(Actor.Position, _currentTarget);
                    mTime = 0;
                    if (!_keepFace)
                    {
                        Vector3 normal = new Vector3();
                        Vector3 v = Actor.Position;
                        this.Entity.Scene.CheckPositonByRayCast(ref v, ref normal);
                        setRotate(normal);
                        _rockDir = _currentTarget - Actor.Position;
            
                    }
                    setForwad();
                    _nodeIndex++;
                }
            }
            else
            {
                if (_distance <= 0)
                {
                    if (!Vector3.Equals(Entity.Position, _currentTarget))
                    {
                        setActorPos(_currentTarget);
                    }
                    isMoving = false;
                    stopForward();
                }
            }
        }

        Vector3 lastNormal;
        public void ResetRotation()
        {
            Vector3 v = Actor.Position;
            Vector3 normal = Vector3.up;
            if (Actor.RotateUpVector)
                this.Entity.Scene.CheckPositonByRayCast(ref v, ref normal);
            if (lastNormal != normal)
            {
                lastNormal = normal;
                Quaternion mTo = Quaternion.identity;

                if (Actor.IsRotateSmoothing)
                {
                    Quaternion curRot = Actor.RealRotation;
                    mTo = Quaternion.Euler(0, curRot.eulerAngles.y, 0);
                }
                else
                {
                    if (_nodeIndex < _path.Count - 1)
                    {
                        Vector3 mTargetOrientation = _currentTarget - Actor.Position;
                        mTo = Quaternion.LookRotation(mTargetOrientation, Vector3.up);
                    }
                    else
                    {
                        Quaternion curRot = Actor.RealRotation;
                        mTo = Quaternion.Euler(0, curRot.eulerAngles.y, 0);
                    }
                }
                if (Actor.RotateUpVector)
                {
                    Vector3 from = mTo * Vector3.forward;
                    Quaternion quat = Quaternion.FromToRotation(Vector3.up, normal);
                    from = quat * from;
                    mTo = Quaternion.LookRotation(from, normal);
                }
                if (Actor.RotateSmooth)
                {
                    Actor.StartSmoothRotation(mTo);
                    return;
                }
                Actor.Rotation = mTo;
            }
        }


        Quaternion mTo;
        private void setRotate(Vector3 normal)
        {
            Vector3 mInitialOrientation = Actor.Orientation;
            Vector3 mTargetOrientation = _currentTarget - Actor.Position;

            mInitialOrientation.y = 0;
            mInitialOrientation = mInitialOrientation.normalized;

            mTargetOrientation.y = 0;
            mTargetOrientation = mTargetOrientation.normalized;
            if (mInitialOrientation.EqualsVector3(ref mTargetOrientation) || Vector3.zero.EqualsVector3(ref mTargetOrientation))
            {
                return;
            }
            mTo = Quaternion.LookRotation(mTargetOrientation, Vector3.up);
            if (Actor.RotateUpVector)
            {
                Vector3 from = mTo * Vector3.forward;
                Quaternion quat = Quaternion.FromToRotation(Vector3.up, normal);
                from = quat * from;
                mTo = Quaternion.LookRotation(from, normal);
            }
            if (Actor.RotateSmooth)
            {
                Actor.StartSmoothRotation(mTo);
                return;
            }
            if (_atEdge)
            {
                return;
            }
            Entity.Rotation = mTo;
        }


        private void UpdateForward()
        {
            if (_distance <= 0)
                return;
            if (checkCollision())
            {
                return;
            }
            float sp = speed;
            float dis = Time.smoothDeltaTime * sp;
            if (dis > _distance)
                dis = _distance;
            _distance -= dis;
            Vector3 v = this._direction * dis;
            v = Actor.Position + v;
            if (!Actor.Scene.IsPositionWalkable(Actor, v))
            {
                Actor.Scene.IsPositionWalkable(Actor, v);
            }
            setActorPos(v);
            ResetRotation();
        }
        
        private void setActorPos(Vector3 v)
        {
            Vector3 normal = new Vector3();
            Vector3 hitpoint = v;
            bool vi = this.Entity.Scene.CheckPositonByRayCast(ref hitpoint, ref normal);
            if (vi)
                v = hitpoint;
            if (Math.Abs(Actor.Position.y - v.y) > 3)
            {
                v.y = Actor.Position.y;
            }
            Actor.Position = v;
        }

        float _distance = -1;
        Vector3 _direction;
        private void setForwad()
        {
            if (Actor.Position.EqualsVector3(ref _currentTarget))
            {
                _distance = -1;
                return;
            }
            _direction = _currentTarget - Actor.Position;
            _direction.y = 0;
            _distance = _direction.magnitude;
            _direction.Normalize();
        }


        private void PlayMoveAnim()
        {
            if (!_keepAni && !isMoving && !Actor.IgnoreMoveAnimation)
            {
                if (!string.IsNullOrEmpty(Actor.SlowMoveAnimation))
                {
                    if (Actor.MoveBlendTime > 0)
                    {
                        Actor.CrossFade(Actor.SlowMoveAnimation, Actor.MoveBlendTime, PlayMode.StopSameLayer, false);
                        Actor.CrossFadeQueued(Actor.MoveAnimation, Actor.MoveBlendTime, QueueMode.CompleteOthers, PlayMode.StopSameLayer, false);
                    }
                    else
                    {
                        Actor.PlayAnimation(Actor.SlowMoveAnimation, PlayMode.StopSameLayer, false);
                        Actor.PlayQueued(Actor.MoveAnimation, QueueMode.CompleteOthers, PlayMode.StopSameLayer, false);
                    }
                }
                else
                {
                    if (Actor.MoveBlendTime > 0)
                        Actor.CrossFade(Actor.MoveAnimation, Actor.MoveBlendTime, PlayMode.StopSameLayer, false);
                    else
                        Actor.PlayAnimation(Actor.MoveAnimation, PlayMode.StopSameLayer, false);
                }
            }
            isMoving = true;
        }
        private void beginMove()
        {
            Enable = true;
            _nodeIndex = 0;
            mTime = 0f;
            UpdatePathNode();
            PlayMoveAnim();
        }

        private void clearData()
        {
            isMoving = false;
            Enable = false;
            _nodeIndex = 0;
            _path.Clear();
            _tempSpeed = 0;
            _atEdge = false;
            _keepFace = false;
            _keepAni = false;
            _distance = -1;
            _lastDegree = -1000;
        }

        private void endMove(bool reach = true)
        {
            if (!isMoving)
                return;
            if (!reach)
                _onNearTarget = null;
            stopForward();
            clearData();
        }

        private void stopForward()
        {
            if (!_keepAni && !Actor.IgnoreMoveAnimation)
            {
                if (!string.IsNullOrEmpty(Actor.BreakAnimation))
                {
                    if (Actor.MoveBlendTime > 0)
                    {
                        Actor.CrossFade(Actor.BreakAnimation, Actor.MoveBlendTime);
                        Actor.CrossFadeQueued(Actor.StandAnimation, Actor.MoveBlendTime);
                    }
                    else
                    {
                        Actor.PlayAnimation(Actor.BreakAnimation);
                        Actor.PlayQueued(Actor.StandAnimation);
                    }
                }
                else
                {
                    if (Actor.MoveBlendTime > 0)
                        Actor.CrossFade(Actor.StandAnimation, Actor.MoveBlendTime);
                    else
                        Actor.PlayAnimation(Actor.StandAnimation);
                }
            }
            if (Actor.OnUserStopMove != null)
                Actor.OnUserStopMove();
            Action onNear = _onNearTarget;
            if (onNear != null)
            {
                _onNearTarget = null;
                onNear();
            }
        }
    }

}

