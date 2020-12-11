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
        /// Holds the offset of the contact in World Coordinates
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

        // Not sure what this is yet... stay tuned me.
        public MyRigidBody body1;
        public MyRigidBody body2;
        public float friction;
        public float restitution;

        public void SetBodyData(MyRigidBody one, MyRigidBody two, float friction, float restitution)
        {
            body1 = one;
            body2 = two;
            this.friction = friction;
            this.restitution = restitution;
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

        float dist = Vector3.Dot(plane.normal, spherePos) - sphere.radius - plane.offset;

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
        float dist = Vector3.Dot(plane.normal , spherePos) - plane.offset;
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
            if (dist <= plane.offset)
            {
                Contact contact = new Contact()
                {
                    point = plane.normal * (dist - plane.offset) + vertexPos,
                    normal = plane.normal,
                    penetration = plane.offset - dist
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

    public class Plane
    {
        public Vector3 normal = Vector3.zero;
        public float offset;

        public Plane() { }

        public Plane(Vector3 normal, float offset)
        {
            this.normal = normal;
            this.offset = offset;
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