using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Integrator
{
    public static readonly List<Particle2D> Particles;

    static Integrator()
    {
        Particles = new List<Particle2D>();
    }

    public static void FixedUpdate(float dt)
    {
        Integrate(dt);
    }

    private static void Integrate(float dt)
    {
        for (int i = 0; i < Particles.Count; i++)
        {
            Particle2D particle = Particles[i];
            if (particle != null)
                Integrate(particle, dt);
            else
            {
                Particles.RemoveAt(i);
                i--;
            }
        }
    }

    private static void Integrate(Particle2D particle, float dt)
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
        Particles.Add(particle);
    }

    public static bool RemoveParticle(Particle2D particle)
    {
        return Particles.Remove(particle);
    }

    public static void AddForce(Particle2D particle, Vector3 force)
    {
        particle.AddForce(force);
    }
}
