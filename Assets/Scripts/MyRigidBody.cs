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

    internal Matrix3X3 inverseInertiaTensor = new Matrix3X3();
    internal Matrix3X3 inverseInertiaTensorWorld = new Matrix3X3();

    internal Vector3 velocity = Vector3.zero;
    internal Vector3 acceleration = Vector3.zero;
    internal Vector3 prevAcceleration = Vector3.zero;

    internal Vector3 angularVelocity = Vector3.zero;

    private Vector3 _accumulatedForces;
    private Vector3 _accumulatedTorque;
    internal bool isAwake;
    internal bool canSleep = false;
    private float _motion;
    private const float SLEEP_EPSILON = 0.3f;

    void Awake()
    {
        InitMassValues();
        CalculateDerivedData();
        WorldManager.Bodies.Add(this);
        isAwake = true;
    }

    void OnDestroy()
    {
        WorldManager.Bodies.Remove(this);
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

    void FixedUpdate()
    {
        //Integrate(Time.deltaTime);
    }

    public void Integrate(float dt)
    {
        if (!isAwake)
            return;

        // Calculate Linear Acceleration from maxForce inputs
        prevAcceleration = acceleration;
        prevAcceleration += _accumulatedForces * inverseMass;

        // Calculate angluar acceleation from torque inputs
        Vector3 angularAcceleration = inverseInertiaTensorWorld.Transform(_accumulatedTorque);

        // Update linear velocity
        velocity += prevAcceleration * dt;

        // Update angular velocity
        angularVelocity += angularAcceleration * dt;

        // impose drag
        velocity *= Mathf.Pow(linearDamping, dt);
        angularVelocity *= Mathf.Pow(angularDamping, dt);

        // Adjust distance
        transform.position += velocity * dt;

        transform.rotation *= Quaternion.Euler(angularVelocity);

        CalculateDerivedData();

        ClearAccumulators();

        // Update the kinetic energy store, and possibly put the body to sleep.
        if (canSleep) {
            float currentMotion = Vector3.Dot(velocity, velocity) + Vector3.Dot(angularVelocity, angularVelocity);

            float bias = Mathf.Pow(0.5f, dt);
            _motion = bias * _motion + (1-bias)*currentMotion;

            if (_motion < SLEEP_EPSILON) 
                SetAwake(false);
            else if (_motion > 10 * SLEEP_EPSILON) 
                _motion = 10 * SLEEP_EPSILON;
        }
    }

    public void SetAwake(bool awake = true)
    {
        if (awake)
        {
            isAwake = true;
            _motion = SLEEP_EPSILON * 2;
        }
        else
        {
            isAwake = false;
            velocity = Vector3.zero;
            angularVelocity = Vector3.zero;
        }
    }

    public void SetCanSleep(bool value)
    {
        canSleep = value;
    }

    public void SetInertiaTensor(Matrix3X3 inertiaTensor)
    {
        inverseInertiaTensor.SetInverse(inertiaTensor);
    }

    internal void CalculateDerivedData()
    {
        transform.rotation.Normalize();
        
        // Calculate the inertiaTensor in world space
        inverseInertiaTensorWorld = inverseInertiaTensor;
        inverseInertiaTensorWorld.SetOrientation(transform.rotation);
    }

    public void AddForce(Vector3 force)
    {
        _accumulatedForces += force;
        isAwake = true;
    }

    public void AddForce(Vector3 force, Vector3 point, Space forceDirectionSpace = Space.World, Space pointSpace = Space.World)
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
}