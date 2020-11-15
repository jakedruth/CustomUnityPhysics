using UnityEngine;

public static class ContactResolver
{
    private static int iterations;
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
}
