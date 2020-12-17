using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCollision : MonoBehaviour
{
    public MyRigidBody sphereA;
    public MyRigidBody sphereB;
    public MyRigidBody boxA;
    public Transform planeA;
    public Transform planeB;

    void Start()
    {
        WorldManager.Bodies.Add(sphereA);
        WorldManager.Bodies.Add(sphereB);
        WorldManager.Bodies.Add(boxA);

        Primitive.Sphere sA = new Primitive.Sphere(sphereA, Vector3.zero, Quaternion.identity, 0.5f);
        Primitive.Sphere sB = new Primitive.Sphere(sphereB, Vector3.zero, Quaternion.identity, 0.5f);
        Primitive.Box bA = new Primitive.Box(boxA, Vector3.zero, Quaternion.identity, Vector3.one * 0.5f);
        Primitive.Plane pA = new Primitive.Plane(planeA.up, planeA.position);
        Primitive.Plane pB = new Primitive.Plane(planeB.up, planeB.position);

        WorldManager.StaticPlanes.Add(pA);
        WorldManager.StaticPlanes.Add(pB);
        WorldManager.Spheres.Add(sA);
        WorldManager.Spheres.Add(sB);
        //WorldManager.Boxes.Add(bA);

        GravityFG gravity = new GravityFG(Vector3.down * 2);
        ForceManager.Add(gravity, sphereA);
        //ForceManager.Add(gravity, sphereB);
        //ForceManager.Add(gravity, boxA);

        //CollisionDetector.CollisionData data = new CollisionDetector.CollisionData(0, 0);
        //int contactCount = 0;
        //contactCount += CollisionDetector.SphereAndSphere(sA, sB, ref data);
        //contactCount += CollisionDetector.SphereAndHalfSpace(sA, p, ref data);
        //contactCount += CollisionDetector.SphereAndHalfSpace(sB, p, ref data);

        //contactCount += CollisionDetector.BoxAndSphere(bA, sA, ref data);
        //contactCount += CollisionDetector.BoxAndSphere(bA, sB, ref data);
        //contactCount += CollisionDetector.BoxAndHalfSpace(bA, p, ref data);

        //Debug.Log(contactCount);

        //foreach (CollisionDetector.Contact contact in data.contacts)
        //{
        //    Debug.DrawRay(contact.point, contact.normal * contact.penetration, Color.red);
        //}
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
            sphereA.AddForce(Vector3.left * 200, sphereA.transform.position + Vector3.up);

        if (Input.GetKeyDown(KeyCode.D))
            sphereA.AddForce(Vector3.right * 200, sphereA.transform.position + Vector3.up);

        if (Input.GetKeyDown(KeyCode.W))
            sphereA.AddForce(Vector3.forward * 200, sphereA.transform.position + Vector3.up);

        if (Input.GetKeyDown(KeyCode.S))
            sphereA.AddForce(Vector3.back * 200, sphereA.transform.position + Vector3.up);

        if (Input.GetKeyDown(KeyCode.Space))
            sphereA.AddForce(Vector3.up * 200);
    }
}
