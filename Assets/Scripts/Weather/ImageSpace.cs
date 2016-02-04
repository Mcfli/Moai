using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class ImageSpace : MonoBehaviour {
    public string imagespace_name;
    public float adjust_speed;

    public AnimationCurve redChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    public AnimationCurve greenChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    public AnimationCurve blueChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

    public AnimationCurve zCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    public AnimationCurve depthRedChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    public AnimationCurve depthGreenChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    public AnimationCurve depthBlueChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

    private ColorCorrectionCurves main_curves;

    // Use this for initialization
    void Start () {
        main_curves = GameObject.FindGameObjectWithTag("Main Camera").GetComponent<ColorCorrectionCurves>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void applyToCamera()
    {
        main_curves.redChannel = redChannel;
        main_curves.greenChannel = greenChannel;
        main_curves.blueChannel = blueChannel;
        main_curves.zCurve = zCurve;
        main_curves.depthRedChannel = depthRedChannel;
        main_curves.depthGreenChannel = depthGreenChannel;
        main_curves.depthBlueChannel = depthBlueChannel;
    }
}
