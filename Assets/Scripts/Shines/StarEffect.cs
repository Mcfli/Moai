using UnityEngine;
using System.Collections;

public class StarEffect : MonoBehaviour {
    public float speed = 1;
    public GameObject explosionPrefab;
	public GameObject beamPrefab;
    public GameObject chargingPrefab;
    public string element;
    //public Material spentMat;

    private Vector3 target;
    private bool isCharging = false;
    private bool isTargetSet = false;
    private bool isAtTarget = false;
    private bool hasExploded = false;
	private bool culledParticles = false;

    // References
    //private GameObject starsParent;
    private Vector3 step;
	private GameObject explosion;
	private GameObject beam;
    private GameObject charge;

    public AudioClip BeamSound;
    AudioSource BeamAudio;

    public void setTarget(Vector3 tar)
    {
        target = tar;
        step = speed * Vector3.Normalize(target - transform.position);
        isTargetSet = true;
    }

    /*public void spendStar()
    {
        GetComponent<Renderer>().material = spentMat;
        for (int i = 0; i < extraStars; i++) Globals.SkyScript.addStar();
    }*/

	// Use this for initialization
	void Start () {
        //starsParent = GameObject.Find("Sky").GetComponent<Sky>().StarsParent;
        BeamAudio = GetComponent<AudioSource>();
        BeamAudio.PlayOneShot(BeamSound, 1F);
        if (chargingPrefab != null)
        {
            charge = Instantiate(chargingPrefab, transform.position + Vector3.up * 10f, Quaternion.identity) as GameObject;
            isCharging = true;
        }
            
    }
	
	// Update is called once per frame
	void Update () {
        //transform.LookAt(Globals.Player.transform.position);
        if (isCharging)
        {
            if (!charge.GetComponent<ParticleSystem>().isPlaying)
            {
                Destroy(charge);
                isCharging = false;
                if (beamPrefab != null)
                    beam = Instantiate(beamPrefab, transform.position, Quaternion.identity) as GameObject;
            }
        }
		if (!isCharging && isTargetSet && !isAtTarget)
			move ();
		//else if (!hasExploded)
		//	explode ();
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
            explode();
            isAtTarget = true;
            Destroy(gameObject);
            Globals.SkyScript.addStar(element);
            /*
            transform.LookAt(starsParent.transform);
            starsParent.transform.localEulerAngles = new Vector3(Random.Range(-(90 - Globals.SkyScript.horizonBufferAngle), 
                (90 - Globals.SkyScript.horizonBufferAngle)), 0, Random.Range(-(90 - Globals.SkyScript.sunAxisShift), (90 - Globals.SkyScript.sunAxisShift)));
            transform.parent = starsParent.transform;
            */
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
