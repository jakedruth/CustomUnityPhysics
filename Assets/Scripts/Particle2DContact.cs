using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle2DContact
{
    public Particle2D particleA;
    public Particle2D particleB;
    public float restitution;
    public Vector3 contactNormal;
    public float penetration;

    public Vector3 particleMovementA;
    public Vector3 particleMovementB;

    public Particle2DContact(Particle2D particleA, Particle2D particleB, float restitution)
    {
        this.particleA = particleA;
        this.particleB = particleB;
        this.restitution = restitution;
    }

    public void Resolve(float dt)
    {
        ResolveVelocity(dt);
        ResolveInterpenetration(dt);
    }

    public float CalculateSeparatingVelocity()
    {
        Vector3 relativeVelocity = particleA.velocity;
        if (particleB != null)
            relativeVelocity -= particleB.velocity;

        return Vector3.Dot(relativeVelocity, contactNormal);
    }

    private void ResolveVelocity(float dt)
    {
        // Find the velocity in the direction of the contact.
        float separatingVelocity = CalculateSeparatingVelocity();
        
        // Check if it needs to be resolved.
        if (separatingVelocity > 0)
        {
            // The contact is either separating, or stationary;
            // no impulse is required.
            return;
        }

        // Calculate the new separating velocity.
        float newSeparatingVelocity = -separatingVelocity * restitution;

        // Check the velocity buildup due to acceleration only
        Vector3 accCausedVelocity = particleA.acceleration;
        if (particleB != null)
        {
            accCausedVelocity -= particleB.acceleration;
        }
        float accCausedSeparationVelocity = Vector3.Dot(accCausedVelocity, contactNormal) * dt;

        // if we've got a closing velocity due to acceleration buildup,
        // remove it from the new separating velocity
        if (accCausedSeparationVelocity < 0)
        {
            newSeparatingVelocity += restitution * accCausedSeparationVelocity;

            // Make sure we haven't removed more than was there to remove
            if (newSeparatingVelocity < 0)
                newSeparatingVelocity = 0;
        }

        float deltaVelocity = newSeparatingVelocity - separatingVelocity;
        
        // Apply the change in velocity to each object in proportion to their inverse mass
        // (i.e., those with lower inverse mass [higher actual mass] get less change in velocity).
        float totalInverseMass = particleA.inverseMass;
        if (particleB != null)
            totalInverseMass += particleB.inverseMass;
        
        // If all particles have infinite mass, then impulses have no effect.
        if (totalInverseMass <= 0)
            return;
        
        // Calculate the impulse to apply.
        float impulse = deltaVelocity / totalInverseMass;
        
        // Find the amount of impulse per unit of inverse mass.
        Vector3 impulsePerIMass = contactNormal * impulse;
        
        // Apply impulses: they are applied in the direction of the contact,
        // and are proportional to the inverse mass.
        particleA.velocity += impulsePerIMass * particleA.inverseMass;

        if (particleB != null)
        {
            // Particle B goes in the opposite direction
            particleB.velocity += impulsePerIMass * -particleB.inverseMass;
        }

    }

    private void ResolveInterpenetration(float dt)
    {
        // If we don’t have any penetration, skip this step.
        if (penetration <= 0) 
            return;
        
        // The movement of each object is based on their inverse mass,
        // so total that.
        float totalInverseMass = particleA.inverseMass;
        if (particleB != null) 
            totalInverseMass += particleB.inverseMass;

        // If all particles have infinite mass, then we do nothing.
        if (totalInverseMass <= 0) 
            return;
        
        // Find the amount of penetration resolution per unit
        // of inverse mass.
        Vector3 movePerIMass = contactNormal * (penetration / totalInverseMass);
        
        // Calculate the movement amounts.
        particleMovementA = movePerIMass * particleA.inverseMass;
        if (particleB != null)
        {
            particleMovementB = movePerIMass * -particleB.inverseMass;
        } 
        else 
        {
            particleMovementB = Vector3.zero;
        }

        // Apply the penetration resolution.
        particleA.transform.position += particleMovementA;
        if (particleB != null) 
        {
            particleB.transform.position += particleMovementB;
        }
    }
}
