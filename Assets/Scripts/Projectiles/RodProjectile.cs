using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RodProjectile : MonoBehaviour
{
    public float length;

    // Start is called before the first frame update
    void Start()
    {
        Particle2D[] particles = transform.GetComponentsInChildren<Particle2D>();

        ParticleRod rod = new ParticleRod(particles[0], particles[1], length);

        particles[0].transform.SetParent(null);
        particles[1].transform.SetParent(null);

        GameManager.ParticleManager.contactGenerators.Add(rod);

        Destroy(gameObject);
    }
}
