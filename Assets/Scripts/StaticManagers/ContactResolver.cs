using System.Runtime.CompilerServices;
using UnityEngine;

public static class ContactResolver
{
    private static int _iterations;
    public static int iterationsUsed;

    private static int _positionIterationsUsed;
    private static int _positionIterations;

    private static int _velocityIterationsUsed;
    private static int _velocityIterations;

    private static float _positionEpsilon = 0.01f;
    private static float _velocityEpsilon = 0.01f;

    public static void SetIterations(int iterationCount)
    {
        _iterations = iterationCount;
        SetIterations(iterationCount, iterationCount);
    }

    public static void SetIterations(int positionIterations, int velocityIterations)
    {
        _positionIterations = positionIterations;
        _velocityIterations = velocityIterations;
    }

    public static void SetEpsilon(int positionEpsilon, int velocityEpsilon)
    {
        _positionEpsilon = positionEpsilon;
        _velocityEpsilon = velocityEpsilon;
    }

    public static void ResolveContacts(Particle2DContact[] contacts, float dt)
    {
        iterationsUsed = 0;
        while (iterationsUsed < _iterations)
        {
            float max = float.MaxValue;
            int maxIndex = -1;
            for (int i = 0; i < contacts.Length; i++)
            {
                float separationVelocity = contacts[i].CalculateSeparatingVelocity();
                if (separationVelocity < max && (separationVelocity < 0 || contacts[i].penetration > 0))
                {
                    max = separationVelocity;
                    maxIndex = i;
                }
            }

            if (maxIndex == -1)
                break;

            contacts[maxIndex].Resolve(dt);

            Vector3 moveA = contacts[maxIndex].particleMovementA;
            Vector3 moveB = contacts[maxIndex].particleMovementB;

            for (int i = 0; i < contacts.Length; i++)
            {
                if (contacts[i].particleA == contacts[maxIndex].particleA)
                {
                    contacts[i].penetration -= Vector3.Dot(moveA, contacts[i].contactNormal);
                }
                else if (contacts[i].particleA == contacts[maxIndex].particleB)
                {
                    contacts[i].penetration -= Vector3.Dot(moveB, contacts[i].contactNormal);
                }

                if (contacts[maxIndex].particleB == null)
                    continue;

                if (contacts[i].particleB == contacts[maxIndex].particleA)
                {
                    contacts[i].penetration += Vector3.Dot(moveA, contacts[i].contactNormal);
                }
                else if (contacts[i].particleB == contacts[maxIndex].particleB)
                {
                    contacts[i].penetration += Vector3.Dot(moveB, contacts[i].contactNormal);
                }
            }

            iterationsUsed++;
        }
    }

    public static void ResolveContacts(CollisionDetector.Contact[] contacts, float dt)
    {
        // Make sure there are contacts to resolve
        if (contacts.Length == 0)
            return;

        // Prepare the contacts for processing
        PrepareContacts(contacts, dt);

        // Resolve the interpenetration problems with the contacts
        AdjustPositions(contacts, dt);

        // Resolve the velocity problems with the contacts
        AdjustVelocities(contacts, dt);
    }

    private static void PrepareContacts(CollisionDetector.Contact[] contacts, float dt)
    {
        for (int i = 0; i < contacts.Length; i++)
        {
            contacts[i].CalculateInternals(dt);
        }
    }

    private static void AdjustPositions(CollisionDetector.Contact[] contacts, float dt)
    {
        // Iteratively resolve interpenetrations in order of severity
        _positionIterationsUsed = 0;
        while (_positionIterationsUsed < _positionIterations)
        {
            // find the biggest penetration
            float max = _positionEpsilon;
            int index = -1;
            for (int i = 0; i < contacts.Length; i++)
            {
                if (contacts[i].penetration > max)
                {
                    max = contacts[i].penetration;
                    index = i;
                }
            }

            if (index < 0)
                break;

            // TODO: Match the awake state at the contact
            // contacts[index].MatchAwakeState();

            // Resolve the penetration.
            contacts[index].ApplyPositionChange(out Vector3[] linearChange, out Vector3[] angularChange, max);

            // Again this action may have changed the penetration of other bodies,
            // so we update the contacts
            for (int i = 0; i < contacts.Length; i++)
            {
                for (int b = 0; b < 2; b++)
                {
                    if (contacts[i].bodies[b] == null)
                        continue;
                    
                    // Check for a match with each body in the newly resolved contact
                    for (int d = 0; d < 2; d++)
                    {
                        if (contacts[i].bodies[b] != contacts[index].bodies[d]) 
                            continue;
                        
                        Vector3 deltaPosition = linearChange[d] + Vector3.Cross(angularChange[d],
                            contacts[i].relativeContactPosition[b]);

                        // The sign of the change is positive if we're dealing with the second body in
                        // a contact and a negative otherwise (because we're subtracting the resolution)
                        float dotProduct = Vector3.Dot(deltaPosition, contacts[i].normal);
                        contacts[i].penetration += dotProduct * ( b == 1 ? 1 : -1);
                    }
                }
            }

            _positionIterationsUsed++;
        }
    }

    private static void AdjustVelocities(CollisionDetector.Contact[] contacts, float dt)
    {
        // Iteratively handle impacts in order of severity
        _velocityIterationsUsed = 0;
        while (_velocityIterationsUsed < _velocityIterations)
        {
            // Find contact with maximum magnitude of probable velocity change
            float max = _velocityEpsilon;
            int index = -1;
            for (int i = 0; i < contacts.Length; i++)
            {
                if (contacts[i].desiredDeltaVelocity > max)
                {
                    max = contacts[i].desiredDeltaVelocity;
                    index = i;
                }
            }

            if (index == -1)
                break;

            // TODO: Match the awake state at the contact
            // contacts[index].matchAwakeState();

            // Do the resolution on the contact
            contacts[index].ApplyVelocityChange(out Vector3[] velocityChange, out Vector3[] rotationChange);

            // With the change in velocity of the two bodies, the update of contact velocity means
            // that some of the relative closing velocities need recomputing
            for (int i = 0; i < contacts.Length; i++)
            {
                // Check each body in the contact
                for (int b = 0; b < 2; b++)
                {
                    if (contacts[i].bodies[b] != null)
                    {
                        // Check for a match with each body in the newly resolve contact
                        for (int d = 0; d < 2; d++)
                        {
                            Vector3 deltaVel = velocityChange[d] + Vector3.Cross(rotationChange[d],
                                contacts[i].relativeContactPosition[b]);

                            // The sign of the change is negative if we're dealing with the second body in a contact
                            contacts[i].contactVelocity +=
                                contacts[i].contactToWorld.TransformTranspose(deltaVel) * (b == 1 ? -1 : 1);
                            contacts[i].CalculateDesiredDeltaVelocity(dt);
                        }
                    }
                }
            }

            _velocityIterationsUsed++;
        }
    }
}
