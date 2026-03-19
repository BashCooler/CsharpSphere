#pragma warning disable CS0618 // Type or member is obsolete

using System.Diagnostics;
using Silk.NET.OpenGL.Legacy;

using static CSharpSphere.Program;

namespace CSharpSphere;

public class Sphere
{
    public int R = 700;
    
    public int UMax = 360;
    public int VMax = 180;
    public int UDiv = 20;
    public int VDiv = 20;

    public Matrix4 TransformationMat = Matrix4.Identity;

    public string Message = "";


    /// <seealso href="https://registry.khronos.org/OpenGL/specs/gl/glspec30.pdf#subsection.2.6.1">
    ///     Спецификация OpenGL 3.0
    /// </seealso>
    public void DrawWireframeSphere(GL gl)
    {
        var watch = Stopwatch.StartNew();

        Vector4[,] points = GeneratePoints();
        Vector4[,] transformedPoints = Transform(points);
        Triangle[] triangles = GenerateTriangles(transformedPoints);
        
        gl.Begin(GLEnum.Lines);
        gl.Color3(0.6f, 0.6f, 0.6f);
        DrawLines(triangles, transformedPoints);
        gl.End();
        
        Message = $"\nВремя кадра: {watch.ElapsedMilliseconds} ms";
        watch.Stop();
    }
    
    
    /// <summary>
    ///     Составляет массив точек сферы
    /// </summary>
    /// <returns>2D массив точек <see cref="Vector4" /></returns>
    /// <seealso href="https://ps-group.github.io/opengl/lesson_11#wow1">
    ///     UV-параметризация сферы
    /// </seealso>
    private Vector4[,] GeneratePoints()
    {
        var points =  new Vector4[VDiv + 1, UDiv + 1];
        
        for (int row = 0; row < VDiv + 1; row++)
        {
            var v = (float)row / VDiv * (VMax * MathF.PI / 180f);

            float sinV = MathF.Sin(v);
            float cosV = MathF.Cos(v);

            for (int col = 0; col < UDiv + 1; col++)
            {
                var u = (float)col / UDiv * (UMax * MathF.PI / 180f);

                float sinU = MathF.Sin(u);
                float cosU = MathF.Cos(u);

                float r = R;
                float x = r * cosU * sinV;
                float y = r * cosV;
                float z = r * sinU * sinV;
                
                points[row, col] = new Vector4(x, y, z);
            }
        }
        
        return points;
    }

    private static Triangle[] GenerateTriangles(Vector4[,] points)
    {
        int cols = points.GetLength(1);
        int rows = points.GetLength(0);
        
        var triangles = new Triangle[2 * (rows - 1) * (cols - 1)];

        var t = 0;
        for (int i = 0; i < rows - 1; i++)
        {
            for (int j = 0; j < cols - 1; j++)
            {
                triangles[t] = new Triangle(
                    (i, j),
                    (i, j + 1),
                    (i + 1, j)
                );
                triangles[t + 1] = new Triangle(
                    (i + 1, j + 1),
                    (i, j + 1),
                    (i + 1, j)
                );
                t += 2;
            }
        }
        
        return triangles;
    }

    private Vector4[,] Transform(Vector4[,] points)
    {
        var result = new Vector4[VDiv + 1, UDiv + 1];
        
        for (int row = 0; row < VDiv + 1; row++)
        {
            for (int col = 0; col < UDiv + 1; col++)
            {
                result[row, col] = points[row, col] * TransformationMat;
            }
        }
        
        return result;
    }
}