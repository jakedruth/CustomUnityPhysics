using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public Vector3 bottomLeft;
    public Vector3 upperRight;

    // Start is called before the first frame update
    void Start()
    {
        MoveTarget();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveTarget()
    {
        Vector3 pos = new Vector3(
            Mathf.Lerp(bottomLeft.x, upperRight.x, Random.Range(0f, 1f)),
            Mathf.Lerp(bottomLeft.y, upperRight.y, Random.Range(0f, 1f)),
            Mathf.Lerp(bottomLeft.z, upperRight.z, Random.Range(0f, 1f)));

        transform.position = pos;
    }
}
