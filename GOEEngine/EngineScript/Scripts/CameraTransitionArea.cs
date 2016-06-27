using UnityEngine;
using System.Collections;

[System.Reflection.Obfuscation(Exclude = true)]
public class CameraTransitionArea : MonoBehaviour 
{
	public float radius;

	public float trsRadiusValue;
	public float trsRadiusBegineTime;
	public float trsRadiusTimeLong;

	public float trsYValue;
	public float trsYBegineTime;
	public float trsYTimeLong;

	public float trsAngleValue;
	public float trsAngleBegineTime;
	public float trsAngleTimeLong;

	public TransitionParam trsRadius;
	public TransitionParam trsY;
	public TransitionParam trsAngle;
	public TransitionParam trsSmooth;

	public bool smooth = true;
	//public float trsTime = 1;

	// 当相机进入区域，保存相机最原始的设置 // 
	//private float sourceRadius;
	//private float sourceY;
	//private float sourceAngle;


	private bool isInArea = false;
	private Transform selfTransform = null;

	public float accelerateTime = 0;
	public float accelerateScale = 1;
	public float accelerateTime2 = 30;
	public float accelerateScale2 = 1;


	private CameraControl cameraControl;
	public Transform target;


	// Use this for initialization
	void Start () 
	{
		selfTransform = transform;
		cameraControl = GameObject.FindObjectOfType<CameraControl>() as CameraControl;

		trsRadius.value = trsRadiusValue;
		trsRadius.beginTime = trsRadiusBegineTime;
		trsRadius.timeLong = trsRadiusTimeLong;

		trsY.value = trsYValue;
		trsY.beginTime = trsYBegineTime;
		trsY.timeLong = trsYTimeLong;

		trsAngle.value = trsAngleValue;
		trsAngle.beginTime = trsAngleBegineTime;
		trsAngle.timeLong = trsAngleTimeLong;

		trsSmooth.value = 0;
		trsSmooth.beginTime = 0;
		trsSmooth.timeLong = 1f;

	}

	// Update is called once per frame
	void Update () 
	{
		if (cameraControl == null || target == null)
			return;

		if (Vector3.Distance(target.position, selfTransform.position) < radius)
		{
			if (!isInArea)
			{
				isInArea = true;

				cameraControl.EnterArea(this);
			}
		}
		else
		{
			if (isInArea)
			{
				isInArea = false;
				cameraControl.LeaveArea(this);
			}

		}
	}

	void OnDrawGizmos()
	{
		Gizmos.DrawIcon (transform.position, "Dog17.png");
	}

	void OnDrawGizmosSelected() 
	{
		
		Gizmos.color = Color.red;
		if (isInArea)
		{
			Gizmos.color = Color.yellow;
		}
		Gizmos.DrawWireSphere(transform.position, radius);
	}


	public bool IsInArea( Vector3 pos )
	{
		if (Vector3.Distance(pos, selfTransform.position) < radius)
		{
			return true;
		}
		return false;
	}

}
