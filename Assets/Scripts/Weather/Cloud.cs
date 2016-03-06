using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cloud : MonoBehaviour {
    // Access variables
    public float placement_radius;
    public float height_variation;
    public float size_variation; //ratio (0.0 - 1.0)
    public float height_base;

    // Tuning variables
    public float max_opacity;
    public float fade_speed;
    [ColorUsageAttribute(false,true,0f,8f,0.125f,3f)] public Color dayEmission;
    [ColorUsageAttribute(false,true,0f,8f,0.125f,3f)] public Color nightEmission;

    // Internal variables
    private bool dissipating;
    private Renderer rend;
    private GameObject Player;
    private Sky SkyScript;
    private Vector3 pos_offset;

    // Use this for initialization
    void Start () {
        Player = GameObject.FindGameObjectWithTag("Player");
        SkyScript = GameObject.Find("Sky").GetComponent<Sky>();
        transform.eulerAngles = new Vector3(0,Random.Range(0,360),0); //random rotation
        transform.localScale *= 1 + Random.Range(-size_variation, size_variation); //random size

        Vector2 radial_offset = Random.insideUnitCircle * placement_radius;
        pos_offset = new Vector3(radial_offset.x, height_base + Random.Range(-height_variation,height_variation), radial_offset.y);
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
        //updateEmission();
	}
    
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
    
    private void handleOpacity(){
        if (dissipating){
            if (rend.material.color.a - fade_speed > 0) {
                rend.material.color -= new Color(0, 0, 0, fade_speed);
            }else Destroy(gameObject);
        }else{
            if (rend.material.color.a < max_opacity) {
                if (rend.material.color.a + fade_speed > max_opacity)
                    rend.material.color = new Color(rend.material.color.r, rend.material.color.b, rend.material.color.g, max_opacity);
                else rend.material.color += new Color(0, 0, 0, fade_speed);
            }
        }
    }
    
    private void moveWithPlayer(){
        transform.position = Player.transform.position + pos_offset;
    }
}
