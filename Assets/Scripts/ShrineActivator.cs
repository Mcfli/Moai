using UnityEngine;
using System.Collections;

public class ShrineActivator : MonoBehaviour {

    public string element;

    private ShrineGrid parentShrine;

    public AudioClip ShrineComplete;
    AudioSource ShrineSuccess;

    // Click this to set parent shrine's element to this prefab's element
	// Use this for initialization
	void Start () {
        parentShrine = transform.parent.gameObject.GetComponent<ShrineGrid>();
        ShrineSuccess = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnMouseDown()
    {
        if (Time.timeScale > 0 && parentShrine != null && !parentShrine.isDone)
        {
            
            float dist = Vector3.Distance(transform.position, Globals.Player.transform.position);
            if (dist < 10f)
            {
                parentShrine.changeElement(element);

                //Shrine Completion Sound
                ShrineSuccess.PlayOneShot(ShrineComplete, .5F);
            }
        }
        
    }
}
