using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ContactResolver
{
    public static int iterations;
    public static int iterationsUsed;

    public static void SetIterations(int iterationCount)
    {
        iterations = iterationCount;
    }

    public static void ResolveContacts(Particle2DContact[] contacts, float dt)
    {
        iterationsUsed = 0;
        while (iterationsUsed < iterations)
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
            iterationsUsed++;
        }
    }
}
