using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Particle2D))]
public class Projectile : MonoBehaviour
{
    public float speed;
    public Vector3 acceleration;

    // Start is called before the first frame update
    void Start()
    {
        Particle2D particle = GetComponent<Particle2D>();
        particle.velocity = transform.right * speed;
        particle.acceleration = acceleration;

        ForceManager.Add(FindObjectOfType<Target>().attractor, particle);
    }
}
