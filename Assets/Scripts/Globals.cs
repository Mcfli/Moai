using UnityEngine;
using System.Collections;

public class Globals : MonoBehaviour {

    public static float time = 0.0f; //incremented by Player.cs
    public static float time_scale = 1.0f;
	public static float time_resolution = Mathf.Pow(10, -17.0f);

    public static string cur_weather;
    public static float water_level = 5;
    public static Biome cur_biome;
}
