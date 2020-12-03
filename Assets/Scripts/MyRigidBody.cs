using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyRigidBody : MonoBehaviour
{
    public float mass = 1;
    [SerializeField] internal float inverseMass = 1;
    internal bool hasFiniteMass;

    [Range(0, 1)] public float linearDamping = 0.999f;
    [Range(0, 1)] public float angularDamping = 0.999f;

    internal Vector3 velocity;
    internal Vector3 acceleration;
    private Vector3 _prevAcceleration;

    internal Vector3 angularVelocity;

    private Vector3 _accumulatedForces;
    private Vector3 _accumulatedTorque;
    internal bool isAwake;

    void Awake()
    {
        InitMassValues();
    }

    private void OnValidate()
    {
        InitMassValues();
    }

    private void InitMassValues()
    {
        mass = Mathf.Clamp(mass, 0, float.MaxValue);
        hasFiniteMass = mass > 0;
        inverseMass = mass > 0 ? 1 / mass : float.PositiveInfinity;
    }

    public void Integrate(float dt)
    {
        // Calculate Linear Acceleration from force inputs
        _prevAcceleration = acceleration;
        _prevAcceleration += _accumulatedForces * inverseMass;

        // TODO: this
        // Calculate angluar acceleation from torque inputs
        Vector3 angularAcceleration = Vector3.zero;

        // Update linear velocity
        velocity += _prevAcceleration * dt;

        // Update angular velocity
        angularVelocity += angularAcceleration * dt;

        // impose drag
        velocity *= Mathf.Pow(linearDamping, dt);
        angularVelocity *= Mathf.Pow(angularDamping, dt);

        // Adjust position
        transform.position += velocity * dt;
        
        Quaternion rot = transform.rotation;
        Quaternion q = new Quaternion(0, 
            angularVelocity.x * dt, 
            angularVelocity.y * dt, 
            angularVelocity.z * dt);
        
        q *= rot;

        rot.x += q.x * 0.5f;
        rot.y += q.y * 0.5f;
        rot.z += q.z * 0.5f;
        rot.w += q.w * 0.5f;

        transform.rotation = rot; // HOLY FUCK IDK WHAT I JUST DID???

        CalculateDerivedData();

        ClearAccumulators();
    }

    private void CalculateDerivedData()
    {
        throw new NotImplementedException();
    }

    public void AddForce(Vector3 force)
    {
        _accumulatedForces += force;
        isAwake = true;
    }

    public void AddForceAtPoint(Vector3 force, Vector3 point, Space forceDirectionSpace = Space.World, Space pointSpace = Space.World)
    {
        if (forceDirectionSpace == Space.Self)
            force = transform.TransformDirection(force);

        if (pointSpace == Space.Self) 
            point = transform.TransformPoint(point);

        Vector3 pt = point;
        pt -= transform.position;

        _accumulatedForces += force;
        _accumulatedTorque += Vector3.Cross(pt, force);

        isAwake = true;
    }

    public void ClearAccumulators()
    {
        _accumulatedForces = Vector3.zero;
        _accumulatedTorque = Vector3.zero;
    }

    public static void TransformInertiaTensor(ref Matrix4x4 iitWorld, Quaternion q, Matrix4x4 iitBody, Matrix4x4 rotmat)
    {
        throw new NotImplementedException();
    }
}