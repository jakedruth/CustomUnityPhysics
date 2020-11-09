using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : MonoBehaviour
{
    public Transform mFirePoint;
    public GameObject[] projectiles;
    private int _projectileIndex;

    public float mMinAngle;
    public float mMaxAngle;
    public float mAngleRate;
    private float _mAngle;
    
    // UpdateForce is called once per frame
    void Update()
    {
        // Handle rotations
        if (Input.GetKey(KeyCode.Alpha1))
            _mAngle += mAngleRate * Time.deltaTime;
        else if (Input.GetKey(KeyCode.Alpha2))
            _mAngle -= mAngleRate * Time.deltaTime;

        _mAngle = Mathf.Clamp(_mAngle, mMinAngle, mMaxAngle);
        transform.rotation = Quaternion.AngleAxis(_mAngle, Vector3.forward);

        // Handle Switching Weapons
        if (Input.GetKeyDown(KeyCode.W))
            _projectileIndex = (_projectileIndex + 1) % projectiles.Length;

        // Handle Firing Gun
        if (Input.GetKeyDown(KeyCode.Return))
            Fire();
    }

    public void Fire()
    {
        GameObject projectile = Instantiate(projectiles[_projectileIndex], mFirePoint.position,
            Quaternion.AngleAxis(_mAngle, Vector3.forward));

        projectile.name = "Projectile";
    }
}
