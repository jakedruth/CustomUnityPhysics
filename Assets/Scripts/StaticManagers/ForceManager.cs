using System.Collections;
using System.Collections.Generic;

public static class ForceManager
{
    internal struct ForceGeneratorParticlePair
    {
        public ForceGenerator2D forceGenerator;
        public Particle2D particle;

        internal ForceGeneratorParticlePair(ForceGenerator2D forceGenerator, Particle2D particle)
        {
            this.forceGenerator = forceGenerator;
            this.particle = particle;
        }

        public void UpdateForce(float dt)
        {
            forceGenerator.UpdateForce(particle, dt);
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
            if (pair.forceGenerator == null || pair.particle == null)
            {
                Remove(pair);
                continue;
            }

            if (pair.forceGenerator.isOn)
                pair.UpdateForce(dt);
        }
    }

    public static void Add(ForceGenerator2D forceGenerator, Particle2D particle)
    {
        Registry.Add(new ForceGeneratorParticlePair(forceGenerator, particle));
    }

    public static void Remove(ForceGenerator2D forceGenerator)
    {
        for (int i = Registry.Count - 1; i >= 0; i--)
        {
            if (Registry[i].forceGenerator == forceGenerator)
                Registry.RemoveAt(i);
        }
    }

    public static void Remove(Particle2D particle)
    {
        for (int i = Registry.Count - 1; i >= 0; i--)
        {
            if (Registry[i].particle == particle)
                Registry.RemoveAt(i);
        }
    }

    internal static void Remove(ForceGeneratorParticlePair pair)
    {
        Registry.Remove(pair);
    }
}
