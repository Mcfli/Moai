using UnityEngine;
using System.Collections;

public class InteractableObject: MonoBehaviour
{

    public GameObject spawn_object;
    public LayerMask cull_layer;
    public float cull_radius;        // How far this must be from other seeds and trees in order to grow.
    public float life_length;        // How long before the seed sprouts into a tree (or dies)
    
    public enum Orientation{forward,backward,up,down,right,left};
    public Orientation heldOrientation;

    private float spawned_time;     // Stores what the game time was when this was spawned. 

    // Use this for initialization
    void Start(){
        spawned_time = Globals.time;
    }

    // Update is called once per frame
    void Update(){
        if(isHeld()){
        }else{
            if (Globals.time - spawned_time > life_length*Globals.time_resolution){
                Collider[] close_trees = Physics.OverlapSphere(transform.position, cull_radius, cull_layer);
                if(close_trees.Length < 1){
                    var RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    Instantiate(spawn_object, transform.position, RandomRotation);
                }
                Destroy(gameObject);
            }
            
            if(Globals.time_scale > 1){ //if fast forwarding
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
        Player p = Globals.Player.GetComponent<Player>();
        return p.getHand1() == this.gameObject || p.getHand2() == this.gameObject;
    }
}