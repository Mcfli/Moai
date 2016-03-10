using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public float initialWaitSpeed = 10.0f;
    public float maxWaitSpeed = 10000.0f;
    public float waitSpeedGrowth = 1.0f;
    public float sprintWaitMultiplier = 5.0f;
    public float grabDistance = 5;
    public float grabSphereRadius = 1;
    public string leftHandInput = "LeftHand";
    public string rightHandInput = "RightHand";
    public string waitInput = "Patience";
    public float minWarpOnWaitDist = 1; // distance from ground you have to be to warp there
    
    private float wait_speed;
	private bool startGroundWarp;
    private GameObject leftObj;
    private GameObject rightObj;
    private float leftSize;
    private float rightSize;
    private Vector3 leftOrigScale;
    private Vector3 rightOrigScale;

    // Use this for initialization
    void Start () {
        wait_speed = initialWaitSpeed;
		startGroundWarp = false;
	}
	
	// Update is called once per frame
	void Update () {
	    //PATIENCE IS POWER
        if (Input.GetButton(waitInput)){
            if(Input.GetButton("Sprint")) Globals.time_scale = wait_speed * 5;
            else Globals.time_scale = wait_speed;
            
            if (wait_speed < maxWaitSpeed) wait_speed += waitSpeedGrowth;
            else wait_speed = maxWaitSpeed;
            
            warpToGround();
        }else{
            Globals.time_scale = 1.0f;
            wait_speed = initialWaitSpeed;
        }
        Globals.time += Globals.time_resolution*Globals.time_scale;
        
        //warp to ground at game start
		if(!startGroundWarp) startGroundWarp = warpToGround();

        //Holding Objects stuff
        if (Input.GetButtonDown(leftHandInput)){
            if(leftObj == null) TryGrabObject(GetMouseHoverObject(grabDistance, grabSphereRadius), true);
            else DropObject(true);
        }else if(Input.GetButtonDown(rightHandInput)){
            if(rightObj == null) TryGrabObject(GetMouseHoverObject(grabDistance, grabSphereRadius), false);
            else DropObject(false);
        }
        
        if(leftObj != null) followHand(leftObj, leftSize, true);
        if(rightObj != null) followHand(rightObj, rightSize, false);
    }
    
    private void followHand(GameObject obj, float objSize, bool isLeft){
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
            if (transform.position.y - (hit.point.y + GetComponent<CharacterController>().height / 2) > minWarpOnWaitDist)
                transform.position = new Vector3(transform.position.x, hit.point.y + GetComponent<CharacterController>().height / 2, transform.position.z);
            return true;
        }
        else return false;
	}
    
    public GameObject getLeftObj(){return leftObj;}
    public GameObject getRightObj(){return rightObj;}

    public bool has(string type) {
        if (type == "") return false;
        if (getLeftObj()) if(leftObj.GetComponent<InteractableObject>().typeID == type) return true;
        if (getRightObj()) if(rightObj.GetComponent<InteractableObject>().typeID == type) return true;
        return false;
    }

    private GameObject GetMouseHoverObject(float range, float radius){
        RaycastHit raycastHit;
        Transform t = Camera.main.transform; //camera transform
        if(Physics.SphereCast(t.position, radius, t.forward, out raycastHit, range)) return raycastHit.collider.gameObject;
        return null;
    }                                                
    
    private bool CanGrab(GameObject obj){
        return obj.GetComponent<InteractableObject>();
    }

    public bool LookingAtGrabbable(){
        if (GetMouseHoverObject(grabDistance, grabSphereRadius) == null) return false;
        return CanGrab(GetMouseHoverObject(grabDistance, grabSphereRadius));
    }
    
    private bool TryGrabObject(GameObject obj, bool isLeft){
        if(obj == null || !CanGrab(obj)) return false;
        Physics.IgnoreCollision(obj.GetComponent<Collider>(), GetComponent<Collider>());
        if (isLeft) {
            leftObj = obj;
            leftSize = obj.GetComponent<Renderer>().bounds.size.magnitude;
            leftOrigScale = obj.transform.localScale;
        }else {
            rightObj = obj;
            rightSize = obj.GetComponent<Renderer>().bounds.size.magnitude;
            rightOrigScale = obj.transform.localScale;
        }
        return true;
    }
    
    private bool DropObject(bool isLeft){
        if(isLeft){
            if(leftObj == null) return false;
            if(leftObj.GetComponent<Rigidbody>() != null){
                leftObj.GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity;
                Physics.IgnoreCollision(leftObj.GetComponent<Collider>(), GetComponent<Collider>(), false);
            }
            leftObj.transform.localScale = leftOrigScale;
            leftObj = null;
        }
        else{
            if (rightObj == null) return false;
            if (rightObj.GetComponent<Rigidbody>() != null){
                rightObj.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                Physics.IgnoreCollision(rightObj.GetComponent<Collider>(), GetComponent<Collider>(), false);
            }
            rightObj.transform.localScale = rightOrigScale;
            rightObj = null;
        }
        return true;
    }
}
