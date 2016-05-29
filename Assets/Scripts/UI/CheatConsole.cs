using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CheatConsole : MonoBehaviour{
    public InputField console;
    public GameObject log;
    public float logLingerTime;
    public GameObject puzzleFinisher;
    public float defaultSuperSpeed;
    public float maxMaxWaitSpeed;

    private UnityEngine.UI.Text[] logLines;
    private List<string> previousCommands = new List<string>();
    private int currentCommandNum;
    private bool active;
    private float lastActiveTime = -1;
    private float defaultRunSpeed;
    private TimeScript timeScript;
    private float defaultWaitSpeed;
    private float defaultTimeToGetToMax;

    void Awake() {
        logLines = log.GetComponentsInChildren<UnityEngine.UI.Text>(true);
        timeScript = GameObject.Find("WorldGen").GetComponent<TimeScript>();
    }

    void Start() {
        active = false;
        log.SetActive(false);
        console.DeactivateInputField();
        console.gameObject.SetActive(false);
        console.text = "";
        defaultRunSpeed = Globals.PlayerScript.firstPersonCont.runSpeed;
        defaultWaitSpeed = timeScript.maxWaitSpeed;
        defaultTimeToGetToMax = timeScript.timeToGetToMaxWait;
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.BackQuote)) toggleConsole();
        console.gameObject.SetActive(active);
        if(active) {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(console.gameObject, null);
            if(Input.GetKeyDown(KeyCode.Escape)) toggleConsole();
            else if(Input.GetKeyDown(KeyCode.Return)) parseCommand(console.text);
            else if(Input.GetKeyDown(KeyCode.UpArrow)) {
                currentCommandNum--;
                if(currentCommandNum < 0) currentCommandNum = 0;
                if(currentCommandNum == previousCommands.Count) console.text = "";
                else console.text = previousCommands[currentCommandNum];
                console.MoveTextEnd(false);
            } else if(Input.GetKeyDown(KeyCode.DownArrow)) {
                currentCommandNum++;
                if(currentCommandNum > previousCommands.Count) currentCommandNum = previousCommands.Count;
                if(currentCommandNum == previousCommands.Count) console.text = "";
                else console.text = previousCommands[currentCommandNum];
                console.MoveTextEnd(false);
            }
            lastActiveTime = Time.realtimeSinceStartup;
        }
        log.SetActive(lastActiveTime > -1 && Time.realtimeSinceStartup - lastActiveTime <= logLingerTime);
    }

    void toggleConsole() {
        active = !active;
        if(!active) console.DeactivateInputField();
        console.gameObject.SetActive(active);
        if(active) console.ActivateInputField();
        console.text = "";
        currentCommandNum = previousCommands.Count;
        Globals.PlayerScript.firstPersonCont.playerControl = !active;
    }

    public void parseCommand(string text){
        if(!text.Equals("")) previousCommands.Add(text);
        text = text.ToLower();
        string[] command = text.Split();
        switch(command[0]) {
            case "":
                break;
            case "stars":
                if(command.Length == 1) {
                    foreach(KeyValuePair<string, List<GameObject>> p in Globals.Stars) Globals.SkyScript.addStar(p.Key);
                    Log("Adding star(s)");
                    break;
                } else if(command.Length == 2) {
                    if(command[1].Equals("fire") || command[1].Equals("water") || command[1].Equals("air") || command[1].Equals("earth")) {
                        Globals.SkyScript.addStar(command[1]);
                        Log("Adding star(s)");
                        break;
                    }
                }
                Log("Usage: stars [fire:water:earth:air]");
                break;
            case "finish":
                if(command.Length == 1) Instantiate(puzzleFinisher, Globals.Player.transform.position, Quaternion.identity);
                else if(command.Length == 2) {
                    int n = -1;
                    if(int.TryParse(command[1], out n)) for(int i = 0; i < n; i++) Instantiate(puzzleFinisher, Globals.Player.transform.position, Quaternion.identity);
                    else {
                        Log("Usage: finish [value]");
                        break;
                    }
                }
                Log("Creating puzzle finisher(s)");
                break;
            case "fly":
                Globals.PlayerScript.firstPersonCont.flyCheat = !Globals.PlayerScript.firstPersonCont.flyCheat;
                if(Globals.PlayerScript.firstPersonCont.flyCheat) Log("Fly on");
                else Log("Fly off");
                break;
            case "flash":
                if(command.Length == 1) {
                    if(Globals.PlayerScript.firstPersonCont.runSpeed != defaultRunSpeed) {
                        Globals.PlayerScript.firstPersonCont.runSpeed = defaultRunSpeed;
                        Log("Normal speed");
                    } else {
                        Globals.PlayerScript.firstPersonCont.runSpeed = 100;
                        Log("Superspeed enabled");
                    }
                    break;
                } else if(command.Length == 2) {
                    float f = -1;
                    if(float.TryParse(command[1], out f)) {
                        Globals.PlayerScript.firstPersonCont.runSpeed = f;
                        Log("Sprint speed set to " + f);
                        break;
                    }
                }
                Log("Usage: flash [runSpeed]");
                break;
            case "chrono":
                Globals.chrono = !Globals.chrono;
                if(Globals.chrono) Log("Waiting movement enabled");
                else Log("Waiting movement disabled");
                break;
            case "jupiter":
                Globals.WeatherManagerScript.changeWeather();
                Log("Changed Weather");
                break;
            case "anxiety":
                if(command.Length == 1) {
                    timeScript.timeToGetToMaxWait = defaultTimeToGetToMax;
                    timeScript.maxWaitSpeed = defaultWaitSpeed;
                    Log("Patience reverted to default");
                    break;
                } else if(command.Length == 2) {
                    float f = -1;
                    if(float.TryParse(command[1], out f)) {
                        if(f > maxMaxWaitSpeed) f = maxMaxWaitSpeed;
                        timeScript.timeToGetToMaxWait = 0;
                        timeScript.maxWaitSpeed = f;
                        Log("Patience is set to " + f);
                        break;
                    }
                }
                Log("Usage: anxiety [speed]");
                break;
            case "pos":
                Log(Globals.Player.transform.position.ToString() + " [" + Globals.Player.transform.eulerAngles.y + "]");
                break;
            case "tp":
                if(command.Length == 3) {
                    float x = -1;
                    float z = -1;
                    if(float.TryParse(command[1], out x) && float.TryParse(command[2], out z)) {
                        Globals.Player.transform.position = new Vector3(x, 0, z);
                        Globals.PlayerScript.warpToGround(100000000, true);
                        Log("Teleported to " + Globals.Player.transform.position.ToString());
                        break;
                    }
                }else if(command.Length == 4) {
                    float x = -1;
                    float y = -1;
                    float z = -1;
                    if(float.TryParse(command[1], out x) && float.TryParse(command[2], out y) && float.TryParse(command[3], out z)) {
                        Globals.Player.transform.position = new Vector3(x, y, z);
                        Log("Teleported to " + Globals.Player.transform.position.ToString());
                        break;
                    }
                }
                Log("Usage: tp {x} [y] {z}");
                break;
            case "seed":
                if(Globals.mode != -1) {
                    Log(Globals.SeedScript.seed.ToString());
                    break;
                }else if(command.Length == 1) {
                    Globals.SeedScript.setSeed = false;
                    Globals.SeedScript.randomizeSeed();
                    Log("Randomized seed");
                    break;
                }else if(command.Length == 2) {
                    int n = -1;
                    if(int.TryParse(command[1], out n)) {
                        Globals.SeedScript.setSeed = true;
                        Globals.SeedScript.seed = n;
                        Globals.SeedScript.randomizeSeed();
                        Log("Set seed to " + Globals.SeedScript.seed);
                        break;
                    }
                }
                Log("Usage: seed [integer]");
                break;
            case "majestic":
                if(command.Length == 7) {
                    float x = -1;
                    float y = -1;
                    float z = -1;
                    float rx = -1;
                    float ry = -1;
                    float rz = -1;
                    if(float.TryParse(command[1], out x) && float.TryParse(command[2], out y) && float.TryParse(command[3], out z) && float.TryParse(command[4], out rx) && float.TryParse(command[5], out ry) && float.TryParse(command[6], out rz)) {
                        Globals.PlayerScript.waypoint.transform.position += new Vector3(x, y, z);
                        Globals.PlayerScript.waypoint.transform.eulerAngles += new Vector3(rx, ry, rz);
                        Log("Nudged");
                        break;
                    } else {
                        Log("Nudge failed");
                        break;
                    }
                }
                foreach(Transform c in Globals.PlayerScript.waypoint.transform) c.gameObject.SetActive(!c.gameObject.activeSelf);
                Log("This is why he calls me the Wandering Eye.");
                break;
            default:
                Log("Unknown Command");
                break;
        }
        toggleConsole();
    }

    public void Log(string message) {
        for(int i = logLines.Length - 1; i > 0; i--) logLines[i].text = logLines[i-1].text;
        logLines[0].text = message;
        lastActiveTime = Time.realtimeSinceStartup;
    }

    public bool isActive() {
        return active;
    }
}