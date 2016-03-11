using UnityEngine;
using System.Collections;

public class Globals : MonoBehaviour {

    public static float time = 0.0f; //incremented by Player.cs
    public static float time_scale = 1.0f;
	public static float time_resolution = Mathf.Pow(10, -20.0f);
    public static float timeOfDay = 0;
    //0 for midnight, 90 for dawn
    //180 for noon, 270 for dusk
    //should not be or exceed 360. incremeted by Sky.cs

    public static Weather cur_weather;
    public static float water_level = 5;
    public static Biome cur_biome;

    public static GameObject Player = GameObject.FindGameObjectWithTag("Player");

    public static Player PlayerScript = Player.GetComponent<Player>();

    public static Vector2 heatMoistureOrigin = new Vector2(25,25);
    public static Vector2 heatMoistureVector = Vector2.zero;
    public static float heatMoistureMin = 50;
    public static float heatMostureDistGuaranteed = 0.1f;  // The distance from the center point a biome is guaranteed to have puzzle
                                                           // FindObjectsOfType added to a shrine
}
