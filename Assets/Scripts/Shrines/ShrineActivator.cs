using UnityEngine;
using System.Collections;

public class ShrineActivator : MonoBehaviour {

    public string element;
    public static bool firstActivate = false;

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

    public bool activate()
    {
        if(active()) return false;
        if (Time.timeScale > 0 && parentShrine != null && !parentShrine.isDone)
        {
            parentShrine.changeElement(element);

            //Shrine Completion Sound
            ShrineSuccess.PlayOneShot(ShrineComplete, .5F);
            firstActivate = true;

            return true;
        }
        return false;
        
    }

    public bool active() {
        return parentShrine.getElement() == element || parentShrine.isDone;
    }
}
