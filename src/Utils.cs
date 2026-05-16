namespace CSharpSphere;


public class Triangle((int, int) p1, (int, int) p2, (int, int) p3)
{
    private readonly int _p1I = p1.Item1;
    private readonly int _p2I = p2.Item1;
    private readonly int _p3I = p3.Item1;
    
    private readonly int _p1J = p1.Item2;
    private readonly int _p2J = p2.Item2;
    private readonly int _p3J = p3.Item2;
    
    public Color Color;
    public bool Outer = true;

    public Vector3 P1(Vector3[,] points) => points[_p1I, _p1J];
    public Vector3 P2(Vector3[,] points) => points[_p2I, _p2J];
    public Vector3 P3(Vector3[,] points) => points[_p3I, _p3J];
    
    public Triangle SetColor(Color color)
    {
        Color = color;
        return this;
    }

    public Triangle SetOuter(bool front)
    {
        Outer = front;
        return this;
    }
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
    public readonly float R = r;
    public readonly float G = g;
    public readonly float B = b;

    public static Color operator * (Color c, float f) => 
        new(c.R * f, c.G * f, c.B * f);
}

public readonly struct Matrix4
{
    private readonly float[,] _mat;

    public Matrix4()
    {
        _mat = new[,]
        {
            {0f, 0f, 0f, 0f},
            {0f, 0f, 0f, 0f},
            {0f, 0f, 0f, 0f},
            {0f, 0f, 0f, 0f}
        };
    }

    private Matrix4(
        (float, float, float, float) r1,
        (float, float, float, float) r2,
        (float, float, float, float) r3,
        (float, float, float, float) r4)
    {
        _mat = new[,]
        {
            {r1.Item1, r2.Item1, r3.Item1, r4.Item1},
            {r1.Item2, r2.Item2, r3.Item2, r4.Item2},
            {r1.Item3, r2.Item3, r3.Item3, r4.Item3},
            {r1.Item4, r2.Item4, r3.Item4, r4.Item4}
        };
    }

    private float this[int i, int j]
    {
        get => _mat[i, j];
        set => _mat[i, j] = value;
    }

    public static Matrix4 Identity => new(
        (1f, 0f, 0f, 0f),
        (0f, 1f, 0f, 0f),
        (0f, 0f, 1f, 0f),
        (0f, 0f, 0f, 1f)
    );

    public static Vector3 operator * (Vector3 v, Matrix4 m)
    {
        float x = 
            v.X * m._mat[0, 0] + 
            v.Y * m._mat[1, 0] + 
            v.Z * m._mat[2, 0] + 
            1f  * m._mat[3, 0];
        float y = 
            v.X * m._mat[0, 1] + 
            v.Y * m._mat[1, 1] + 
            v.Z * m._mat[2, 1] + 
            1f  * m._mat[3, 1];
        float z = 
            v.X * m._mat[0, 2] + 
            v.Y * m._mat[1, 2] + 
            v.Z * m._mat[2, 2] + 
            1f  * m._mat[3, 2];
        
        return new Vector3(x, y, z);
    }
    
    public static Matrix4 operator * (Matrix4 a, Matrix4 b)
    {
        var c = new Matrix4();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                c[i, j] = 0;
                for (int k = 0; k < 4; k++) 
                    c[i, j] += a[i, k] * b[k, j];
            }
        }
        return c;
    }

    public static Matrix4 GetRotateX(int angle)
    {
        float cos = MathF.Cos(angle * MathF.PI / 180f);
        float sin = MathF.Sin(angle * MathF.PI / 180f);
        return new Matrix4(
            (1f, 0f  , 0f  , 0f),
            (0f, cos , sin , 0f),
            (0f, -sin, cos , 0f),
            (0f, 0f  , 0f  , 1f)
        );
    }
    
    public static Matrix4 GetRotateY(int angle)
    {
        float cos = MathF.Cos(angle * MathF.PI / 180f);
        float sin = MathF.Sin(angle * MathF.PI / 180f);
        return new Matrix4(
            (cos, 0f, -sin, 0f),
            (0f , 1f, 0f  , 0f),
            (sin, 0f, cos , 0f),
            (0f , 0f, 0f  , 1f)
        );
    }
    
    public static Matrix4 GetRotateZ(int angle)
    {
        float cos = MathF.Cos(angle * MathF.PI / 180f);
        float sin = MathF.Sin(angle * MathF.PI / 180f);
        return new Matrix4(
            (cos , sin, 0f, 0f),
            (-sin, cos, 0f, 0f),
            (0f  , 0f , 1f, 0f),
            (0f  , 0f , 0f, 1f)
        );
    }
}
