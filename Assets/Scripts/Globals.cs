using UnityEngine;
using System.Collections;

public class Globals : MonoBehaviour {

    public static float time = 0.0f; //incremented by TimeScript
    public static float time_scale = 1.0f;
	public static float time_resolution = Mathf.Pow(10, -20.0f);
    public static float deltaTime;

    public static float timeOfDay = 0;
    //incremeted by Sky
    //0 for midnight, 90 for dawn
    //180 for noon, 270 for dusk
    //should not be or exceed 360

    public static Weather cur_weather; // set by WeatherManager
    public static float water_level; // set by WaterScript
    public static Biome cur_biome;
    public static Vector2 cur_chunk;

    //references
    public static GameObject Player = GameObject.Find("Player");
    public static Player PlayerScript = Player.GetComponent<Player>();
    public static Sky SkyScript = GameObject.Find("Sky").GetComponent<Sky>();
    public static WeatherManager WeatherManagerScript = GameObject.Find("Weather").GetComponent<WeatherManager>();

    // Heat & Moisture
    public static Vector2 heatMoistureOrigin = new Vector2(25,25);
    public static Vector2 heatMoistureVector = Vector2.zero;
    public static float heatMoistureMin = 50;
    public static float heatMostureDistGuaranteed = 50f;  // The distance from the center point a biome is guaranteed to have puzzle
                                                           // FindObjectsOfType added to a shrine

    public static T CopyComponent<T>(GameObject destination, T source) where T : Component{
        System.Type type = source.GetType();
        T component = destination.GetComponent<T>();
        if (!component) component = destination.AddComponent(type) as T;
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo f in fields)
        {
            if (f.IsStatic) continue;
            f.SetValue(component, f.GetValue(source));
        }
        System.Reflection.PropertyInfo[] properties = type.GetProperties();
        foreach (System.Reflection.PropertyInfo prop in properties)
        {
            if (!prop.CanWrite || !prop.CanWrite || prop.Name == "name") continue;
            prop.SetValue(component, prop.GetValue(source, null), null);
        }
        return component;
    }

    public static Vector2 WaterFireEarthAirOrigin = new Vector2(0.5f,0.5f);
    public static Vector2 WaterFireEarthAirVector = Vector2.zero;
    public static float WaterFireEarthAirMin = 0.5f;
    public static float WaterFireEarthAirDistGuaranteed = 10f;  // The distance from the center point a biome is guaranteed to have puzzle
                                                           // FindObjectsOfType added to a shrine
}
