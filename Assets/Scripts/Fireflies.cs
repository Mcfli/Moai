using UnityEngine;
using System.Collections;

public class Fireflies : MonoBehaviour {
    ParticleSystem ff;
    private ParticleSystem.Particle[] m_Particles;
    public float particleVel = 0.0f;
    private ParticleSystem.LimitVelocityOverLifetimeModule fireflyVel;
    private ParticleSystem.EmissionModule fireflyEmission;
    private ParticleSystem.MinMaxCurve waitRate;

	// Use this for initialization
	void Start () {
        ff = GetComponent<ParticleSystem>();
        fireflyVel = ff.limitVelocityOverLifetime;
        fireflyEmission = ff.emission;
        waitRate = fireflyEmission.rate;
	}
	
	// Update is called once per frame
	void Update () {
        if (Globals.timeOfDay < 90 || Globals.timeOfDay > 270)
        {
            if(!ff.isPlaying)
            {
                ff.Play();
            }
        }
        else
        {
            if (ff.isPlaying)
            {
                ff.Stop();
            }
        }
        // speed up particles during waiting
        if (Globals.time_scale > 1)
        {
            if (ff.isPlaying)
            {
                InitializeIfNeeded();
                fireflyVel.dampen = 0.0f;
                waitRate.constantMax = 1000.0f;
                fireflyEmission.rate = waitRate;
                int numParticlesAlive = ff.GetParticles(m_Particles);
                for (int i = 0; i < numParticlesAlive; i++)
                {
                    m_Particles[i].velocity = m_Particles[i].velocity.normalized * Globals.time_scale * particleVel;
                    m_Particles[i].lifetime += Time.deltaTime - (Time.deltaTime * Globals.time_scale) * 2;
                }
                ff.SetParticles(m_Particles, numParticlesAlive);
            }
        }
        else
        {
            if (ff.isPlaying)
            {
                if (fireflyEmission.rate.constantMax == 1000.0f)
                {
                    fireflyVel.dampen = 1;
                    waitRate.constantMax = 12.0f;
                    fireflyEmission.rate = waitRate;

                }
            }
        }
	}

    void InitializeIfNeeded()
    {
        if (m_Particles == null || m_Particles.Length < ff.maxParticles)
        {
            m_Particles = new ParticleSystem.Particle[ff.maxParticles];
        }
    }
}
