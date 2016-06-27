using UnityEngine;
public class BigTerrainXSetting : MonoBehaviour
{
    public Vector3 selfPos;
    public Vector3 selfAngle;
    public bool save = false;
    public bool load = false;
    public void SaveSettings()
    {
        selfPos = this.transform.position;
        selfAngle = this.transform.localEulerAngles;
    }
    public void LoadSettings()
    {
        this.transform.position = selfPos;
        this.transform.localEulerAngles = selfAngle;
    }
    void Start()
    {

    }
    void Update()
    {
        if (save)
        {
            SaveSettings();
            save = false;
        }
        if (load)
        {
            LoadSettings();
            load = false;
        }
    }

}