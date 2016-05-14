using UnityEngine;
using System.Collections;

public class Waypoint : MonoBehaviour {

	// Use this for initialization
	void OnEnable () {
        transform.position = snapToTerrain(transform.position);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private Vector3 snapToTerrain(Vector3 pos)
    {
        Vector3 ret = pos;
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(pos.x, 10000000, pos.z), Vector3.down);
        int terrain = LayerMask.GetMask("Terrain");

        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain))
        {

            ret = new Vector3(pos.x, hit.point.y, pos.z);

        }
        return ret;
    }
}
