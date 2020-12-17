using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;
using UnityEngine;

public static class CollisionDetector
{
    public static bool DetectCollision(Particle2D particleA, Particle2D particleB)
    {
        float sqrDistance = (particleB.transform.position - particleA.transform.position).sqrMagnitude;
        float radiusSum = particleA.radius + particleB.radius;

        return sqrDistance <= radiusSum * radiusSum;
    }

    /// <summary>
    /// A contact represent b bodies in contact. Resolving a contact removes their
    /// interpenetration, and applies sufficient impulse to keep them apart. Colliding
    /// bodies may also rebound. Contacts can be used to represent positional joints,
    /// by making the contact constraint keep the bodies in their correct orientation.
    /// </summary>
    public class Contact
    {
        /// <summary>
        /// Holds the distance of the contact in World Coordinates
        /// </summary>
        public Vector3 point;

        /// <summary>
        /// Holds the direction of the contact in world coordinates
        /// </summary>
        public Vector3 normal;

        /// <summary>
        /// Holds the dept of penetration at the contact point. If both bodies are specified,
        /// then the contact point should be midway between the interpenetrating points
        /// </summary>
        public float penetration;

        public MyRigidBody[] bodies = new MyRigidBody[2];
        public float friction;
        public float restitution;

        /// <summary>
        /// A transform matrix that converts co-ordinates in the contact's
        /// frame of reference to world co-ordinates. The columns of this
        /// matrix form an orthonormal set of vectors.
        /// </summary>
        internal Matrix3X3 contactToWorld;
        //internal Quaternion contactWorldRotation;

        /// <summary>
        /// Holds the closing velocity at the point of contact. This is set
        /// when the calculateInternals function is run.
        /// </summary>
        internal Vector3 contactVelocity;

        /// <summary>
        /// Holds the required change in velocity for this contact to resolved.
        /// </summary>
        internal float desiredDeltaVelocity;

        /// <summary>
        /// Holds the world space position of the contact point relative to
        /// centre of each body. This is set when the calculateInternals function is run.
        /// </summary>
        internal Vector3[] relativeContactPosition = new Vector3[2];

        public void SetBodyData(MyRigidBody one, MyRigidBody two, float friction, float restitution)
        {
            bodies[0] = one;
            bodies[1] = two;
            this.friction = friction;
            this.restitution = restitution;
        }

        /// <summary>
        /// Constructs an arbitrary orthonormal basis for the contact. This is stored as a 3 x 3 matrix,
        /// where each vector is a column (in other words, the matrix transforms contact space into world space).
        /// The x direction is generated from the contact normal,
        /// and the y and z  directions are set so that they are at right angles to it.
        /// </summary>
        protected void CalculateContactBasis()
        {
            Vector3[] contactTangent = new Vector3[2];
            contactToWorld = new Matrix3X3();
            //contactWorldRotation = Quaternion.identity;

            // Check whether the Z-axis is nearer to the X- or Y-axis
            if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
            {
                // Scaling factor to ensure the results are normalized.
                float s = 1.0f / Mathf.Sqrt(normal.z * normal.z + normal.x * normal.x);

                // The new X-axis is at right angles to the world Y-axis.
                contactTangent[0].x = normal.z * s;
                contactTangent[0].y = 0;
                contactTangent[0].z = -normal.x * s;
                
                // The new Y-axis is at right angles to the new X- and Z-axes.
                contactTangent[1].x = normal.y * contactTangent[0].x;
                contactTangent[1].y = normal.z * contactTangent[0].x - normal.x * contactTangent[0].z;
                contactTangent[1].z = -normal.y * contactTangent[0].x;
            }
            else
            {
                // Scaling factor to ensure the results are normalized.
                float s = 1.0f / Mathf.Sqrt(normal.z * normal.z + normal.y * normal.y);
                
                // The new X-axis is at right angles to the world X-axis.
                contactTangent[0].x = 0;
                contactTangent[0].y = -normal.z * s;
                contactTangent[0].z = normal.y * s;

                // The new Y-axis is at right angles to the new X- and Z-axes.
                contactTangent[1].x = normal.y * contactTangent[0].z - normal.z * contactTangent[0].y;
                contactTangent[1].y = -normal.x * contactTangent[0].z;
                contactTangent[1].z = normal.x * contactTangent[0].y;
            }

            contactToWorld.SetComponents(contactTangent[1], normal, contactTangent[0]);
        }

        protected Vector3 CalculateFrictionlessImpulse(Matrix3X3[] inverseInertiaTensor)
        {
            Vector3 impulseContact = Vector3.zero;

            // Build a vector that shows the change in velocity in
            // world space for a unit impulse in the direction of the contact
            // normal.
            Vector3 deltaVelWorld = Vector3.Cross(relativeContactPosition[0], normal);
            deltaVelWorld = inverseInertiaTensor[0].Transform(deltaVelWorld);
            deltaVelWorld = Vector3.Cross(deltaVelWorld, relativeContactPosition[0]);

            // Work out the change in velocity in contact coordiantes.
            float deltaVelocity = Vector3.Dot(deltaVelWorld, normal);

            // Add the linear component of velocity change
            deltaVelocity += bodies[0].inverseMass;

            // Check if we need to the second body's data
            if (bodies[1])
            {
                // Go through the same transformation sequence again
                Vector3 delta = Vector3.Cross(relativeContactPosition[1], normal);
                delta = inverseInertiaTensor[1].Transform(delta);
                delta = Vector3.Cross(delta, relativeContactPosition[1]);

                // Add the change in velocity due to rotation
                deltaVelocity += Vector3.Dot(delta, normal);

                // Add the change in velocity due to linear motion
                deltaVelocity += bodies[1].inverseMass;
            }

            // Calculate the required size of the impulse
            impulseContact.y = desiredDeltaVelocity / deltaVelocity;
            impulseContact.x = 0;
            impulseContact.z = 0;
            return impulseContact;
        }

        public void CalculateInternals(float dt)
        {
            // check to see if the first object is null, and swap if it is.
            if (bodies[0] == null)
            {
                // check to see if the second body is null
                if (bodies[1] == null)
                    return;

                // Swap the bodies so the first body is not null
                SwapBodies();
            }

            // Calculate a set of axes at the contact point
            CalculateContactBasis();

            // store the relative position of the contact relative to each body.
            relativeContactPosition[0] = point - bodies[0].transform.position;
            if (bodies[1] != null) 
                relativeContactPosition[1] = point - bodies[1].transform.position;

            // find the relative velocity of the bodies at the contact point
            contactVelocity = CalculateLocalVelocity(0, dt);
            if (bodies[1])
                contactVelocity -= CalculateLocalVelocity(1, dt);

            // Calculate the desired change in velocity for resolution
            CalculateDesiredDeltaVelocity(dt);
        }

        public void SwapBodies()
        {
            normal *= -1;

            MyRigidBody temp = bodies[0];
            bodies[0] = bodies[1];
            bodies[1] = temp;
        }

        private Vector3 CalculateLocalVelocity(int bodyIndex, float dt)
        {
            // Get the rigid body based on the index
            MyRigidBody body = bodies[bodyIndex];

            // Work out the velocity of the contact point
            Vector3 velocity = Vector3.Cross(body.angularVelocity, relativeContactPosition[bodyIndex]);
            velocity += body.velocity;

            // Turn the velocity into contact coordinates
            Vector3 vel = contactToWorld.TransformTranspose(velocity);

            // Calculate the amount of velocity that is due to forces without reactions
            Vector3 accVelocity = body.prevAcceleration * dt;

            // Calculate the velocity in contact-coordinates.
            accVelocity = contactToWorld.TransformTranspose(accVelocity);

            // We ignore any component of acceleration in the contact normal
            // direction, we are only interested in planar acceleration
            accVelocity.x = 0;

            // Add the planar velocities - if there's enough friction they will
            // be removed during velocity resolution
            vel += accVelocity;

            // And return it
            return vel;
        }

        internal void CalculateDesiredDeltaVelocity(float dt)
        {
            const float velocityLimit = 0.25f;

            // Calculate the acceleration induced velocity accumulated this frame
            float velocityFromAcc = 0;

            if (bodies[0] != null) 
                velocityFromAcc += Vector3.Dot(bodies[0].prevAcceleration * dt, normal);

            if (bodies[1] != null)
                velocityFromAcc -= Vector3.Dot(bodies[1].prevAcceleration * dt, normal);

            // If the velocity is very slow, limit the restitution
            float thisRestitution = restitution;
            if (Mathf.Abs(contactVelocity.y) < velocityLimit)
                thisRestitution = 0f;

            // Combine the bounce velocity with the removed acceleration velocity
            desiredDeltaVelocity = -contactVelocity.y - thisRestitution * (contactVelocity.y - velocityFromAcc);
        }

        public void ApplyPositionChange(out Vector3[] velocityChange, out Vector3[] rotationChange, float max)
        {
            velocityChange = new Vector3[2];
            rotationChange = new Vector3[2];

            // Get hold of the inverse mass and inverse inertia tensor, both is world coordinates
            Matrix3X3[] inverseInertiaTensor = new Matrix3X3[2];
            inverseInertiaTensor[0] = bodies[0].inverseInertiaTensorWorld;
            if (bodies[1] != null)
                inverseInertiaTensor[1] = bodies[1].inverseInertiaTensorWorld;

            // We will calculate the impulse for each contact axis
            Vector3 impulsiveContact = Math.Abs(friction) < 0.001f 
                ? CalculateFrictionlessImpulse(inverseInertiaTensor) 
                : CalculateFrictionImpulse(inverseInertiaTensor);

            // Convert the impulse to world coordinates
            Vector3 impulse = contactToWorld.Transform(impulsiveContact);

            // Split in the impulse into linear and rotational components
            Vector3 impulsiveTorque = Vector3.Cross(relativeContactPosition[0], impulse);
            rotationChange[0] = inverseInertiaTensor[0].Transform(impulsiveTorque);
            velocityChange[0] = Vector3.zero;
            velocityChange[0] += impulse * bodies[0].inverseMass;

            // apply the changes
            bodies[0].velocity += velocityChange[0];
            bodies[0].angularVelocity += rotationChange[0];

            // apply the changes to the other body if not null
            if (bodies[1] != null)
            {
                // Work out body one's linear and angular changes
                impulsiveTorque = Vector3.Cross(impulse, relativeContactPosition[1]);
                rotationChange[1] = inverseInertiaTensor[1].Transform(impulsiveTorque);
                velocityChange[1] = Vector3.zero;
                velocityChange[1] += impulse * -bodies[1].inverseMass;

                // And apply them.
                bodies[1].velocity += velocityChange[1];
                bodies[1].angularVelocity += rotationChange[1];
            }
        }

        private Vector3 CalculateFrictionImpulse(Matrix3X3[] inverseInertiaTensor)
        {
            // TODO: Implement friction based impulses
            return CalculateFrictionlessImpulse(inverseInertiaTensor);
        }

        public void ApplyVelocityChange(out Vector3[] velocityChange, out Vector3[] rotationChange)
        {
            velocityChange = new Vector3[2];
            rotationChange = new Vector3[2];

            // Get hold of the inverse mass and inverse inertia tensor, both in world coordinates
            Matrix3X3[] inverseInertiaTensor = new Matrix3X3[2];
            inverseInertiaTensor[0] = bodies[0].inverseInertiaTensorWorld;
            if (bodies[1] != null)
                inverseInertiaTensor[1] = bodies[1].inverseInertiaTensorWorld;

            // We will calculate the impulse for each contact axis
            Vector3 impulsiveContact = Math.Abs(friction) < 0.001f 
                ? CalculateFrictionlessImpulse(inverseInertiaTensor) 
                : CalculateFrictionImpulse(inverseInertiaTensor);

            // Convert impulse to world coordinates
            Vector3 impulse = contactToWorld.Transform(impulsiveContact);

            // Split in the impulse into linear and rotational components
            Vector3 impulsiveTorque = Vector3.Cross(relativeContactPosition[0], impulse);
            rotationChange[0] = inverseInertiaTensor[0].Transform(impulsiveTorque);
            velocityChange[0] = Vector3.zero;
            velocityChange[0] += impulse * bodies[0].inverseMass;

            // Apply the changes
            bodies[0].velocity += velocityChange[0];
            bodies[0].angularVelocity += rotationChange[0];

            // apply the changes to the other body if not null
            if (bodies[1] != null)
            {
                // Work out body one's linear and angular changes
                impulsiveTorque = Vector3.Cross(impulse, relativeContactPosition[1]);
                rotationChange[1] = inverseInertiaTensor[1].Transform(impulsiveTorque);
                velocityChange[1] = Vector3.zero;
                velocityChange[1] += impulse * -bodies[1].inverseMass;

                // And apply them.
                bodies[1].velocity += velocityChange[1];
                bodies[1].angularVelocity += rotationChange[1];
            }
        }
    }

    public struct CollisionData
    {
        /// <summary>
        /// Holds the contact array to write into.
        /// </summary>
        public List<Contact> contacts;

        // TODO: Figure out how this works
        public float friction;
        // TODO: Figure out how this works
        public float restitution;

        public CollisionData(float friction, float restitution)
        {
            contacts = new List<Contact>();
            this.friction = friction;
            this.restitution = restitution;
        }
    }

    public static int DetectCollisionWithStaticGround(Primitive.Plane plane, Primitive b, ref CollisionData data)
    {
        switch (b)
        {
            case Primitive.Sphere sphere:
                return SphereAndHalfSpace(sphere, plane, ref data);
            case Primitive.Box box:
                return BoxAndHalfSpace(box, plane, ref data);
        }

        Debug.Log("Checking planeA against another planeA");

        return 0;
    }

    public static int SphereAndSphere(Primitive.Sphere one, Primitive.Sphere two, ref CollisionData data)
    {
        // Cache the sphere positions.
        Vector3 positionOne = one.GetWorldPosition();
        Vector3 positionTwo = two.GetWorldPosition();

        // Find the vector between the objects.
        Vector3 delta = positionTwo - positionOne;
        float size = delta.magnitude;

        // See if it is large enough.
        if (size <= 0.0f || size >= one.radius + two.radius)
            return 0;

        // We manually create the normal, because we have the
        // size to hand.
        Vector3 normal = delta / size;
        Contact contact = new Contact
        {
            normal = normal,
            point = positionOne + delta * 0.5f,
            penetration = (one.radius + two.radius - size)
        };
        contact.SetBodyData(one.body, two.body, data.friction, data.restitution);
        
        data.contacts.Add(contact);
        return 1;
    }

    public static int SphereAndHalfSpace(Primitive.Sphere sphere, Primitive.Plane plane, ref CollisionData data)
    {
        Vector3 spherePos = sphere.GetWorldPosition();

        float dist = Vector3.Dot(plane.normal, spherePos) - sphere.radius - plane.distance;

        if (dist >= 0)
            return 0;

        Contact contact = new Contact()
        {
            normal = plane.normal,
            penetration = -dist,
            point = spherePos - plane.normal * (dist + sphere.radius)
        };

        contact.SetBodyData(sphere.body, null, data.friction, data.restitution);

        data.contacts.Add(contact);

        return 1;
    }

    public static int SphereAndTruePlane(Primitive.Sphere sphere, Primitive.Plane plane, ref CollisionData data)
    {
        Vector3 spherePos = sphere.GetWorldPosition();
        float dist = Vector3.Dot(plane.normal , spherePos) - plane.distance;
        if (dist > sphere.radius)
            return 0;

        Vector3 normal = plane.normal;
        float penetration = -dist;
        if (dist < 0)
        {
            normal *= -1;
            penetration = -penetration;
        }

        penetration += sphere.radius;

        Contact contact = new Contact()
        {
            normal = normal,
            penetration = penetration,
            point = spherePos - plane.normal * dist
        };
        contact.SetBodyData(sphere.body, null, data.friction, data.restitution);

        data.contacts.Add(contact);

        return 1;
    }

    public static int BoxAndHalfSpace(Primitive.Box box, Primitive.Plane plane, ref CollisionData data)
    {
        // TODO: Check for intersection before doing math.

        Vector3[] verts = box.GetVerticesWorldSpace();
        int contactCount = 0;
        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 vertexPos = verts[i];
            float dist = Vector3.Dot(vertexPos, plane.normal);
            if (dist <= plane.distance)
            {
                Contact contact = new Contact()
                {
                    point = plane.normal * (dist - plane.distance) + vertexPos,
                    normal = plane.normal,
                    penetration = plane.distance - dist
                };
                contact.SetBodyData(box.body, null, data.friction, data.restitution);
                data.contacts.Add(contact);
                contactCount++;
            }
        }

        return contactCount;
    }

    public static int BoxAndSphere(Primitive.Box box, Primitive.Sphere sphere, ref CollisionData data)
    {
        Vector3 relativeCenter = box.body.transform.InverseTransformPoint(sphere.GetWorldPosition());
        if (Mathf.Abs(relativeCenter.x) - sphere.radius > box.halfSize.x ||
            Mathf.Abs(relativeCenter.y) - sphere.radius > box.halfSize.y ||
            Mathf.Abs(relativeCenter.z) - sphere.radius > box.halfSize.z)
        {
            return 0;
        }

        // clamp each coordinate to the box
        Vector3 closestPoint = new Vector3(
            Mathf.Clamp(relativeCenter.x, -box.halfSize.x, box.halfSize.x),
            Mathf.Clamp(relativeCenter.y, -box.halfSize.y, box.halfSize.y),
            Mathf.Clamp(relativeCenter.z, -box.halfSize.z, box.halfSize.z));

        float dist = (closestPoint - relativeCenter).magnitude;
        if (dist * dist > sphere.radius * sphere.radius)
            return 0;

        Vector3 closestPointWorld = box.body.transform.TransformPoint(closestPoint);
        Contact contact = new Contact
        {
            normal = (closestPointWorld - sphere.GetWorldPosition()).normalized,
            point = closestPointWorld,
            penetration = sphere.radius - dist
        };
        contact.SetBodyData(box.body, sphere.body, data.friction, data.restitution);
        data.contacts.Add(contact);
        return 1;
    }

    //public static int BoxAndBox(Primitive.Box one, Primitive.Box two, ref CollisionData data)
    //{
    //    Vector3 toCenter = two.GetWorldPosition() - one.GetWorldPosition();



    //    float bestOverlap = float.MaxValue;
    //    int bestCase = -1;

    //    Vector3[] axes = GetAxes(one, two);
    //    for (int i = 0; i < axes.Length; i++)
    //    {
    //        Vector3 axis = axes[i];

    //        // Check for axes that were geneared by (almost) parallel edges.
    //        if (axis.sqrMagnitude < 0.001)
    //            continue;

    //        axis.Normalize();

    //        float overlap = PenetrationOnAxis(one, two, axis, toCenter);
    //        if (overlap < 0)
    //            return 0;

    //        if (overlap < bestOverlap)
    //        {
    //            bestOverlap = overlap;
    //            bestCase = i;
    //        }
    //    }

    //    if (bestCase < 3)
    //}

    public static float PenetrationOnAxis(Primitive.Box one, Primitive.Box two, Vector3 axis, Vector3 toCenter)
    {
        float oneProject = TransformToAxis(one, axis);
        float twoProject = TransformToAxis(two, axis);

        float distance = Mathf.Abs(Vector3.Dot(toCenter, axis));

        return oneProject + twoProject - distance;
    }

    private static Vector3[] GetAxes(Primitive.Box a, Primitive.Box b)
    {
        Matrix4x4 mA = a.body.transform.localToWorldMatrix;
        Matrix4x4 mB = b.body.transform.localToWorldMatrix;

        Vector3 a0 = mA.GetColumn(0);
        Vector3 a1 = mA.GetColumn(1);
        Vector3 a2 = mA.GetColumn(2);

        Vector3 b0 = mB.GetColumn(0);
        Vector3 b1 = mB.GetColumn(1);
        Vector3 b2 = mB.GetColumn(2);

        return new[]
        {
            // Face axes for object A.
            a0,                     // 0
            a1,                     // 1   
            a2,                     // 2

            // Face axes for object B.
            b0,                     // 3
            b1,                     // 4
            b2,                     // 5   

            // Edge-edge axes
            Vector3.Cross(a0, b0),  // 6
            Vector3.Cross(a0, b1),  // 7
            Vector3.Cross(a0, b2),  // 8
            Vector3.Cross(a1, b0),  // 9
            Vector3.Cross(a1, b1),  // 10
            Vector3.Cross(a1, b2),  // 11
            Vector3.Cross(a2, b0),  // 12
            Vector3.Cross(a2, b1),  // 13
            Vector3.Cross(a2, b2)   // 14
        };
    }

    private static float TransformToAxis(Primitive.Box box, Vector3 axis)
    {
        Matrix4x4 m = box.body.transform.localToWorldMatrix;
        Vector3 x = m.GetColumn(0);
        Vector3 y = m.GetColumn(1);
        Vector3 z = m.GetColumn(2);

        return
            box.halfSize.x * Mathf.Abs(Vector3.Dot(axis, x)) +
            box.halfSize.y * Mathf.Abs(Vector3.Dot(axis, y)) +
            box.halfSize.z * Mathf.Abs(Vector3.Dot(axis, z));
    }
}

public abstract class Primitive
{
    public MyRigidBody body;
    protected Vector3 offset = Vector3.zero;
    protected Quaternion orientation = Quaternion.identity;

    public Vector3 GetWorldPosition()
    {
        return body.transform.TransformPoint(offset);
    }

    public Quaternion GetWorldRotation()
    {
        return body.transform.rotation * orientation;
    }


    public class Sphere : Primitive
    {
        public float radius;

        public Sphere() { }

        public Sphere(MyRigidBody body, Vector3 offset, Quaternion orientation, float radius)
        {
            this.body = body;
            this.offset = offset;
            this.orientation = orientation;
            this.radius = radius;
        }
    }

    public class Plane : Primitive
    {
        public Vector3 normal = Vector3.zero;
        public float distance;

        public Plane() { }

        public Plane(Vector3 normal, float distance)
        {
            this.normal = normal;
            this.distance = distance;
        }

        public Plane(Vector3 normal, Vector3 position)
        {
            this.normal = normal;
            this.distance = Vector3.Dot(normal, position);
        }
    }

    public class Box : Primitive
    {
        public Vector3 halfSize = Vector3.zero;

        public Box() { }

        public Box(MyRigidBody body, Vector3 offset, Quaternion orientation, Vector3 halfSize)
        {
            this.body = body;
            this.offset = offset;
            this.orientation = orientation;
            this.halfSize = halfSize;
        }

        public Vector3[] GetVerticesLocalSpace()
        {
            return new[]
            {
               offset + orientation * new Vector3(-halfSize.x, -halfSize.y, -halfSize.z),
               offset + orientation * new Vector3(-halfSize.x, -halfSize.y, +halfSize.z),
               offset + orientation * new Vector3(-halfSize.x, +halfSize.y, -halfSize.z),
               offset + orientation * new Vector3(-halfSize.x, +halfSize.y, +halfSize.z),
               offset + orientation * new Vector3(+halfSize.x, -halfSize.y, -halfSize.z),
               offset + orientation * new Vector3(+halfSize.x, -halfSize.y, +halfSize.z),
               offset + orientation * new Vector3(+halfSize.x, +halfSize.y, -halfSize.z),
               offset + orientation * new Vector3(+halfSize.x, +halfSize.y, +halfSize.z)
            };
        }

        public Vector3[] GetVerticesWorldSpace()
        {
            return new[]
            {
                body.transform.TransformPoint(offset + orientation * new Vector3(-halfSize.x, -halfSize.y, -halfSize.z)),
                body.transform.TransformPoint(offset + orientation * new Vector3(-halfSize.x, -halfSize.y, +halfSize.z)),
                body.transform.TransformPoint(offset + orientation * new Vector3(-halfSize.x, +halfSize.y, -halfSize.z)),
                body.transform.TransformPoint(offset + orientation * new Vector3(-halfSize.x, +halfSize.y, +halfSize.z)),
                body.transform.TransformPoint(offset + orientation * new Vector3(+halfSize.x, -halfSize.y, -halfSize.z)),
                body.transform.TransformPoint(offset + orientation * new Vector3(+halfSize.x, -halfSize.y, +halfSize.z)),
                body.transform.TransformPoint(offset + orientation * new Vector3(+halfSize.x, +halfSize.y, -halfSize.z)),
                body.transform.TransformPoint(offset + orientation * new Vector3(+halfSize.x, +halfSize.y, +halfSize.z))
            };
        }
    }
}