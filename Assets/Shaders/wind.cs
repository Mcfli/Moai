using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class wind : MonoBehaviour {
    List<Material> animatedMaterials;

    void Start() {
        animatedMaterials = new List<Material>();
        Object[] allMaterials = Resources.LoadAll("Materials", typeof(Material));
        for(int i = 0; i < allMaterials.Length; i++) {
            Material m = (Material)allMaterials[i];
            if(m.HasProperty("_Animate"))
                if(m.GetFloat("_Animate") == 1)
                    animatedMaterials.Add(m);
        }

        foreach(Material m in animatedMaterials)
            m.SetFloat("_TimeVar", 0);
    }

    void Update() {
        foreach(Material m in animatedMaterials)
            m.SetFloat("_TimeVar", Globals.time / Globals.time_resolution);
    }
}
