using UnityEngine;
using System.Collections;

public class TreeScript : MonoBehaviour {

    public GameObject seed_object;
    public GameObject prefab;
    public float max_health = 100;
    public float radius;
    public float spawn_delay;
    public float spawn_delay_variance;
    public int spawn_limit;
    public float cull_radius;
    public int cull_max_density;
    public float target_scale;
    public float grow_speed;
    public float grow_speed_variance;
    public float life_span;
    public float growDieAnimationRatio = 0.1f;
    // this means the first 10% of its health will have growing animation, and the last 10% dying animation
    public AnimationCurve height_vs_time;

    public Vector3 saved_position;
    public float age;
    public Quaternion saved_rotation;

    public float cul_spread;
    public bool onFire;
    
    public bool useNewAnimationSystem; //should be deleted when done debugging
    
    private GameObject player;
    private LayerMask treeMask;
    private bool done = false;
    private float lastSpawned = 0.0f;
    private int numSpawned;
    private float health;
    private Animation anim;
    private float time_unloaded;
    
    private int state; //0:growing,1:mature,2:dying


    private GameObject fire;
    private GameObject Torch;


    public void saveTransforms(){
        saved_position = transform.position;
        saved_rotation = transform.rotation;
    }

    // Take all data from tree and copy it to this tree
    public void copyFrom(TreeScript tree){ //WTF IS THIS
        prefab = tree.prefab;
        seed_object = tree.seed_object;
        max_health = tree.max_health;
        radius = tree.radius;
        spawn_delay = tree.spawn_delay;
        spawn_delay_variance = tree.spawn_delay_variance;
        spawn_limit = tree.spawn_limit;
        cull_radius = tree.cull_radius;
        cull_max_density = tree.cull_max_density;
        target_scale = tree.target_scale;
        grow_speed = tree.grow_speed;
        grow_speed_variance = tree.grow_speed_variance;
        saved_position = tree.saved_position;
        saved_rotation = tree.saved_rotation;
        time_unloaded = tree.time_unloaded;
        transform.position = tree.saved_position;
        transform.rotation = tree.saved_rotation;
        growDieAnimationRatio = tree.growDieAnimationRatio;
        done = tree.done;
        numSpawned = tree.numSpawned;
        health = tree.health;
        age = tree.age + (Globals.time) * grow_speed;
        //foreach (AnimationState state in anim) state.time = age;
    }

    // Use this for initialization
    void Awake(){
        anim = GetComponent<Animation>();
        fire = Resources.Load("fire") as GameObject;
        player = GameObject.FindGameObjectWithTag("Player");
        
        age = 0.0f;
        health = max_health;
        treeMask = LayerMask.GetMask("Tree");
        spawn_delay += Random.value * 2 * spawn_delay_variance - spawn_delay_variance;
        //Collider[] hitColiders = Physics.OverlapSphere(Vector3.zero, radius);
        numSpawned = 0;
        lastSpawned = Time.time;
        grow_speed += Random.value * 2 * grow_speed_variance - grow_speed_variance;
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(transform.position.x,10000000,transform.position.z), Vector3.down);
        int terrain = LayerMask.GetMask("Terrain");
        
        foreach (AnimationState animState in anim) animState.speed = 0; //fixes twitching
        
        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain)){

            if (hit.point.y < Globals.water_level)
                Destroy(gameObject);
            else
                transform.position = new Vector3(transform.position.x, hit.point.y - 1, transform.position.z);
        }
        else{
            Destroy(gameObject);
        }
	}
	
	// Update is called once per frame
	void Update () {
        stickToGround();
        Cull();
        Grow();
        if(age >= 0.03)
            Propogate();
        if (onFire)
            fireSpread();
    }

    private void stickToGround()
    {
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(transform.position.x, 10000000, transform.position.z), Vector3.down);
        int terrain = LayerMask.GetMask("Terrain");

        if (Physics.Raycast(rayDown, out hit, Mathf.Infinity, terrain))
        {
            {
                transform.position = new Vector3(transform.position.x, hit.point.y - 1, transform.position.z);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Creates seeds in a radius around this tree if it's ready to
    private void Propogate()
    {
        if (!done)
        {
            if (Time.time - lastSpawned > spawn_delay)
            {
                lastSpawned = Time.time;
                Vector2 randomPoint = Random.insideUnitCircle;
                var RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                Instantiate(seed_object, new Vector3(randomPoint.x * radius + transform.position.x, transform.position.y + 10f, randomPoint.y * radius + transform.position.z), RandomRotation);
                numSpawned++;
                if (spawn_limit > 0 && numSpawned >= spawn_limit)
                {
                    done = true;
                }
            }
        }
    }

    // If there are too many or nearby trees for too long, destroy this one
    private void Cull()
    {
        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, cull_radius, treeMask);
        if (age > life_span || objectsInRange.Length > cull_max_density)
        {
            health -= 1;
            if(health <= 0)
            {
                done = true;

                Destroy(gameObject);
            }    
        }
        else if (health < max_health)
        {
            health += 0.1f;
        }
        else if (health > max_health)
        {
            health = max_health;
        }
    }

    private void Grow()
    {
        float anim_progress = 0.0f;
        time_unloaded = Globals.time;
        if (age < 1000)
            age += Globals.time_scale * grow_speed;
        
        if(useNewAnimationSystem){
            if(age/life_span < growDieAnimationRatio){//growing
                if(!anim.IsPlaying("growing")) anim.Play("growing");
                anim["growing"].time = anim["growing"].length * age/(life_span*growDieAnimationRatio);
                state = 0;
            }else if(age/life_span > 1 - growDieAnimationRatio){//dying
                if(!anim.IsPlaying("dying")) anim.Play("dying");
                anim["dying"].time = anim["dying"].length * (age - (1 - growDieAnimationRatio)*life_span) / (life_span*growDieAnimationRatio);
                state = 1;
            }else{ //mature
                if(!anim.IsPlaying("growing")) anim.Play("growing");
                anim["growing"].time = anim["growing"].length;
                state = 2;
            }
        }else{
            state = 1;
            foreach (AnimationState animState in anim){
                //state.speed = Globals.time_scale*grow_speed;
                animState.time = age;
                anim_progress = animState.time / animState.length;
            }
        }
        float growth = height_vs_time.Evaluate(anim_progress);
        GetComponent<BoxCollider>().size = new Vector3(0.4f+0.6f*growth, target_scale*growth, 0.4f+0.6f*growth);
        GetComponent<BoxCollider>().center = new Vector3(0, target_scale*growth*0.5f, 0);

    }
    
    private bool playerHasObject(string objName){
        if (player.GetComponent<Player>().getHand1())
            if (player.GetComponent<Player>().getHand1().name == objName)
                return true;
        if (player.GetComponent<Player>().getHand2())
            if (player.GetComponent<Player>().getHand2().name == objName)
                return true;
        return false;
    }

    void OnMouseDown(){
        if (playerHasObject("Torch")){
            if (!onFire) onFire = true;
            /*GameObject Instance = (GameObject)*/Instantiate(fire, transform.position, Quaternion.identity);
        }
    }
    
    private void fireSpread(){
        Collider[] objectsInRange = Physics.OverlapSphere(transform.position, cul_spread, treeMask);
        for (int i = 0; i < objectsInRange.Length; i++){
            GameObject curtree = objectsInRange[i].gameObject;
            if (!curtree.GetComponent<TreeScript>().onFire){
                curtree.GetComponent<TreeScript>().onFire = true;
                /*GameObject Instance = (GameObject)*/Instantiate(fire, curtree.transform.position, Quaternion.identity);
            }        
        }
    }
    
    public int getState(){ //0:growing,1:mature,2:dying,3:burnt
        if(onFire) return 3;
        else return state;
    }
}
