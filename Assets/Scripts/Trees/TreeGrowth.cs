using UnityEngine;
using System.Collections;

public class TreeGrowth : MonoBehaviour {

    public float target_scale;
    public float grow_speed;
    public float grow_speed_variance;

    void Start()
    {
        grow_speed += Random.value * 2 * grow_speed_variance - grow_speed_variance;
    }

	private void Update(){
        Grow();
	}

    private void Grow() {
        Vector3 v3Scale = new Vector3(target_scale, target_scale, target_scale);
        transform.localScale = Vector3.Lerp(transform.localScale, v3Scale, Time.deltaTime * grow_speed);
    }
}
