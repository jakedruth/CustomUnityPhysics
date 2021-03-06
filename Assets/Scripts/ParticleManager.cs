﻿using System;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public static class ParticleManager
{
    public static int iterations;
    public static int maxContacts;
    public static List<Particle2D> particles;
    public static List<ParticleContactGenerator> contactGenerators;

    static ParticleManager()
    {
        maxContacts = int.MaxValue;
        ContactResolver.SetIterations(iterations = int.MaxValue);

        particles = new List<Particle2D>();
        contactGenerators = new List<ParticleContactGenerator>();
    }

    public static void RunPhysics(float dt)
    {
        // Update forces
        ForceManager.FixedUpdate(dt);

        // Integrate particles
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            Integrator.Integrate(particles[i], dt);
        }

        // Detect if 2 Particles are colliding
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            // Check if particle is Target
            if (particles[i].tag == "Target")
                continue;


            // Check if particle is below Y Kill zone
            if (particles[i].transform.position.y <= -10)
            {
                Object.Destroy(particles[i].gameObject);
                continue;
            }

            for (int j = particles.Count - 1; j > i; j--)
            {
                if (i == j)
                    continue;

                // Check if particle is Target
                if (particles[j].tag == "Target")
                    continue;

                if (CollisionDetector.DetectCollision(particles[i], particles[j]))
                {
                    Object.Destroy(particles[i].gameObject);
                    Object.Destroy(particles[j].gameObject);
                }
            }
        }

        Particle2DContact[] contacts = GenerateContacts();
        if (contacts.Length > 0)
        {
            ContactResolver.SetIterations(contacts.Length * 2);
            ContactResolver.ResolveContacts(contacts, dt);
        }
    }

    public static Particle2DContact[] GenerateContacts()
    {
        int limit = maxContacts;
        List<Particle2DContact> contacts = new List<Particle2DContact>();
        for (int i = contactGenerators.Count - 1; i >= 0; i--)
        {
            int used = contactGenerators[i].AddContact(ref contacts, limit);
            limit -= used;

            if (limit < 0) // We've run ou of contacts to fill and we're missing some contacts
                break;
        }

        return contacts.ToArray();
    }
}
