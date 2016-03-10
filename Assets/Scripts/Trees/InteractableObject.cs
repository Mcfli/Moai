using UnityEngine;
using System.Collections;

public class InteractableObject: MonoBehaviour
{
    public string typeID;            // "" will always return false when comparing
    public GameObject spawn_object;
    public LayerMask cull_layer;
    public float cull_radius;        // How far this must be from other seeds and trees in order to grow.
    public float life_length;        // How long before the seed sprouts into a tree (or dies)

    private float lifeRemain;        // how long left before the seed dies

    // Use this for initialization
    void Start(){
        lifeRemain = life_length;
    }

    // Update is called once per frame
    void Update(){
        if(isHeld()){
            //do nothing
        }else{
            lifeRemain -= Globals.time_scale;
            
            if (lifeRemain < 0){
                Collider[] close_trees = Physics.OverlapSphere(transform.position, cull_radius, cull_layer);
                if(close_trees.Length < 1){
                    var RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    Instantiate(spawn_object, transform.position, RandomRotation);
                }
                Destroy(gameObject);
            }
            
            if(Globals.time_scale > 1){ // if fast forwarding
                RaycastHit hit;
                Ray rayDown = new Ray(transform.position, Vector3.down);
                int terrain = LayerMask.GetMask("Terrain");
                if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain))
                    transform.position = new Vector3(transform.position.x, hit.point.y + GetComponent<Collider>().bounds.extents.y, transform.position.z);
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                GetComponent<Rigidbody>().velocity = Vector3.zero;
            }else{
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            }
        }
    }
    
    private bool isHeld(){
        return Globals.PlayerScript.getRightObj() == this.gameObject || Globals.PlayerScript.getLeftObj() == this.gameObject;
    }

    // use this for TYPE of InteractableObject
    // use "==" (on the gameObject) for INSTANCE of object
    public bool sameType(InteractableObject x) {
        return typeID == x.typeID;
    }
}