using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public float wait_speed_init = 10.0f;
    public float wait_speed_max = 10000.0f;
    public float wait_speed_growth = 1.0f;
    public float faster_wait_speed_multiplier = 5.0f;
    public string leftHand;
    public string rightHand;
    
    private float wait_speed;
	private bool startGroundWarp;
    private GameObject hand1;
    private float hand1size;
    private GameObject hand2;
    private float hand2size;

    // Use this for initialization
    void Start () {
        wait_speed = wait_speed_init;
		startGroundWarp = false;
	}
	
	// Update is called once per frame
	void Update () {
	    //PATIENCE IS POWER
        if (Input.GetButton("Wait")){
            if(Input.GetButton("Speed")) Globals.time_scale = wait_speed * 5;
            else Globals.time_scale = wait_speed;
            
            if (wait_speed < wait_speed_max) wait_speed += wait_speed_growth;
            else wait_speed = wait_speed_max;
        }else{
            Globals.time_scale = 1.0f;
            wait_speed = wait_speed_init;
        }
        Globals.time += Globals.time_resolution*Globals.time_scale;
        
        //warp to ground at game start
		if(!startGroundWarp) startGroundWarp = warpToGround();
        
        //Inventory stuff
        //Debug.Log(GetMouseHoverObject(5)); //return what you're looking at
			
        if(Input.GetButton(rightHand)){
            if(hand1 == null) TryGrabObject1(GetMouseHoverObject(5));
            else DropObject1();
        }
        
        if(Input.GetButton(leftHand)){
            if(hand2 == null) TryGrabObject2(GetMouseHoverObject(5));
            else DropObject2();
        }
        
        if(hand1 != null){
            Vector3 newPosition = (gameObject.transform.position + Vector3.Lerp(Camera.main.transform.forward, Camera.main.transform.right, 0.3f)* hand1size);
            hand1.transform.position = newPosition;
            hand1.transform.forward = Camera.main.transform.forward;
        }
        
        if(hand2 != null){
            Vector3 newPosition = (gameObject.transform.position + Vector3.Lerp(Camera.main.transform.forward, Camera.main.transform.right * -1, 0.3f) * hand2size);
            hand2.transform.position = newPosition;
            hand2.transform.forward = Camera.main.transform.forward;
        }
	}
    
	public bool warpToGround(){
		RaycastHit hit;
        Ray rayDown = new Ray(transform.position, Vector3.down);
        int terrain = LayerMask.GetMask("Terrain");
        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain)){
			transform.position = new Vector3(transform.position.x, hit.point.y + GetComponent<CharacterController>().height/2, transform.position.z);
			return true;
		}else return false;
	}
    
    public GameObject getHand1(){return hand1;}
    public GameObject getHand2(){return hand2;}
    
    private GameObject GetMouseHoverObject(float range){
        Vector3 position = gameObject.transform.position;
        RaycastHit raycastHit;
        Vector3 target = position + Camera.main.transform.forward * range;
        if(Physics.Linecast(position,target,out raycastHit)) return raycastHit.collider.gameObject;
        return null;
    }                                                
    
    private bool CanGrab(GameObject obj){
        return obj.tag == "Object";
    }
    
    void TryGrabObject1(GameObject obj){
        if(obj == null || !CanGrab(obj)) return;
        hand1 = obj;
        Physics.IgnoreCollision(hand1.GetComponent<Collider>(), GetComponent<Collider>());
        hand1size = obj.GetComponent<Renderer>().bounds.size.magnitude;
        
    }
    
    void DropObject1(){
        if(hand1 == null) return;
        
        if(hand1.GetComponent<Rigidbody>() != null){
            hand1.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
            Physics.IgnoreCollision(hand1.GetComponent<Collider>(), GetComponent<Collider>(), false);
        }
        
        hand1 = null;
    }
    
    void TryGrabObject2(GameObject obj){
        if(obj == null || !CanGrab(obj)) return;
        hand2 = obj;
        Physics.IgnoreCollision(hand2.GetComponent<Collider>(), GetComponent<Collider>());
        hand2size = obj.GetComponent<Renderer>().bounds.size.magnitude;
        
    }
    
    void DropObject2(){
        if(hand2 == null) return;
        
        if(hand2.GetComponent<Rigidbody>() != null){
            hand2.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
            Physics.IgnoreCollision(hand2.GetComponent<Collider>(), GetComponent<Collider>(), false);
        }
        
        hand2 = null;
    }
}
