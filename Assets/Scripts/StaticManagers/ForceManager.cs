using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

public static class ForceManager
{
    internal struct Pair
    {
        public ForceGenerator2D forceGenerator;
        public Particle2D particle;

        internal Pair(ForceGenerator2D forceGenerator, Particle2D particle)
        {
            this.forceGenerator = forceGenerator;
            this.particle = particle;
        }

        public void Update(float dt)
        {
            forceGenerator.UpdateForce(particle, dt);
        }
    }

    private static readonly List<Pair> Registry;

    static ForceManager()
    {
        Registry = new List<Pair>();
    }

    public static void FixedUpdate(float dt)
    {
        foreach (Pair pair in Registry)
        {
            if (pair.forceGenerator == null || pair.particle == null)
            {
                Remove(pair);
                continue;
            }

            if (pair.forceGenerator.isOn)
                pair.Update(dt);
        }
    }

    public static void Add(ForceGenerator2D forceGenerator, Particle2D particle)
    {
        Registry.Add(new Pair(forceGenerator, particle));
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

    internal static void Remove(Pair pair)
    {
        Registry.Remove(pair);
    }
}
