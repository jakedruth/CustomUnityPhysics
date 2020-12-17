using System.Collections;
using System.Collections.Generic;

public static class ForceManager
{
    internal struct ForceGeneratorParticlePair
    {
        public ForceGenerator forceGenerator;
        public MyRigidBody body;

        internal ForceGeneratorParticlePair(ForceGenerator forceGenerator, MyRigidBody body)
        {
            this.forceGenerator = forceGenerator;
            this.body = body;
        }

        public void UpdateForce(float dt)
        {
            forceGenerator.UpdateForce(body, dt);
        }
    }
    private static readonly List<ForceGeneratorParticlePair> Registry;

    static ForceManager()
    {
        Registry = new List<ForceGeneratorParticlePair>();
    }

    public static void FixedUpdate(float dt)
    {
        for (int i = Registry.Count - 1; i >= 0; i--)
        {
            ForceGeneratorParticlePair pair = Registry[i];
            if (pair.forceGenerator == null || pair.body == null)
            {
                Remove(pair);
                continue;
            }

            if (pair.forceGenerator.isOn)
                pair.UpdateForce(dt);
        }
    }

    public static void Add(ForceGenerator forceGenerator, MyRigidBody body)
    {
        Registry.Add(new ForceGeneratorParticlePair(forceGenerator, body));
    }

    public static void Remove(ForceGenerator forceGenerator)
    {
        for (int i = Registry.Count - 1; i >= 0; i--)
        {
            if (Registry[i].forceGenerator == forceGenerator)
                Registry.RemoveAt(i);
        }
    }

    public static void Remove(MyRigidBody body)
    {
        for (int i = Registry.Count - 1; i >= 0; i--)
        {
            if (Registry[i].body == body)
                Registry.RemoveAt(i);
        }
    }

    internal static void Remove(ForceGeneratorParticlePair pair)
    {
        Registry.Remove(pair);
    }
}
