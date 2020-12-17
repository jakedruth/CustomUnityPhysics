using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

public class WorldManager
{
    public static int iterations;
    public static int maxContacts;

    internal static readonly List<MyRigidBody> Bodies;
    internal static readonly List<Primitive.Plane> StaticPlanes;
    internal static readonly List<Primitive.Sphere> Spheres;
    internal static readonly List<Primitive.Box> Boxes;

    static WorldManager()
    {
        maxContacts = int.MaxValue;
        ContactResolver.SetIterations(iterations = int.MaxValue);

        Bodies = new List<MyRigidBody>();
        StaticPlanes = new List<Primitive.Plane>();
        Spheres = new List<Primitive.Sphere>();
        Boxes = new List<Primitive.Box>();
    }

    public static void RunPhysics(float dt)
    {
        // Update forces
        ForceRegistry.FixedUpdate(dt);

        // Intergrate My Rigid Bodies
        Integrate(dt);

        // Generate Contacts
        CollisionDetector.CollisionData data = GenerateContacts(0.0f, 0.2f);

        // Handle Contacts
        if (data.contacts.Count > 0)
        {
            CollisionDetector.Contact[] contacts = data.contacts.ToArray();
            //foreach (CollisionDetector.Contact contact in contacts)
            //{
            //    Debug.DrawRay(contact.point, contact.normal * contact.penetration, Color.red);
            //}
            //foreach (CollisionDetector.Contact contact in contacts)
            //{
            //    if (contact.bodies[0] != null)
            //    {
            //        // works for moving sphere positions, but not rotations
            //        contact.bodies[0].transform.position += contact.normal * (contact.penetration + 0.0001f);
            //        float speedIntoContact = Vector3.Dot(contact.bodies[0].velocity, contact.normal);
            //        contact.bodies[0].velocity -= contact.normal * speedIntoContact;
            //    }
            //}
            
            ContactResolver.SetIterations(contacts.Length * 2); 
            ContactResolver.ResolveContacts(contacts, dt);
        }

    }

    private static void Integrate(float dt)
    {
        ForceManager.FixedUpdate(dt);
        for (int i = 0; i < Bodies.Count; i++)
        {
            Bodies[i].Integrate(dt);
        }
    }

    private static CollisionDetector.CollisionData GenerateContacts(float friction, float restitution)
    {
        int limit = maxContacts;
        CollisionDetector.CollisionData data = new CollisionDetector.CollisionData(friction, restitution);
        int contactCount = 0;
        for (int planeIndex = 0; planeIndex < StaticPlanes.Count; planeIndex++)
        {
            Primitive.Plane plane = StaticPlanes[planeIndex];
            for (int i = 0; i < Spheres.Count; i++)
            {
                Primitive.Sphere sphere = Spheres[i];
                contactCount += CollisionDetector.SphereAndHalfSpace(sphere, plane, ref data);

                if (contactCount >= limit) // We've run out of contacts to fill and we're missing some contacts
                    break;
            }

            for (int i = 0; i < Boxes.Count; i++)
            {
                Primitive.Box box = Boxes[i];
                contactCount += CollisionDetector.BoxAndHalfSpace(box, plane, ref data);

                if (contactCount >= limit)
                    break;
            }
        }

        return data;
    }
}
