using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public float wait_speed_init = 10.0f;
    public float wait_speed_max = 100.0f;
    public float wait_speed_growth = 1.0f;

    private float wait_speed;

    // Use this for initialization
    void Start () {
        wait_speed = wait_speed_init;
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetButton("Wait"))
        {
            Globals.time_scale = wait_speed;
            if (wait_speed < wait_speed_max)
                wait_speed += wait_speed_growth;
            else if (wait_speed > wait_speed_max)
                wait_speed = wait_speed_max;
        }
        else
        {
            Globals.time_scale = 1.0f;
            wait_speed = wait_speed_init;
        }
        Globals.time += Globals.time_resolution*Globals.time_scale;
	}
}
