using UnityEngine;
using System.Collections;

public class Water : MonoBehaviour {

    //This script enables underwater effects. Attach to main camera.

    //Define variable
    public Color fogColor;

    //The scene's default fog settings
    private bool defaultFog;
    private Color defaultFogColor;
    private float defaultFogDensity;
    //private Material defaultSkybox;
    //private Material noSkybox;

    void Start()
    {
        //Set the background color

        //The scene's default fog settings
        defaultFog = RenderSettings.fog;
        defaultFogColor = RenderSettings.fogColor;
        defaultFogDensity = RenderSettings.fogDensity;
        
        Camera.main.backgroundColor = new Color(0, 0.4f, 0.7f, 1);
    }

    void Update(){
        Vector3 new_pos = new Vector3(Globals.Player.transform.position.x,Globals.water_level,Globals.Player.transform.position.z);
        transform.position = new_pos;
        if (Globals.Player.transform.position.y+1.5 < transform.position.y){
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = 0.04f;
            GameObject.Find("Weather").GetComponent<WeatherManager>().hideWeather();
        }else{
            RenderSettings.fog = defaultFog;
            RenderSettings.fogColor = defaultFogColor;
            RenderSettings.fogDensity = defaultFogDensity;
            GameObject.Find("Weather").GetComponent<WeatherManager>().showWeather();
        }
    }
}
