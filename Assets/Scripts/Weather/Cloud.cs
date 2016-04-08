using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cloud : MonoBehaviour {
    // Tuning variables
    public float max_opacity;
    public float fade_speed;
    //[ColorUsageAttribute(false,true,0f,8f,0.125f,3f)] public Color dayEmission;
    //[ColorUsageAttribute(false,true,0f,8f,0.125f,3f)] public Color nightEmission;

    // Internal variables
    //private Sky SkyScript;
    private Renderer rend;
    private bool dissipating;

    void Awake() {
        //SkyScript = GameObject.Find("Sky").GetComponent<Sky>();
        dissipating = false;
        rend = GetComponent<Renderer>();
        rend.material.color *= new Color(1, 1, 1, 0.0f);
    }

    public void dissipate(){
        dissipating = true;
    }
	
	// Update is called once per frame
	void Update () {
        handleOpacity();
        //updateEmission();
	}
    
    private void handleOpacity(){
        if (dissipating){
            if (rend.material.color.a - fade_speed*Globals.time_scale > 0) {
                rend.material.color -= new Color(0, 0, 0, fade_speed*Globals.time_scale);
            }else Destroy(gameObject);
        }else{
            if (rend.material.color.a < max_opacity) {
                if (rend.material.color.a + fade_speed*Globals.time_scale > max_opacity)
                    rend.material.color = new Color(rend.material.color.r, rend.material.color.b, rend.material.color.g, max_opacity);
                else rend.material.color += new Color(0, 0, 0, fade_speed*Globals.time_scale);
            }
        }
    }

    /*
    private void updateEmission(){ //doesn't work yet
		float ratio;
		if(Globals.timeOfDay > SkyScript.horizonBufferAngle && Globals.timeOfDay <= 180 - SkyScript.horizonBufferAngle)
            ratio = 0; //day
		else if(Globals.timeOfDay > 180 - SkyScript.horizonBufferAngle && Globals.timeOfDay <= 180 + SkyScript.horizonBufferAngle)
            ratio = (Globals.timeOfDay - (180 - SkyScript.horizonBufferAngle)) / (SkyScript.horizonBufferAngle * 2); //sunset
		else if(Globals.timeOfDay > 180 + SkyScript.horizonBufferAngle && Globals.timeOfDay <= 360 - SkyScript.horizonBufferAngle)
            ratio = 1; //night
		else if(Globals.timeOfDay <= 30)
            ratio = 0.5f - (Globals.timeOfDay - 0) / (SkyScript.horizonBufferAngle * 2); //above horizon
		else
            ratio = 1 - (Globals.timeOfDay - (360 - SkyScript.horizonBufferAngle)) / (SkyScript.horizonBufferAngle * 2); //below horizon
        rend.material.EnableKeyword("_EMISSION");
        rend.material.SetColor("_EmmisionColor", dayEmission * (1 - ratio) + nightEmission * ratio);
        //Debug.Log(rend.material.GetColor("_EmmisionColor"));
    }
    */
}
