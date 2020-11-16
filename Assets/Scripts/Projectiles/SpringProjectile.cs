using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringProjectile : MonoBehaviour
{
    public float springConstant;

    // Start is called before the first frame update
    void Start()
    {
        Particle2D[] particles = transform.GetComponentsInChildren<Particle2D>();
        for (int i = 0; i < particles.Length; i++)
        {
            for (int j = 0; j < particles.Length; j++)
            {
                if (i == j)
                    continue;

                float dist = Mathf.Abs((particles[j].transform.position - particles[i].transform.position).magnitude);
                ForceManager.Add(new SpringForceGenerator(particles[j], springConstant, dist), particles[i]);
            }

            particles[i].transform.SetParent(null);
        }
        Destroy(gameObject);
    }
}
