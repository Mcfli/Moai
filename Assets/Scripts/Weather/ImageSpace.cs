using UnityEngine;
using System.Collections;
using UnityStandardAssets.ImageEffects;

public class ImageSpace : MonoBehaviour {
    public string imagespace_name;
    public float adjust_speed;

    public AnimationCurve redChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    public AnimationCurve greenChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    public AnimationCurve blueChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
    public float saturation = 1.0f;

    private ColorCorrectionCurves main_curves;

    public void applyToCamera()
    {
        main_curves = Camera.main.GetComponent<ColorCorrectionCurves>();
        
        main_curves.redChannel = curveLerp(redChannel);
        main_curves.greenChannel = greenChannel;
        main_curves.blueChannel = blueChannel;
        main_curves.UpdateParameters();
        
        main_curves.saturation = Mathf.Lerp(main_curves.saturation,saturation,Time.deltaTime*adjust_speed);
    }

    private AnimationCurve curveLerp(AnimationCurve c1,AnimationCurve c2,float time)
    {
        AnimationCurve newCurve = new AnimationCurve();

        foreach(Keyframe frame in c1.keys)
        {
            if (curveHasTime(newCurve, frame.time)) continue;
            Keyframe newKey = new Keyframe();
            newKey.time = frame.time;
            newKey.value = Mathf.Lerp(frame.value,c2.Evaluate(frame.time),time);
            newCurve.AddKey(newKey);
        }

        foreach (Keyframe frame in c2.keys)
        {
            if (curveHasTime(newCurve, frame.time)) continue;
            Keyframe newKey = new Keyframe();
            newKey.time = frame.time;
            newKey.value = Mathf.Lerp(c1.Evaluate(frame.time),frame.value, time);
            newCurve.AddKey(newKey);
        }

        return newCurve;
    }

    // Returns whether curve has a key at time
    private bool curveHasTime(AnimationCurve curve,float time)
    {
        foreach(Keyframe key in curve.keys)
        {
            if (key.time == time) return true;
        }
        return false;
    }
}
