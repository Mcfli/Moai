using UnityEngine;
using System.Collections;

public class MMBackground : MonoBehaviour {
    public Biome biome;
    public float time;
    public Vector3 cameraPosition;
    public Vector3 cameraRotation;
    public Vector2 titlePosition; //ratio -1 to 1
    public float titleScale; //compared to Screen.height
    public Vector2 buttonsPosition; //ratio -1 to 1
}
