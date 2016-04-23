using UnityEngine;
using System.Collections;

public class DoodadGroup : MonoBehaviour {
    public float maxRange = 10f;
    public float minRange = 2f;
    // Use this for initialization
    void Start () {
        randomizeChildren();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void randomizeChildren()
    {
        foreach(Transform child in transform)
        {
            GameObject obj = child.gameObject;
            placeObject(obj);
        }
    }

    void placeObject(GameObject obj)
    {
        RaycastHit hit;
        float theta = Random.Range(0, Mathf.PI * 2);
        float dist = Random.Range(minRange, maxRange);
        Vector3 randomPos = transform.position;
        randomPos.x += Mathf.Cos(theta) * dist;
        randomPos.z += Mathf.Sin(theta) * dist;
        randomPos.y = 100000000;
        Ray rayDown = new Ray((randomPos), Vector3.down);
        int terrain = LayerMask.GetMask("Terrain");
        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain))
        {
            randomPos.y = hit.point.y;
            obj.transform.position = randomPos;
            obj.transform.Rotate(Vector3.up * Random.Range(0, 2 * Mathf.PI));
        }
    }
}
