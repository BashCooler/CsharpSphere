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
    public bool TwoStep = false;
    public Vector4 OuterColor = new(0.9f, 0.2f, 0.2f);
    public Vector4 InnerColor = new(0.2f, 0.2f, 0.9f);

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
            Update = false;
        }

        if (Shading)
            DrawPolygons(_triangles, _points);
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
                    (i + 1, j + 1),
                    (i, j + 1),
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
        
    }
}