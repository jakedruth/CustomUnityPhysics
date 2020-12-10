using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager
{
    internal static readonly List<MyRigidBody> Bodies;

    static WorldManager()
    {
        Bodies = new List<MyRigidBody>();
    }

    public static void RunPhysics(float dt)
    {
        ForceRegistry.FixedUpdate(dt);
        Integrate(dt);
    }

    private static void Integrate(float dt)
    {
        ForceManager.FixedUpdate(dt);
        for (int i = 0; i < Bodies.Count; i++)
        {
            Bodies[i].Integrate(dt);
        }
    }
}
