using UnityEngine;

using System.Collections;

public struct TransitionParam
{
	public float value;
	public float beginTime;
	public float timeLong;
}

public class CameraControl : MonoBehaviour 
{
	public float radius = 5;
	public float Y = 5;
	public float angle = 0;

    public Vector3 offSet = Vector3.zero;
    public Vector3 offSetAdjust = Vector3.zero;
    private Vector3 offSetTarget = Vector3.zero;
    private bool offSetUpdate = true;
	
	// 相机参数原始的备份 //
	private float cacheRadius;
	private float cacheY;
	private float cacheAngle;
	
	// 相机变换的起始参数 //
	private float sourceRadius = 5;
	private float sourceY = 5;
	private float sourceAngle = 0;
	
	
	// 相机变换的目标参数 //
	//private float transitionRadius = 5;
	//private float transitionY = 5;
	//private float transitionAngle = 0;
	private TransitionParam transitionRadius;
	private TransitionParam transitionY;
	private TransitionParam transitionAngle;
	
	public bool isSmooth = true;
    private bool cacheIsSmooth = true;
	public float cameraSmoothing = 0.5f;
    public float cameraSmoothStill = 1.2f;
	public float lookatOffset = 0;
	
	private Vector3 cameraVelocity;
	private Vector3 targetPos;
	private Vector3 lookatPos;
	
	private bool move = false;
	//private float moveTime = 2f;
	private float curTime = 0;
	
	public Transform mLookAt = null;
	private Transform mSelf = null;
	
	private CameraTransitionArea mCurArea = null;
	
	private bool reset = false;
	
	private float targetFov = -1f;

    public float accelerateTime = 0;
    public float accelerateScale = 1;
    public float accelerateTime2 = 30;
    public float accelerateScale2 = 1;


    public AnimationClip clip;
    private bool mPlayAnimation = false;
	private Animation mAnim = null;
    private Transform oldParent;
    private GameObject newParent;

    public float roll;
    public float yaw;
    public float pitch;

    private float yOffset = 0;
    private Vector3 _lastPosition = Vector3.zero;


    public float YOffset
    {
        set { yOffset = value; }
        get { return yOffset; }
    }

    public Vector3 OffsetSmoothTo
    {
        set 
        {
            offSetTarget = value;
            StopCoroutine("offsetSmoothTo");
            StartCoroutine("offsetSmoothTo");
        }
    }

    private IEnumerator offsetSmoothTo()
    {
        Vector3 offsetFrom = offSet;
        float time = 0;
        while (Vector3.Distance(offSet, offSetTarget) > 0.01f)
        {
            offSet = Vector3.Lerp(offsetFrom, offSetTarget, time += Time.deltaTime);
            yield return new WaitForSeconds(0.01f);
        }
        offSet = offSetTarget;
    }


    private float NormalizeAngle(float angle)
    {
        int deg = (int)(Mathf.Rad2Deg * angle);
        deg %= 360;

        if (deg < 0)
        {
            deg = 360 + deg;
        }


        angle = deg * Mathf.Deg2Rad;
        return angle;
    }

    void Start()
    {
        mSelf = transform;
        sourceRadius = radius;
        sourceY = Y;
        angle = NormalizeAngle(angle);
        sourceAngle = angle;//

        cacheRadius = radius;
        cacheY = Y;
        cacheAngle = angle;

        cacheIsSmooth = isSmooth;
        InterruptSmooth = false;
        reset = true;
        if (mLookAt)
            _lastPosition = mLookAt.position;
    }
	
    public void CacheValue(float radius, float y, float angle)
    {
		cacheRadius = radius;
		cacheY = y;
        cacheAngle = NormalizeAngle(angle);
    }

	public void SetTarget(Transform trans)
	{
        if (mLookAt != null && (trans.position - mLookAt.position).magnitude < 0.1f)
        {
            reset = false;
            mLookAt = trans;
            return;
        }
        mLookAt = trans;
        float x = sourceRadius * Mathf.Sin(sourceAngle) * Mathf.Cos(0);
        float z = sourceRadius * Mathf.Cos(sourceAngle);
        Vector3 cameraOffset = new Vector3(x, sourceY, z);

        Quaternion currentRotation = Quaternion.Euler (0, mLookAt.eulerAngles.y, 0);

		lookatPos = mLookAt.position + currentRotation * Vector3.forward * lookatOffset;

        //Quaternion currentRotation = Quaternion.Euler(0, mLookAt.eulerAngles.y, 0);

        transform.position = lookatPos + cameraOffset;
        transform.LookAt(lookatPos);
        targetPos = mLookAt.position;
        reset = false;
    }

    public void ChangeTarget(Transform trans)
    {
        if (trans == mLookAt)
        {
            return;
        }
        mLookAt = trans;
        targetPos = mLookAt.position;
    }

	
	private void setFov(float fov)
	{
		if (this.gameObject.GetComponent<Camera>().fieldOfView == fov)
		{
			return;
		}
		targetFov = fov;
        if (targetFov > this.GetComponent<Camera>().fieldOfView)
		{
			StartCoroutine("AddFov");
		}
		else
		{
			StartCoroutine("SubFov");
		}
	}
	
	public float Fov
	{
        get { return this.GetComponent<Camera>().fieldOfView; }
		set 
		{
			setFov(value);
		}
	}	
	
	private IEnumerator AddFov()
	{
        while (targetFov > this.GetComponent<Camera>().fieldOfView)
		{

            this.GetComponent<Camera>().fieldOfView += 0.2f;
			yield return new WaitForSeconds(0.01f);
		}
	}
	
	private IEnumerator SubFov()
	{
        while (targetFov < this.GetComponent<Camera>().fieldOfView)
		{
            this.GetComponent<Camera>().fieldOfView -= 0.2f;
			yield return new WaitForSeconds(0.01f);
		}
	}
	
	void Update ()
	{
        if (mLookAt == null)
			return;
        if(mPlayAnimation)
        {
            PlayAnimationUpdate();
        }
		if (move)
		{
			if (Mathf.Abs(transitionRadius.value - radius) < 0.001f && Mathf.Abs(transitionY.value - Y) < 0.001f && Mathf.Abs(transitionAngle.value - angle) < 0.001f)
			{
				move = false;
				curTime = 0;
				return;
			}
			float temp = 1f;
            if (curTime < accelerateTime)
			{
				temp = accelerateScale;
			}
            if (curTime > accelerateTime2)
            {
                temp = accelerateScale2;
            }

			curTime += Time.smoothDeltaTime * temp;
			if (curTime > transitionRadius.beginTime && Mathf.Abs(transitionRadius.value - radius) > 0.001f)
			{
				float t = (curTime - transitionRadius.beginTime) / transitionRadius.timeLong;
                radius = Mathf.Lerp(sourceRadius, transitionRadius.value*temp, t);
			}
			
			if (curTime > transitionY.beginTime && Mathf.Abs(transitionY.value - Y) > 0.001f)
			{
				float t = (curTime - transitionY.beginTime) / transitionY.timeLong;
				Y = Mathf.Lerp(sourceY, transitionY.value*temp, t);
			}
			
			if (curTime > transitionAngle.beginTime && Mathf.Abs(transitionAngle.value - angle) > 0.001f)
			{
				float t = (curTime - transitionAngle.beginTime) / transitionAngle.timeLong;
				angle = Mathf.Lerp(sourceAngle, transitionAngle.value, t);
			}
		}
		
		float x = radius * Mathf.Sin (angle) * Mathf.Cos (0);
		float z = radius * Mathf.Cos (angle);
		Vector3 cameraOffset = new Vector3(x, Y, z);
		
		Quaternion currentRotation = Quaternion.Euler (0, mLookAt.eulerAngles.y, 0);

        // 移动和非移动的时候的Offset不一样 //
        float realForwardOffset = lookatOffset;
        float realCameraSmooth = cameraSmoothing;
        if( _lastPosition != mLookAt.position )
        {

        }
        else
        {
            realForwardOffset = 0f;
            realCameraSmooth = cameraSmoothStill;
        }
        _lastPosition = mLookAt.position;
		
		if(reset)
		{
			lookatPos = mLookAt.position + currentRotation * Vector3.forward * lookatOffset;
		}
        else if (isSmooth && !InterruptSmooth)
		{
            Vector3 pos = mLookAt.position + currentRotation * Vector3.forward * realForwardOffset;
            lookatPos = Vector3.SmoothDamp(lookatPos, pos, ref cameraVelocity, realCameraSmooth);
		}
		else
		{
			Vector3 pos = targetPos + currentRotation * Vector3.forward * lookatOffset;
			lookatPos = Vector3.SmoothDamp(lookatPos, pos, ref cameraVelocity, cameraSmoothing);
			lookatPos += mLookAt.position - targetPos;
		}
		Vector3 newOffset = offSet;
        if(!mPlayAnimation)
        {
            newOffset.y += YOffset;
            mSelf.position = lookatPos + cameraOffset;
            mSelf.LookAt(lookatPos + newOffset);
        }
        else
        {
            if (newParent != null)
            {
                Quaternion rot = Quaternion.LookRotation(-cameraOffset + newOffset);
                newParent.transform.rotation = rot;
                newParent.transform.position = lookatPos + cameraOffset;
            }
        }
        targetPos = mLookAt.position;
        mSelf.RotateAroundLocal(Vector3.right, pitch);
        mSelf.RotateAroundLocal(Vector3.up, yaw);
        mSelf.RotateAroundLocal(Vector3.forward, roll);
		reset = false;
        InterruptSmooth = false;
        mSelf.position = mSelf.position + offSetAdjust;
	}

	public void Transition(float srcRadius, float srcY, float srcAngle, TransitionParam targetRadius, TransitionParam targetY, TransitionParam targetAngle)
	{
		sourceRadius = srcRadius;
		sourceY = srcY;
		sourceAngle = srcAngle;

        targetAngle.value = NormalizeAngle(targetAngle.value);
        if (Mathf.Abs(sourceAngle - targetAngle.value) > Mathf.PI)
        {
            if (sourceAngle > targetAngle.value)
            {
                targetAngle.value += Mathf.PI * 2;
            }
            else
            {
                targetAngle.value -= Mathf.PI * 2;
            }

        }
		
		transitionRadius = targetRadius;
		transitionY = targetY;
		transitionAngle = targetAngle;
		
		//moveTime = time;
		curTime = 0;
		move = true;
	}
	
	public void SmoothRadius(float value, float begin, float time)
	{
		sourceRadius = radius;
		sourceY = Y;
		sourceAngle = angle;
		
		transitionRadius.value = value;
		transitionRadius.beginTime = begin;
		transitionRadius.timeLong = time;
		
		if(!move)
		{
			transitionY.value = sourceY;
			transitionAngle.value = sourceAngle;
		}
		
		curTime = 0;
		move = true;
	}
	
	public void SmoothY(float value, float begin, float time)
	{
		sourceRadius = radius;
		sourceY = Y;
		sourceAngle = angle;
		
		transitionY.value = value;
		transitionY.beginTime = begin;
		transitionY.timeLong = time;
		
		if(!move)
		{
			transitionRadius.value = sourceRadius;
			transitionAngle.value = sourceAngle;
		}
		
		curTime = 0;
		move = true;
	}

    public bool InterruptSmooth
    {  set;  get;
    }
	public void SmoothAngle(float value, float begin, float time)
	{
		sourceRadius = radius;
		sourceY = Y;
        sourceAngle = NormalizeAngle(angle);// *Mathf.Rad2Deg % 360;

        value = NormalizeAngle(value * Mathf.Deg2Rad);
        if (Mathf.Abs(sourceAngle - value) > Mathf.PI)
        {
            if (sourceAngle > value)
            {
                value += Mathf.PI * 2;
            }
            else
            {
                value -= Mathf.PI * 2;
            }
            
        }
		//sourceAngle *= Mathf.Deg2Rad;

        transitionAngle.value = value;// *Mathf.Deg2Rad;
		transitionAngle.beginTime = begin;
		transitionAngle.timeLong = time;
		
		if(!move)
		{
			transitionRadius.value = sourceRadius;
			transitionY.value = sourceY;
		}
		
		curTime = 0;
		move = true;
        //accelerateTime = time * 0.5f;
        //accelerateScale = 10f;
        //accelerateTime2 = time * 0.5f;
        //accelerateScale2 = 0.1f;


	}
	
	public void LeaveArea(CameraTransitionArea area)
	{
		if (area == mCurArea)
		{
			TransitionParam tpRadius = area.trsRadius;
			tpRadius.value = cacheRadius;
			TransitionParam tpY = area.trsY;
			tpY.value = cacheY;
			TransitionParam tpAngle = area.trsAngle;
			tpAngle.value = cacheAngle;

            isSmooth = cacheIsSmooth;
			Transition(radius, Y, angle, tpRadius, tpY, tpAngle);
		}
	}
	
	public void EnterArea(CameraTransitionArea area)
	{
		mCurArea = area;
		isSmooth = area.smooth;
        accelerateTime = area.accelerateTime;
        accelerateScale = area.accelerateScale;
        accelerateTime2 = area.accelerateTime2;
        accelerateScale2 = area.accelerateScale2;
		Transition(radius, Y, angle, area.trsRadius, area.trsY, area.trsAngle);
	}

    public void PlayAnimation()
    {
        if (clip == null)
            return;
        mPlayAnimation = true;

        oldParent = mSelf.parent;
        newParent = new GameObject("TempCameraParent");
        newParent.transform.parent = mLookAt;
        newParent.transform.localRotation = mSelf.rotation;

        transform.parent = newParent.transform;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        mAnim = gameObject.GetComponent<Animation>() as Animation;
        if (mAnim == null)
        {
            mAnim = gameObject.AddComponent<Animation>() as Animation;
        }
        
        mAnim.AddClip(clip, "CameraAnim");
        mAnim.Play("CameraAnim");
        
    }

    private void PlayAnimationUpdate()
    {
        if (!mAnim.IsPlaying("CameraAnim"))
        {
            mPlayAnimation = false;
            mSelf.parent = oldParent;
        }
    }
}
