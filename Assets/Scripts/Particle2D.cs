using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle2D : MonoBehaviour
{
    public bool ignoreForces;
    public float mass = 1;
    public float radius = 1;
    internal float inverseMass;
    internal Vector3 velocity = Vector3.zero;
    internal Vector3 acceleration = Vector3.zero;
    internal Vector3 accumulatedForces = Vector3.zero;
    [Range(0, 1)]
    public float dampingConstant = 0.999f;

    void Start()
    {
        inverseMass = mass > 0 ? 1 / mass : float.PositiveInfinity;
        ParticleManager.particles.Add(this);
    }

    void OnDestroy()
    {
        //ForceManager.Remove(this);
        ParticleManager.particles.Remove(this);
    }

    public void AddForce(Vector3 force)
    {
        accumulatedForces += force;
    }
}
