using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ParticleContactGenerator
{
    /// <summary>
    /// Generates the contacts to keep this link from being violated.
    /// </summary>
    /// <param name="contact">refrence to a particle contact</param>
    /// <param name="limit">The limit</param>
    /// <returns>0 if no contact, 1 if there is a contact</returns>
    public abstract int AddContact(ref Particle2DContact contact, int limit);
}


public abstract class Particle2DLink : ParticleContactGenerator
{
    public Particle2D particleA;
    public Particle2D particleB;

    protected float CurrentLength()
    {
        return Vector3.Distance(particleA.transform.position, particleB.transform.position);
    }
}

public class ParticleCable : Particle2DLink
{
    public float maxLength;
    public float restitution;
    public override int AddContact(ref Particle2DContact contact, int limit)
    {
        float length = CurrentLength();
     
        // check if the cable is overextended
        if (length < maxLength)
            return 0;

        // initialize the contact
        contact.particleA = particleA;
        contact.particleB = particleB;

        // calculate the normal
        Vector3 normal = particleB.transform.position - particleA.transform.position;
        normal.Normalize();
        contact.contactNormal = normal;

        contact.penetration = length - maxLength;
        contact.restitution = restitution;

        return 1;
    }
}

public class ParticleRod : Particle2DLink
{
    public float length;

    public override int AddContact(ref Particle2DContact contact, int limit)
    {
        float currentLength = CurrentLength();

        // check if the rod is too long or too short
        if (Math.Abs(currentLength - length) < 0.001f)
            return 0;

        // initialize the contact
        contact.particleA = particleA;
        contact.particleB = particleB;

        // calculate the normal
        Vector3 normal = particleB.transform.position - particleA.transform.position;
        normal.Normalize();
        
        // determine if the rod is extending or compressing
        if (currentLength > length)
        {
            contact.contactNormal = normal;
            contact.penetration = currentLength - length;
        }
        else
        {
            contact.contactNormal = -normal;
            contact.penetration = length - currentLength;
        }

        // always us zero restitution (no bounciness).
        contact.restitution = 0;

        return 1;
    }
}
