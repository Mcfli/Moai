using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    public float grabDistance = 4;
    public float grabSphereRadius = 0;
    public float minWarpOnWaitDist = 0.5f; // distance from ground you have to be to warp there
    public LayerMask collisionLayers;

    private Collider thisCollider;
    private float cameraHeight;
    
    private InteractableObject heldObj;
    private float heldObjSize;
    private Vector3 heldObjOrigScale;
    private bool underwater;

    public AudioClip SpeedUpSFX;
    AudioSource playerAudio;
    [HideInInspector] public AudioSource pickupDropAudio;

    private UnityStandardAssets.Characters.FirstPerson.FirstPersonController firstPersonCont;
    private GameObject playerModel;

    private Camera mainCamera;
    private Vector3 playerCamPos;
    private Quaternion playerCamRot;
    private bool inCinematic = false;
    public float cinematicTimeScale;
    public float waitCamDistance = 60.0f;
    private float camDistance = 60.0f;
    private float theta = 0.0f;

    void Awake() {
        thisCollider = GetComponent<Collider>();
        cameraHeight = GameObject.FindGameObjectWithTag("MainCamera").transform.localPosition.y;
        firstPersonCont = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
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
	}
	
	// Update is called once per frame
	void Update () {
        if(Globals.mode != 0 || Globals.time_scale > 1) firstPersonCont.lookLock = true;
        else firstPersonCont.lookLock = false;

        if(Globals.mode != 0) return;

        if (Globals.time_scale > 1) {
            warpToGround(transform.position.y);
            if(!playerAudio.isPlaying) playerAudio.PlayOneShot(SpeedUpSFX, .2f);
            firstPersonCont.enabled = false;

            if(Globals.time_scale > cinematicTimeScale) {
                if(playerAudio.isPlaying) playerAudio.Stop();
                if(!playerModel.activeInHierarchy) {
                    playerModel.SetActive(true);
                } else {
                    RaycastHit hit;
                    Ray rayDown = new Ray(new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y + 100000, mainCamera.transform.position.z), Vector3.down);
                    int terrain = LayerMask.GetMask("Terrain");

                    if(Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain)) {
                        camDistance += 5 * Time.deltaTime;
                        theta += 0.5f * Time.deltaTime;
                        float targetY = hit.point.y - playerModel.transform.position.y + 80;
                        mainCamera.transform.localPosition = new Vector3(camDistance * Mathf.Cos(theta), Mathf.Lerp(mainCamera.transform.localPosition.y, targetY, Time.deltaTime), camDistance * Mathf.Sin(theta));
                    }
                }
                mainCamera.transform.LookAt(playerModel.transform.position);
                inCinematic = true;
            }
        }
        if(Globals.time_scale == 1) {
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

        //Holding Objects stuff
        if (Input.GetButtonDown("Use") && Globals.mode == 0 && Globals.time_scale == 1) {
            if (heldObj == null && LookingAtGrabbable()) TryGrabObject(GetHover().collider.gameObject);
            else if (TryUseObject()) { }
            else DropObject();
        }
        if(heldObj != null) followHand(heldObj, heldObjSize);

        checkUnderwater();
    }

    public bool isUnderwater() {return underwater;}

    private void checkUnderwater()
    {
        bool isUnder = false;
        float xpos = transform.position.x;
        float ypos = transform.position.y + cameraHeight + 10;
        float zpos = transform.position.z;
        // Check if player is in each lake in current chunk
        if (Globals.WaterManagerScript == null ||
            Globals.WaterManagerScript.lakesInChunk(GenerationManager.worldToChunk(transform.position)) == null)
                return;
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
		RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(transform.position.x, fromHeight, transform.position.z), Vector3.down);
        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, collisionLayers)){
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

    private RaycastHit GetHover() {
        RaycastHit raycastHit;
        Transform t = Camera.main.transform; //camera transform
        Physics.SphereCast(t.position, grabSphereRadius, t.forward, out raycastHit, grabDistance);
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
        if (heldObj != null) return heldObj.canUse(GetHover());
        return false;
    }

    private bool TryUseObject() {
        if (heldObj != null) return heldObj.tryUse(GetHover());
        return false;
    }
    
    private bool TryGrabObject(GameObject obj){
        if(obj == null || !CanGrab(obj)) return false;
        Physics.IgnoreCollision(obj.GetComponent<Collider>(), thisCollider);
        heldObj = obj.GetComponent<InteractableObject>();
        heldObjSize = obj.GetComponent<Renderer>().bounds.size.magnitude;
        heldObjOrigScale = obj.transform.localScale;
        heldObj.pickedUp();
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
