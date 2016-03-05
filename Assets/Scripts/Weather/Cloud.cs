using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cloud : MonoBehaviour {
    // Access variables
    public float placement_radius;
    public float height_variation;
    public float size_variation; //ratio (0.0 - 1.0)
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
        SkyScript = GameObject.Find("Sky").GetComponent<Sky>();
        Vector2 radial_offset = Random.insideUnitCircle * placement_radius;
        transform.eulerAngles = new Vector3(0,Random.Range(0,360),0); //random rotation
        transform.localScale *= 1 + Random.Range(-size_variation, size_variation); //random size

        pos_offset = new Vector3(radial_offset.x, height_base + Random.Range(-height_variation,height_variation), radial_offset.y);
        cur_opacity = 0.0f;
        dissipating = false;
        rend = GetComponent<Renderer>();
        rend.material.color *= new Color (1,1,1,0.0f);
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
        rend.material.EnableKeyword("_EMISSION");
        rend.material.SetColor("_EmmisionColor", dayEmission * (1 - ratio) + nightEmission * ratio);
        //Debug.Log(rend.material.GetColor("_EmmisionColor"));
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
