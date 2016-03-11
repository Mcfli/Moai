using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeatherManager : MonoBehaviour {

    // Tuning variables
    public float height = 300;
    public float updateTime; //amount of time weather lasts (may change to same weather)

    public List<GameObject> cloudPrefabs;
    public float cloudPlacementRadius;
    public float cloudHeightVariation;
    public float cloudSizeVariation; //ratio (0.0 - 1.0)

    // Internal variables
    private float lastUpdated;
	private bool visibleParticles;
    private ParticleSystem activeParticleSystem;
    private List<GameObject> clouds; // holds all currently loaded clouds
    private Biome lastBiome;

    private Vector3 curParticlePosition;

    void Awake(){
        clouds = new List<GameObject>();
    }

    void Start() {
        visibleParticles = true;
        lastUpdated = 0;
        changeWeather();
        lastBiome = Globals.cur_biome;
        curParticlePosition = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update(){
        if (Globals.time > lastUpdated + updateTime * Globals.time_resolution || lastBiome != Globals.cur_biome) {
            lastUpdated = Globals.time;
            changeWeather();
        }

        if (Globals.time_scale > 1) hideWeather();
        else showWeather();

        //check if visiable
        if (activeParticleSystem) activeParticleSystem.gameObject.SetActive(visibleParticles);
        //move with player
        transform.position = new Vector3(Globals.Player.transform.position.x, height, Globals.Player.transform.position.z);
        lastBiome = Globals.cur_biome;
    }
	
	public void hideWeather(){visibleParticles = false;}
	public void showWeather(){visibleParticles = true;}
	public void toggleWeather(){visibleParticles = !visibleParticles;}
	public bool isVisible(){return visibleParticles;}

    public void changeWeather(){
        if (Globals.cur_biome == null) return;
        Weather lastWeather = null;
        if(Globals.cur_weather) lastWeather = Globals.cur_weather;

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
        if (lastWeather != Globals.cur_weather) {
            if (activeParticleSystem) {
                Destroy(activeParticleSystem.gameObject);
                activeParticleSystem = null;
            }
            if (Globals.cur_weather.particleS) {
                activeParticleSystem = Instantiate(Globals.cur_weather.particleS);
                activeParticleSystem.transform.localPosition = curParticlePosition;
            }
            Globals.cur_weather.imageSpace.applyToCamera();
            changeClouds(Globals.cur_weather.numberOfClouds);
        }
    }
    
    private void changeClouds(int target_clouds){
        while(clouds.Count < target_clouds){ //i want more clouds
            GameObject c = Instantiate(cloudPrefabs[Mathf.FloorToInt(Random.value*(cloudPrefabs.Count-1))]);
            c.transform.parent = transform;
            c.transform.localEulerAngles = new Vector3(0, Random.Range(0f, 360f), 0); //random rotation
            c.transform.localScale *= 1 + Random.Range(-cloudSizeVariation, cloudSizeVariation); //random size
            Vector2 radial_offset = Random.insideUnitCircle * cloudPlacementRadius;
            c.transform.localPosition = new Vector3(radial_offset.x, Random.Range(-cloudHeightVariation, cloudHeightVariation), radial_offset.y);
            clouds.Add(c);
        }
        
        while(clouds.Count > target_clouds) { //i want less clouds
            GameObject c = clouds[0];
            c.GetComponent<Cloud>().dissipate();
            clouds.RemoveAt(0);
        }
    }

    // moves particle system right above player
    // called from GenerationManager, not called constantly (will not follow player directly)
    public void moveParticles(Vector3 chunkCenter){
        curParticlePosition = new Vector3(chunkCenter.x, height, chunkCenter.z);
        if (activeParticleSystem != null)
            activeParticleSystem.transform.position = curParticlePosition;
    }
}
