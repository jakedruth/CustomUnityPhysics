using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectSpawner : MonoBehaviour
{
    public SimpleObject prefab;
    public Vector3 size;
    public float rate;
    private float _timer;

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= rate)
        {
            _timer -= rate;
            SpawnSimpleObject();
        }
    }

    private void SpawnSimpleObject()
    {
        Vector3 position = transform.position;
        position.x += Random.Range(-size.x, size.x) * 0.5f;
        position.y += Random.Range(-size.y, size.y) * 0.5f;
        position.z += Random.Range(-size.z, size.z) * 0.5f;

        SimpleObject obj = Instantiate(prefab, position, Quaternion.identity);
    }
}
