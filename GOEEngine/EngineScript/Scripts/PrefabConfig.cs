using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[System.Reflection.Obfuscation(Exclude = true)]
public class PrefabConfig : MonoBehaviour {

    public string PrefabName = string.Empty;
	void Awake()
    {
        if (PrefabName == string.Empty)
            PrefabName = gameObject.name;
    }
}
