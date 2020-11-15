using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ParticleContactGenerator
{
    /// <summary>
    /// Generates the contacts to keep this link from being violated.
    /// </summary>
    /// <param name="contacts">refrence to a particle contact</param>
    /// <param name="limit">The limit</param>
    /// <returns>0 if no contact, 1 if there is a contact</returns>
    public abstract int AddContact(ref List<Particle2DContact> contacts, int limit);
}


public abstract class Particle2DLink : ParticleContactGenerator
{
    public Particle2D particleA;
    public Particle2D particleB;

    protected Particle2DLink(Particle2D particleA, Particle2D particleB)
    {
        this.particleA = particleA;
        this.particleB = particleB;
    }

    protected float CurrentLength()
    {
        if (particleA == null || particleB == null)
            return -1;

        return Vector3.Distance(particleA.transform.position, particleB.transform.position);
    }
}

public class ParticleCable : Particle2DLink
{
    public float maxLength;
    public float restitution;

    public ParticleCable(Particle2D particleA, Particle2D particleB, float maxLength, float restitution) 
        : base(particleA, particleB)
    {
        this.maxLength = maxLength;
        this.restitution = restitution;
    }

    public override int AddContact(ref List<Particle2DContact> contacts, int limit)
    {
        float length = CurrentLength();
     
        // check if the cable is overextended
        if (length < maxLength)
            return 0;

        Particle2DContact contact = new Particle2DContact();

        // initialize the contact
        contact.particleA = particleA;
        contact.particleB = particleB;

        // calculate the normal
        Vector3 normal = particleB.transform.position - particleA.transform.position;
        normal.Normalize();
        contact.contactNormal = normal;

        contact.penetration = length - maxLength;
        contact.restitution = restitution;

        contacts.Add(contact);

        return 1;
    }
}

public class ParticleRod : Particle2DLink
{
    public float targetLength;

    public ParticleRod(Particle2D particleA, Particle2D particleB, float length) : base(particleA, particleB)
    {
        targetLength = length;
    }

    public override int AddContact(ref List<Particle2DContact> contacts, int limit)
    {
        float length = CurrentLength();

        // check if the rod not at the targetLength or if no length
        if (Math.Abs(length - targetLength) < 0.001f || length < 0)
            return 0;

        Particle2DContact contact = new Particle2DContact();

        // initialize the contact
        contact.particleA = particleA;
        contact.particleB = particleB;

        // calculate the normal
        Vector3 normal = particleB.transform.position - particleA.transform.position;
        normal.Normalize();
        
        // determine if the rod is extending or compressing
        if (length > targetLength)
        {
            contact.contactNormal = normal;
            contact.penetration = length - targetLength;
        }
        else
        {
            contact.contactNormal = -normal;
            contact.penetration = targetLength - length;
        }

        // always us zero restitution (no bounciness).
        contact.restitution = 0;

        contacts.Add(contact);

        return 1;
    }
}
