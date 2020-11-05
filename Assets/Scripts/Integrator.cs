using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Integrator : MonoBehaviour
{
    public static Integrator instance;
    private List<Particle2D> _particles;

    public static List<Particle2D> Particles
    {
        get { return instance._particles; }
    }

    void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }

        instance = this;
        _particles = new List<Particle2D>();
        DontDestroyOnLoad(gameObject);
    }

    void FixedUpdate()
    {
        Integrate();
    }

    private void Integrate()
    {
        for (int i = 0; i < _particles.Count; i++)
        {
            Particle2D particle = _particles[i];
            if (particle != null)
                Integrate(particle, Time.fixedDeltaTime);
            else
            {
                _particles.RemoveAt(i);
                i--;
            }
        }
    }

    private void Integrate(Particle2D particle, float dt)
    {
        if (particle.inverseMass <= 0) // Immovable Object
            return;

        // integrate velocity into position
        particle.transform.position += particle.velocity * dt;

        // get the acceleration
        Vector3 acceleration = particle.acceleration;

        // check to see if particle should ignore accumulated forces
        if (!particle.ignoreForces)
        {
            acceleration += particle.accumulatedForces * particle.inverseMass;
        }

        // integrate acceleration into velocity
        particle.velocity += acceleration * dt;

        // apply damping to the velocity
        float damping = Mathf.Pow(particle.dampingConstant, dt);
        particle.velocity *= damping;

        // clear accumulated forces
        particle.accumulatedForces = Vector3.zero;
    }

    public static void AddParticle(Particle2D particle)
    {
        instance._particles.Add(particle);
    }

    public static bool RemoveParticle(Particle2D particle)
    {
        return instance._particles.Remove(particle);
    }
}
