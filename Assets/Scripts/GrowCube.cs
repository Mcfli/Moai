using UnityEngine;
using System.Collections;

public class GrowCube : MonoBehaviour {
    public Vector3 target_scale;
    public float grow_speed;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.Q))
        {
            Grow();
        }
	}

    void Grow()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, target_scale, Time.deltaTime * grow_speed);
    }
}
