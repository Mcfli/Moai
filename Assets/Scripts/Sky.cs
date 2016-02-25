using UnityEngine;
using System.Collections;

public class Sky : MonoBehaviour {
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
	
	private float timeRemain;

    void Start(){
		changeSky(new Color(0.22f, 0.6f, 1.0f, 1.0f), new Color(0.8f, 0.592f, 0.961f, 1.0f), new Color(0.867f, 0.753f, 0.659f, 1.0f), 1.0f, 1.0f, 1.0f, 0); //default
    }

    void Update(){
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
	
	//"time" is number of frames it takes to finish changing to new sky
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
		
		color1D = (color1 - RenderSettings.skybox.GetColor("_Color1"))/time;
		color2D = (color2 - RenderSettings.skybox.GetColor("_Color2"))/time;
		color3D = (color3 - RenderSettings.skybox.GetColor("_Color3"))/time;
		exponent1D = (exponent1 - RenderSettings.skybox.GetFloat("_Exponent1"))/time;
		exponent2D = (exponent2 - RenderSettings.skybox.GetFloat("_Exponent2"))/time;
		intensityD = (intensity - RenderSettings.skybox.GetFloat("_Intensity"))/time;
		timeRemain = time;
	}
}

//time of day, seasons, weather, biome