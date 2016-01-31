using UnityEngine;
using System.Collections;

public class TreePropagate : MonoBehaviour {

    public Transform prefab;
    public Vector3 center;
    public float radius;
    public float spawnDelay;
    public float cullRadius;
    private LayerMask LayerToCheck;
    private bool done = false;
    private float lastSpawned = 0.0f;
    private Vector2 squareVec;
    private int numSpawned;
    UnityRandom rand = new UnityRandom();

	// Use this for initialization
	void Awake () {
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
            Cull();
        }
	}
	
	// Update is called once per frame
	void Update () {
        if(!done){
            if(Time.time - lastSpawned > spawnDelay){
                lastSpawned = Time.time;
                squareVec = rand.PointInASquare(); 
                var RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                Instantiate(prefab, new Vector3(squareVec.x * radius + transform.position.x, 0, squareVec.y * radius + transform.position.z),RandomRotation);
                numSpawned++;
                if (numSpawned == 5){
                    done = true;
                }
            }
        }
	}

    void Cull()
    {
        int terrain = ~((1 << 8) | (1 << 9));
        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, cullRadius, terrain);
        if (objectsInRange.Length > 0)
        {
            Debug.Log(transform.position);
            Debug.Log(objectsInRange.Length);
            Debug.Log(objectsInRange[0]);
            Destroy(gameObject);
        }
    }
}
