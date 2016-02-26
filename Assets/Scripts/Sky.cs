using UnityEngine;
using System.Collections;

public class Sky : MonoBehaviour {
	//attach to worldgen
	public float timeresPerDegree = 10;
	
	public GameObject SunLight;
	public float sunMaxIntensity = 1f;
	public GameObject MoonLight; //moon should be attached to sun
	public float moonMaxIntensity = 0.5f;
	public float horizonBufferAngle = 0f;
	
	//G stands for Goal
	private Color color1G;
	private Color color2G;
	private Color color3G;
	private float exponent1G;
	private float exponent2G;
	private float intensityG;
	
	//D stands for Delta
	private Color color1D;
	private Color color2D;
	private Color color3D;
	private float exponent1D;
	private float exponent2D;
	private float intensityD;
	
	private GameObject Player;
	private Vector3 originalSunAngle;
	private float timeRemain;
	private float timeOfDay; //0 for dawn, 90 for noon, 180 for dusk, 270 for midnight. Should not be or exceed 360.

    void Start(){
		Player = GameObject.FindGameObjectWithTag("Player");
		//timeOfDay = Mathf.Repeat(Globals.time/Globals.time_resolution/timeresPerDegree + originalSunAngle.x, 360);
		changeSky(new Color(0.22f, 0.6f, 1.0f, 1.0f), new Color(0.8f, 0.592f, 0.961f, 1.0f), new Color(0.867f, 0.753f, 0.659f, 1.0f), 1.0f, 1.0f, 1.0f, 0); //default
		originalSunAngle = SunLight.transform.eulerAngles;
    }

    void Update(){
		timeOfDay = Mathf.Repeat(Globals.time/Globals.time_resolution/timeresPerDegree + originalSunAngle.x, 360);
		updateSunMoon();
		updateSky();
    }
	
	void updateSunMoon(){
		SunLight.transform.eulerAngles = new Vector3(timeOfDay, originalSunAngle.y, originalSunAngle.z); //update angle of sun/moon with timeofday
		SunLight.transform.position = new Vector3(Player.transform.position.x, 0, Player.transform.position.z); //follow player
		
		// Modulate Sun intensity
		if(timeOfDay > horizonBufferAngle && timeOfDay <= 180 - horizonBufferAngle){ //day
			SunLight.GetComponent<Light>().intensity = sunMaxIntensity;
			MoonLight.GetComponent<Light>().intensity = 0.0f;
		}else if(timeOfDay > 180 - horizonBufferAngle && timeOfDay <= 180 + horizonBufferAngle){ //sunset
			SunLight.GetComponent<Light>().intensity = sunMaxIntensity * (1.0f - (timeOfDay - (180 - horizonBufferAngle)) / (horizonBufferAngle * 2));
			MoonLight.GetComponent<Light>().intensity = moonMaxIntensity * ((timeOfDay - (180 - horizonBufferAngle)) / (horizonBufferAngle * 2));
		}else if(timeOfDay > 180 + horizonBufferAngle && timeOfDay <= 360 - horizonBufferAngle){ //night
			SunLight.GetComponent<Light>().intensity = 0.0f;
			MoonLight.GetComponent<Light>().intensity = moonMaxIntensity;
		}else if(timeOfDay < 30){ //sunrise (above horizon)
			SunLight.GetComponent<Light>().intensity = sunMaxIntensity * (0.5f + (timeOfDay / (horizonBufferAngle * 2)));
			MoonLight.GetComponent<Light>().intensity = moonMaxIntensity * (0.5f - (timeOfDay / (horizonBufferAngle * 2)));
		}else{ // sunrise (below horizon)
			SunLight.GetComponent<Light>().intensity = sunMaxIntensity * ((timeOfDay - (360 - horizonBufferAngle)) / (horizonBufferAngle * 2));
			MoonLight.GetComponent<Light>().intensity = moonMaxIntensity * (1.0f - (timeOfDay - (360 - horizonBufferAngle)) / (horizonBufferAngle * 2));
		}
	}
	
	void updateSky(){
		if(timeRemain == 0) return;
		
		RenderSettings.skybox.SetColor("_Color1", RenderSettings.skybox.GetColor("_Color1") + color1D * Globals.time_scale);
		RenderSettings.skybox.SetColor("_Color2", RenderSettings.skybox.GetColor("_Color2") + color2D * Globals.time_scale);
		RenderSettings.skybox.SetColor("_Color3", RenderSettings.skybox.GetColor("_Color3") + color3D * Globals.time_scale);
		RenderSettings.skybox.SetFloat("_Exponent1", RenderSettings.skybox.GetFloat("_Exponent1") + exponent1D * Globals.time_scale);
		RenderSettings.skybox.SetFloat("_Exponent2", RenderSettings.skybox.GetFloat("_Exponent2") + exponent2D * Globals.time_scale);
		RenderSettings.skybox.SetFloat("_Intensity", RenderSettings.skybox.GetFloat("_Intensity") + intensityD * Globals.time_scale);
		timeRemain -=  Globals.time_scale;
		
		if(timeRemain <= 0){ //catch overshooting
			RenderSettings.skybox.SetColor("_Color1", color1G);
			RenderSettings.skybox.SetColor("_Color2", color2G);
			RenderSettings.skybox.SetColor("_Color3", color3G);
			RenderSettings.skybox.SetFloat("_Exponent1", exponent1G);
			RenderSettings.skybox.SetFloat("_Exponent2", exponent2G);
			RenderSettings.skybox.SetFloat("_Intensity", intensityG);
			timeRemain = 0;
		}
	}
	
	//"time" is number of degrees it takes to finish changing to new sky
	//if time is 0, change sky immediately; time should not be less than 0
	void changeSky(Color color1, Color color2, Color color3, float exponent1, float exponent2, float intensity, float time){
		color1G = color1;
		color2G = color2;
		color3G = color3;
		exponent1G = exponent1;
		exponent2G = exponent2;
		intensityG = intensity;
		
		if(time == 0){
			RenderSettings.skybox.SetColor("_Color1", color1G);
			RenderSettings.skybox.SetColor("_Color2", color2G);
			RenderSettings.skybox.SetColor("_Color3", color3G);
			RenderSettings.skybox.SetFloat("_Exponent1", exponent1G);
			RenderSettings.skybox.SetFloat("_Exponent2", exponent2G);
			RenderSettings.skybox.SetFloat("_Intensity", intensityG);
			timeRemain = time;
			return;
		}
		
		color1D = (color1 - RenderSettings.skybox.GetColor("_Color1")) / (time * timeresPerDegree);
		color2D = (color2 - RenderSettings.skybox.GetColor("_Color2")) / (time * timeresPerDegree);
		color3D = (color3 - RenderSettings.skybox.GetColor("_Color3")) / (time * timeresPerDegree);
		exponent1D = (exponent1 - RenderSettings.skybox.GetFloat("_Exponent1")) / (time * timeresPerDegree);
		exponent2D = (exponent2 - RenderSettings.skybox.GetFloat("_Exponent2")) / (time * timeresPerDegree);
		intensityD = (intensity - RenderSettings.skybox.GetFloat("_Intensity")) / (time * timeresPerDegree);
		timeRemain = time*timeresPerDegree;
	}
}

//time of day, seasons, weather, biome