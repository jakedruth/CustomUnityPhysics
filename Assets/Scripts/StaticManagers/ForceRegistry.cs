using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class ForceRegistry
{
    internal struct ForceBodyPair
    {
        public ForceGenerator forceGenerator;
        public MyRigidBody rigidBody;

        internal ForceBodyPair(ForceGenerator fg, MyRigidBody body)
        {
            forceGenerator = fg;
            rigidBody = body;
        }

        public void UpdateForce(float dt)
        {
            forceGenerator.UpdateForce(rigidBody, dt);
        }
    }

    private static readonly List<ForceBodyPair> Registry;

    static ForceRegistry()
    {
        Registry = new List<ForceBodyPair>();
    }

    public static void FixedUpdate(float dt)
    {
        for (int i = Registry.Count - 1; i >= 0; i--)
        {
            ForceBodyPair pair = Registry[i];
            if (pair.forceGenerator == null || pair.rigidBody)
            {
                Remove(pair);
                continue;
            }

            if (pair.forceGenerator.isOn)
                pair.UpdateForce(dt);
        }
    }

    public static void Add(ForceGenerator forceGenerator, MyRigidBody rigidBody)
    {
        Registry.Add(new ForceBodyPair(forceGenerator, rigidBody));
    }

    public static void Remove(ForceGenerator forceGenerator)
    {
        for (int i = Registry.Count - 1; i >= 0; i--)
        {
            if (Registry[i].forceGenerator == forceGenerator)
                Registry.RemoveAt(i);
        }
    }

    public static void Remove(MyRigidBody rigidBody)
    {
        for (int i = Registry.Count - 1; i >= 0; i--)
        {
            if (Registry[i].rigidBody == rigidBody)
                Registry.RemoveAt(i);
        }
    }

    internal static void Remove(ForceBodyPair pair)
    {
        Registry.Remove(pair);
    }
}
