#pragma warning disable CS0618 // Type or member is obsolete

using System.Diagnostics;
using System.Numerics;
using Silk.NET.OpenGL.Legacy;

namespace CSharpSphere;

public class Sphere
{
    public float Radius = 1.0f;
    public float UMax = 2 * MathF.PI;
    public float VMax = MathF.PI;
    public int UDiv = 20;
    public int VDiv = 20;
    
    public readonly Messenger Messenger = new();

    private Vector3[,] GeneratePoints()
    {
        var points =  new Vector3[VDiv + 1, UDiv + 1];
        
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

                float x = Radius * cosU * sinV;
                float y = Radius * cosV;
                float z = Radius * sinU * sinV;
                
                points[row, col] = new Vector3(x, y, z);
            }
        }
        
        return points;
    }

    private static Triangle[] GenerateTriangles(Vector3[,] points)
    {
        int cols = points.GetLength(1);
        int rows = points.GetLength(0);
        
        var triangles = new Triangle[2 * cols * rows];

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

    /// <summary>
    ///     Вызывает методы <see cref="GeneratePoints" /> и <see cref="GenerateTriangles" /> для создания
    ///     массива точек и массива треугольников. Выполняет отрисовку сферы посредством OpenGL.
    ///     <list type="number">
    ///         <item>Процесс начинается вызовом <see cref="GL.Begin(GLEnum)" /></item>
    ///         <item>
    ///             Происходит отрисовка линий <see cref="GLEnum.Lines" /> между вершинами треугольников.
    ///             Вершины (точки) задаются методом <see cref="GL.Vertex2(float, float)" />
    ///         </item>
    ///         <item>Процесс завершается командой <see cref="GL.End" /></item>
    ///     </list>
    /// </summary>
    /// <seealso href="https://registry.khronos.org/OpenGL/specs/gl/glspec30.pdf#subsection.2.6.1">
    ///     Спецификация OpenGL 3.0
    /// </seealso>
    public void DrawWireframeSphere(GL gl)
    {
        var watch = Stopwatch.StartNew();

        var points = GeneratePoints();
        var triangles = GenerateTriangles(points);
        
        const float scale = 0.85f;
        
        gl.Begin(GLEnum.Lines);
        gl.Color3(0.8f, 0.8f, 0.8f);
        foreach (var tri in triangles)
        {
            var p1 = points[tri.Point1.Row, tri.Point1.Col];
            var p2 = points[tri.Point2.Row, tri.Point2.Col];
            var p3 = points[tri.Point3.Row, tri.Point3.Col];
            
            gl.Vertex2(p1.X * scale, p1.Y * scale);
            gl.Vertex2(p2.X * scale, p2.Y * scale);

            gl.Vertex2(p2.X * scale, p2.Y * scale);
            gl.Vertex2(p3.X * scale, p3.Y * scale);

            gl.Vertex2(p3.X * scale, p3.Y * scale);
            gl.Vertex2(p1.X * scale, p1.Y * scale);
        }
        gl.End();
        
        Messenger.Update(watch.ElapsedMilliseconds);
        watch.Stop();
    }
}