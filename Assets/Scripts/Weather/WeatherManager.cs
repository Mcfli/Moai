﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeatherManager : MonoBehaviour {
    // Tuning variables
    public float cloudHeight = 300;
    public Vector2 cloudMovement;
    public float chanceOfCloudFade;
    public float particlesHeight = 500;
    public float updateTime; //amount of time weather lasts (may change to same weather)

    public List<GameObject> cloudPrefabs;
    public List<GameObject> darkCloudPrefabs;
    public float cloudPlacementRadius;
    public float cloudHeightVariation;
    public float cloudSizeVariation; //ratio (0.0 - 1.0)

    public float skyIntensityLerpSpeed;

    // Internal variables
    private float lastUpdated;
    private ParticleSystem activeParticleSystem;
    private ParticleSystem.Particle[] m_Particles;
    public float particleVel = 0.0f;
    private List<Cloud> clouds; // holds all currently loaded clouds
    private List<Cloud> darkClouds;
    private Biome lastBiome;
    private Vector3 curParticlePosition;
    private float skyIntensityMultipler = 1;

    // Player for audio source
    private AudioSource weatherAudioSource;
    private bool wasPlaying = false;
    public AudioClip rainAudio;
    public AudioClip Wind;

    void Awake(){
        clouds = new List<Cloud>();
        darkClouds = new List<Cloud>();
    }

    void Start() {
        weatherAudioSource = Camera.main.gameObject.AddComponent<AudioSource>();
        initializeWeather();
    }

    // Update is called once per frame
    void Update(){
        if(!Globals.cur_weather) return;

        //if(Globals.mode == -1) return;

        if (Globals.time > lastUpdated + updateTime * Globals.time_resolution) {
            changeWeather();
        }else if(lastBiome != Globals.cur_biome) {
            bool found = false;
            foreach(Weather w in Globals.cur_biome.weatherTypes) {
                if(w.name == Globals.cur_weather.name) {
                    found = true;
                    break;
                }
            }
            if(!found) changeWeather();
        }

        if(skyIntensityMultipler < Globals.cur_weather.skyIntensityMultiplier) {
            skyIntensityMultipler += skyIntensityLerpSpeed * Globals.deltaTime / Globals.time_resolution;
            if(skyIntensityMultipler > Globals.cur_weather.skyIntensityMultiplier)
                skyIntensityMultipler = Globals.cur_weather.skyIntensityMultiplier;
        } else if(skyIntensityMultipler > Globals.cur_weather.skyIntensityMultiplier) {
            skyIntensityMultipler -= skyIntensityLerpSpeed * Globals.deltaTime / Globals.time_resolution;
            if(skyIntensityMultipler < Globals.cur_weather.skyIntensityMultiplier)
                skyIntensityMultipler = Globals.cur_weather.skyIntensityMultiplier;
        } // else do nothing

        checkIfVisibleParticles();

        changeClouds(Mathf.FloorToInt(Globals.time_scale));
        moveClouds();
        if(Random.value < chanceOfCloudFade * Globals.time_scale && clouds.Count > 0) {
            int randomCloud = Random.Range(0, clouds.Count);
            clouds[randomCloud].dissipate();
            clouds.RemoveAt(randomCloud);
            createCloud(false);
        }
        if(Random.value < chanceOfCloudFade * Globals.time_scale && darkClouds.Count > 0) {
            int randomCloud = Random.Range(0, darkClouds.Count);
            darkClouds[randomCloud].dissipate();
            darkClouds.RemoveAt(randomCloud);
            createCloud(true);
        }

        lastBiome = Globals.cur_biome;
        ApplyImageSpace();
        if(activeParticleSystem)
        {
            InitializeIfNeeded();
            activeParticleSystem.gravityModifier = 0;
            int numParticlesAlive = activeParticleSystem.GetParticles(m_Particles);
            if(Time.timeScale == 0) activeParticleSystem.Pause();
            else if(activeParticleSystem.isPaused) activeParticleSystem.Play();
            else {
                for(int i = 0; i < numParticlesAlive; i++)
                    m_Particles[i].velocity = Vector3.forward * activeParticleSystem.startSpeed * Mathf.Pow(Globals.time_scale, 0.3f);
                activeParticleSystem.SetParticles(m_Particles, numParticlesAlive);
            }
        }

        if (Globals.time_scale > 1)
        {
            if(weatherAudioSource.isPlaying)
            {
                wasPlaying = true;
                weatherAudioSource.Stop();
            }
        }
        else
        {
            if(!weatherAudioSource.isPlaying && wasPlaying)
            {
                wasPlaying = false;
                weatherAudioSource.Play();
            }
        }
    }

    public float getSkyIntensityMultipler() {
        return skyIntensityMultipler;
    }

    public void InitializeIfNeeded()
    {
        if (m_Particles == null || m_Particles.Length < activeParticleSystem.maxParticles)
        {
            m_Particles = new ParticleSystem.Particle[activeParticleSystem.maxParticles];
        }
    }
	
    public void initializeWeather() {
        changeWeather();
        checkIfVisibleParticles();
        lastBiome = Globals.cur_biome;
        moveParticles(Globals.Player.transform.position);
    }
	
	//public void hideWeather(){visibleParticles = false;}
	//public void showWeather(){visibleParticles = true;}
	//public void toggleWeather(){visibleParticles = !visibleParticles;}
	//public bool isVisible(){return visibleParticles;}

    private bool checkIfVisibleParticles() { //returns true if visible
        bool visibleParticles = !(Globals.PlayerScript.isUnderwater());
        if(activeParticleSystem) activeParticleSystem.gameObject.SetActive(visibleParticles);
        return visibleParticles;
    }

    public void changeWeather(){
        if (Globals.cur_biome == null) return;
        Weather lastWeather = null;
        if(Globals.cur_weather) lastWeather = Globals.cur_weather;

        //choose weather
        float roll = 0;
        for(int i = 0; i < Globals.cur_biome.weatherChance.Count; i++) roll += Globals.cur_biome.weatherChance[i];
        roll *= Random.value;
        for(int i = 0; i < Globals.cur_biome.weatherChance.Count; i++){
            if(roll <= Globals.cur_biome.weatherChance[i]) {
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
                activeParticleSystem.transform.parent = transform;
                activeParticleSystem.transform.position = curParticlePosition;
                if (Globals.cur_weather.name == "Rain")
                {
                    weatherAudioSource.clip = rainAudio;
                    weatherAudioSource.loop = true;
                    weatherAudioSource.Play();
                } else {
                    weatherAudioSource.clip = Wind;
                    weatherAudioSource.loop = true;
                    weatherAudioSource.Play();
                }
            } else {
                weatherAudioSource.clip = Wind;
                weatherAudioSource.loop = true;
                weatherAudioSource.Play();
            }

        }
        lastUpdated = Globals.time;
    }

    private void ApplyImageSpace()
    {
        //if()
        Globals.cur_weather.imageSpace.applyToCamera();
    }

    private void changeClouds(int amount) {
        int target = (Globals.time_scale < Globals.SkyScript.timeScaleThatHaloAppears) ? Globals.cur_weather.numberOfClouds : 0;
        for(int i = 0; i < amount; i++) {
            if(Globals.cur_weather.darkClouds) {
                if(clouds.Count > 0) {
                    clouds[0].dissipate();
                    clouds.RemoveAt(0);
                }
                if(darkClouds.Count < target) createCloud(true); //i want more clouds
                else if(darkClouds.Count > target) { //i want less clouds
                    darkClouds[0].dissipate();
                    darkClouds.RemoveAt(0);
                } else break;
            } else {
                if(darkClouds.Count > 0) {
                    darkClouds[0].dissipate();
                    darkClouds.RemoveAt(0);
                }
                if(clouds.Count < target) createCloud(false); //i want more clouds
                else if(clouds.Count > target) { //i want less clouds
                    clouds[0].dissipate();
                    clouds.RemoveAt(0);
                } else break;
            }
        }
    }

    private Cloud createCloud(int prefabNum, Vector3 location, Vector3 rotation, float scale, bool dark) {
        GameObject c;
        if(dark) c = Instantiate(darkCloudPrefabs[prefabNum]);
        else c = Instantiate(cloudPrefabs[prefabNum]);
        c.transform.parent = transform;
        c.transform.eulerAngles = rotation;
        c.transform.localScale *= scale;
        c.transform.position = location;
        Cloud cl = c.GetComponent<Cloud>();
        if(dark) darkClouds.Add(cl);
        else clouds.Add(cl);
        return cl;
    }

    private Cloud createCloud(bool dark) { //random everything
        Vector2 radial_offset = Random.insideUnitCircle * cloudPlacementRadius;
        return createCloud(Random.Range(0, cloudPrefabs.Count),
                           new Vector3(radial_offset.x + Globals.Player.transform.position.x,
                                       Random.Range(-cloudHeightVariation, cloudHeightVariation) + cloudHeight,
                                       radial_offset.y + Globals.Player.transform.position.z),
                           new Vector3(0, Random.Range(0f, 360f), 0),
                           1 + Random.Range(-cloudSizeVariation, cloudSizeVariation),
                           dark
               );
    }

    private Cloud createCloud(Vector3 location, bool dark) { //random everything except location
        return createCloud(Random.Range(0, cloudPrefabs.Count),
                           location, new Vector3(0, Random.Range(0f, 360f), 0),
                           1 + Random.Range(-cloudSizeVariation, cloudSizeVariation),
                           dark
               );
    }

    private void moveClouds() {
        for(int i = 0; i < clouds.Count; i++) {
            clouds[i].gameObject.transform.position += new Vector3(cloudMovement.x, 0, cloudMovement.y) * Globals.time_scale;
            Vector2 cloudPos = new Vector2(clouds[i].gameObject.transform.position.x, clouds[i].gameObject.transform.position.z);
            Vector2 playerPos = new Vector2(Globals.Player.transform.position.x, Globals.Player.transform.position.z);
            if(Vector2.Distance(cloudPos, playerPos) > cloudPlacementRadius) {
                clouds[i].dissipate();
                clouds.RemoveAt(i);
                Vector2 npos = Vector2.Lerp(playerPos * 2 - cloudPos, playerPos, 0.05f);
                createCloud(new Vector3(npos.x, Random.Range(-cloudHeightVariation, cloudHeightVariation) + cloudHeight, npos.y), false);
                i--;
            }
        }
        for(int i = 0; i < darkClouds.Count; i++) {
            darkClouds[i].gameObject.transform.position += new Vector3(cloudMovement.x, 0, cloudMovement.y) * Globals.time_scale;
            Vector2 cloudPos = new Vector2(darkClouds[i].gameObject.transform.position.x, darkClouds[i].gameObject.transform.position.z);
            Vector2 playerPos = new Vector2(Globals.Player.transform.position.x, Globals.Player.transform.position.z);
            if(Vector2.Distance(cloudPos, playerPos) > cloudPlacementRadius) {
                darkClouds[i].dissipate();
                darkClouds.RemoveAt(i);
                Vector2 npos = Vector2.Lerp(playerPos * 2 - cloudPos, playerPos, 0.05f);
                createCloud(new Vector3(npos.x, Random.Range(-cloudHeightVariation, cloudHeightVariation) + cloudHeight, npos.y), true);
                i--;
            }
        }
    }

    // moves particle system right above player
    // called from GenerationManager, not called constantly (will not follow player directly)
    public void moveParticles(Vector3 chunkCenter){
        curParticlePosition = new Vector3(chunkCenter.x, particlesHeight, chunkCenter.z);
        if (activeParticleSystem != null)
            activeParticleSystem.transform.position = curParticlePosition;
    }

    public AudioSource getWeatherAudioSource() {
        return weatherAudioSource;
    }
}
