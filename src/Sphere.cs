#pragma warning disable CS0618 // Type or member is obsolete

using System.Diagnostics;
using Silk.NET.OpenGL.Legacy;

namespace CSharpSphere;

public class Sphere
{
    public float R = 1.0f;
    public float UMax = 2 * MathF.PI;
    public float VMax = MathF.PI;
    public int UDiv = 20;
    public int VDiv = 20;

    public int AngleX;
    public int AngleY;
    public int AngleZ;
    
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
        DrawLines(gl, triangles, transformedPoints, 0.85f);
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
            var v = (float)row / VDiv * VMax;

            float sinV = MathF.Sin(v);
            float cosV = MathF.Cos(v);

            for (int col = 0; col < UDiv + 1; col++)
            {
                var u = (float)col / UDiv * UMax;

                float sinU = MathF.Sin(u);
                float cosU = MathF.Cos(u);

                float x = R * cosU * sinV;
                float y = R * cosV;
                float z = R * sinU * sinV;
                
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
        
        var transformationMat =
            Matrix4.Identity * Matrix4.GetRotateX(AngleX) 
                             * Matrix4.GetRotateY(AngleY) 
                             * Matrix4.GetRotateZ(AngleZ);
        
        for (int row = 0; row < VDiv + 1; row++)
        {
            for (int col = 0; col < UDiv + 1; col++)
            {
                result[row, col] = points[row, col] * transformationMat;
            }
        }
        
        return result;
    }

    private static void DrawLines(GL gl, Triangle[] triangles, Vector4[,] points, float scale)
    {
        foreach (var tri in triangles)
        {
            var p1 = points[tri.Point1.I, tri.Point1.J];
            var p2 = points[tri.Point2.I, tri.Point2.J];
            var p3 = points[tri.Point3.I, tri.Point3.J];
            
            gl.Vertex2(p1.X * scale, p1.Y * scale);
            gl.Vertex2(p2.X * scale, p2.Y * scale);

            gl.Vertex2(p2.X * scale, p2.Y * scale);
            gl.Vertex2(p3.X * scale, p3.Y * scale);

            gl.Vertex2(p3.X * scale, p3.Y * scale);
            gl.Vertex2(p1.X * scale, p1.Y * scale);
        }
    }
}