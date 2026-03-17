using System.Diagnostics;

namespace CSharpSphere;


public struct Triangle((int, int) point1, (int, int) point2, (int, int) point3)
{
    public VertexIndex Point1 = new(point1.Item1, point1.Item2);
    public VertexIndex Point2 = new(point2.Item1, point2.Item2);
    public VertexIndex Point3 = new(point3.Item1, point3.Item2);
}


public readonly struct VertexIndex(int i, int j)
{
    public readonly int I = i;
    public readonly int J = j;

    public override string ToString() => $"({I}, {J})";
}


public struct Vector4(float x, float y, float z, float w = 1f)
{
    public readonly float X = x;
    public readonly float Y = y;
    public readonly float Z = z;
    public readonly float W = w;
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

    public static Matrix4 Identity => new Matrix4(new float[,]
    {
        {1f, 0f, 0f, 0f},
        {0f, 1f, 0f, 0f},
        {0f, 0f, 1f, 0f},
        {0f, 0f, 0f, 1f}
    });

    public static Vector4 operator * (Vector4 v, Matrix4 m)
    {
        return Multiply(v, m);
    }
    
    public static Matrix4 operator * (Matrix4 a, Matrix4 b)
    {
        return Multiply(a, b);
    }

    private static Matrix4 Multiply(Matrix4 a, Matrix4 b)
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

    private static Vector4 Multiply(Vector4 v, Matrix4 m)
    {
        return new Vector4(
            v.X * m.Mat[0, 0] + v.Y * m.Mat[1, 0] + v.Z * m.Mat[2, 0] + v.W * m.Mat[3, 0],
            v.X * m.Mat[0, 1] + v.Y * m.Mat[1, 1] + v.Z * m.Mat[2, 1] + v.W * m.Mat[3, 1],
            v.X * m.Mat[0, 2] + v.Y * m.Mat[1, 2] + v.Z * m.Mat[2, 2] + v.W * m.Mat[3, 2],
            v.X * m.Mat[0, 3] + v.Y * m.Mat[1, 3] + v.Z * m.Mat[2, 3] + v.W * m.Mat[3, 3]);
    }

    public static Matrix4 GetRotateX(float angle)
    {
        float cos = MathF.Cos(angle * MathF.PI / 180f);
        float sin = MathF.Sin(angle * MathF.PI / 180f);
        var rotateX = new Matrix4(new float[,]
        {
            {1f, 0f  , 0f  , 0f},
            {0f, cos , sin , 0f},
            {0f, -sin, cos , 0f},
            {0f, 0f  , 0f  , 1f}
        });
        return rotateX;
    }
    
    public static Matrix4 GetRotateY(float angle)
    {
        float cos = MathF.Cos(angle * MathF.PI / 180f);
        float sin = MathF.Sin(angle * MathF.PI / 180f);
        var rotateX = new Matrix4(new float[,]
        {
            {cos, 0f, -sin, 0f},
            {0f , 1f, 0f  , 0f},
            {sin, 0f, cos , 0f},
            {0f , 0f, 0f  , 1f}
        });
        return rotateX;
    }
    
    public static Matrix4 GetRotateZ(float angle)
    {
        float cos = MathF.Cos(angle * MathF.PI / 180f);
        float sin = MathF.Sin(angle * MathF.PI / 180f);
        var rotateX = new Matrix4(new float[,]
        {
            {cos , sin, 0f, 0f},
            {-sin, cos, 0f  , 0f},
            {0f  , 0f , 1f , 0f},
            {0f  , 0f , 0f  , 1f}
        });
        return rotateX;
    }
}