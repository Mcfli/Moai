using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    public float grabDistance = 4;
    public float grabSphereRadius = 0;
    public float minWarpOnWaitDist = 0.5f; // distance from ground you have to be to warp there

    private Collider thisCollider;
    private float cameraHeight;
    
	private bool startGroundWarp;
    private InteractableObject leftObj;
    private InteractableObject rightObj;
    //private float leftSize;
    private float rightSize;
    private Vector3 leftOrigScale;
    private Vector3 rightOrigScale;
    private bool underwater;

    public AudioClip speedUp;
    AudioSource playerAudio;

    private UnityStandardAssets.Characters.FirstPerson.FirstPersonController firstPersonCont;
    private GameObject playerModel;

    private Camera mainCamera;
    private Vector3 playerCamPos;
    private Quaternion playerCamRot;
    private bool inCinematic = false;
    public float cinematicTimeScale;

    void Awake() {
        thisCollider = GetComponent<Collider>();
        cameraHeight = GameObject.FindGameObjectWithTag("MainCamera").transform.localPosition.y;
        firstPersonCont = GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
        playerModel = transform.FindChild("moai").gameObject;
        mainCamera = Camera.main;
        playerCamPos = mainCamera.transform.localPosition;
        playerCamRot = mainCamera.transform.localRotation;
    }

    // Use this for initialization
    void Start () {
		startGroundWarp = false;
        playerAudio = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
        //warp to ground at game start
        if(!startGroundWarp) startGroundWarp = warpToGround();

        if (Globals.time_scale > 1) {
            warpToGround();
            if(!playerAudio.isPlaying) { //sound
                playerAudio.loop = true;
                playerAudio.PlayOneShot(speedUp, .2f);
            }
            if (firstPersonCont.enabled)
            {
                firstPersonCont.enabled = !firstPersonCont.enabled;
            }
        }
        if (Globals.time_scale > cinematicTimeScale)
        {
            playerModel.SetActive(true);
            mainCamera.transform.localPosition = new Vector3(-100, 0, 0);
            inCinematic = true;
        }
        if (Globals.time_scale == 1)
        {
            if(inCinematic)
            {
                inCinematic = false;
                mainCamera.transform.localPosition = playerCamPos;
                mainCamera.transform.localRotation = playerCamRot;
                playerModel.SetActive(false);
            }
            if (!firstPersonCont.enabled)
            {
                firstPersonCont.enabled = !firstPersonCont.enabled;
            }
        }

        //Holding Objects stuff
        if (Input.GetButtonDown("Use")){
            if (rightObj == null && GetHover().collider && !IsTree(GetHover().collider.gameObject)) 
                TryGrabObject(GetHover().collider.gameObject, false);
            else if (GetHover().collider && IsTree(GetHover().collider.gameObject))
                TryPunchTree(GetHover().collider.gameObject);
            else if (TryUseObject(false)) { }
            else DropObject(false);
        }
        if(rightObj != null) followHand(rightObj, rightSize, false);

        underwater = Globals.Player.transform.position.y + cameraHeight < Globals.water_level;
    }

    public bool isUnderwater() {return underwater;}
    
    private void followHand(InteractableObject obj, float objSize, bool isLeft){
        Transform t = Camera.main.transform;
        float scale = 0.5f; //temp
        float angleAway = 0.5f; //temp
        obj.transform.position = t.position + Vector3.Lerp(Vector3.Lerp(t.forward, t.right * ((isLeft) ? -1 : 1), angleAway), t.up * -1, 0.15f) * objSize*2 * scale;
        obj.transform.localScale = ((isLeft) ? leftOrigScale : rightOrigScale) * scale;
        obj.transform.forward = Camera.main.transform.forward;
    }
    
	public bool warpToGround(){
		RaycastHit hit;
        Ray rayDown = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, LayerMask.GetMask("Terrain"))){
            if (transform.position.y - (hit.point.y + cameraHeight) > minWarpOnWaitDist)
                transform.position = new Vector3(transform.position.x, hit.point.y + cameraHeight, transform.position.z);
            return true;
        }
        else return false;
	}
    
    public InteractableObject getRightObj(){return rightObj;}

    public bool has(string type) {
        if (type == "") return false;

        if (getRightObj()) if (rightObj.typeID == type) return true;
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

    private bool IsTree(GameObject obj)
    {
        return obj.GetComponent<TreeScript>();
    }

    public bool LookingAtGrabbable(){
        if (GetHover().collider == null) return false;
        return CanGrab(GetHover().collider.gameObject) || IsTree(GetHover().collider.gameObject);
    }

    public bool[] canUse() {
        bool[] val = {false, false};
        if (leftObj != null) val[0] = leftObj.canUse(GetHover());
        if (rightObj != null) val[1] = rightObj.canUse(GetHover());
        return val;
    }

    private bool TryUseObject(bool isLeft) {
        if (isLeft) {
            if (leftObj != null) return leftObj.tryUse(GetHover());
        } else {
            if (rightObj != null) return rightObj.tryUse(GetHover());
        } return false;
    }
    
    private bool TryGrabObject(GameObject obj, bool isLeft){
        if(obj == null || !CanGrab(obj)) return false;
        Physics.IgnoreCollision(obj.GetComponent<Collider>(), thisCollider);
        if (isLeft) {
            leftObj = obj.GetComponent<InteractableObject>();
            //leftSize = obj.GetComponent<Renderer>().bounds.size.magnitude;
            leftOrigScale = obj.transform.localScale;
            leftObj.pickedUp();
        }else{
            rightObj = obj.GetComponent<InteractableObject>();
            rightSize = obj.GetComponent<Renderer>().bounds.size.magnitude;
            rightOrigScale = obj.transform.localScale;
            rightObj.pickedUp();
        }
        return true;
    }

    private bool TryPunchTree(GameObject obj)
    {
        if (obj == null || !IsTree(obj)) return false;
        TreeScript tree = obj.GetComponent<TreeScript>();
        Vector3 offset = Vector3.Normalize(transform.position - obj.transform.position) + new Vector3(Random.value*2, Random.value * 2, Random.value*2)
             + Vector3.up * 10*cameraHeight;
        Instantiate(tree.seed_object,obj.transform.position + offset,Quaternion.identity);
        return true;
    }
    
    public bool DropObject(bool isLeft){
        if(isLeft){
            if (leftObj == null) return false;
            Rigidbody objRigidbody = leftObj.GetComponent<Rigidbody>();
            Collider objCollider = leftObj.GetComponent<Collider>();
            if (objRigidbody != null){
                objRigidbody.velocity = objRigidbody.velocity;
                Physics.IgnoreCollision(objCollider, thisCollider, false);
            }
            leftObj.transform.localScale = leftOrigScale;
            leftObj = null;
        }else{
            if (rightObj == null) return false;
            Rigidbody objRigidbody = rightObj.GetComponent<Rigidbody>();
            Collider objCollider = rightObj.GetComponent<Collider>();
            if (objRigidbody != null) {
                objRigidbody.velocity = objRigidbody.velocity;
                Physics.IgnoreCollision(objCollider, thisCollider, false);
            }
            rightObj.transform.localScale = rightOrigScale;
            rightObj = null;
        }
        return true;
    }
}
