using UnityEngine;
using System.Collections;

public class StarEffect : MonoBehaviour {
    public float speed = 1;
    public GameObject explosionPrefab;
	public GameObject beamPrefab;
	public string element;

    private Vector3 target;
    private bool isTargetSet = false;
    private bool isAtTarget = false;
    private bool hasExploded = false;

    // References
    private GameObject starsParent;
    private Vector3 step;

    public void setTarget(Vector3 tar)
    {
        step = speed * Vector3.Normalize(target - transform.position);
        target = tar;
        isTargetSet = true;
    }

	// Use this for initialization
	void Start () {
        starsParent = GameObject.Find("Sky").GetComponent<Sky>().StarsParent;
		if(beamPrefab != null)
			Instantiate (beamPrefab, transform.position, Quaternion.identity);
    }
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(Globals.Player.transform.position);
        if (isTargetSet && !isAtTarget)
            move();
        else if (!hasExploded)
            explode();
	}

    // Move the star towards its target
    private void move()
    {
        if(Vector3.Distance(transform.position,target) > speed)
        {
            transform.position += step;

        }
        else
        {
            isAtTarget = true;
            transform.parent = starsParent.transform;
        }
    }

    // Create particle system explosion at origin
    private void explode()
    {
        if (explosionPrefab != null) Instantiate(explosionPrefab, transform.position, Quaternion.identity);
    }
}
