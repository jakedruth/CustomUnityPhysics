using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle2D : MonoBehaviour
{
    public bool ignoreForces;
    public float mass = 1;
    public float radius = 1;
    internal float inverseMass;
    internal Vector3 velocity;
    internal Vector3 acceleration;
    internal Vector3 accumulatedForces;
    [Range(0, 1)]
    public float dampingConstant = 0.999f;

    void Start()
    {
        inverseMass = mass > 0 ? 1 / mass : float.PositiveInfinity;
        GameManager.ParticleManager.particles.Add(this);
    }

    void OnDestroy()
    {
        ForceManager.Remove(this);
        GameManager.ParticleManager.particles.Remove(this);
    }

    public void AddForce(Vector3 force)
    {
        accumulatedForces += force;
    }
}
