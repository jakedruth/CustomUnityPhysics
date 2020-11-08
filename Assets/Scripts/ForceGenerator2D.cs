using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ForceGenerator2D
{
    public bool isOn;
    protected ForceGenerator2D()
    {
        isOn = true;
    }

    public abstract void UpdateForce(Particle2D particle, float dt);
}

public class DirectionalForceGenerator : ForceGenerator2D
{
    public Vector3 force;

    public DirectionalForceGenerator(Vector3 force)
    {
        this.force = force;
    }

    public override void UpdateForce(Particle2D particle, float dt)
    {
        if (particle == null)
        {
            ForceManager.Remove(this);
            return;
        }

        particle.AddForce(force);
    }
}

public class PointForceGenerator : ForceGenerator2D
{
    public Vector3 point;
    public float magnitude;
    private float _range;
    private float _sqrRange;

    public PointForceGenerator(Vector3 point, float magnitude, float range)
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

    public override void UpdateForce(Particle2D particle, float dt)
    {
        Vector3 displacement = point - particle.transform.position;
        if (displacement.sqrMagnitude <= _sqrRange)
        {
            float dist = displacement.magnitude;
            Vector3 direction = displacement / dist;
            float proportion = 1 - dist / _range;

            Vector3 force = direction * magnitude * proportion;
            particle.AddForce(force);
        }
    }
}

public class BuoyancyForceGenerator : ForceGenerator2D
{
    public float maxDepth;
    public float volume;
    public float waterHeight;
    public float liquidDensity;

    public BuoyancyForceGenerator(float maxDepth, float volume, float waterHeight, float liquidDensity = 1000)
    {
        this.maxDepth = maxDepth;
        this.volume = volume;
        this.waterHeight = waterHeight;
        this.liquidDensity = liquidDensity;
    }

    public override void UpdateForce(Particle2D particle, float dt)
    {
        float depth = particle.transform.position.y;

        if (depth <= waterHeight - maxDepth)
            return;

        Vector3 force = Vector3.zero;

        if (depth >= waterHeight + maxDepth)
        {
            force.y = -liquidDensity * volume;
        }
        else
        {
            force.y = -liquidDensity * volume * ((depth - waterHeight + maxDepth) / (2 * maxDepth));
        }

        particle.AddForce(force);
    }
}
