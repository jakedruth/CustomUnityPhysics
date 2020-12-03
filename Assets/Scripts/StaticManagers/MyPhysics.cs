using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public static class MyPhysics
{
    public static void Test()
    {

    }

    /// <summary>
    /// Get the inertia tensor of any rectangular six-sided object, where the object has constant density
    /// </summary>
    /// <param name="mass">The mass of the cuboid</param>
    /// <param name="sizeX">The extent of the cuboid along the X-axis</param>
    /// <param name="sizeY">The extent of the cuboid along the Y-axis</param>
    /// <param name="sizeZ">The extent of the cuboid along the Z-axis</param>
    /// <returns>The inertia tensor of the cuboid</returns>
    public static Matrix3X3 GetInertiaTensorCuboid(float mass, float sizeX, float sizeY, float sizeZ)
    {
        const float fraction = 1f / 12f;
        return new Matrix3X3(
            fraction * mass * (sizeY * sizeY + sizeZ * sizeZ), 0, 0,
            0, fraction * mass * (sizeX * sizeX + sizeZ * sizeZ), 0,
            0, 0, fraction * mass * (sizeX * sizeX + sizeY * sizeY)
        );
    }

    /// <summary>
    /// Get the inertia tensor of any sphere, where the object has constant density
    /// </summary>
    /// <param name="mass">The mass of the sphere</param>
    /// <param name="radius">The radius of the sphere</param>
    /// <returns>The inertia Tensor of the sphere</returns>
    public static Matrix3X3 GetInertiaTensorSphere(float mass, float radius)
    {
        const float fraction = 2f / 5f;
        return new Matrix3X3(
            fraction * mass * radius * radius, 0, 0,
            0, fraction * mass * radius * radius, 0,
            0, 0, fraction * mass * radius * radius
        );
    }

    /// <summary>
    /// Get the inertia tensor of any sphere, where the object has all of it's mass on the surface of the sphere
    /// </summary>
    /// <param name="mass">The mass of the sphere</param>
    /// <param name="radius">The radius of the sphere</param>
    /// <returns>The inertia Tensor of the shell of a sphere</returns>
    public static Matrix3X3 GetInertiaTensorSphereShell(float mass, float radius)
    {
        const float fraction = 2f / 3f;
        return new Matrix3X3(
            fraction * mass * radius * radius, 0, 0,
            0, fraction * mass * radius * radius, 0,
            0, 0, fraction * mass * radius * radius
        );
    }

    /// <summary>
    /// Get the inertia tensor of an ellipsoid, where the object has constant density
    /// </summary>
    /// <param name="mass">The mass of the ellipsoid</param>
    /// <param name="radiusX">the radius of the ellispoid along the X-axis</param>
    /// <param name="radiusY">the radius of the ellispoid along the Y-axis</param>
    /// <param name="radiusZ">the radius of the ellispoid along the Z-axis</param>
    /// <returns>The inertia Tensor of the ellipsoid</returns>
    public static Matrix3X3 GetInertiaTensorEllipsoid(float mass, float radiusX, float radiusY, float radiusZ)
    {
        const float fraction = 1f / 5f;
        return new Matrix3X3(
            fraction * mass * (radiusY * radiusY + radiusZ * radiusZ), 0, 0,
            0, fraction * mass * (radiusX * radiusX + radiusZ * radiusZ), 0,
            0, 0, fraction * mass * (radiusX * radiusX + radiusY * radiusY)
        );
    }
}

public class Matrix3X3
{
    private readonly float[] _mat = new float[9];

    public Matrix3X3()
    { }

    public Matrix3X3(float a, float b, float c, float d, float e, float f, float g, float h, float i)
    {
        _mat[0] = a;
        _mat[1] = b;
        _mat[2] = c;
        _mat[3] = d;
        _mat[4] = e;
        _mat[5] = f;
        _mat[6] = g;
        _mat[7] = h;
        _mat[8] = i;
    }

    public static Vector3 operator *(Matrix3X3 m, Vector3 v)
    {
        return new Vector3(
            v.x * m[0] + v.y * m[1] + v.z * m[2],
            v.x * m[3] + v.y * m[4] + v.z * m[5],
            v.x * m[6] + v.y * m[7] + v.z * m[8]);
    }

    public static Matrix3X3 operator *(Matrix3X3 a, Matrix3X3 b)
    {
        return new Matrix3X3(
            a[0] * b[0] + a[1] * b[3] + a[2] + b[6], 
            a[0] * b[1] + a[1] * b[4] + a[2] + b[7],
            a[0] * b[2] + a[1] * b[3] + a[2] + b[8],

            a[3] * b[0] + a[4] * b[3] + a[5] + b[6],
            a[3] * b[1] + a[4] * b[4] + a[5] + b[7],
            a[3] * b[2] + a[4] * b[5] + a[5] + b[8],

            a[6] * b[0] + a[7] * b[3] + a[8] + b[6],
            a[6] * b[1] + a[7] * b[4] + a[8] + b[7],
            a[6] * b[2] + a[7] * b[5] + a[8] + b[8]);
    }

    public Vector3 Transform(Vector3 vector)
    {
        return this * vector;
    }

    /// <summary>
    /// Sets the matrix to be the inverse of the given matrix
    /// </summary>
    /// <param name="m">The given Matrix</param>
    public Matrix3X3 SetInverse(Matrix3X3 m)
    {
        float t1 = m[0]*m[4];
        float t2 = m[0]*m[5];
        float t3 = m[1]*m[3];
        float t4 = m[2]*m[3];
        float t5 = m[1]*m[6];
        float t6 = m[2]*m[6];
        
        // Calculate the determinant.
        float determinant = t1 * m[8] - t2 * m[7] - t3 * m[8] + t4 * m[7] + t5 * m[5] - t6 * m[4];
        
        // Make sure the determinant is non-zero.
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (determinant == 0.0f)
            return this;

        float inverseDeterminant = 1.0f / determinant;
        _mat[0] = +(m[4] * m[8] - m[5] * m[7]) * inverseDeterminant;
        _mat[1] = -(m[1] * m[8] - m[2] * m[7]) * inverseDeterminant;
        _mat[2] = +(m[1] * m[5] - m[2] * m[4]) * inverseDeterminant;
        _mat[3] = -(m[3] * m[8] - m[5] * m[6]) * inverseDeterminant;
        _mat[4] = +(m[0] * m[8] - t6) * inverseDeterminant;
        _mat[5] = -(t2 - t4) * inverseDeterminant;
        _mat[6] = +(m[3] * m[7] - m[4] * m[6]) * inverseDeterminant;
        _mat[7] = -(m[0] * m[7] - t5) * inverseDeterminant;
        _mat[8] = (t1 - t3) * inverseDeterminant;

        return this;
    }

    public Matrix3X3 GetInverse()
    {
        return new Matrix3X3().SetInverse(this);
    }

    public void Invert()
    {
        SetInverse(this);
    }

    public Matrix3X3 SetTranspose(Matrix3X3 m)
    {
        _mat[0] = m[0];
        _mat[1] = m[3];
        _mat[2] = m[6];
        _mat[3] = m[1];
        _mat[4] = m[4];
        _mat[5] = m[7];
        _mat[6] = m[2];
        _mat[7] = m[5];
        _mat[8] = m[8];

        return this;
    }

    public Matrix3X3 GetTranspose()
    {
        return new Matrix3X3().SetTranspose(this);
    }

    public void SetOrientation(Quaternion q)
    {
        _mat[0] = 1 - (2 * q.y * q.y + 2 * q.z * q.z);
        _mat[1] = 2 * q.x * q.y + 2 * q.z * q.w;
        _mat[2] = 2 * q.x * q.z - 2 * q.y * q.w;
        _mat[3] = 2 * q.x * q.y - 2 * q.z * q.w;
        _mat[4] = 1 - (2 * q.x * q.x + 2 * q.z * q.z);
        _mat[5] = 2 * q.y * q.z + 2 * q.x * q.w;
        _mat[6] = 2 * q.x * q.z + 2 * q.y * q.w;
        _mat[7] = 2 * q.y * q.z - 2 * q.x * q.w;
        _mat[8] = 1 - (2 * q.x * q.x + 2 * q.y * q.y);
    }

    public float this[int index]
    {
        get { return _mat[index]; }
        set { _mat[index] = value; }
    }

    public float this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= 3 || y < 0 || y >= 3)
                throw new IndexOutOfRangeException("X and Y are bounded to [0, 3)");
            return _mat[x + y * 3];
        }
        set
        {
            if (x < 0 || x >= 3 || y < 0 || y >= 3)
                throw new IndexOutOfRangeException("X and Y are bounded to [0, 3)");
            _mat[x + y * 3] = value;
        }
    }
}

public class Matrix3X4
{
    private readonly float[] _mat = new float[12];

    public Matrix3X4()
    { }

    public Matrix3X4(float a, float b, float c, float d, float e, float f, float g, float h, float i, float j, float k, float l)
    {
        _mat[0] = a;
        _mat[1] = b;
        _mat[2] = c;
        _mat[3] = d;
        _mat[4] = e;
        _mat[5] = f;
        _mat[6] = g;
        _mat[7] = h;
        _mat[8] = i;
        _mat[9] = j;
        _mat[10] = k;
        _mat[11] = l;
    }

    public static Vector3 operator *(Matrix3X4 m, Vector3 v)
    {
        return new Vector3(
            v.x * m[0] + v.y * m[1] + v.z + m[2] + m[3],
            v.x * m[4] + v.y * m[5] + v.z + m[6] + m[7],
            v.x * m[8] + v.y * m[9] + v.z + m[10] + m[11]);
    }

    public static Matrix3X4 operator *(Matrix3X4 a, Matrix3X4 b)
    {
        return new Matrix3X4 (
            b[0] * a[0] + b[4] * a[1] + b[8]  * a[2]  + 0,
            b[1] * a[0] + b[5] * a[1] + b[9]  * a[2]  + 0,
            b[2] * a[0] + b[6] * a[1] + b[10] * a[2]  + 0,
            b[3] * a[0] + b[7] * a[1] + b[11] * a[2]  + a[3],
            b[0] * a[4] + b[4] * a[5] + b[8]  * a[6]  + 0,
            b[1] * a[4] + b[5] * a[5] + b[9]  * a[6]  + 0,
            b[2] * a[4] + b[6] * a[5] + b[10] * a[6]  + 0,
            b[3] * a[4] + b[7] * a[5] + b[11] * a[6]  + a[7],
            b[0] * a[8] + b[4] * a[9] + b[8]  * a[10] + 0,
            b[1] * a[8] + b[5] * a[9] + b[9]  * a[10] + 0,
            b[2] * a[8] + b[6] * a[9] + b[10] * a[10] + 0,
            b[3] * a[8] + b[7] * a[9] + b[11] * a[10] + a[11]
        );
    }

    public Vector3 Transform(Vector3 vector)
    {
        return this * vector;
    }

    public Matrix3X4 SetInverse(Matrix3X4 m)
    {
        float determinant = GetDeterminant();
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (determinant == 0)
            return this;

        float inverseDeterminant = 1 / determinant;

        _mat[0] = (-m[9] * m[6] + m[5] * m[10]) * inverseDeterminant;
        _mat[4] = (m[8] * m[6] - m[4] * m[10]) * inverseDeterminant;
        _mat[8] = (-m[8] * m[5] + m[4] * m[9] * m[15]) * inverseDeterminant;
        _mat[1] = (m[9] * m[2] - m[1] * m[10]) * inverseDeterminant;
        _mat[5] = (-m[8] * m[2] + m[0] * m[10]) * inverseDeterminant;
        _mat[9] = (m[8] * m[1] - m[0] * m[9] * m[15]) * inverseDeterminant;
        _mat[2] = (-m[5] * m[2] + m[1] * m[6] * m[15]) * inverseDeterminant;
        _mat[6] = (+m[4] * m[2] - m[0] * m[6] * m[15]) * inverseDeterminant;
        _mat[10] = (-m[4] * m[1] + m[0] * m[5] * m[15]) * inverseDeterminant;
        _mat[3] = (m[9] * m[6] * m[3]
                   - m[5] * m[10] * m[3]
                   - m[9] * m[2] * m[7]
                   + m[1] * m[10] * m[7]
                   + m[5] * m[2] * m[11]
                   - m[1] * m[6] * m[11]) * inverseDeterminant;
        _mat[7] = (-m[8] * m[6] * m[3]
                   + m[4] * m[10] * m[3]
                   + m[8] * m[2] * m[7]
                   - m[0] * m[10] * m[7]
                   - m[4] * m[2] * m[11]
                   + m[0] * m[6] * m[11]) * inverseDeterminant;
        _mat[11] = (m[8] * m[5] * m[3]
                    - m[4] * m[9] * m[3]
                    - m[8] * m[1] * m[7]
                    + m[0] * m[9] * m[7]
                    + m[4] * m[1] * m[11]
                    - m[0] * m[5] * m[11]) * inverseDeterminant;

        return this;
    }

    private float GetDeterminant()
    {
        return _mat[8] * _mat[5] * _mat[2] +
               _mat[4] * _mat[9] * _mat[2] +
               _mat[8] * _mat[1] * _mat[6] -
               _mat[0] * _mat[9] * _mat[6] -
               _mat[4] * _mat[1] * _mat[10] +
               _mat[0] * _mat[5] * _mat[10];
    }

    public Matrix3X4 GetInverse()
    {
        return new Matrix3X4().SetInverse(this);
    }

    public void Invert()
    {
        SetInverse(this);
    }

    public void SetOrientationsAndPosition(Quaternion q, Vector3 pos)
    {
        _mat[0] = 1 - (2 * q.y * q.y + 2 * q.z * q.z);
        _mat[1] = 2 * q.x * q.y + 2 * q.z * q.w;
        _mat[2] = 2 * q.x * q.z - 2 * q.y * q.w;
        _mat[3] = pos.x;
        _mat[4] = 2 * q.x * q.y - 2 * q.z * q.w;
        _mat[5] = 1 - (2 * q.x * q.x + 2 * q.z * q.z);
        _mat[6] = 2 * q.y * q.z + 2 * q.x * q.w;
        _mat[7] = pos.y;
        _mat[8] = 2 * q.x * q.z + 2 * q.y * q.w;
        _mat[9] = 2 * q.y * q.z - 2 * q.x * q.w;
        _mat[10] = 1 - (2 * q.x * q.x + 2 * q.y * q.y);
        _mat[11] = pos.z;
    }

    public Vector3 LocalToWorld(Vector3 point)
    {
        return Transform(point);
    }

    public Vector3 WorldToLocal(Vector3 point)
    {
        return TransformInverse(point);
    }

    public Vector3 TransformInverse(Vector3 vector)
    {
        Vector3 tmp = vector;
        tmp.x -= _mat[3];
        tmp.y -= _mat[7];
        tmp.z -= _mat[11];
        return new Vector3(
            tmp.x * _mat[0] +
            tmp.y * _mat[4] +
            tmp.z * _mat[8],
            tmp.x * _mat[1] +
            tmp.y * _mat[5] +
            tmp.z * _mat[9],
            tmp.x * _mat[2] +
            tmp.y * _mat[6] +
            tmp.z * _mat[10]
        );
    }

    public Vector3 TransformDirection(Vector3 vector)
    {
        return new Vector3(
            vector.x * _mat[0] +
            vector.y * _mat[1] +
            vector.z * _mat[2],
            vector.x * _mat[4] +
            vector.y * _mat[5] +
            vector.z * _mat[6],
            vector.x * _mat[8] +
            vector.y * _mat[9] +
            vector.z * _mat[10]
        );
    }

    public Vector3 TransformInverseDirection(Vector3 vector)
    {
        return new Vector3(
            vector.x * _mat[0] +
            vector.y * _mat[4] +
            vector.z * _mat[8],
            vector.x * _mat[1] +
            vector.y * _mat[5] +
            vector.z * _mat[9],
            vector.x * _mat[2] +
            vector.y * _mat[6] +
            vector.z * _mat[10]
        );
    }

    public Vector3 LocalToWorldDirection(Vector3 direction)
    {
        return TransformDirection(direction);
    }

    public Vector3 WorldToLocalDirection(Vector3 direction)
    {
        return TransformInverseDirection(direction);
    }

    public float this[int index]
    {
        get { return _mat[index]; }
        set { _mat[index] = value; }
    }

    public float this[int x, int y]
    {
        get
        {
            if (x < 0 || x >= 3 || y < 0 || y >= 4)
                throw new IndexOutOfRangeException("X is bounded to [0, 3) and Y is bounded to [0, 4)");
            return _mat[x + y * 3];
        }
        set {
            if (x < 0 || x >= 3 || y < 0 || y >= 4)
                throw new IndexOutOfRangeException("X is bounded to [0, 3) and Y is bounded to [0, 4)");
            _mat[x + y * 3] = value; }
    }
}
