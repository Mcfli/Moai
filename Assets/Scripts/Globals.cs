using UnityEngine;
using System.Collections;

public class Globals : MonoBehaviour {

    public static float time = 0.0f; //incremented by Player.cs
    public static float time_scale = 1.0f;
	public static float time_resolution = Mathf.Pow(10, -17.0f);
    public static float timeOfDay = 0;
    //0 for dawn, 90 for noon
    //180 for dusk, 270 for midnight
    //should not be or exceed 360. incremeted by Sky.cs

    public static Weather cur_weather;
    public static float water_level = 5;
    public static Biome cur_biome;

    public static GameObject Player = GameObject.FindGameObjectWithTag("Player");
}
