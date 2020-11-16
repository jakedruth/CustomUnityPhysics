using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CollisionDetector
{
    public static bool DetectCollision(Particle2D particleA, Particle2D particleB)
    {
        float sqrDistance = (particleB.transform.position - particleA.transform.position).sqrMagnitude;
        float radiusSum = particleA.radius + particleB.radius;

        return sqrDistance <= radiusSum * radiusSum;
    }
}
