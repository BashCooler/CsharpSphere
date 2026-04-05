#pragma warning disable CS0618 // Type or member is obsolete

using System.Diagnostics;

using static CSharpSphere.Program;

namespace CSharpSphere;

public class Sphere
{
    public int R = 700;
    public int UMax = 360;
    public int VMax = 180;
    public int UDiv = 20;
    public int VDiv = 21;

    private Vector4[,] _points = null!;
    private Triangle[] _triangles = null!;
    public bool Update = true;
    
    public bool Shading = false;
    public bool TwoStep = true;
    public Color OuterColor = new(0.9f, 0.2f, 0.2f);
    public Color InnerColor = new(0.2f, 0.2f, 0.9f);

    public Matrix4 TransformationMat = Matrix4.Identity;

    /// <seealso href="https://registry.khronos.org/OpenGL/specs/gl/glspec30.pdf#subsection.2.6.1">
    ///     Спецификация OpenGL 3.0
    /// </seealso>
    public void Draw()
    {
        var watch = Stopwatch.StartNew();

        if (Update)
        {
            GeneratePoints();
            GenerateTriangles();
            Transform();
            if (Shading) GenerateColors();
            Update = false;
        }

        if (Shading)
            DrawPolygons(_triangles, _points, TwoStep);
        else
            DrawLines(_triangles, _points);
        
        watch.Stop();
    }
    
    
    /// <summary>
    ///     Составляет массив точек сферы
    /// </summary>
    /// <returns>2D массив точек <see cref="Vector4" /></returns>
    /// <seealso href="https://ps-group.github.io/opengl/lesson_11#wow1">
    ///     UV-параметризация сферы
    /// </seealso>
    private void GeneratePoints()
    {
        var points = new Vector4[VDiv + 1, UDiv + 1];

        Parallel.For(0, VDiv + 1, row =>
        {
            var v = (float)row / VDiv * (VMax * MathF.PI / 180f);

            float sinV = MathF.Sin(v);
            float cosV = MathF.Cos(v);

            for (int col = 0; col < UDiv + 1; col++)
            {
                var u = (float)col / UDiv * (UMax * MathF.PI / 180f);

                float sinU = MathF.Sin(u);
                float cosU = MathF.Cos(u);
                
                float x = R * cosU * sinV;
                float y = R * cosV;
                float z = R * sinU * sinV;
                
                points[row, col] = new Vector4(x, y, z);
            }
        });
        
        _points = points;
    }
    
    private void GenerateTriangles()
    {
        int cols = UDiv + 1;
        int rows = VDiv + 1;
        
        var triangles = new Triangle[2 * (rows - 1) * (cols - 1)];

        Parallel.For(0, rows - 1, i =>
        {
            for (int j = 0; j < cols - 1; j++)
            {
                int index = 2 * (i * (cols - 1) + j);
                
                triangles[index] = new Triangle(
                    (i, j),
                    (i, j + 1),
                    (i + 1, j)
                );
                triangles[index + 1] = new Triangle(
                    (i, j + 1),
                    (i + 1, j + 1),
                    (i + 1, j)
                );
            }
        });
        
        _triangles = triangles;
    }
    
    private void Transform()
    {
        var result = new Vector4[VDiv + 1, UDiv + 1];

        Parallel.For(0, VDiv + 1, row =>
        {
            for (int col = 0; col < UDiv + 1; col++)
                result[row, col] = _points[row, col] * TransformationMat;
        });
        
       _points = result;
    }

    private void GenerateColors()
    {
        var lightPos = new Vector4(0f, 0f, 1f, 0f);

        foreach (Triangle tri in _triangles)
        {
            Vector4 n = NewellNormal([
                _points[tri.IdxP1.I, tri.IdxP1.J], 
                _points[tri.IdxP2.I, tri.IdxP2.J], 
                _points[tri.IdxP3.I, tri.IdxP3.J]]);
            
            n = Vector4.Normalize(n);
            float cos = n.X * lightPos.X + n.Y * lightPos.Y + n.Z * lightPos.Z;
            cos = Math.Clamp(cos, -1f, 1f);
            
            if (cos >= 0) 
                tri.SetColor(OuterColor * cos).SetFront(true);
            else
                tri.SetColor(InnerColor * MathF.Abs(cos)).SetFront(false);
        }
    }

    private static Vector4 NewellNormal(Vector4[] points)
    {
        var n = new Vector4(0, 0, 0, 0);
        
        for (int i = 0; i < points.Length; i++)
        {
            Vector4 p0 = points[i];
            Vector4 p1 = points[(i + 1) % points.Length];
            
            n.X += (p0.Y - p1.Y) * (p0.Z + p1.Z);
            n.Y += (p0.Z - p1.Z) * (p0.X + p1.X);
            n.Z += (p0.X - p1.X) * (p0.Y + p1.Y);
        }
        return n;
    }
}
