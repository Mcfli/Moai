using UnityEngine;
using System.Collections;

public class InteractableObject: MonoBehaviour
{

    public GameObject spawn_object;
    public LayerMask cull_layer;
    public float cull_radius;        // How far this must be from other seeds and trees in order to grow.
    public float life_length;        // How long before the seed sprouts into a tree (or dies)

    private float lifeRemain;        // how long left before the seed dies

    private Rigidbody rb;
    private Collider coll;


    // Use this for initialization
    void Start(){
        lifeRemain = life_length;
        rb = GetComponent<Rigidbody>();
        coll = GetComponent<Collider>();
        // Start delayed update
        StartCoroutine("tickUpdate");
    }

    // Update is called once per frame
    void Update(){
        
    }

    // Coroutine
    IEnumerator tickUpdate()
    {
        while(true)
        {
            yield return new WaitForSeconds(1);
            if (isHeld())
            {
                //do nothing
            }
            else
            {
                lifeRemain -= (Globals.time_scale * 60);

                if (lifeRemain < 0)
                {
                    Collider[] close_trees = Physics.OverlapSphere(transform.position, cull_radius, cull_layer);
                    if (close_trees.Length < 1)
                    {
                        var RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                        Instantiate(spawn_object, transform.position, RandomRotation);
                    }
                    Destroy(gameObject);
                }

                if (Globals.time_scale > 1)
                { //if fast forwarding
                    RaycastHit hit;
                    Ray rayDown = new Ray(transform.position, Vector3.down);
                    int terrain = LayerMask.GetMask("Terrain");
                    if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain))
                        transform.position = new Vector3(transform.position.x, hit.point.y + coll.bounds.extents.y, transform.position.z);
                    rb.constraints = RigidbodyConstraints.FreezeAll;
                    rb.velocity = Vector3.zero;
                }
                else
                {
                    rb.constraints = RigidbodyConstraints.None;
                }
            }
        }
    }
    
    private bool isHeld(){
        return Globals.PlayerScript.getRightObj() == this.gameObject || Globals.PlayerScript.getLeftObj() == this.gameObject;
    }
}