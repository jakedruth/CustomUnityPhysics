using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Particle2D))]
public class Target : MonoBehaviour
{
    private Particle2D _particle;

    public float radius;
    public Vector3 acceleration;

    [Header("Spawning variables")]
    public Vector3 bottomLeft;
    public Vector3 upperRight;

    void Awake()
    {
        _particle = GetComponent<Particle2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        MoveTarget();
        _particle.acceleration = acceleration;
        ForceManager.Add(new BuoyancyForceGenerator(radius, Mathf.PI * radius * radius, -0f, 0.3f), _particle);
    }

    // UpdateForce is called once per frame
    void Update()
    {
        // TODO: Update when implementing a particle manager
        foreach (Particle2D particle in FindObjectsOfType<Particle2D>())
        {
            if (particle.gameObject == gameObject) 
                continue;

            Vector3 displacement = particle.transform.position - transform.position;
            if (displacement.sqrMagnitude <= radius * radius)
            {
                Destroy(particle.gameObject);
                MoveTarget();
                GameManager.Score += 1;
                break;
            }
        }
    }

    public void MoveTarget()
    {
        Vector3 pos = new Vector3(
            Mathf.Lerp(bottomLeft.x, upperRight.x, Random.Range(0f, 1f)),
            Mathf.Lerp(bottomLeft.y, upperRight.y, Random.Range(0f, 1f)),
            Mathf.Lerp(bottomLeft.z, upperRight.z, Random.Range(0f, 1f)));

        _particle.velocity = Vector3.zero;
        transform.position = pos;
    }
}