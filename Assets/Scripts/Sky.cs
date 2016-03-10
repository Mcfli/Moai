using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sky : MonoBehaviour {
    //attach to sky
    public float timePerDegree = 100; //x axis
    public float initialTimeOfDay = 180; //noon
    public GameObject DayNightSpin;
    public GameObject SunLight;
    public GameObject MoonLight;
    public GameObject Halo;
    public GameObject StarsParent;
    public GameObject starPrefab;
    public float horizonBufferAngle = 30;
    public float sunAxisShift = 20;
    public float daysPerYear = 100; // z axis
    public float timeScaleThatHaloAppears = 1000;
    public int skyDuringHalo = 3; //0 for midnight, 3 for noon, etc

    //finals
    private GameObject Player;
    private Vector3 originalSkyAngle;
    private float sunMaxIntensity; //will take intensity of light set in editor
    private float moonMaxIntensity; //ditto above
    private float starAlpha;

    private SkyGradient skyGoal;
    private SkyGradient skyDelta;
    private float timeRemain;
    private int numOfStars;

    void Awake() { //set finals
        Player = GameObject.FindGameObjectWithTag("Player");
        originalSkyAngle = transform.eulerAngles;
        sunMaxIntensity = SunLight.GetComponent<Light>().intensity;
        moonMaxIntensity = MoonLight.GetComponent<Light>().intensity;
        starPrefab = GameObject.Instantiate(starPrefab);
        starPrefab.GetComponent<Renderer>().sharedMaterial = Material.Instantiate(starPrefab.GetComponent<Renderer>().sharedMaterial);
        starAlpha = starPrefab.GetComponent<Renderer>().sharedMaterial.GetColor("_Color").a;
        RenderSettings.skybox = Object.Instantiate(RenderSettings.skybox); //comment out when debugging
    }

    void Start() {
        Halo.SetActive(false);
        numOfStars = 0;
        addStar(); addStar(); addStar(); addStar(); addStar(); addStar(); //temp
    }

    void Update() {
        Globals.timeOfDay = Mathf.Repeat(Globals.time / Globals.time_resolution / timePerDegree + initialTimeOfDay, 360);
        updateTransforms();
        if (Globals.time_scale < timeScaleThatHaloAppears) { //normal day/night cycle
            DayNightSpin.SetActive(true);
            StarsParent.SetActive(true);
            Halo.SetActive(false);
            updateIntensity();
            updateSky();
            updateStarColor();
            //updateSkyByTime();
        } else { //do crazy dilation thing
            DayNightSpin.SetActive(false);
            StarsParent.SetActive(false);
            Halo.SetActive(true);
            setSky(getSkyGradient(Globals.cur_biome.SkyGradients[skyDuringHalo]));
        }
    }

    private void updateTransforms() {
        DayNightSpin.transform.localEulerAngles = new Vector3(Globals.timeOfDay, 0, 0); //update angle of sun/moon with Globals.timeOfDay - x axis
        StarsParent.transform.localEulerAngles = new Vector3(-(Mathf.PingPong(Globals.timeOfDay, 180) - 90) / 90 * horizonBufferAngle, 0, 0);
        float axis = sunAxisShift * Mathf.Sin(2 * Mathf.PI * Mathf.Repeat(Globals.time / Globals.time_resolution, daysPerYear * 360 * timePerDegree) / (daysPerYear * 360 * timePerDegree)); //tilt of sun/moon - z axis
        transform.eulerAngles = new Vector3(originalSkyAngle.x, originalSkyAngle.y, originalSkyAngle.z + axis); //update angle of sun/moon ring with time of year
        transform.position = new Vector3(Player.transform.position.x, 0, Player.transform.position.z); //follow player
    }

    private void updateStarColor() {
        Color newStarColor = starPrefab.GetComponent<Renderer>().sharedMaterial.GetColor("_Color");
        if (Globals.timeOfDay > 180 && Globals.timeOfDay <= 180 + horizonBufferAngle) newStarColor.a = starAlpha * (Globals.timeOfDay - 180) / horizonBufferAngle; //sunset
        else if (Globals.timeOfDay > 180 + horizonBufferAngle && Globals.timeOfDay <= 360 - horizonBufferAngle) newStarColor.a = 1; //night
        else if (Globals.timeOfDay > 360 - horizonBufferAngle) newStarColor.a = starAlpha * (360 - Globals.timeOfDay) / horizonBufferAngle; //sunrise
        else newStarColor.a = 0; //day
        starPrefab.GetComponent<Renderer>().sharedMaterial.SetColor("_Color", newStarColor);
    }

    private void updateIntensity() { // Modulate sun/moon intensity
        if (timeIsBetween(90 - horizonBufferAngle, 90 + horizonBufferAngle)) { //sunrise
            float r = ratio(90 - horizonBufferAngle, 90 + horizonBufferAngle);
            SunLight.GetComponent<Light>().intensity = sunMaxIntensity * r;
            MoonLight.GetComponent<Light>().intensity = moonMaxIntensity * (1 - r);
        } else if (timeIsBetween(90 + horizonBufferAngle, 270 - horizonBufferAngle)) { //day
            SunLight.GetComponent<Light>().intensity = sunMaxIntensity;
            MoonLight.GetComponent<Light>().intensity = 0;
        } else if (timeIsBetween(270 - horizonBufferAngle, 270 + horizonBufferAngle)) { //sunset
            float r = ratio(270 - horizonBufferAngle, 270 + horizonBufferAngle);
            SunLight.GetComponent<Light>().intensity = sunMaxIntensity * (1 - r);
            MoonLight.GetComponent<Light>().intensity = moonMaxIntensity * r;
        } else { //night
            SunLight.GetComponent<Light>().intensity = 0;
            MoonLight.GetComponent<Light>().intensity = moonMaxIntensity;
        }
    }

    private void updateSky() { // gradual sky color change
        /*SkyGradient[] gradients = { //temp
            new SkyGradient(new Color(0.055f, 0.114f, 0.141f, 1.0f), new Color(0.086f, 0.114f, 0.208f, 1.0f), new Color(0.114f, 0.125f, 0.208f, 1.0f), 1.0f, 1.0f, 1.0f),
            new SkyGradient(new Color(0.094f, 0.149f, 0.212f, 1.0f), new Color(0.129f, 0.149f, 0.271f, 1.0f), new Color(0.173f, 0.161f, 0.278f, 1.0f), 1.0f, 1.0f, 1.0f),
            new SkyGradient(new Color(0.220f, 0.600f, 1.000f, 1.0f), new Color(0.800f, 0.592f, 0.961f, 1.0f), new Color(0.867f, 0.753f, 0.659f, 1.0f), 1.0f, 1.0f, 1.0f),
            new SkyGradient(new Color(0.463f, 0.882f, 0.882f, 1.0f), new Color(0.475f, 0.882f, 1.000f, 1.0f), new Color(0.718f, 0.945f, 1.000f, 1.0f), 1.0f, 1.0f, 1.0f),
            new SkyGradient(new Color(0.604f, 0.961f, 1.000f, 1.0f), new Color(0.729f, 0.973f, 1.000f, 1.0f), new Color(0.855f, 0.984f, 1.000f, 1.0f), 1.0f, 1.0f, 1.0f),
            new SkyGradient(new Color(0.490f, 0.067f, 0.620f, 1.0f), new Color(0.792f, 0.118f, 0.408f, 1.0f), new Color(0.933f, 0.212f, 0.243f, 1.0f), 1.0f, 1.0f, 1.0f)
        };*/
        float[] timeMarks = { 0, 90 - horizonBufferAngle, 90 + horizonBufferAngle, 180, 270 - horizonBufferAngle, 270 + horizonBufferAngle };
        for(int i = 0; i < timeMarks.Length; i++) {
            int j = i + 1; if (j == timeMarks.Length) j = 0;
            if (timeIsBetween(timeMarks[i], timeMarks[j])) {
                float r = ratio(timeMarks[i], timeMarks[j]);
                setSky(getSkyGradient(Globals.cur_biome.SkyGradients[i]) * (1 - r) + getSkyGradient(Globals.cur_biome.SkyGradients[j]) * r);
                break;
            }
        }
	}

    // is time between a (inclusive) and b (exclusive)?
    // a and b should be less than 360
    private bool timeIsBetween(float a, float b){
        if (a < b) return Globals.timeOfDay >= a && Globals.timeOfDay < b;
        else return Globals.timeOfDay >= a || Globals.timeOfDay < b;
    }

    // returns the progress of timeOfDay through a and b as a ratio.
    // Will return -1 if timeOfDay is not between a and b or if a or b is more than or equal to 360
    private float ratio(float a, float b) {
        if (!timeIsBetween(a, b) || a >= 360 || b >= 360) return -1;
        if (a == b) return 0.5f;
        if (a < b) return (Globals.timeOfDay - a) / (b - a);
        else {
            float simTime = Globals.timeOfDay + (360 - a);
            if (simTime >= 360) simTime -= 360;
            return simTime / (b + 360 - a);
        }
    }

    // puts star in sky
    // called when shrine complete
    private void addStar(){
		numOfStars++;
		GameObject star = Instantiate(starPrefab, Vector3.up * 10000, Quaternion.identity) as GameObject;
		star.transform.SetParent(StarsParent.transform, false);
		Vector3 origRot = StarsParent.transform.localEulerAngles;
		StarsParent.transform.localEulerAngles = new Vector3(Random.Range(-(90-horizonBufferAngle), (90-horizonBufferAngle)), 0, Random.Range(-(90-sunAxisShift), (90-sunAxisShift)));
		star.transform.SetParent(null);
		StarsParent.transform.localEulerAngles = origRot;
		star.transform.SetParent(StarsParent.transform);
		star.transform.LookAt(StarsParent.transform);
	}
	
	public int getNumberOfStars(){
		return numOfStars;
	}
	
	//"time" is number of degrees it takes to finish changing to new sky
	//if time is 0, change sky immediately; time should not be less than 0
	//currently unused
	private void changeSkyByTime(SkyGradient s, float time){
		skyGoal = s;
		if(time <= 0){
			timeRemain = 0;
			setSky(skyGoal);
		}else{
			timeRemain = time * timePerDegree;
			skyDelta = s - getSkyGradient(RenderSettings.skybox) / timeRemain;
		}
	}
	
	//unused ... FOR NOW (dun dun DUUUUUUUUN)
	private void updateSkyByTime(){
		if(timeRemain == 0) return;
		timeRemain -=  Globals.time_scale;
		
		if(timeRemain <= 0){ //catch overshooting
			timeRemain = 0;
			setSky(skyGoal);
		}else setSky(getSkyGradient(RenderSettings.skybox) + skyDelta * Globals.time_scale);
	}

    // turns the sky into the inputed SkyGradient
	private void setSky(SkyGradient s){
		RenderSettings.skybox.SetColor("_Color1", s.topColor);
		RenderSettings.skybox.SetColor("_Color2", s.horizonColor);
		RenderSettings.skybox.SetColor("_Color3", s.bottomColor);
		RenderSettings.skybox.SetFloat("_Exponent1", s.exponentFactorForTopHalf);
		RenderSettings.skybox.SetFloat("_Exponent2", s.exponentFactorForBottomHalf);
		RenderSettings.skybox.SetFloat("_Intensity", s.intensityAmplifier);
    }

    // converts a Material into a SkyGradient class
    private SkyGradient getSkyGradient(Material m) {
        return new SkyGradient(
            m.GetColor("_Color1"),
            m.GetColor("_Color2"),
            m.GetColor("_Color3"),
            m.GetFloat("_Exponent1"),
            m.GetFloat("_Exponent2"),
            m.GetFloat("_Intensity")
        );
    }

    // internal "data structure" class for storing skybox gradients
    private class SkyGradient {
        public Color topColor;
        public Color horizonColor;
        public Color bottomColor;
        public float exponentFactorForTopHalf;
        public float exponentFactorForBottomHalf;
        public float intensityAmplifier;

        public SkyGradient(Color c1, Color c2, Color c3, float e1, float e2, float i) {
            topColor = c1;
            horizonColor = c2;
            bottomColor = c3;
            exponentFactorForTopHalf = e1;
            exponentFactorForBottomHalf = e2;
            intensityAmplifier = i;
        }

        public static SkyGradient operator +(SkyGradient a, SkyGradient b) {
            return new SkyGradient(
                a.topColor + b.topColor,
                a.horizonColor + b.horizonColor,
                a.bottomColor + b.bottomColor,
                a.exponentFactorForTopHalf + b.exponentFactorForTopHalf,
                a.exponentFactorForBottomHalf + b.exponentFactorForBottomHalf,
                a.intensityAmplifier + b.intensityAmplifier
            );
        }

        public static SkyGradient operator -(SkyGradient a, SkyGradient b) {
            return new SkyGradient(
                a.topColor - b.topColor,
                a.horizonColor - b.horizonColor,
                a.bottomColor - b.bottomColor,
                a.exponentFactorForTopHalf - b.exponentFactorForTopHalf,
                a.exponentFactorForBottomHalf - b.exponentFactorForBottomHalf,
                a.intensityAmplifier - b.intensityAmplifier
            );
        }

        public static SkyGradient operator *(SkyGradient s, float f) {
            return new SkyGradient(
                s.topColor * f,
                s.horizonColor * f,
                s.bottomColor * f,
                s.exponentFactorForTopHalf * f,
                s.exponentFactorForBottomHalf * f,
                s.intensityAmplifier * f
            );
        }

        public static SkyGradient operator /(SkyGradient s, float f) {
            return new SkyGradient(
                s.topColor / f,
                s.horizonColor / f,
                s.bottomColor / f,
                s.exponentFactorForTopHalf / f,
                s.exponentFactorForBottomHalf / f,
                s.intensityAmplifier / f
            );
        }
    }
}