using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Integrator
{
    static Integrator()
    { }

    public static void Integrate(Particle2D particle, float dt)
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

    public static void AddForce(Particle2D particle, Vector3 force)
    {
        particle.AddForce(force);
    }
}
