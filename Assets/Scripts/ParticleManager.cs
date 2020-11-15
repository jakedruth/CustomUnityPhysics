using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager
{
    public int iterations;
    public int maxContacts;
    public List<Particle2D> particles;
    public List<ParticleContactGenerator> contactGenerators;

    public ParticleManager(int maxContacts, int iterations)
    {
        this.maxContacts = maxContacts;
        ContactResolver.SetIterations(this.iterations = iterations);

        particles = new List<Particle2D>();
        contactGenerators = new List<ParticleContactGenerator>();
    }

    public void FixedUpdate(float dt)
    {
        // Update forces
        ForceManager.FixedUpdate(dt);

        // Integrate particles
        for (int i = 0; i < particles.Count; i++)
        {
            Integrator.Integrate(particles[i], dt);
        }

        Particle2DContact[] contacts = GenerateContacts();
        if (contacts.Length > 0)
        {
            ContactResolver.SetIterations(contacts.Length * 2);
            ContactResolver.ResolveContacts(contacts, dt);
        }
    }

    public Particle2DContact[] GenerateContacts()
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
