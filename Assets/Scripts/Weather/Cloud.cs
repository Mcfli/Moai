using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cloud : MonoBehaviour {
    // Access variables
    public float placement_radius;
    public float height_variation;
    public float height_base;
    [ColorUsageAttribute(false,true,0f,8f,0.125f,3f)] public Color dayEmission;
    [ColorUsageAttribute(false,true,0f,8f,0.125f,3f)] public Color nightEmission;

    // Tuning variables
    public float max_opacity;
    public float fade_speed;

    // Internal variables
    private float cur_opacity;
    public bool dissipating;
    private Renderer rend;
    private GameObject Player;
    private Sky SkyScript;
    private Vector3 pos_offset;

    // Use this for initialization
    void Start () {
        Player = GameObject.FindGameObjectWithTag("Player");
        SkyScript = GameObject.FindGameObjectWithTag("Sky").GetComponent<Sky>();
        float radial_offset = 2 * Random.value * placement_radius;
        float angle = 2*Random.value * Mathf.PI;
        transform.eulerAngles = new Vector3(0,Random.Range(0,360),0); //random rotation

        pos_offset = new Vector3(radial_offset*Mathf.Cos(angle),
            height_base+2*Random.value*height_variation-height_variation,
            radial_offset * Mathf.Sin(angle));
        cur_opacity = 0.0f;
        dissipating = false;
        rend = GetComponent<Renderer>();
        rend.material.color *= new Color (1,1,1,0.0f);
        //rend.material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        rend.material.EnableKeyword("_EMISSION");
	}

    public void dissipate(){
        dissipating = true;
    }
	
	// Update is called once per frame
	void Update () {
        handleOpacity();
        moveWithPlayer();
        updateEmission();
	}
    
    private void updateEmission(){ //doesn't work yet
		float ratio;
		if(SkyScript.getTimeOfDay() > SkyScript.horizonBufferAngle && SkyScript.getTimeOfDay() <= 180 - SkyScript.horizonBufferAngle)
            ratio = 0; //day
		else if(SkyScript.getTimeOfDay() > 180 - SkyScript.horizonBufferAngle && SkyScript.getTimeOfDay() <= 180 + SkyScript.horizonBufferAngle)
            ratio = (SkyScript.getTimeOfDay() - (180 - SkyScript.horizonBufferAngle)) / (SkyScript.horizonBufferAngle * 2); //sunset
		else if(SkyScript.getTimeOfDay() > 180 + SkyScript.horizonBufferAngle && SkyScript.getTimeOfDay() <= 360 - SkyScript.horizonBufferAngle)
            ratio = 1; //night
		else if(SkyScript.getTimeOfDay() <= 30)
            ratio = 0.5f - (SkyScript.getTimeOfDay() - 0) / (SkyScript.horizonBufferAngle * 2); //above horizon
		else
            ratio = 1 - (SkyScript.getTimeOfDay() - (360 - SkyScript.horizonBufferAngle)) / (SkyScript.horizonBufferAngle * 2); //below horizon
        rend.material.SetColor("_EmmisionColor", dayEmission * (1 - ratio) + nightEmission * ratio);
    }
    
    private void handleOpacity()
    {
        if (!dissipating)
        {
            if (cur_opacity < max_opacity)
            {
                cur_opacity += fade_speed;
                if (cur_opacity > max_opacity) cur_opacity = max_opacity;
                rend.material.color += new Color(0, 0, 0, fade_speed);
            }
        }
        else
        {
            if (cur_opacity > 0)
            {
                cur_opacity -= fade_speed;
                rend.material.color -= new Color(0, 0, 0, fade_speed);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
    
    private void moveWithPlayer()
    {
        transform.position = Player.transform.position + pos_offset;
    }
}
