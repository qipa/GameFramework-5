using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Reflection.Obfuscation(Exclude = true)]
public class SceneAnimal : MonoBehaviour
{
    enum AnimalStates
    {
        Idle,
        Run,
        Scare,
    }
    public GameObject Prefab;
    public Animation[] StandbyAnimations;
    public Animation StandAnimation;
    public Animation RunAnimation;
    public Animation ScareAnimation;
    public float ScareRadius = 10;
    public Transform[] WayPoints;
    public float PathWidth;
    public float WalkSpeed;
    public int StandByRate = 10;
    public int WalkRate = 30;

    GameObject go;
    string runAnimName, standAnimName, scareAnimName;
    string[] standbyAnimNames;
    float lastThinkTime;
    AnimalStates state;
    const float ThinkInterval = 1f;
    int currentIdx;
    List<Vector3> points;
    bool hasScare = false;

    Vector3 startPos, endPos;
    float startTime;
    float scareStartTime, scareTime;
    CameraControl cameraCtrl;

    // Use this for initialization
    void Start()
    {
        go = GameObject.Instantiate(Prefab) as GameObject;
        runAnimName = RunAnimation.name;
        standAnimName = StandAnimation.name;
        if(ScareAnimation !=null)
        {
            hasScare = true;
            scareTime = ScareAnimation.clip.length;
            scareAnimName = ScareAnimation.name;
            cameraCtrl = Camera.main.GetComponent<CameraControl>();
            if (!cameraCtrl)
                hasScare = false;
        }
        standbyAnimNames = new string[StandbyAnimations.Length];
        for (int i = 0; i < StandbyAnimations.Length; i++)
            standbyAnimNames[i] = StandbyAnimations[i].name;
        go.GetComponent<Animation>().PlayQueued(standAnimName);
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        points = new List<Vector3>();
        points.Add(transform.position);
        foreach (Transform i in WayPoints)
            points.Add(i.position);
        state = AnimalStates.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        StateUpdate();
        if ((Time.time - lastThinkTime) > ThinkInterval)
        {

            lastThinkTime = Time.time;
            Think();
        }
    }

    void StateUpdate()
    {
        switch (state)
        {
            case AnimalStates.Idle:
                {
                    CheckShouldScare();
                }
                break;
            case AnimalStates.Run:
                {
                    if (!CheckShouldScare())
                    {
                        float time = Time.time - startTime;

                        Vector3 dir = (endPos - startPos).normalized;
                        Vector3 tar = go.transform.position + dir;
                        transform.LookAt(tar);
                        transform.position = startPos + (dir * time * WalkSpeed);

                        if ((endPos - transform.position).magnitude < 1)
                        {
                            state = AnimalStates.Idle;
                            go.GetComponent<Animation>().CrossFade(standAnimName);
                        }
                    }
                }
                break;
            case AnimalStates.Scare:
                {
                    if (Time.time - scareStartTime > scareTime)
                        state = AnimalStates.Idle;
                }
                break;
        }
    }

    bool CheckShouldScare()
    {
        if(!hasScare)
            return false;

        if ((transform.position - cameraCtrl.mLookAt.transform.position).magnitude > ScareRadius)
        {
            scareStartTime = Time.time;
            go.GetComponent<Animation>().CrossFade(scareAnimName);
            go.GetComponent<Animation>().CrossFadeQueued(standAnimName);
            state = AnimalStates.Scare;
            return true;
        }       
        else
            return false;
    }

    void Think()
    {
        switch (state)
        {
            case AnimalStates.Idle:
                {
                    if (Random.Range(0, 99) < StandByRate)
                    {
                        int idx = Random.Range(0, standbyAnimNames.Length);
                        go.GetComponent<Animation>().CrossFade(standbyAnimNames[idx]);
                        go.GetComponent<Animation>().CrossFadeQueued(standAnimName);
                    }
                    else
                    {
                        if (Random.Range(0, 99) < WalkRate)
                        {
                            int delta = (Random.Range(0, 3) - 1);
                            currentIdx = currentIdx + delta;
                            if (currentIdx < 0)
                                currentIdx = 0;
                            if (currentIdx >= points.Count)
                                currentIdx = points.Count - 1;

                            Vector3 dst = points[currentIdx];
                            Quaternion rot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                            dst += (rot * Vector3.forward).normalized * Random.Range(0, PathWidth);
                            startPos = transform.position;
                            endPos = dst;
                            startTime = Time.time;
                            state = AnimalStates.Run;
                            go.GetComponent<Animation>().CrossFade(runAnimName);
                            //Vector3.forward
                        }
                    }
                }
                break;
        }

    }
}
