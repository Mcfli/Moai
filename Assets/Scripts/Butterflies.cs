using UnityEngine;
using System.Collections;

public class Butterflies : MonoBehaviour {

    private ParticleSystem butterflies;
    private ParticleSystem.Particle[] m_Particles;
    public float particleVel = 0.0f;
    private ParticleSystem.LimitVelocityOverLifetimeModule butterflyVel;
    private ParticleSystem.EmissionModule butterflyEmission;
    private ParticleSystem.MinMaxCurve waitRate;

	// Use this for initialization
	void Start () {
        butterflies = GetComponent<ParticleSystem>();
        butterflyVel = butterflies.limitVelocityOverLifetime;
        butterflyEmission = butterflies.emission;
        waitRate = butterflyEmission.rate;
	}
	
	// Update is called once per frame
	void Update () {
        if (Globals.timeOfDay > 90 && Globals.timeOfDay < 270)
        {
            if (!butterflies.isPlaying)
            {
                butterflies.Play();
            }
        }
        else
        {
            if (butterflies.isPlaying)
            {
                butterflies.Stop();
            }
        }

        // speed up particles during waiting
        if (Globals.time_scale > 1)
        {
            if (butterflies.isPlaying)
            {
                InitializeIfNeeded();
                butterflyVel.dampen = 0.0f;
                waitRate.constantMax = 1000.0f;
                butterflyEmission.rate = waitRate;
                int numParticlesAlive = butterflies.GetParticles(m_Particles);
                for (int i = 0; i < numParticlesAlive; i++)
                {
                    m_Particles[i].velocity = m_Particles[i].velocity.normalized * Globals.time_scale * particleVel;
                    m_Particles[i].lifetime += Time.deltaTime - (Time.deltaTime * Globals.time_scale) * 2;
                }
                butterflies.SetParticles(m_Particles, numParticlesAlive);
            }
        }
        else
        {
            if (butterflies.isPlaying)
            {
                if (butterflyEmission.rate.constantMax == 1000.0f)
                {
                    butterflyVel.dampen = 1;
                    waitRate.constantMax = 12.0f;
                    butterflyEmission.rate = waitRate;

                }
            }
        }
	}

    void InitializeIfNeeded()
    {
        if (m_Particles == null || m_Particles.Length < butterflies.maxParticles)
        {
            m_Particles = new ParticleSystem.Particle[butterflies.maxParticles];
        }
    }
}
