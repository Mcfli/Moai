using UnityEngine;
using System.Collections;

public class StarEffect : MonoBehaviour {
    public float speed = 1;
    public GameObject explosionPrefab;
	public GameObject beamPrefab;
	public string element;
    public Material spentMat;

    private Vector3 target;
    private bool isTargetSet = false;
    private bool isAtTarget = false;
    private bool hasExploded = false;
	private bool culledParticles = false;

    // References
    private GameObject starsParent;
    private Vector3 step;
	private GameObject explosion;
	private GameObject beam;

    public void setTarget(Vector3 tar)
    {
        target = tar;
        step = speed * Vector3.Normalize(target - transform.position);
        isTargetSet = true;
    }

    public void spendStar()
    {
        GetComponent<Renderer>().material = spentMat;
    }

	// Use this for initialization
	void Start () {
        starsParent = GameObject.Find("Sky").GetComponent<Sky>().StarsParent;
		if(beamPrefab != null)
			beam = Instantiate (beamPrefab, transform.position, Quaternion.identity) as GameObject;
    }
	
	// Update is called once per frame
	void Update () {
        transform.LookAt(Globals.Player.transform.position);
		if (isTargetSet && !isAtTarget)
			move ();
		else if (!hasExploded)
			explode ();
		else if (!culledParticles)
			cullParticles ();
		
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
        if (explosionPrefab != null)
			explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity) as GameObject;
		hasExploded = true;
    }

	private void cullParticles(){
		if ((beam!=null&&!beam.GetComponent<ParticleSystem>().isPlaying) ||
			(explosion!= null && !explosion.GetComponent<ParticleSystem> ().isPlaying)) {
			if(explosion != null) Destroy (explosion);
			if(beam != null) Destroy (beam);
		}
	}
}
