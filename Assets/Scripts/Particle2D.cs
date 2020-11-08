using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle2D : MonoBehaviour
{
    public bool ignoreForces;
    public float mass = 1;
    internal float inverseMass;
    internal Vector3 velocity;
    internal Vector3 acceleration;
    internal Vector3 accumulatedForces;
    [Range(0, 1)]
    public float dampingConstant = 0.999f;

    // Start is called before the first frame update
    void Start()
    {
        if (mass != 0)
            inverseMass = 1 / mass;
        else
            inverseMass = float.PositiveInfinity;

        Integrator.AddParticle(this);
    }

    void Update()
    {
        const float killZone = -10;
        if (transform.position.y <= killZone)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        Integrator.RemoveParticle(this);
        ForceManager.Remove(this);
    }

    public void AddForce(Vector3 force)
    {
        accumulatedForces += force;
    }
}
