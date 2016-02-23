using UnityEngine;
using System.Collections;

public class Water : MonoBehaviour {

    //This script enables underwater effects. Attach to main camera.

    //Define variable
    public Color fogColor;

    //The scene's default fog settings
    private bool defaultFog;
    private Color defaultFogColor;
    private float defaultFogDensity;
    private Material defaultSkybox;
    private Material noSkybox;
    private Camera cam;
    private GameObject player;

    void Start()
    {
        //Set the background color

        //The scene's default fog settings
        defaultFog = RenderSettings.fog;
        defaultFogColor = RenderSettings.fogColor;
        defaultFogDensity = RenderSettings.fogDensity;
        defaultSkybox = RenderSettings.skybox;

        //
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        player = GameObject.FindGameObjectWithTag("Player");
        cam.backgroundColor = new Color(0, 0.4f, 0.7f, 1);
		noSkybox = null;
    }

    void Update()
    {
        Vector3 new_pos = new Vector3(player.transform.position.x,Globals.water_level,player.transform.position.z);
        transform.position = new_pos;
        if (player.transform.position.y+1.5 < transform.position.y)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = 0.04f;
            RenderSettings.skybox = noSkybox;
        }
        else
        {
            RenderSettings.fog = defaultFog;
            RenderSettings.fogColor = defaultFogColor;
            RenderSettings.fogDensity = defaultFogDensity;
            RenderSettings.skybox = defaultSkybox;
        }
    }
}
