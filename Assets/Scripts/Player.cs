using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    public float grabDistance = 4;
    public float waypointDist = 20;
    public float grabSphereRadius = 0;
    public float minWarpOnWaitDist = 0.5f; // distance from ground you have to be to warp there
    public LayerMask collisionLayers;
    public float teleportBackUpLevel = -10000;

    private Collider thisCollider;
    private float cameraHeight;
    
    private InteractableObject heldObj;
    private float heldObjSize;
    private Vector3 heldObjOrigScale;
    private bool underwater;

    public AudioClip SpeedUpSFX;
    AudioSource playerAudio;
    [HideInInspector] public AudioSource pickupDropAudio;

    public UnityStandardAssets.Characters.FirstPerson.FirstPersonController firstPersonCont;
    private GameObject playerModel;
    private UnityStandardAssets.ImageEffects.DepthOfField DOF;
    private MusicManager musicMan;

    private Camera mainCamera;
    private Vector3 playerCamPos;
    private Quaternion playerCamRot;
    private bool inCinematic = false;
    //public float cinematicTimeScale;
    public float waitCamDistance = 60.0f;
    public float camZoomOutPerSec = 5;
    public float camRotateDegreePerSec = 0.5f;
	public float zoomInSpeed = 5.0f;
    private float camDistance = 60.0f;
    private float theta = 0.0f;

    public GameObject waypoint;

    void Awake() {
        thisCollider = GetComponent<Collider>();
        cameraHeight = GameObject.FindGameObjectWithTag("MainCamera").transform.localPosition.y;
        firstPersonCont = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
        DOF = Camera.main.GetComponent<UnityStandardAssets.ImageEffects.DepthOfField>();
        musicMan = Camera.main.GetComponent<MusicManager>();
        playerModel = transform.FindChild("moai").gameObject;
        mainCamera = Camera.main;
        playerCamPos = mainCamera.transform.localPosition;
        playerCamRot = mainCamera.transform.localRotation;
        pickupDropAudio = mainCamera.gameObject.AddComponent<AudioSource>();
        pickupDropAudio.playOnAwake = false;
        pickupDropAudio.volume = 0.25f;
    }

    // Use this for initialization
    void Start () {
        playerAudio = GetComponent<AudioSource>();
        waypoint.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
        updateSettings();

        if (Globals.mode != 0 || (Globals.time_scale > 1 && !Globals.chrono)) firstPersonCont.lookLock = true;
        else firstPersonCont.lookLock = false;

        if(Globals.mode != 0) return;
        
        if (!StarEffect.isEffectPlaying && Input.GetButtonDown("Patience") && !playerAudio.isPlaying && !Globals.MenusScript.GetComponent<CheatConsole>().isActive()) playerAudio.PlayOneShot(SpeedUpSFX, .2f);
        else if (!StarEffect.isEffectPlaying && Input.GetButtonUp("Patience") && playerAudio.isPlaying) playerAudio.Stop();

        if (Globals.time_scale > 1) {
            if(!Globals.Player.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().flyCheat) warpToGround(transform.position.y);
            if(!playerAudio.isPlaying) playerAudio.PlayOneShot(SpeedUpSFX, .2f);
            firstPersonCont.enabled = Globals.chrono;

            if(Globals.time_scale > Globals.SkyScript.timeScaleThatHaloAppears && Globals.settings["WaitCinematic"] == 1 && !Globals.chrono) {
                if(playerAudio.isPlaying) playerAudio.Stop();
                if(!playerModel.activeInHierarchy) {
                    playerModel.SetActive(true);
                } else {
                    RaycastHit hit;
                    Ray rayDown = new Ray(new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y + 100000, mainCamera.transform.position.z), Vector3.down);
                    int terrain = LayerMask.GetMask("Terrain");

                    if(Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain)) {
                        camDistance += camZoomOutPerSec * Time.deltaTime;
                        theta += camRotateDegreePerSec * Time.deltaTime;
                        float targetY = Mathf.Max(hit.point.y - playerModel.transform.position.y + 80, 20);
                        mainCamera.transform.localPosition = new Vector3(camDistance * Mathf.Cos(theta), Mathf.Lerp(mainCamera.transform.localPosition.y, targetY, Time.deltaTime), camDistance * Mathf.Sin(theta));
                    }
                }
                mainCamera.transform.LookAt(playerModel.transform.position);
                inCinematic = true;
            }
        }
        if(Globals.time_scale == 1) {
			if(Vector3.Distance(mainCamera.transform.localPosition, playerCamPos) > 25.0f)
			{
				Vector3 direction = Vector3.Normalize (playerCamPos - mainCamera.transform.localPosition);
				mainCamera.transform.localPosition += direction * zoomInSpeed;
				//mainCamera.transform.localPosition = Vector3.Lerp (mainCamera.transform.localPosition, playerCamPos, Time.deltaTime * camLerpPlayer);
				//mainCamera.transform.localRotation = Quaternion.Lerp (mainCamera.transform.localRotation, playerCamRot, Time.deltaTime * camLerpPlayer);
			}
			else
			{
				if(inCinematic) {
					inCinematic = false;

					mainCamera.transform.localPosition = playerCamPos;
					mainCamera.transform.localRotation = playerCamRot;
					playerModel.SetActive(false);
					camDistance = waitCamDistance;
					theta = 0.0f;
				}
				firstPersonCont.enabled = true;
			}
            
        }

        //Holding Objects stuff
        if (Input.GetButtonDown("Use") && Globals.mode == 0 && Globals.time_scale == 1) {
            if(TryUseObject()) { }
            else if (heldObj == null && LookingAtGrabbable()) TryGrabObject(GetHover().collider.gameObject);
            else DropObject();
        }
        if(heldObj != null && !inCinematic) followHand(heldObj, heldObjSize);

        //falling through world
        if(transform.position.y < teleportBackUpLevel) warpToGround(3000, true);

        // Waypoints
        if (Input.GetButtonDown("Waypoint") && Globals.mode == 0 && Globals.time_scale == 1)
        {
            if (checkWaypoint().collider != null)
            {
                if (checkWaypoint().collider.gameObject.layer == LayerMask.NameToLayer("Terrain"))
                {
                    waypoint.transform.position = checkWaypoint().point;
                    waypoint.transform.LookAt(transform);
                    waypoint.transform.eulerAngles = new Vector3(0, waypoint.transform.eulerAngles.y, 0);
                    waypoint.SetActive(true);
                }
                else if (checkWaypoint().collider.gameObject.layer == LayerMask.NameToLayer("Waypoint"))
                {
                    waypoint.SetActive(false);
                }
            }
        }

        checkUnderwater();
    }

    public bool isUnderwater() {return underwater;}

    public void updateSettings() {
        DOF.enabled = (Globals.settings["DOF"] == 1);
        firstPersonCont.setHeadBob(Globals.settings["Bobbing"] == 1);
        firstPersonCont.setInvertY(Globals.settings["InvertMouse"] == 1);
        firstPersonCont.getMouseLook().XSensitivity = Globals.settings["Sensitivity"];
        firstPersonCont.getMouseLook().YSensitivity= Globals.settings["Sensitivity"];
        AudioListener.volume = Globals.settings["MasterVol"] / 100f;
        QualitySettings.shadowDistance = Globals.settings["ShadowDist"] * 10;
        musicMan.Volume = Globals.settings["MusicVol"] / 100f;
        RenderSettings.ambientIntensity = Globals.settings["Brightness"] / 50f;
        if(Globals.mode == -1) {
            firstPersonCont.getFOVKick().originalFov = 60;
            Camera.main.fieldOfView = 60;
        }else if(firstPersonCont.getFOVKick().originalFov != Globals.settings["FOV"]) {
            firstPersonCont.getFOVKick().originalFov = Globals.settings["FOV"];
            Camera.main.fieldOfView = Globals.settings["FOV"];
        }
        if(Screen.fullScreen != (Globals.settings["Screenmode"] == 1)) Screen.fullScreen = !Screen.fullScreen;
        firstPersonCont.useFovKick = (Globals.settings["FOVKick"] == 1);
        firstPersonCont.getMouseLook().smooth = (Globals.settings["SmoothCamera"] == 1);
    }

    private void checkUnderwater()
    {
        bool isUnder = false;
        float xpos = mainCamera.transform.position.x;
        float ypos = mainCamera.transform.position.y + 10;
        float zpos = mainCamera.transform.position.z;
        // Check if player is in each lake in current chunk
		if (Globals.WaterManagerScript == null || Globals.WaterManagerScript.lakesInChunk (GenerationManager.worldToChunk (transform.position)) == null) {
			underwater = false;
			return;
		}
        foreach(GameObject go in Globals.WaterManagerScript.lakesInChunk(GenerationManager.worldToChunk(transform.position)))
        {
            
            WaterBody wb = go.GetComponent<WaterBody>();
            float xMin = wb.center.x - 0.5f * wb.size.x;
            float xMax = wb.center.x + 0.5f * wb.size.x;
            float zMin = wb.center.z - 0.5f * wb.size.z;
            float zMax = wb.center.z + 0.5f * wb.size.z;

            if (wb == null) continue;
            // If player is in this lake, they are underwater
            if(xpos > xMin && xpos < xMax &&
                zpos > zMin && zpos < zMax &&
                ypos < wb.center.y)
            {
                isUnder = true;
                break;
            }
        }
        underwater = isUnder;
    }

    private void followHand(InteractableObject obj, float objSize){
        Transform t = Camera.main.transform;
        float scale = 0.5f; //temp
        float angleAway = 0.5f; //temp
        obj.transform.position = t.position + Vector3.Lerp(Vector3.Lerp(t.forward, t.right, angleAway), t.up * -1, 0.15f) * objSize*2 * scale;
        obj.transform.localScale = heldObjOrigScale * scale;
        obj.transform.forward = Camera.main.transform.forward;
    }
    
	public bool warpToGround(float fromHeight, bool overrideMinWarpDist = false){

        LayerMask coll = collisionLayers;
        // If we don't want islands, take out
        if (Globals.loading || overrideMinWarpDist)
        {
            int islandInt = LayerMask.GetMask("Islands");
            coll = coll.value ^ islandInt;
        }
            
        RaycastHit hit;

        Ray rayDown = new Ray(new Vector3(transform.position.x, fromHeight, transform.position.z), Vector3.down);
        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, coll)){
            if (transform.position.y - (hit.point.y + cameraHeight) > minWarpOnWaitDist || overrideMinWarpDist)
                transform.position = new Vector3(transform.position.x, hit.point.y + cameraHeight, transform.position.z);
            return true;
        }
        else return false;
	}
    
    public InteractableObject getHeldObj(){return heldObj;}

    public bool has(string type) {
        if (type == "") return false;

        if (getHeldObj()) if (heldObj.typeID == type) return true;
        return false;
    }

    public string holding() {
        if(getHeldObj()) return heldObj.typeID;
        else return "";
    }

    public RaycastHit GetHover() {
        RaycastHit raycastHit;
        Transform t = Camera.main.transform; //camera transform
        Physics.SphereCast(t.position, grabSphereRadius, t.forward, out raycastHit, grabDistance);
        return raycastHit;
    }

    public RaycastHit checkWaypoint()
    {
        RaycastHit raycastHit;
        Transform t = Camera.main.transform; //camera transform
        Physics.SphereCast(t.position, grabSphereRadius, t.forward, out raycastHit, waypointDist);
        return raycastHit;
    }

    private bool CanGrab(GameObject obj){
        return obj.GetComponent<InteractableObject>();
    }

    public bool LookingAtGrabbable(){
        if (GetHover().collider == null) return false;
        return CanGrab(GetHover().collider.gameObject);
    }

    public bool canUse() {
        if(heldObj == null) {
            if(GetHover().collider) {
                ShrineActivator sa = GetHover().collider.gameObject.GetComponent<ShrineActivator>();
                WordWall ww = GetHover().collider.gameObject.GetComponent<WordWall>();
                Obelisk ob = GetHover().collider.gameObject.GetComponent<Obelisk>();
                TeleportStone ts = GetHover().collider.gameObject.GetComponent<TeleportStone>();
                if(sa) return !sa.active();
                else if(ww) return true;
                else if(ob) return ob.usable();
                else if(ts) return true;
            }
        } else return heldObj.canUse(GetHover());
        return false;
    }

    private bool TryUseObject() {
        if(heldObj == null) {
            if(GetHover().collider) {
                ShrineActivator sa = GetHover().collider.gameObject.GetComponent<ShrineActivator>();
                if(sa) return sa.activate();
            }
        } else return heldObj.tryUse(GetHover());
        return false;
    }
    
    private bool TryGrabObject(GameObject obj){
        if(obj == null || !CanGrab(obj)) return false;
        Physics.IgnoreCollision(obj.GetComponent<Collider>(), thisCollider);
        heldObj = obj.GetComponent<InteractableObject>();
        heldObj.pickedUp();
        heldObjSize = obj.GetComponent<Renderer>().bounds.size.magnitude;
		obj.GetComponent<Renderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        heldObjOrigScale = obj.transform.localScale;
        heldObj.GetComponent<Rigidbody>().isKinematic = true;
        return true;
    }

    public bool DropObject(){
        if (heldObj == null) return false;
        Rigidbody objRigidbody = heldObj.GetComponent<Rigidbody>();
        Collider objCollider = heldObj.GetComponent<Collider>();
        if (objRigidbody != null) {
            objRigidbody.velocity = objRigidbody.velocity;
            Physics.IgnoreCollision(objCollider, thisCollider, false);
        }
        heldObj.transform.localScale = heldObjOrigScale;
        heldObj.GetComponent<Rigidbody>().isKinematic = false;
        heldObj.GetComponent<Renderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        heldObj.dropped();
        if(!objRigidbody) {
            heldObj.transform.position = mainCamera.transform.position + mainCamera.transform.forward * 1;
            objRigidbody.velocity = mainCamera.transform.forward * 1;
        }
        heldObj = null;
        return true;
    }

    public bool isInCinematic() {
        return inCinematic;
    }
}
