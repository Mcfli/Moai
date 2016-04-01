using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public float initialWaitSpeed = 10.0f;
    public float maxWaitSpeed = 10000.0f;
    public AnimationCurve waitSpeedGrowth;
    public float timeToGetToMaxWait = 300; // 5 minutes
    public float sprintWaitMultiplier = 5.0f;
    public float grabDistance = 5;
    public float grabSphereRadius = 1;

    public string waitInput = "Patience";
    public float minWarpOnWaitDist = 1; // distance from ground you have to be to warp there

    private Collider thisCollider;
    private CharacterController thisCharacterController;
    
    private float waitingFor;
	private bool startGroundWarp;
    private InteractableObject leftObj;
    private InteractableObject rightObj;
    private float leftSize;
    private float rightSize;
    private Vector3 leftOrigScale;
    private Vector3 rightOrigScale;

    public AudioClip speedUp;
    AudioSource audio; //unused, complains about hiding

    void Awake() {
        thisCollider = GetComponent<Collider>();
        thisCharacterController = GetComponent<CharacterController>();
    }

    // Use this for initialization
    void Start () {
		startGroundWarp = false;
        audio = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
        //Speed Up Sound
        if (Input.GetButton(waitInput))
        {
            if (!audio.isPlaying)
            {
                audio.loop = true;
                audio.PlayOneShot(speedUp, .2f);
            }
        }

        if (Input.GetButton(waitInput)) { //PATIENCE IS POWER
            if (waitingFor < timeToGetToMaxWait) Globals.time_scale = initialWaitSpeed + waitSpeedGrowth.Evaluate(waitingFor / timeToGetToMaxWait) * (maxWaitSpeed - initialWaitSpeed);
            else Globals.time_scale = maxWaitSpeed;

            if(Input.GetButton("Sprint")) waitingFor += Time.deltaTime * sprintWaitMultiplier;
            else waitingFor += Time.deltaTime;
            
            warpToGround();
        }else{
            Globals.time_scale = 1;
            waitingFor = 0;
        }

        Globals.deltaTime = Globals.time_resolution * Globals.time_scale * Time.deltaTime;
        Globals.time += Globals.deltaTime;

        //warp to ground at game start
        if (!startGroundWarp) startGroundWarp = warpToGround();

        //Holding Objects stuff
        if (Input.GetButtonDown("Use")){
            if(rightObj == null && GetHover().collider) TryGrabObject(GetHover().collider.gameObject, false);
            else if (TryUseObject(false)) { }
            else DropObject(false);
        }

        if(rightObj != null) followHand(rightObj, rightSize, false);
    }
    
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
            if (transform.position.y - (hit.point.y + thisCharacterController.height / 2) > minWarpOnWaitDist)
                transform.position = new Vector3(transform.position.x, hit.point.y + thisCharacterController.height / 2, transform.position.z);
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

    public bool LookingAtGrabbable(){
        if (GetHover().collider == null) return false;
        return CanGrab(GetHover().collider.gameObject);
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
            leftSize = obj.GetComponent<Renderer>().bounds.size.magnitude;
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
