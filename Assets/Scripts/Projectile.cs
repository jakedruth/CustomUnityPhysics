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
        Particle2D particle2D = GetComponent<Particle2D>();
        particle2D.velocity = transform.right * speed;
        particle2D.acceleration = acceleration;
    }
}
