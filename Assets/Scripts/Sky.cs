using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sky : MonoBehaviour {
    //attach to sky
    public float timePerDegree = 5;             //x axis, in seconds (5 is 30 min day)
    public float initialTimeOfDay = 180;        //noon
    public GameObject DayNightSpin;
    public Light SunLight;
    public Light MoonLight;
    public GameObject Halo;
    public GameObject StarsParent;
    public GameObject normalStar;
    public GameObject fireStar;
    public GameObject waterStar;
    public GameObject airStar;
    public GameObject earthStar;
    public int extraStars = 5; //number of normal stars that are generated when spent
    public float maxStarAlpha = 1;
    public float horizonBufferAngle = 30;
    public float starBufferAngle = 10;
    public float elementalStarAngleRadius = 10; // angle from center
    public float sunAxisShift = 20;             // tilt of sun/moon rotation
    public float daysPerYear = 365;             // z axis
    public float timeScaleThatHaloAppears = 1000;
    public int skyDuringHalo = 3;               //0 for midnight, 3 for noon, etc
    public float skyLerpSpeed = 0.25f; //ratio per second

    //finals
    private GameObject Player;
    private Vector3 originalSkyAngle;
    private float sunMaxIntensity; //will take intensity of light set in editor
    private float moonMaxIntensity; //ditto above

    private SkyGradient skyGoal;
    private SkyGradient skyDelta;
    private float timeRemain;
    
    private List<SkyGradient> mixedSkyGradients; //final sky set
    private List<Biome> biomeSet; //biomes that are in the mix - first one is dominant
    private List<float> biomeSkyRatios; //amount of each biome that should be put into the skyset

    private List<GameObject> listOfStars; //holds all stars

    void Awake() { //set finals
        Player = GameObject.FindGameObjectWithTag("Player");
        originalSkyAngle = transform.eulerAngles;
        sunMaxIntensity = SunLight.intensity;
        moonMaxIntensity = MoonLight.intensity;
        RenderSettings.skybox = Object.Instantiate(RenderSettings.skybox); //comment out when debugging
        mixedSkyGradients = new List<SkyGradient>();
        biomeSet = new List<Biome>();
        biomeSkyRatios = new List<float>();
    }

    void Start() {
        Halo.SetActive(false);
        listOfStars = new List<GameObject>();
    }

    void Update() {
        Globals.timeOfDay = Mathf.Repeat(Globals.time / Globals.time_resolution / timePerDegree + initialTimeOfDay, 360);
        updateTransforms();
        if (Globals.time_scale < timeScaleThatHaloAppears) { //normal day/night cycle
            DayNightSpin.SetActive(true);
            StarsParent.SetActive(true);
            Halo.SetActive(false);
            updateIntensity();
            updateMixedSky();
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

    /***---------- STARS ----------***/

    public GameObject addStar() { // puts star in sky, called when shrine complete
        return addStar("normal");
    }

    public GameObject addStar(string element) {
        if(element.Equals("fire") || element.Equals("water") || element.Equals("air") || element.Equals("earth"))
            return addStar(element, new Vector2(Random.Range(-elementalStarAngleRadius, elementalStarAngleRadius), Random.Range(-elementalStarAngleRadius, elementalStarAngleRadius)));
        return addStar(element, new Vector2(Random.Range(-(90 - starBufferAngle), (90 - starBufferAngle)), Random.Range(-(90 - starBufferAngle), (90 - starBufferAngle))));
    }

    public GameObject addStar(string element, Vector2 angle) {
        GameObject star = createStar(element);
        Vector3 originalRotation = star.transform.eulerAngles;

        star.transform.SetParent(StarsParent.transform);
        star.transform.localPosition = Vector3.up * 10000;
        Vector3 origRot = StarsParent.transform.localEulerAngles;
        StarsParent.transform.localEulerAngles = new Vector3(angle.x, 0, angle.y);
        star.transform.SetParent(null);
        StarsParent.transform.rotation = Quaternion.identity;
        star.transform.SetParent(StarsParent.transform);
        StarsParent.transform.localEulerAngles = origRot;
        star.transform.LookAt(StarsParent.transform);
        star.transform.eulerAngles += originalRotation;
        return star;
    }

    public GameObject changeStar(string element, GameObject oldStar) {
        GameObject star = createStar(element);

        star.transform.position = oldStar.transform.position;
        star.transform.rotation = oldStar.transform.rotation;
        star.transform.SetParent(StarsParent.transform);
        star.transform.LookAt(StarsParent.transform);

        removeStar(oldStar);

        return star;
    }

    public void removeStar(GameObject oldStar) {
        listOfStars.Remove(oldStar);
        Destroy(oldStar);
    }

    private GameObject createStar(string element) {
        GameObject star;
        if(element.Equals("fire")) star = Instantiate(fireStar) as GameObject;
        else if(element.Equals("water")) star = Instantiate(waterStar) as GameObject;
        else if(element.Equals("air")) star = Instantiate(airStar) as GameObject;
        else if(element.Equals("earth")) star = Instantiate(earthStar) as GameObject;
        else star = Instantiate(normalStar) as GameObject;
        if(Globals.Stars.ContainsKey(element)) {
            Globals.Stars[element].Add(star);
            Globals.MenusScript.GetComponent<StarHUD>().addStar(element);
        }
        listOfStars.Add(star);
        return star;
    }

    public int getNumberOfStars() {
        return listOfStars.Count;
    }

    public void clearStars() {
        while(listOfStars.Count > 0) {
            Destroy(listOfStars[0]);
            listOfStars.RemoveAt(0);
        }
    }

    public void populateSky(int numOfStars) {
        for(int i = 0; i < numOfStars; i++) Globals.SkyScript.addStar();
    }

    public void removeNulls() {
        for(int i = 0; i < listOfStars.Count; i++) if(!listOfStars[i]) listOfStars.RemoveAt(i);
    }

    private void updateStarColor() {
        if(listOfStars.Count == 0) return;
        Color newStarColor = listOfStars[0].GetComponent<Renderer>().material.GetColor("_Color");
        if(timeIsBetween(90 - horizonBufferAngle, 90)) newStarColor.a = maxStarAlpha * (1 - ratio(90 - horizonBufferAngle, 90)); //moonset
        else if(timeIsBetween(90, 270)) newStarColor.a = 0; //day
        else if(timeIsBetween(270, 270 + horizonBufferAngle)) newStarColor.a = maxStarAlpha * ratio(270, 270 + horizonBufferAngle); //moonrise
        else newStarColor.a = 1; //night
        foreach(GameObject s in listOfStars) s.GetComponent<Renderer>().material.SetColor("_Color", newStarColor);
    }

    /***---------- SUN AND MOON ----------***/

    private void updateTransforms() {
        DayNightSpin.transform.localEulerAngles = new Vector3(Globals.timeOfDay, 0, 0); //update angle of sun/moon with Globals.timeOfDay - x axis
        StarsParent.transform.localEulerAngles = new Vector3(ratio(270, 90) * starBufferAngle * 2 - starBufferAngle, 0, 0);
        float axis = sunAxisShift * Mathf.Sin(2 * Mathf.PI * Mathf.Repeat(Globals.time / Globals.time_resolution, daysPerYear * 360 * timePerDegree) / (daysPerYear * 360 * timePerDegree)); //tilt of sun/moon - z axis
        transform.eulerAngles = new Vector3(originalSkyAngle.x, originalSkyAngle.y, originalSkyAngle.z + axis); //update angle of sun/moon ring with time of year
        transform.position = new Vector3(Player.transform.position.x, 0, Player.transform.position.z); //follow player
    }

    private void updateIntensity() { // Modulate sun/moon intensity
        if (timeIsBetween(90 - horizonBufferAngle, 90 + horizonBufferAngle)) { //sunrise
            float r = ratio(90 - horizonBufferAngle, 90 + horizonBufferAngle);
            SunLight.intensity = sunMaxIntensity * r;
            MoonLight.intensity = moonMaxIntensity * (1 - r);
        } else if (timeIsBetween(90 + horizonBufferAngle, 270 - horizonBufferAngle)) { //day
            SunLight.intensity = sunMaxIntensity;
            MoonLight.intensity = 0;
        } else if (timeIsBetween(270 - horizonBufferAngle, 270 + horizonBufferAngle)) { //sunset
            float r = ratio(270 - horizonBufferAngle, 270 + horizonBufferAngle);
            SunLight.intensity = sunMaxIntensity * (1 - r);
            MoonLight.intensity = moonMaxIntensity * r;
        } else { //night
            SunLight.intensity = 0;
            MoonLight.intensity = moonMaxIntensity;
        }
        SunLight.intensity *= Globals.WeatherManagerScript.getSkyIntensityMultipler();
        MoonLight.intensity *= Globals.WeatherManagerScript.getSkyIntensityMultipler();
    }

    /***---------- SKYBOX ----------***/

    private void updateMixedSky() {
        if(biomeSet.Count == 0) { // if empty, set the gradient to current biome
            biomeSet.Add(Globals.cur_biome);
            biomeSkyRatios.Add(1f);
            for(int i = 0; i < 6; i++) mixedSkyGradients.Add(getSkyGradient(biomeSet[0].SkyGradients[i]));
            return;
        }

        if(biomeSet[0] != Globals.cur_biome) { // if current biome is not the dominant one (changed biome)
            int i = biomeSet.IndexOf(Globals.cur_biome);
            if(i < 0) { // if current biome is not in the list
                biomeSet.Insert(0, Globals.cur_biome);
                biomeSkyRatios.Insert(0, 0);
            } else { // if it's in the list, but not at the top
                float temp = biomeSkyRatios[i];
                biomeSet.RemoveAt(i);
                biomeSkyRatios.RemoveAt(i);
                biomeSet.Insert(0, Globals.cur_biome);
                biomeSkyRatios.Insert(0, temp);
            }
        }

        if(biomeSet.Count == 1) return; // if there's only one biome, don't do anything

        // update ratios
        biomeSkyRatios[0] += skyLerpSpeed * Globals.deltaTime / Globals.time_resolution;
        if(biomeSkyRatios[0] >= 1) { // if dominant becomes 1
            biomeSet.RemoveRange(1, biomeSet.Count - 1);
            biomeSkyRatios[0] = 1;
            biomeSkyRatios.RemoveRange(1, biomeSkyRatios.Count - 1);
        } else { //else subtract lerp amount evenly amount other biomes
            float lerpAmount = skyLerpSpeed / (biomeSet.Count - 1) * Globals.deltaTime / Globals.time_resolution;
            for(int i = 1; i < biomeSkyRatios.Count; i++) {
                biomeSkyRatios[i] -= lerpAmount;
                if(biomeSkyRatios[i] <= 0) {
                    biomeSet.RemoveAt(i);
                    biomeSkyRatios.RemoveAt(i);
                    i--;
                }
            }
        }

        //change sky gradient
        for(int i = 0; i < 6; i++) { // for each time of day
            mixedSkyGradients[i] = new SkyGradient();
            for(int j = 0; j < biomeSet.Count; j++) mixedSkyGradients[i] += getSkyGradient(biomeSet[j].SkyGradients[i]) * biomeSkyRatios[j];
        }
    }

    private void updateSky() { // gradual sky color change
        float[] timeMarks = { 0, 90 - horizonBufferAngle, 90 + horizonBufferAngle, 180, 270 - horizonBufferAngle, 270 + horizonBufferAngle };
        for(int i = 0; i < timeMarks.Length; i++) {
            int j = i + 1; if (j == timeMarks.Length) j = 0;
            if (timeIsBetween(timeMarks[i], timeMarks[j])) {
                float r = ratio(timeMarks[i], timeMarks[j]);
                SkyGradient s = mixedSkyGradients[i] * (1 - r) + mixedSkyGradients[j] * r;
                s.intensityAmplifier *= Globals.WeatherManagerScript.getSkyIntensityMultipler();
                setSky(s);
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

        public SkyGradient() {
            topColor = Color.black;
            horizonColor = Color.black;
            bottomColor = Color.black;
            exponentFactorForTopHalf = 0;
            exponentFactorForBottomHalf = 0;
            intensityAmplifier = 0;
        }

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