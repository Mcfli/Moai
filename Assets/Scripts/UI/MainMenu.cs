using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour {
    public int seed;
    public GameObject moai;
    public Vector3 moaiPosition;
    public Vector3 moaiRotation;
    public Vector3 cameraPosition;
    public Vector3 cameraRotation;
    public List<Vector2> chunksToLoad;

    private UnityStandardAssets.Characters.FirstPerson.FirstPersonController firstPersonCont;

    void Awake() {
        firstPersonCont = Globals.Player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
    }

    // Update is called once per frame
    void Start () {
        if(Globals.mode == -1) {
            Random.seed = seed;
            Globals.cur_chunk = GenerationManager.worldToChunk(Globals.Player.transform.position);
            Globals.cur_biome = Globals.GenerationManagerScript.chooseBiome(Globals.cur_chunk);
            Globals.WeatherManagerScript.changeWeather();
            Globals.WeatherManagerScript.moveParticles(GenerationManager.chunkToWorld(Globals.cur_chunk) + new Vector3(Globals.GenerationManagerScript.chunk_size * 0.5f, 0, Globals.GenerationManagerScript.chunk_size * 0.5f));
            foreach(Vector2 v in chunksToLoad)
                Globals.GenerationManagerScript.generateChunk(v, Globals.GenerationManagerScript.inLoadDistance(Globals.Player.transform.position, v, Globals.GenerationManagerScript.chunk_detail_dist));

            moai.SetActive(true);
            moai.transform.position = moaiPosition;
            moai.transform.eulerAngles = moaiRotation;
            Globals.MenusScript.switchTo(3);
        }
    }

    void Update() {
        if(Globals.mode == -1) {
            Camera.main.transform.position = cameraPosition;
            Camera.main.transform.eulerAngles = cameraRotation;
            firstPersonCont.lookLock = true;
            firstPersonCont.getMouseLook().SetCursorLock(false);
        }
    }
}
