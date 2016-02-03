using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeatherManager : MonoBehaviour {

    // Tuning variables
    public float update_time;
    public ParticleSystem[] particle_systems;

    // Internal variables
    private List<GameObject> clouds;        // holds all currently loaded clouds
    private int target_clouds;
    private float last_updated;

    void Awake()
    {
        last_updated = Time.time;
        clouds = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Globals.cur_weather == "rainy")
            doRain();
        else if (Globals.cur_weather == "sunny")
            doSunny();
        else
            doSnowy();
    }

    private void doRain()
    {
        target_clouds = 20;
        // PLACEHOLDER Fade skybox to dark skybox
        if (clouds.Count<target_clouds && Time.time > last_updated + update_time)
        {
            // Instantiate cloud prefab and add to cloud array
        }
    }


    private void doSunny()
    {
        target_clouds = 0;
        // PLACEHOLDER Fade skybox to dark skybox
        if (clouds.Count > target_clouds && Time.time > last_updated + update_time)
        {
            GameObject cloud = clouds[0];
            // cloud.dissipate()
            clouds.RemoveAt(0);
        }
    }


    private void doSnowy()
    {
        target_clouds = 20;
        if (clouds.Count < target_clouds && Time.time > last_updated + update_time)
        {
            // Instantiate cloud prefab and add to cloud array
        }
    }
}
