using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeatherManager : MonoBehaviour {
    public string cur_weather;      // String holding the current weather state
    List<GameObject> Clouds;        // holds all currently loaded clouds

    private int target_clouds;

    //List<ParticleSystem> Precipitation - holds all currently loaded particle systems

    // Update is called once per frame
    void Update()
    {
        if (cur_weather == "rainy")
            doRain();
        else if (cur_weather == "sunny")
            doSunny();
        else
            doSnowy();
    }

    private void doRain()
    {
        target_clouds = 20;
    }


    private void doSunny()
    {

    }


    private void doSnowy()
    {

    }

	
	
}
