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

    internal Vector3 velocity;
    internal Vector3 acceleration;
    private Vector3 _prevAcceleration;

    internal Vector3 angularVelocity;

    private Vector3 _accumulatedForces;
    private Vector3 _accumulatedTorque;
    internal bool isAwake;

    // TODO: Remove this
    private AnchoredSpringFG spring;
    private GravityFG gravity;

    void Awake()
    {
        InitMassValues();
        CalculateDerivedData();
        SetInertiaTensor(MyPhysics.GetInertiaTensorSphere(mass, 0.5f));
        
        spring = new AnchoredSpringFG(new Vector3(0, 3, 0), new Vector3(0, 0.5f, 0), 5, 1);
        gravity = new GravityFG(Vector3.down * 2);
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
        gravity.UpdateForce(this, Time.deltaTime);
        spring.UpdateForce(this, Time.deltaTime);

        Debug.DrawLine(spring.anchor, transform.TransformPoint(spring.connectionPoint), Color.red, Time.deltaTime, false);
        Debug.DrawRay(transform.position, transform.up, Color.green, Time.deltaTime, false);
        Integrate(Time.deltaTime);
    }

    public void Integrate(float dt)
    {
        // Calculate Linear Acceleration from force inputs
        _prevAcceleration = acceleration;
        _prevAcceleration += _accumulatedForces * inverseMass;

        // Calculate angluar acceleation from torque inputs
        Vector3 angularAcceleration = inverseInertiaTensorWorld.Transform(_accumulatedTorque);

        // Update linear velocity
        velocity += _prevAcceleration * dt;

        // Update angular velocity
        angularVelocity += angularAcceleration * dt;

        // impose drag
        velocity *= Mathf.Pow(linearDamping, dt);
        angularVelocity *= Mathf.Pow(angularDamping, dt);

        // Adjust position
        transform.position += velocity * dt;

        transform.rotation *= Quaternion.Euler(angularVelocity);

        CalculateDerivedData();

        ClearAccumulators();
    }

    public void SetInertiaTensor(Matrix3X3 inertiaTensor)
    {
        inverseInertiaTensor.SetInverse(inertiaTensor);
    }

    private void CalculateDerivedData()
    {
        transform.rotation.Normalize();
        
        // Calculate the inertiaTensor in world space

        // TODO: this function
        //TransformInertiaTensor(inverseInertiaTensor);

        inverseInertiaTensorWorld = inverseInertiaTensor;
        inverseInertiaTensorWorld.SetOrientation(transform.rotation);
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

        Matrix3X3 m = new Matrix3X3();
    }

    private static void TransformInertiaTensor(ref Matrix3X3 iitWorld, Quaternion q, Matrix3X3 iitBody, Matrix4x4 rotmat)
    {


        throw new NotImplementedException();
    }
}