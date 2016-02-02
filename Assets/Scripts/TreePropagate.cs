using UnityEngine;
using System.Collections;

public class TreePropagate : MonoBehaviour {

    public GameObject prefab;
    public Vector3 center;
    public float max_life = 100;
    public float radius;
    public float spawnDelay;
    public int spawnLimit;
    public float cullRadius;
    public int cullMaxDensity;
    private LayerMask treeMask = LayerMask.GetMask("Tree");
    private bool done = false;
    private float lastSpawned = 0.0f;
    private Vector2 squareVec;
    private int numSpawned;
    UnityRandom rand = new UnityRandom();
    public float life;

    // Use this for initialization
    void Awake () {
        life = max_life;
        Collider[] hitColiders = Physics.OverlapSphere(center, radius);
        numSpawned = 0;
        lastSpawned = Time.time;
        transform.localScale = new Vector3(1, 1, 1);
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(transform.position.x,10000000,transform.position.z), Vector3.down);
        int terrain = LayerMask.GetMask("Terrain");

        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain))
        {
            {
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            }
        }
        else
        {
            Destroy(gameObject);
        }
	}
	
	// Update is called once per frame
	void Update () {
        Cull();
        if (!done){
            if(Time.time - lastSpawned > spawnDelay){
                lastSpawned = Time.time;
                squareVec = rand.PointInASquare(); 
                var RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                Instantiate(prefab, new Vector3(squareVec.x * radius + transform.position.x, 0, squareVec.y * radius + transform.position.z),RandomRotation);
                numSpawned++;
                if (spawnLimit>0 && numSpawned >= spawnLimit){
                    done = true;
                }
            }
        }   
    }

    // If there are too many or nearby trees for too long, destroy this one
    void Cull()
    {
        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, cullRadius, treeMask);
        if (objectsInRange.Length > cullMaxDensity)
        {
            life -= 0.1f;
            if(life <= 0)
            {
                done = true;
                Destroy(gameObject);
            }
            
        }
        else if (life < max_life)
        {
            life += 0.01f;
        }
        else if (life > max_life)
        {
            life = max_life;
        }
    }
}
