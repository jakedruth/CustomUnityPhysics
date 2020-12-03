using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class ForceGenerator
{
    public bool isOn;
    protected ForceGenerator()
    {
        isOn = true;
    }

    public abstract void UpdateForce(MyRigidBody body, float dt);
}

public class GravityFG : ForceGenerator
{
    public Vector3 gravity;

    public GravityFG(Vector3 gravity)
    {
        this.gravity = gravity;
    }

    public override void UpdateForce(MyRigidBody body, float dt)
    {
        if (body == null || !body.hasFiniteMass)
            return;
        
        body.AddForce(gravity * body.mass);
    }
}

public class DirectionalFG : ForceGenerator
{
    public Vector3 force;

    public DirectionalFG(Vector3 force)
    {
        this.force = force;
    }

    public override void UpdateForce(MyRigidBody body, float dt)
    {
        if (body == null || !body.hasFiniteMass)
            return;

        body.AddForce(force);
    }
}

public class PointFG: ForceGenerator
{
    public Vector3 point;
    public float magnitude;
    private float _range;
    private float _sqrRange;

    public PointFG(Vector3 point, float magnitude, float range)
    {
        this.point = point;
        this.magnitude = magnitude;
        SetRange(range);
    }

    public void SetRange(float range)
    {
        _range = range;
        _sqrRange = _range * _range;
    }

    public float GetRange()
    {
        return _range;
    }

    public override void UpdateForce(MyRigidBody body, float dt)
    {
        Vector3 displacement = point - body.transform.position;
        if (displacement.sqrMagnitude <= _sqrRange)
        {
            float dist = displacement.magnitude;
            Vector3 direction = displacement / dist;
            float proportion = 1 - dist / _range;

            Vector3 force = direction * magnitude * proportion;
            body.AddForce(force);
        }
    }
}

public class BuoyancyFG : ForceGenerator
{
    public float maxDepth;
    public float volume;
    public float waterHeight;
    public float dampingConstant;
    public float liquidDensity;

    public BuoyancyFG(float maxDepth, float volume, float waterHeight, float dampingConstant, float liquidDensity = 1000)
    {
        this.maxDepth = maxDepth;
        this.volume = volume;
        this.waterHeight = waterHeight;
        this.dampingConstant = dampingConstant;
        this.liquidDensity = liquidDensity;
    }

    public override void UpdateForce(MyRigidBody body, float dt)
    {
        float depth = body.transform.position.y;

        if (depth >= waterHeight + maxDepth)
            return;

        Vector3 force = Vector3.zero;

        if (depth <= waterHeight - maxDepth)
        {
            force.y = liquidDensity * volume;
        }
        else
        {
            force.y = liquidDensity * volume * ((depth - maxDepth - waterHeight) / (2 * maxDepth));
        }

        body.AddForce(force);
        body.velocity *= Mathf.Pow(dampingConstant, dt);
    }
}

public class SpringFG : ForceGenerator
{
    public Vector3 connectionPoint;
    public Vector3 otherConnectionPoint;
    public MyRigidBody other;
    public float springConstant;
    public float restLength;

    public SpringFG(Vector3 connectionPoint, Vector3 otherConnectionPoint, MyRigidBody other, float springConstant, float restLength)
    {
        this.connectionPoint = connectionPoint;
        this.otherConnectionPoint = otherConnectionPoint;
        this.other = other;
        this.springConstant = springConstant;
        this.restLength = restLength;
    }

    public override void UpdateForce(MyRigidBody body, float dt)
    {
        if (other == null)
            return;

        Vector3 lws = body.transform.TransformPoint(connectionPoint);
        Vector3 ows = other.transform.TransformPoint(otherConnectionPoint);

        Vector3 force = lws - ows;
        float magnitude = force.magnitude;
        magnitude = Mathf.Abs(magnitude - restLength);
        magnitude *= springConstant;

        force.Normalize();
        force *= -magnitude;

        body.AddForceAtPoint(force, lws);
    }
}
