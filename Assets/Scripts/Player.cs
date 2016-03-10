using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public float wait_speed_init = 10.0f;
    public float wait_speed_max = 100.0f;
    public float wait_speed_growth = 1.0f;
    public float faster_wait_speed_multiplier = 5.0f;

    private float wait_speed;
	private bool startGroundWarp;

    // Use this for initialization
    void Start () {
        wait_speed = wait_speed_init;
		startGroundWarp = false;
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetButton("Wait"))
        {
            if(Input.GetButton("Speed")) Globals.time_scale = wait_speed * 5;
            else Globals.time_scale = wait_speed;
            
            if (wait_speed < wait_speed_max)
                wait_speed += wait_speed_growth;
            else wait_speed = wait_speed_max;
        }
        else
        {
            Globals.time_scale = 1.0f;
            wait_speed = wait_speed_init;
        }
        Globals.time += Globals.time_resolution*Globals.time_scale;
		if(!startGroundWarp) startGroundWarp = warpToGround();
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
}
