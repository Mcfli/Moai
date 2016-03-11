using UnityEngine;
using System.Collections;

public class InteractableObject: MonoBehaviour
{
    public string typeID;            // "" will always return false when comparing
    public GameObject spawn_object;
    public LayerMask cull_layer;
    public float cull_radius;        // How far this must be from other seeds and trees in order to grow.
    public float life_length;        // How long before the object disappears
    public float growTime;           // How long before the seed sprouts
    public GameObject dirtMound;     // prefab - instantiates and hides when instatiated
    public Vector3 dirtMoundOffset;

    private Rigidbody thisRigidbody;
    private Collider thisCollider;
    private float timeRemain;        // how long left before the seed dies
    private bool planted;
    private bool wasHeld;

    void Awake() {
        thisRigidbody = GetComponent<Rigidbody>();
        thisCollider = GetComponent<Collider>();

        if (dirtMound) {
            dirtMound = Instantiate(dirtMound);
            dirtMound.transform.SetParent(transform, false);
            dirtMound.transform.position += dirtMoundOffset;
            dirtMound.transform.localScale = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);
            dirtMound.SetActive(false);
        }
        planted = false;
    }


    // Use this for initialization
    void Start(){
        timeRemain = life_length;
        wasHeld = false;
        StartCoroutine("tickUpdate");
    }

    // Update is called once per frame
    void Update() {
        if(!isHeld()) timeRemain -= Globals.deltaTime / Globals.time_resolution;
    }

    
    IEnumerator tickUpdate() {
        while (true) {
            yield return new WaitForSeconds(1);

            if (isHeld()) {
                wasHeld = true;
            } else if (planted) {
                wasHeld = false;
                if (timeRemain < 0) tryToTurnIntoTree();
            } else {
                if (wasHeld) { //just dropped
                    wasHeld = false;
                    timeRemain = life_length;
                    warpToGround(true);
                }
                if (timeRemain < 0) Destroy(gameObject);

                // if fast forwarding, warp to ground
                if (Globals.time_scale > 1) {
                    warpToGround(false);
                    thisRigidbody.isKinematic = true;
                } else {
                    thisRigidbody.isKinematic = false;
                }
            }
        }
    }

    private void tryToTurnIntoTree() {
        Collider[] close_trees = Physics.OverlapSphere(transform.position, cull_radius, cull_layer);
        if (close_trees.Length < 5) {
            var RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            Instantiate(spawn_object, transform.position, RandomRotation);
        }
        Destroy(gameObject);
    }

    private void warpToGround(bool onlyIfUnderground) {
        RaycastHit hit;
        Ray rayDown = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(rayDown, out hit, 10000000, LayerMask.GetMask("Terrain"))) {
            bool warp = false;
            if (onlyIfUnderground){
                if (hit.point.y + thisCollider.bounds.extents.y > transform.position.y) warp = true;
            } else {
                warp = true;
            }
            if(warp) transform.position = new Vector3(transform.position.x, hit.point.y + thisCollider.bounds.extents.y, transform.position.z);
        }
    }
    
    private bool isHeld(){
        return Globals.PlayerScript.getRightObj() == this || Globals.PlayerScript.getLeftObj() == this;
    }

    public bool canUse(RaycastHit r) {
        if (r.collider == null) return false;
        if (r.collider.gameObject.layer == LayerMask.NameToLayer("Terrain")) return true;
        return false;
    }

    public bool tryUse(RaycastHit r) {
        if (r.collider == null) return false;
        if (r.collider.gameObject.layer == LayerMask.NameToLayer("Terrain")) return plant(r.point);
        return false;
    }

    public void pickedUp() {
        unplant();
    }

    public bool plant(Vector3 place) {
        if (planted) return false;
        if (Globals.PlayerScript.getLeftObj() == this) Globals.PlayerScript.DropObject(true);
        if (Globals.PlayerScript.getRightObj() == this) Globals.PlayerScript.DropObject(false);
        transform.position = place;
        thisRigidbody.isKinematic = true;
        if(dirtMound) dirtMound.SetActive(true);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        timeRemain = growTime;
        planted = true;
        return true;
    }

    public bool unplant() {
        if (!planted) return false;
        thisRigidbody.isKinematic = false;
        if (dirtMound) dirtMound.SetActive(false);
        planted = false;
        return true;
    }

    // use this for TYPE of InteractableObject
    // use "==" (on the gameObject) for INSTANCE of object
    public bool sameType(InteractableObject x) {
        return typeID == x.typeID;
    }
}