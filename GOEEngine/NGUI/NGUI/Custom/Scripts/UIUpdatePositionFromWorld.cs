using UnityEngine;
using System.Collections;

[AddComponentMenu("Game/UI/UpdatePositionFromWorld")]
public class UIUpdatePositionFromWorld : MonoBehaviour
{

    public bool ManualUpdate = true;

    private Vector3 mWorldPosition = Vector3.zero;
    private Transform mCachedTrans;
    private Camera mUICamera;
    float lastTime;
    UIRoot root;

    public Vector3 WorldPosition
    {
        get { return mWorldPosition; }
        set { mWorldPosition = value; }
    }
    void Start()
    {
        mCachedTrans = this.transform;
        root = gameObject.GetComponent<UIRoot>();
    }

    void OnEnable()
    {
        if (root)
            root.Reset();
    }

    void Update()
    {
        if (!mCachedTrans.parent)
            return;
        if (!ManualUpdate)
        {
            mWorldPosition = mCachedTrans.parent.position;
        }

        if (mUICamera == null)
        {
            mUICamera = NGUITools.FindCameraForLayer(gameObject.layer);
        }

        if ((mUICamera != null) && (Camera.main != null))
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(mWorldPosition);
            Vector3 uiWorldPos = mUICamera.ScreenToWorldPoint(screenPos);
            
            mCachedTrans.position = uiWorldPos;

            mCachedTrans.rotation = mUICamera.transform.rotation;
        }
    }
}
