using UnityEngine;
using System.Collections;

public class TeleportStone : MonoBehaviour {

    public GameObject linkedObelisk;
    public float lightUpDistance = 10f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnMouseDown()
    {
        float dist = Vector3.Distance(Globals.Player.transform.position, transform.position);
        if (dist < lightUpDistance && Globals.time_scale > 0 && Time.timeScale > 0)
        {
            Vector3 pos = linkedObelisk.transform.position;
            Globals.Player.transform.position = pos + new Vector3(-10, 0, -10);
        }
    }
}
