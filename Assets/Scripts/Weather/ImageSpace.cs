using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class ImageSpace : MonoBehaviour {
    public string imagespace_name;
    public float adjust_speed;

    //public AnimationCurve redChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    //public AnimationCurve greenChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    //public AnimationCurve blueChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    public float saturation = 1.0f;

    private ColorCorrectionCurves main_curves;

    public void applyToCamera()
    {
        main_curves = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ColorCorrectionCurves>();
        /*
        main_curves.redChannel = redChannel;
        main_curves.greenChannel = greenChannel;
        main_curves.blueChannel = blueChannel;
        main_curves.zCurve = zCurve;
        main_curves.depthRedChannel = depthRedChannel;
        main_curves.depthGreenChannel = depthGreenChannel;
        main_curves.depthBlueChannel = depthBlueChannel;
        main_curves.UpdateParameters();
        */
        main_curves.saturation = Mathf.Lerp(main_curves.saturation,saturation,Time.deltaTime*adjust_speed);
    }
}
