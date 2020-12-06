using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager
{
    public static WorldManager instance;
    private readonly List<MyRigidBody> _bodies;

    public WorldManager()
    {
        instance = this;
        _bodies = new List<MyRigidBody>();
    }

    public void RunPhysics(float dt)
    {
        ForceRegistry.FixedUpdate(dt);
        Integrate(dt);
    }

    private void Integrate(float dt)
    {
        ForceManager.FixedUpdate(dt);
        for (int i = 0; i > _bodies.Count; i++)
        {
            _bodies[i].Integrate(dt);
        }
    }

    public static void AddMyRigidBody(MyRigidBody body)
    {
        instance._bodies.Add(body);
    }
}
