using UnityEngine;
using System.Collections;

public class Tree : MonoBehaviour {

    public GameObject seed_object;
    public float max_life = 100;
    public float radius;
    public float spawn_delay;
    public float spawn_delay_variance;
    public int spawn_limit;
    public float cull_radius;
    public int cull_max_density;
    public float target_scale;
    public float grow_speed;
    public float grow_speed_variance;

    public Vector3 saved_position;
    public float age;
    public Quaternion saved_rotation;

    private LayerMask treeMask;
    private bool done = false;
    private float lastSpawned = 0.0f;
    private Vector2 squareVec;
    private int numSpawned;
    UnityRandom rand = new UnityRandom();
    private float life;
    private Animation anim;
    private float time_unloaded;

    public void saveTransforms()
    {
        saved_position = transform.position;
        saved_rotation = transform.rotation;
    }

    // Take all data from tree and copy it to this tree
    public void copyFrom(Tree tree)
    {
        seed_object = tree.seed_object;
        max_life = tree.max_life;
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
        Debug.Log(time_unloaded);
        age = tree.age + (Globals.time) * grow_speed;
        transform.position = tree.saved_position;
        transform.rotation = tree.saved_rotation;
   
        done = tree.done;
        numSpawned = tree.numSpawned;
        life = tree.life;


        
        foreach (AnimationState state in anim)
        {
            state.time = age;
        }
    }

    // Use this for initialization
    void Awake () {
        anim = GetComponent<Animation>();
        
        age = 0.0f;
        life = max_life;
        treeMask = LayerMask.GetMask("Tree");
        spawn_delay += rand.Value() * 2 * spawn_delay_variance - spawn_delay_variance;
        Collider[] hitColiders = Physics.OverlapSphere(Vector3.zero, radius);
        numSpawned = 0;
        lastSpawned = Time.time;
        grow_speed += Random.value * 2 * grow_speed_variance - grow_speed_variance;
        RaycastHit hit;
        Ray rayDown = new Ray(new Vector3(transform.position.x,10000000,transform.position.z), Vector3.down);
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
	
	// Update is called once per frame
	void Update () {
        stickToGround();
        Cull();
        Grow();
        Propogate();
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
                squareVec = rand.PointInASquare();
                var RandomRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
                Instantiate(seed_object, new Vector3(squareVec.x * radius + transform.position.x, 0, squareVec.y * radius + transform.position.z), RandomRotation);
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
        if (objectsInRange.Length > cull_max_density)
        {
            life -= 1;
            if(life <= 0)
            {
                done = true;

                Destroy(gameObject);
            }    
        }
        else if (life < max_life)
        {
            life += 0.1f;
        }
        else if (life > max_life)
        {
            life = max_life;
        }
    }

    private void Grow()
    {
        time_unloaded = Globals.time;
        if (age < 1000)
            age += Globals.time_scale * grow_speed;
        foreach (AnimationState state in anim)
        {
            //state.speed = Globals.time_scale*grow_speed;
            state.time = age;

        }
        
    }
}
