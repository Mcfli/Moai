using UnityEngine;
using System.Collections;

public class TreePropagate : MonoBehaviour {

    public Transform prefab;
    public Vector3 center;
    public float radius;
    public float spawnDelay;
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
        Ray rayDown = new Ray(transform.position, Vector3.down);
        Ray rayUp = new Ray(transform.position, Vector3.up);
        LayerMask trees = ~(1 << 8);
        if (Physics.Raycast(rayDown, out hit))
        {
            if (hit.collider != null)
            {
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            }
        }
        else if (Physics.Raycast(rayUp, out hit, Mathf.Infinity, trees))
        {
            if (hit.collider != null)
            {
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
                Debug.Log("Success");
                Debug.DrawRay(rayUp.origin,rayUp.direction,Color.white,100.0f);
            }
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
                if (numSpawned == 3){
                    done = true;
                }
            }
        }
	}
}
