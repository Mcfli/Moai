﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour {
    public List<MMBackground> titleScreens;
    public int loadingScreen;
    public UnityEngine.UI.Image loadingBackdrop;
    public UnityEngine.UI.Image loadingIcon;
    public List<Sprite> loadingWallpapers;
    public RectTransform title;
    public RectTransform buttonsParent;
    public bool playLogo = true;
    public GameObject logo;

    private MMBackground mmback;
    private UnityStandardAssets.Characters.FirstPerson.FirstPersonController firstPersonCont;
    private int loadStep = -1;
    private Vector3 origCamPos;
    private Vector3 origCamRot;
    private GameObject scene;
    private FadeInOut fader;
    private int logoStep = 0;

    void Awake() {
        firstPersonCont = Globals.Player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
        Globals.settings["Screenmode"] = (Screen.fullScreen ? 1 : 0);
        fader = GetComponent<FadeInOut>();
    }

    // Update is called once per frame
    void Start () {
        if(Globals.mode != -1 && loadStep == -1) return;
        setupMain();
    }

    void Update() {
        if(Globals.mode != -1 && loadStep == -1) return;

        if(Input.GetKeyDown(KeyCode.Escape) && loadStep == -1) Globals.MenusScript.switchToInitial();

        if(loadStep == 0) {
            prepLoad();
            Globals.loading = true;
            loadStep = 1;
            Globals.mode = 0;
            StartCoroutine(loadingDelay());
            return;
        } else if(loadStep == 1) {
            float rot = Mathf.PingPong(Time.time, 2) - 1;
            loadingIcon.rectTransform.localScale = new Vector3(rot,
                loadingIcon.rectTransform.localScale.y, loadingIcon.rectTransform.localScale.z);
        } else if(loadStep == 2) {
            loadGame();
            Globals.loading = false;
            loadStep = -1;
            return;
        }

        if(playLogo) {
            if(Input.GetKey(KeyCode.Escape)) {
                fader.fade(Color.clear, 0);
                logo.SetActive(false);
                playLogo = false;
                Globals.WeatherManagerScript.getWeatherAudioSource().volume = 1;
                Camera.main.GetComponent<MusicManager>().Play();
            }else if(logoStep == 0) {
                Globals.WeatherManagerScript.getWeatherAudioSource().volume = 0;
                Camera.main.GetComponent<MusicManager>().Stop(false);
                fader.fade(new Color(34f / 255, 44f / 255, 55f / 255, 1), 0f);
                fader.fade(new Color(255f / 255, 220f / 255, 24f / 255, 1), 1f);
                logoStep++;
            }else if(logoStep == 1) {
                if(!fader.isFading()) {
                    logo.SetActive(true);
                    logoStep++;
                }
            } else if(logoStep == 2) {
                if(!logo.activeSelf) {
                    fader.fade(Color.clear, 1f);
                    Globals.WeatherManagerScript.getWeatherAudioSource().volume = 1;
                    Camera.main.GetComponent<MusicManager>().Play();
                    playLogo = false;
                    title.gameObject.GetComponent<TitleAnimate>().animate();
                }
            }
        }

        Camera.main.transform.position = mmback.cameraPosition;
        Camera.main.transform.eulerAngles = mmback.cameraRotation;
        Globals.WeatherManagerScript.moveParticles(Camera.main.transform.position);
        firstPersonCont.lookLock = true;
        firstPersonCont.getMouseLook().SetCursorLock(false);
        title.anchoredPosition = new Vector2(mmback.titlePosition.x * Screen.width, mmback.titlePosition.y * Screen.height) / 2;
        float aspectRatio = title.sizeDelta.x / title.sizeDelta.y;
        title.sizeDelta = new Vector2(Screen.height * mmback.titleScale * aspectRatio, Screen.height * mmback.titleScale);
        buttonsParent.anchoredPosition = new Vector2(mmback.buttonsPosition.x * Screen.width, mmback.buttonsPosition.y * Screen.height) / 2;
    }


    public void startGame() {
        loadStep = 0;
    }

    private IEnumerator loadingDelay()
    {
        while (!GenerationManager.doneLoading)
            yield return null;
        loadStep = 2;
        /*if(Globals.GenerationManagerScript.doneLoading) loadStep = 2;
        else StartCoroutine("loadingDelay");*/

    }

    private void prepLoad() {
        Globals.MenusScript.switchTo(loadingScreen);
        Random.seed = (int)System.DateTime.Now.Ticks;
        loadingBackdrop.sprite = loadingWallpapers[Random.Range(0, loadingWallpapers.Count)];
        Random.seed = Globals.SeedScript.randomizeSeed();
        Globals.SkyScript.clearStars();
        Globals.GenerationManagerScript.initiateWorld();
        Globals.WeatherManagerScript.initializeWeather();
        //Camera.main.GetComponent<MusicManager>().Stop(false);
        //AudioListener.volume = 0;
    }

    private void loadGame() {
        
        if(scene) Destroy(scene);

        Globals.MenusScript.switchTo(-1);
        //Camera.main.GetComponent<MusicManager>().Play();
        //AudioListener.volume = 1;

        Camera.main.transform.localPosition = origCamPos;
        Camera.main.transform.localEulerAngles = origCamRot;
        firstPersonCont.enabled = true;
        firstPersonCont.lookLock = false;
        firstPersonCont.getMouseLook().SetCursorLock(true);

        int originalSeed = Random.seed;
        Random.seed = Globals.SeedScript.seed;
        Vector2 randomSpot = Random.insideUnitCircle * Globals.GenerationManagerScript.chunk_size;
        Globals.Player.transform.position = new Vector3(randomSpot.x, 0, randomSpot.y);
        Globals.PlayerScript.warpToGround(3000, true);
        Random.seed = originalSeed;

        Globals.GenerationManagerScript.changeChunk();
    }

    public void setupMain() {
        Random.seed = (int)System.DateTime.Now.Ticks;
        setupMain(Random.Range(0, titleScreens.Count));
    }

    public void setupMain(int background) {
        mmback = titleScreens[background];

        scene = Instantiate(mmback.gameObject);
        Globals.cur_biome = mmback.biome;
        if(mmback.time < 0) Globals.time = Random.Range(0, 1800 * Globals.time_resolution);
        else Globals.time = mmback.time * Globals.time_resolution;
        Globals.SkyScript.populateSky(100);

        firstPersonCont.enabled = false;
        firstPersonCont.lookLock = true;
        origCamPos = Camera.main.transform.localPosition;
        origCamRot = Camera.main.transform.localEulerAngles;
        Camera.main.transform.position = mmback.cameraPosition;
        Camera.main.transform.eulerAngles = mmback.cameraRotation;
        firstPersonCont.getMouseLook().SetCursorLock(false);
    }

    public void generateMap(int r) {
        List<string> names = new List<string> { "Icefield", "Glacier", "Desert", "Inferno", "Forest", "RedwoodForest", "SwampLand", "MushroomLand" };
        List<int> count = new List<int> { 0, 0, 0, 0, 0, 0, 0, 0 };
        List<Color> colors = new List<Color> { Color.white, Color.cyan, Color.yellow, Color.red, Color.green, new Color(0, 0.25f, 0, 1), Color.blue, Color.magenta };
        Texture2D tex = new Texture2D(r * 2, r * 2, TextureFormat.RGB24, false);
            for(int y = -r; y < r; y++) {
                for(int x = -r; x < r; x++) {
                    string biome = Globals.GenerationManagerScript.chooseBiome(new Vector2(x, y)).biomeName;
                    for(int i = 0; i < names.Count; i++) {
                        if(biome == names[i]) {
                            tex.SetPixel(x + r, y + r, colors[i]);
                            count[i]++;
                            break;
                        }
                    }
                }
            }
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            Object.Destroy(tex);
            System.IO.File.WriteAllBytes(Application.dataPath + "/../SavedScreen.png", bytes);
            for(int i = 0; i < names.Count; i++) Debug.Log(names[i] + ": " + (((float)count[i] / (r * r * 4)) * 100) + "%");

            float chunk_size = Globals.GenerationManagerScript.chunk_size;
            Texture2D tex2 = new Texture2D(r * 2, r * 2, TextureFormat.RGB24, false);
            for(int y = -r; y < r; y++) {
                for(int x = -r; x < r; x++) {
                    float WaterFire = Globals.GenerationManagerScript.WaterFireMap.genPerlin((x + 0.5f) * chunk_size, (y + 0.5f) * chunk_size, 0);
                    float EarthAir = Globals.GenerationManagerScript.EarthAirMap.genPerlin((x + 0.5f) * chunk_size, (y + 0.5f) * chunk_size, 0);
                    float amp = Globals.GenerationManagerScript.AmplifyMap.genPerlin((x + 0.5f) * chunk_size, (y + 0.5f) * chunk_size, 0);
                    tex2.SetPixel(x + r, y + r, new Color(WaterFire,EarthAir,amp,1));
                }
            }
            tex2.Apply();
            byte[] bytes2 = tex2.EncodeToPNG();
            Object.Destroy(tex2);
            System.IO.File.WriteAllBytes(Application.dataPath + "/../SavedScreen2.png", bytes2);
    }
}
