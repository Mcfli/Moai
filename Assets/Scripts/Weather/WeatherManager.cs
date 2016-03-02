using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeatherManager : MonoBehaviour {

    // Tuning variables
    public float update_time;
    public ParticleSystem[] particle_prefabs;
    // 0 - rain
    // 1 - snow
    // 2 - mist
    // 3 - sand
    // 4 - ash
    public ImageSpace[] image_spaces;
    // 0 - clear day
    // 1 - rainy day

    public GameObject cloud;

    // Internal variables
    private List<GameObject> clouds;        // holds all currently loaded clouds
    private int target_clouds;
    private float last_updated;
    private ParticleSystem rain;
    private ParticleSystem snow;
    private GameObject player;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        last_updated = Time.time;
        clouds = new List<GameObject>();
        Globals.cur_weather = "sunny";

        rain = Instantiate(particle_prefabs[0]);
        snow = Instantiate(particle_prefabs[1]);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > last_updated + update_time)
        {
            last_updated = Time.time;
            updateWeather();
        }

        // Handle weather
        if (Globals.cur_weather == "rain")
            doRain();
        else if (Globals.cur_weather == "sunny")
            doSunny();
        else
            doSnowy();
    }

    public void updateWeather()
    {
        // Switch weather
        if (Globals.cur_weather == null)
        {
            Globals.cur_weather = "sunny";
            return;
        }
        Globals.cur_weather = Globals.cur_biome.weatherTypes[Random.Range(0, (Globals.cur_biome.weatherTypes.Count - 1))];
        
    }

    private void doRain()
    {
        rain.gameObject.SetActive(true);
        snow.gameObject.SetActive(false);

        target_clouds = 40;
        image_spaces[1].applyToCamera();
        if (clouds.Count<target_clouds)
        {


            GameObject c = Instantiate(cloud);
            clouds.Add(c);
           // last_updated = Time.time;
        }
    }


    private void doSunny()
    {
        rain.gameObject.SetActive(false);
        snow.gameObject.SetActive(false);

        target_clouds = 0;
        image_spaces[0].applyToCamera();
        // PLACEHOLDER Fade skybox to dark skybox
        if (clouds.Count > target_clouds)
        {
            GameObject cloud = clouds[0];
            cloud.GetComponent<Cloud>().dissipate();
            clouds.RemoveAt(0);
            //last_updated = Time.time;
        }
    }

    private void doSnowy()
    {
        rain.gameObject.SetActive(false);
        snow.gameObject.SetActive(true);
        image_spaces[2].applyToCamera();

        target_clouds = 40;
        if (clouds.Count < target_clouds)
        {
            // Instantiate cloud prefab and add to cloud array
            //last_updated = Time.time;
            GameObject c = Instantiate(cloud);
            clouds.Add(c);
        }
    }

    public void moveWithPlayer()
    {
        rain.transform.position = player.transform.position + new Vector3(0, 300, 0);
        snow.transform.position = player.transform.position + new Vector3(0, 300, 0);
    }
}
