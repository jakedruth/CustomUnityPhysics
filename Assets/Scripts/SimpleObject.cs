using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Particle2D))]
public class SimpleObject : MonoBehaviour
{
    private Particle2D _particle;
    public float randomHorizontalVelocity;
    public Vector3 acceleration;

    void Awake()
    {
        _particle = GetComponent<Particle2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _particle.acceleration = acceleration;
        _particle.velocity.x = Random.Range(-randomHorizontalVelocity, randomHorizontalVelocity);
        ForceManager.Add(new BuoyancyForceGenerator(_particle.radius, Mathf.PI * _particle.radius * _particle.radius, -0.5f, 0.3f), _particle);
    }
}
