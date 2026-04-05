using System.Diagnostics;

namespace CSharpSphere;


public class Triangle((int, int) p1, (int, int) p2, (int, int) p3)
{
    private readonly VertexIndex _idxP1 = new(p1.Item1, p1.Item2);
    private readonly VertexIndex _idxP2 = new(p2.Item1, p2.Item2);
    private readonly VertexIndex _idxP3 = new(p3.Item1, p3.Item2);
    
    public Color Color;
    public bool Front = true;

    public Vector3 GetP1(Vector3[,] points) => points[_idxP1.I, _idxP1.J];
    public Vector3 GetP2(Vector3[,] points) => points[_idxP2.I, _idxP2.J];
    public Vector3 GetP3(Vector3[,] points) => points[_idxP3.I, _idxP3.J];
    
    public Triangle SetColor(Color color)
    {
        Color = color;
        return this;
    }

    public Triangle SetFront(bool front)
    {
        Front = front;
        return this;
    }
}


public readonly struct VertexIndex(int i, int j)
{
    public readonly int I = i;
    public readonly int J = j;

    public override string ToString() => $"({I}, {J})";
}


public struct Vector3(float x, float y, float z)
{
    public float X = x;
    public float Y = y;
    public float Z = z;

    public static Vector3 Normalize(Vector3 v)
    {
        float len = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
        if (len == 0f)
            return new Vector3(0f, 0f, 0f);

        float inv = 1.0f / MathF.Sqrt(len);
        return new Vector3(v.X * inv, v.Y * inv, v.Z * inv);
    }
}

public struct Color(float r, float g, float b)
{
    public System.Numerics.Vector3 Rgb = new(r, g, b);
    
    public float R => Rgb.X;
    public float G => Rgb.Y;
    public float B => Rgb.Z;

    public static Color operator *(Color c, float f)
    {
        return new Color(c.R*f, c.G*f, c.B*f);
    }
}

public struct Matrix4
{
    public float[,] Mat;

    public Matrix4(float[,] matrix)
    {
        Debug.Assert(
            matrix.GetLength(0) == 4 || matrix.GetLength(1) == 4, 
            "" +
            "Matrix4 matrix must have 4 rows and 4 columns, " +
            $"not {matrix.GetLength(0)} and {matrix.GetLength(1)}.");
        Mat = matrix;
    }

    public static Matrix4 Identity => new(new[,]
    {
        {1f, 0f, 0f, 0f},
        {0f, 1f, 0f, 0f},
        {0f, 0f, 1f, 0f},
        {0f, 0f, 0f, 1f}
    });

    public static Vector3 operator * (Vector3 v, Matrix4 m)
    {
        const float w = 1.0f;
        return new Vector3(
            v.X * m.Mat[0, 0] + v.Y * m.Mat[1, 0] + v.Z * m.Mat[2, 0] + w * m.Mat[3, 0],
            v.X * m.Mat[0, 1] + v.Y * m.Mat[1, 1] + v.Z * m.Mat[2, 1] + w * m.Mat[3, 1],
            v.X * m.Mat[0, 2] + v.Y * m.Mat[1, 2] + v.Z * m.Mat[2, 2] + w * m.Mat[3, 2]);
    }
    
    public static Matrix4 operator * (Matrix4 a, Matrix4 b)
    {
        var c = new float[4, 4];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                c[i, j] = 0;
                for (int k = 0; k < 4; k++)
                {
                    c[i, j] += a.Mat[i, k] * b.Mat[k, j];
                }
            }
        }
        return new Matrix4(c);
    }

    public static Matrix4 GetRotateX(int angle)
    {
        float cos = MathF.Cos(angle * MathF.PI / 180f);
        float sin = MathF.Sin(angle * MathF.PI / 180f);
        return new Matrix4(new[,]
        {
            {1f, 0f  , 0f  , 0f},
            {0f, cos , sin , 0f},
            {0f, -sin, cos , 0f},
            {0f, 0f  , 0f  , 1f}
        });
    }
    
    public static Matrix4 GetRotateY(int angle)
    {
        float cos = MathF.Cos(angle * MathF.PI / 180f);
        float sin = MathF.Sin(angle * MathF.PI / 180f);
        return new Matrix4(new[,]
        {
            {cos, 0f, -sin, 0f},
            {0f , 1f, 0f  , 0f},
            {sin, 0f, cos , 0f},
            {0f , 0f, 0f  , 1f}
        });
    }
    
    public static Matrix4 GetRotateZ(int angle)
    {
        float cos = MathF.Cos(angle * MathF.PI / 180f);
        float sin = MathF.Sin(angle * MathF.PI / 180f);
        return new Matrix4(new[,]
        {
            {cos , sin, 0f, 0f},
            {-sin, cos, 0f  , 0f},
            {0f  , 0f , 1f , 0f},
            {0f  , 0f , 0f  , 1f}
        });
    }
}
