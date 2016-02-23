using UnityEngine;
using System.Collections;

public class Sky : MonoBehaviour {
	//G stands for Goal
	private float atmThickG;
	private Color tintG;
	private Color groundG;
	private float expoG;
	
	//D stands for Delta
	private float atmThickD;
	private Color tintD;
	private Color groundD;
	private float expoD;
	
	private float timeRemain;

    void Start(){
		changeSky(1.0f, new Color(0.5f, 0.5f, 0.5f, 1.0f), new Color(0.369f, 0.349f, 0.341f, 1.0f), 1.3f, 0); //default
		changeSky(1.0f, new Color(1.0f, 0.0f, 0.0f, 1.0f), new Color(0.369f, 0.349f, 0.341f, 1.0f), 1.3f, 10000); //red sky
    }

    void Update(){
		if(timeRemain == 0) return;
		
		RenderSettings.skybox.SetFloat("_AtmosphereThickness", RenderSettings.skybox.GetFloat("_AtmosphereThickness") + atmThickD * Globals.time_scale);
		RenderSettings.skybox.SetColor("_SkyTint", RenderSettings.skybox.GetColor("_SkyTint") + tintD * Globals.time_scale);
		RenderSettings.skybox.SetColor("_GroundColor", RenderSettings.skybox.GetColor("_GroundColor") + groundD * Globals.time_scale);
		RenderSettings.skybox.SetFloat("_Exposure", RenderSettings.skybox.GetFloat("_Exposure") + expoD * Globals.time_scale);
		timeRemain -=  Globals.time_scale;
		
		if(timeRemain <= 0){ //catch overshooting
			RenderSettings.skybox.SetFloat("_AtmosphereThickness", atmThickG);
			RenderSettings.skybox.SetColor("_SkyTint", tintG);
			RenderSettings.skybox.SetColor("_GroundColor", groundG);
			RenderSettings.skybox.SetFloat("_Exposure", expoG);
			timeRemain = 0;
		}
    }
	
	//"time" is number of "time_resolution"s it takes to finish changing to new sky
	//if time is 0, change sky immediately; time should not be less than 0
	void changeSky(float atmThick, Color tint, Color ground, float expo, float time){
		atmThickG = atmThick;
		tintG = tint;
		groundG = ground;
		expoG = expo;
		
		if(time == 0){
			RenderSettings.skybox.SetFloat("_AtmosphereThickness", atmThickG);
			RenderSettings.skybox.SetColor("_SkyTint", tintG);
			RenderSettings.skybox.SetColor("_GroundColor", groundG);
			RenderSettings.skybox.SetFloat("_Exposure", expoG);
			timeRemain = time;
			return;
		}
		
		atmThickD = (atmThick - RenderSettings.skybox.GetFloat("_AtmosphereThickness"))/time;
		tintD = (tint - RenderSettings.skybox.GetColor("_SkyTint"))/time;
		groundD = (ground - RenderSettings.skybox.GetColor("_GroundColor"))/time;
		expoD = (expo - RenderSettings.skybox.GetFloat("_Exposure"))/time;
		timeRemain = time;
	}
}
