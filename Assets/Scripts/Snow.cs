using UnityEngine;
using System.Collections;

public class Snow : MonoBehaviour {
    public float radius;
    public ParticleSystem.MinMaxCurve rate;
    public float angle;
    private ChunkGenerator chunkGen;
    private ParticleSystem partSys;
    
	// Use this for initialization
	void Start () {
        GameObject chunk = chunkGen.chunk;
        partSys = chunk.AddComponent<ParticleSystem>();
        partSys.transform.rotation = Quaternion.Euler(90, 0, 0);
        radius = 1000;
        angle = 60;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.1f);
        curve.AddKey(0.75f, 1.0f);
        rate = new ParticleSystem.MinMaxCurve(100.0f, curve);
        // get the shape to modify
        var shape = partSys.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = angle;
        shape.radius = radius;
        
        // modify emission
        var emission = partSys.emission;
        emission.enabled = true;
        emission.type = ParticleSystemEmissionType.Time;
        emission.rate = rate;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
