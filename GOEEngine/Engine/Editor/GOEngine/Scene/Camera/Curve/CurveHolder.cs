using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CurveHolderNew : ScriptableObject

//public class CurveHolder 
{
    public AnimationCurve anglex;
    public AnimationCurve angley;
    public AnimationCurve anglez;
    public AnimationCurve posx;
    public AnimationCurve posy;
    public AnimationCurve posz;
    public MotionType mType;
    public RotationType rType;
    public bool isLocal;

    public AnimationCurve rotx;
    public AnimationCurve roty;
    public AnimationCurve rotz;
    public AnimationCurve rotw;

    public List<string> animationStateData;//动作事件名称数据存这里纯粹只是为了方便

    public CurveHolderNew()
    {
        anglex = new AnimationCurve();
        angley = new AnimationCurve();
        anglez = new AnimationCurve();
        posx = new AnimationCurve();
        posy = new AnimationCurve();
        posz = new AnimationCurve();
        rotx = new AnimationCurve();
        roty = new AnimationCurve();
        rotz = new AnimationCurve();
        rotw = new AnimationCurve();
        mType = MotionType.loop;
        rType = RotationType.self;
        isLocal = true;
        animationStateData = null;
    }
    public CurveHolderNew(CurveHolderNew source)
    {
        name = source.name;
        anglex = new AnimationCurve(source.anglex.keys);
        angley = new AnimationCurve(source.angley.keys);
        anglez = new AnimationCurve(source.anglez.keys);
        posx = new AnimationCurve(source.posx.keys);
        posy = new AnimationCurve(source.posy.keys);
        posz = new AnimationCurve(source.posz.keys);
        rotx = new AnimationCurve(source.rotx.keys);
        roty = new AnimationCurve(source.roty.keys);
        rotz = new AnimationCurve(source.rotz.keys);
        rotw = new AnimationCurve(source.rotw.keys);
        mType = source.mType;
        rType = source.rType;
        if (source.animationStateData !=null)
            animationStateData = new List<string> (source.animationStateData);
        isLocal = source.isLocal;
    }
	
	/*public void StoreOldData ( CurveHolder source )
	{
		name = source.name;
		mType = source.mType;
		rType = source.rType;
		isLocal = source.isLocal;
		
		Clear();
		anglex = new AnimationCurve(source.rotationx.keys);
        angley = new AnimationCurve(source.rotationy.keys);
        anglez = new AnimationCurve(source.rotationz.keys);
		posx = new AnimationCurve(source.curvex.keys);
        posy = new AnimationCurve(source.curvey.keys);
        posz = new AnimationCurve(source.curvez.keys);
		
		CoverEulerAnglesToRotation();
		
		if (source.animationStateData !=null)
            animationStateData = new List<string> (source.animationStateData);
	}*/

    public float MaxTime
    {
        get 
        {
            return Mathf.Max(
                new float[] { 
                    CurveUtility.GetEndTime(posx), 
                    CurveUtility.GetEndTime(posy),
                    CurveUtility.GetEndTime(posz),
                    CurveUtility.GetEndTime(anglex),
                    CurveUtility.GetEndTime(angley),
                    CurveUtility.GetEndTime(anglez)
                }
                );
        }
    }

    public float EndTime
    {
        get 
        {
            float endTime = MaxTime;
            if (mType == MotionType.oncepingpong)
            {
                endTime *= 2;
            }
            if (mType == MotionType.loop || mType == MotionType.pingpong)
            {
                endTime = Mathf.Pow(2.0f, 63);
            }
            return endTime;
        }
    }

    public bool TimeOut(float time)
    {
        return EndTime < time;
    }

    public int AddKey(float time)
    {
        Vector3 position = Vector3.zero;
		Vector3 euler_angles = Vector3.zero;
        //Quaternion rotation = Quaternion.identity;
        return AddKey(time, position, euler_angles);
    }
    public int AddKeyOnlyPos(float time, Vector3 position)
    {
		Vector3 euler_angles = Vector3.zero;
        //Quaternion rotation = Quaternion.identity;
        return AddKey(time, position, euler_angles);
    }
    public int AddKeyOnlyAngles(float time, Vector3 euler_angles)
    {
        Vector3 position = Vector3.zero;
        return AddKey(time, position, euler_angles);
    }
    public int AddKey(float time, Vector3 position , Vector3 euler_angles)
    {
        Quaternion quat = Quaternion.identity;
        quat.eulerAngles = euler_angles;
        int [] values = {
                            posx.AddKey(time, position.x),
                            posy.AddKey(time, position.y),
                            posz.AddKey(time, position.z),
                            anglex.AddKey(time, euler_angles.x),
                            angley.AddKey(time, euler_angles.y),
                            anglez.AddKey(time, euler_angles.z),
                            rotx.AddKey(time, quat.x),
                            roty.AddKey(time, quat.y),
                            rotz.AddKey(time, quat.z),
                            rotw.AddKey(time, quat.w),
                        };
        return Mathf.Min(values);
    }

    public void ModifyKey(float time, Vector3 position, Vector3 euler_angles)
    {
        CurveUtility.ModifyCurveKey(posx, time, position.x);
        CurveUtility.ModifyCurveKey(posy, time, position.y);
        CurveUtility.ModifyCurveKey(posz, time, position.z);

        CurveUtility.ModifyCurveKey(anglex, time, euler_angles.x);
        CurveUtility.ModifyCurveKey(angley, time, euler_angles.y);
        CurveUtility.ModifyCurveKey(anglez, time, euler_angles.z);
		
		Quaternion quat = Quaternion.identity;
		quat.eulerAngles = euler_angles;
        CurveUtility.ModifyCurveKey(rotx, time, quat.x);
        CurveUtility.ModifyCurveKey(roty, time, quat.y);
        CurveUtility.ModifyCurveKey(rotz, time, quat.z);
        CurveUtility.ModifyCurveKey(rotw, time, quat.w);
    }

    public void RemoveKey(float time)
    {
        CurveUtility.RemoveKey(posx, time);
        CurveUtility.RemoveKey(posy, time);
        CurveUtility.RemoveKey(posz, time);
        CurveUtility.RemoveKey(anglex, time);
        CurveUtility.RemoveKey(angley, time);
        CurveUtility.RemoveKey(anglez, time);
        CurveUtility.RemoveKey(rotx, time);
        CurveUtility.RemoveKey(roty, time);
        CurveUtility.RemoveKey(rotz, time);
        CurveUtility.RemoveKey(rotw, time);
    }

    public void Clear()
    {
        CurveUtility.ClearCurve(posx);
        CurveUtility.ClearCurve(posy);
        CurveUtility.ClearCurve(posz);
        CurveUtility.ClearCurve(anglex);
        CurveUtility.ClearCurve(angley);
        CurveUtility.ClearCurve(anglez);
        CurveUtility.ClearCurve(rotx);
        CurveUtility.ClearCurve(roty);
        CurveUtility.ClearCurve(rotz);
        CurveUtility.ClearCurve(rotw);
    }
	public Vector3 GetAnglesOfCurve ( float requestedTime )
	{
		return new Vector3(
            anglex.Evaluate ( requestedTime ),
            angley.Evaluate ( requestedTime ),
            anglez.Evaluate ( requestedTime )
            );
	}
    public Quaternion GetRotationOfCurve(float requestedTime )
    {
        /*Quaternion rotation = new Quaternion();
        rotation.eulerAngles = new Vector3(anglex.Evaluate(requestedTime),
                                angley.Evaluate(requestedTime),
                                anglez.Evaluate(requestedTime)
                                );
        
        return rotation;*/
        return new Quaternion(
            rotx.Evaluate(requestedTime),
            roty.Evaluate(requestedTime),
            rotz.Evaluate(requestedTime),
            rotw.Evaluate(requestedTime)
            );
							    
    }
	
    public Vector3 GetPosOfCurve( float requestedTime )
    {
	    return new Vector3(
            posx.Evaluate ( requestedTime ),
            posy.Evaluate ( requestedTime ),
            posz.Evaluate ( requestedTime )
            );

    }

    public Vector3 GetStartPosition()
    {
        return GetPosOfCurve(0);
    }
    public Vector3 GetEndPosition()
    {
        return GetPosOfCurve(MaxTime);
    }
    public Quaternion GetStartRotation()
    {
        return GetRotationOfCurve(0);
    }
    public Quaternion GetEndRotation()
    {
        return GetRotationOfCurve(MaxTime);
    }


    /// <summary>
    /// 根据移动方式获取对应时间的坐标
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public Vector3 GetPositionByType(float time)
    {
        Vector3 pos = GetPosOfCurve(time);
        switch (mType)
        {
	        case MotionType.once:
                if (time < 0)
                    pos = GetStartPosition();
                if (time <= MaxTime)
                    pos = GetPosOfCurve(time);
                if (time > MaxTime)
                    pos = GetEndPosition();
                break;
            case MotionType.loop:
                pos = GetPosOfCurve(time % MaxTime);
                break;
            case MotionType.pingpong:
                if ((int)(time / MaxTime) % 2 == 0)
                    pos = GetPosOfCurve(time % MaxTime);
                else
                    pos = GetPosOfCurve(MaxTime - time % MaxTime);
                break;
            case MotionType.oncepingpong:
                if ((int)(time / MaxTime) == 0 )
                    pos = GetPosOfCurve(time);
                if ((int)(time / MaxTime) == 1 )
                    pos = GetPosOfCurve(MaxTime - time % MaxTime);
                
                break;
            default:
                break;
        }
        return pos;
    }

    /// <summary>
    /// 根据旋转方式获取对应时间的旋转量
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    public Quaternion GetRotationByType(float time)
    {
        if (time > MaxTime)
            time %= MaxTime; 
        Quaternion rotation = GetRotationOfCurve(time);
        switch (rType)
        {
            case RotationType.self:
                break;
            case RotationType.tangent:
                Vector3 currentPosition = GetPositionByType(time);
                Vector3 nextPosition = GetPositionByType(time + 0.1f);
                rotation = Quaternion.FromToRotation(currentPosition, nextPosition - currentPosition);
                break;
            case RotationType.target:
                break;
            case RotationType.lookcurve:
                break;
            default:
                break;
        }
        return rotation;
    }

    public List<float> GetFixedFrameTimes()
    {
        List<float> times = new List<float>();
        foreach (Keyframe kf in posx.keys)
        {
            if (CurveUtility.IsKeyFrame(posy, kf.time) &&
                CurveUtility.IsKeyFrame(posz, kf.time) &&
                CurveUtility.IsKeyFrame(anglex, kf.time) &&
                CurveUtility.IsKeyFrame(angley, kf.time) &&
                CurveUtility.IsKeyFrame(anglez, kf.time)
                )
            {
                times.Add(Mathf.Round(kf.time * 100 )/100);
            }
        }
        return times;
    }

    public static List<string> CoverAnimationDictToList(Dictionary <float, string> dict)
    {
        List <string> list = new List <string>();
        foreach(KeyValuePair<float, string> kv in dict)
        {
            list.Add(kv.Key.ToString());
            list.Add(kv.Value);

        }
        return list;
    }

    public static Dictionary<float, string> CoverListToAnimationDict(List<string> list)
    {
        Dictionary<float, string> dict = new Dictionary<float, string>();
        for (int i = 0; i < list.Count; i+=2)
        {
            float key;
            Single.TryParse(list[i], out key);
            dict[key] = list[i + 1];
        }
        return dict;
    }
	
	public void CoverEulerAnglesToRotation()
	{
		foreach ( Keyframe kf in anglex.keys )
		{
			float time = kf.time;
			Quaternion quat = Quaternion.identity;
			quat.eulerAngles = new Vector3 ( anglex.Evaluate (time),
				angley.Evaluate (time),
				anglez.Evaluate (time));
			
			rotx.AddKey ( new Keyframe ( time, quat.x ));
			roty.AddKey ( new Keyframe ( time, quat.y ));
			rotz.AddKey ( new Keyframe ( time, quat.z ));
			rotw.AddKey ( new Keyframe ( time, quat.w ));
		}
	}

    
}

