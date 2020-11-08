using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Particle2D))]
public class Target : MonoBehaviour
{
    private Particle2D _particle;

    public float radius;
    public Vector3 bottomLeft;
    public Vector3 upperRight;

    public PointForceGenerator attractor;

    void Awake()
    {
        _particle = GetComponent<Particle2D>();
        attractor = new PointForceGenerator(transform.position, 20, 10);
    }

    // Start is called before the first frame update
    void Start()
    {
        MoveTarget();
        ForceManager.Add(new DirectionalForceGenerator(Vector3.down * 2), _particle);
        ForceManager.Add(new BuoyancyForceGenerator(1f, 10, 0), _particle);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Particle2D particle in Integrator.Particles)
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

        attractor.point = pos;
        transform.position = pos;
    }
}
