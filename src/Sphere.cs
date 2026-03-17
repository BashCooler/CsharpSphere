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

    public float angleX = 0f;
    public float angleY = 0f;
    public float angleZ = 0f;
    
    public string Message = "";

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
        
        var transformationMat = Matrix4.Identity * Matrix4.GetRotateX(angleX) * Matrix4.GetRotateY(angleY) * Matrix4.GetRotateZ(angleZ);
        
        for (int row = 0; row < VDiv + 1; row++)
        {
            for (int col = 0; col < UDiv + 1; col++)
            {
                result[row, col] = points[row, col] * transformationMat;
            }
        }
        
        return result;
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
        var transformedPoints = Transform(points);
        var triangles = GenerateTriangles(transformedPoints);
        
        const float scale = 0.85f;
        
        gl.Begin(GLEnum.Lines);
        gl.Color3(0.6f, 0.6f, 0.6f);
        foreach (var tri in triangles)
        {
            var p1 = transformedPoints[tri.Point1.I, tri.Point1.J];
            var p2 = transformedPoints[tri.Point2.I, tri.Point2.J];
            var p3 = transformedPoints[tri.Point3.I, tri.Point3.J];
            
            gl.Vertex2(p1.X * scale, p1.Y * scale);
            gl.Vertex2(p2.X * scale, p2.Y * scale);

            gl.Vertex2(p2.X * scale, p2.Y * scale);
            gl.Vertex2(p3.X * scale, p3.Y * scale);

            gl.Vertex2(p3.X * scale, p3.Y * scale);
            gl.Vertex2(p1.X * scale, p1.Y * scale);
        }
        gl.End();
        
        Message = $"\nВремя кадра: {watch.ElapsedMilliseconds} ms";
        watch.Stop();
    }
}