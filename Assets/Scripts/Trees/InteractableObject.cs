using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InteractableObject: MonoBehaviour{
    public static float droppedObjectLifeLength = 300;

    //prefab
    public string typeID;            // "" will always return false when comparing
    public GameObject spawn_object;
    public LayerMask cull_layer;
    public float cull_radius;        // How far this must be from other seeds and trees in order to grow.
    public int growAttempts;         // times it tries to grow before killing itself
    public float growTime;           // How long before the seed sprouts
    public float growTimeVariance;   // ratio of variance if not player placed
    public LayerMask collisionLayer;
    public GameObject dirtMound;     // prefab - instantiates and hides when instatiated
    public Vector3 dirtMoundOffset;

    //references
    private Rigidbody thisRigidbody;
    private Collider thisCollider;

    private float timeRemain;        // how long until next check
    private bool planted;
    public bool playerPlanted;
    private bool held;
    private int attempts;

    public AudioClip PickUp;
    public AudioClip CorrectSpot;
    private AudioSource pickupDropAudio;

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
        playerPlanted = false;
    }


    // Use this for initialization
    void Start(){
        timeRemain = droppedObjectLifeLength;
        held = isHeld();
        pickupDropAudio = Globals.PlayerScript.pickupDropAudio;
    }

    // Update is called once per frame
    void Update() {
        if(!held) timeRemain -= Globals.deltaTime / Globals.time_resolution;

        if (held) { //held
            attempts = 0;
            timeRemain = Mathf.Infinity;
            playerPlanted = true;
        } else if (planted) { //planted
            if(timeRemain < 0 && spawn_object) tryToTurnIntoTree();
        } else { //dropped
            // if fast forwarding, warp to ground
            if(Globals.time_scale > 1) {
                warpToGround(0, transform.position, Mathf.Infinity);
                thisRigidbody.isKinematic = true;
                plant(transform.position);
            } else thisRigidbody.isKinematic = false;
            if(timeRemain < 0) Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision collision) {
        if(!planted && !held && collision.collider.gameObject.layer == LayerMask.NameToLayer("Terrain")) plant(collision.contacts[0].point);
    }

    private void tryToTurnIntoTree() {
        if (planted){
            if(Physics.OverlapSphere(transform.position, cull_radius, cull_layer).Length < 1) { //other conditions should go here
                var RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                GameObject g = Instantiate(spawn_object, transform.position + spawn_object.transform.position, RandomRotation) as GameObject;
                TreeScript t = g.GetComponent<TreeScript>();
                t.findForest();
                if(playerPlanted) t.lifeSpan = t.baseLifeSpan;
                Destroy(gameObject);
            } else if(attempts < growAttempts) {
                timeRemain = growTime;
                if(!playerPlanted) timeRemain *= Random.Range(-growTimeVariance, growTimeVariance);
                attempts++;
            } else Destroy(gameObject);
        }
    }

    private void warpToGround(float amountAboveGround, Vector3 origin, float maxDistance) {
        bool hitSuccess = false;
        RaycastHit hit;
        Ray rayFromTop = new Ray(origin, Vector3.down);
        if(Physics.Raycast(rayFromTop, out hit, maxDistance, collisionLayer)) hitSuccess = true;

        if(hitSuccess) {
            transform.position = new Vector3(transform.position.x, hit.point.y + amountAboveGround, transform.position.z);
            thisRigidbody.velocity = Vector3.zero;
            thisRigidbody.ResetInertiaTensor();
        }
    }
    
    private bool isHeld(){
        return Globals.PlayerScript.getHeldObj() == this;
    }

    public bool canUse(RaycastHit r) {
        if (r.collider == null) return false;
        if (r.collider.gameObject.layer == LayerMask.NameToLayer("Terrain")) return true;
        return false;
    }

    public bool tryUse(RaycastHit r) {
        if (r.collider == null) return false;
        if (r.collider.gameObject.layer == LayerMask.NameToLayer("Terrain")) {
            if(plant(r.point)) {
                pickupDropAudio.PlayOneShot(CorrectSpot, .2F);
                return true;
            } else return false;
        }

        return false;
    }

    public void pickedUp() {
        unplant();
        pickupDropAudio.PlayOneShot(PickUp, .2f);
        held = true;

        Vector2 coordinates = GenerationManager.worldToChunk(transform.position);
        if(DoodadManager.loaded_doodads.ContainsKey(coordinates)) DoodadManager.loaded_doodads[coordinates].Remove(gameObject);
    }

    public void dropped() {
        timeRemain = droppedObjectLifeLength;
        if(thisRigidbody) if(!thisRigidbody.isKinematic) warpToGround(thisCollider.bounds.extents.y * 2, transform.position + Vector3.up * thisCollider.bounds.extents.y * 2, thisCollider.bounds.extents.y * 2);
        held = false;
    }

    public bool plant(Vector3 place) {
        if (planted) return false;
        if (Globals.PlayerScript.getHeldObj() == this) Globals.PlayerScript.DropObject();
        transform.position = place;
        if(!thisRigidbody) thisRigidbody = GetComponent<Rigidbody>();
        thisRigidbody.isKinematic = true;
        if(dirtMound) dirtMound.SetActive(true);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        timeRemain = growTime;
        if(!playerPlanted) timeRemain *= Random.Range(1 - growTimeVariance, 1 + growTimeVariance);
        planted = true;
        Vector2 coordinates = GenerationManager.worldToChunk(transform.position);
        if(!DoodadManager.loaded_doodads.ContainsKey(coordinates)) DoodadManager.loaded_doodads[coordinates] = new List<GameObject>();
        DoodadManager.loaded_doodads[coordinates].Add(gameObject);
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