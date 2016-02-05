using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeatherManager : MonoBehaviour {

    // Tuning variables
    public float update_time;
    public ParticleSystem[] particle_systems;
    // 0 - rain
    // 1 - snow
    // 2 - mist
    // 3 - sand
    // 4 - ash
    public ImageSpace[] image_spaces;
    // 0 - clear day
    // 0 - rainy day


    public GameObject cloud;

    // Internal variables
    private List<GameObject> clouds;        // holds all currently loaded clouds
    private int target_clouds;
    private float last_updated;

    void Awake()
    {
        last_updated = Time.time;
        clouds = new List<GameObject>();
        Globals.cur_weather = "sunny";
    }

    // Update is called once per frame
    void Update()
    {
        // Switch weather
        if (Time.time > last_updated + update_time)
        {
            last_updated = Time.time;
            if (Globals.cur_weather == "rainy")
                Globals.cur_weather = "sunny";
            else if (Globals.cur_weather == "rainy")
                Globals.cur_weather = "snowy";
            else
                Globals.cur_weather = "rainy";
        }


        // Handle weather
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
        image_spaces[1].applyToCamera();
        if (clouds.Count<target_clouds && Time.time > last_updated + update_time)
        {
            // Instantiate cloud prefab and add to cloud array
            Instantiate(cloud);
            last_updated = Time.time;
        }
    }


    private void doSunny()
    {
        target_clouds = 0;
        image_spaces[0].applyToCamera();
        // PLACEHOLDER Fade skybox to dark skybox
        if (clouds.Count > target_clouds && Time.time > last_updated + update_time)
        {
            GameObject cloud = clouds[0];
            cloud.GetComponent<Cloud>().dissipate();
            clouds.RemoveAt(0);
            last_updated = Time.time;
        }
    }


    private void doSnowy()
    {
        target_clouds = 20;
        if (clouds.Count < target_clouds && Time.time > last_updated + update_time)
        {
            // Instantiate cloud prefab and add to cloud array
            last_updated = Time.time;
        }
    }
}
