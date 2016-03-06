using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeatherManager : MonoBehaviour {

    // Tuning variables
    public float height = 300;
    public float updateTime; //amount of time weather lasts (may change to same weather)

    //public float particleSystemHeight;

    public List<GameObject> cloudPrefabs;
    //public float cloudBaseHeight;
    public float cloudPlacementRadius;
    public float cloudHeightVariation;
    public float cloudSizeVariation; //ratio (0.0 - 1.0)

    // Internal variables
    private float lastUpdated;
	private bool visibleParticles;
    private ParticleSystem activeParticleSystem;
    private List<GameObject> clouds; // holds all currently loaded clouds

    void Awake(){
        clouds = new List<GameObject>();
    }

    void Start() {
        visibleParticles = true;
        lastUpdated = 0;
    }

    // Update is called once per frame
    void Update(){
        if (Globals.time > lastUpdated + updateTime * Globals.time_resolution) {
            lastUpdated += updateTime * Globals.time_resolution;
            changeWeather();
        }

        //check if visiable
        if (activeParticleSystem) activeParticleSystem.gameObject.SetActive(visibleParticles);

        //move with player
        transform.position = Globals.Player.transform.position + new Vector3(0, height, 0);
    }
	
	public void hideWeather(){visibleParticles = false;}
	public void showWeather(){visibleParticles = true;}
	public void toggleWeather(){visibleParticles = !visibleParticles;}
	public bool isVisible(){return visibleParticles;}

    public void changeWeather(){
        if (Globals.cur_biome == null) return;

        Debug.Log("Changing Weather");

        //choose weather
        float roll = 0;
        for(int i = 0; i < Globals.cur_biome.weatherChance.Count; i++) roll += Globals.cur_biome.weatherChance[i];
        roll *= Random.value;
        for(int i = 0; i < Globals.cur_biome.weatherChance.Count; i++){
            if(roll - Globals.cur_biome.weatherChance[i] < 0) {
                Globals.cur_weather = Globals.cur_biome.weatherTypes[i];
                break;
            }else roll -= Globals.cur_biome.weatherChance[i];
        }

        // Switch to weather
        if (activeParticleSystem) {
            Destroy(activeParticleSystem.gameObject);
            activeParticleSystem = null;
        }
        if (Globals.cur_weather.particleS) {
            activeParticleSystem = Instantiate(Globals.cur_weather.particleS);
            activeParticleSystem.transform.parent = transform;
        }
        Globals.cur_weather.imageSpace.applyToCamera();
        changeClouds(Globals.cur_weather.numberOfClouds);
    }
    
    private void changeClouds(int target_clouds){
        while(clouds.Count < target_clouds){ //i want more clouds
            GameObject c = Instantiate(cloudPrefabs[Mathf.FloorToInt(Random.value*(cloudPrefabs.Count-1))]);
            //transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0); //random rotation
            //transform.localScale *= 1 + Random.Range(-size_variation, size_variation); //random size
            clouds.Add(c);
        }
        
        while(clouds.Count > target_clouds) { //i want less clouds
            GameObject c = clouds[0];
            c.GetComponent<Cloud>().dissipate();
            clouds.RemoveAt(0);
        }
    }
}
