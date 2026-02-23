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
            float v = (float)row / VDiv * VMax;

            float sinV = MathF.Sin(v);

            for (int col = 0; col < UDiv + 1; col++)
            {
                float u = (float)col / UDiv * UMax;

                float sinU = MathF.Sin(u);
                float cosU = MathF.Cos(u);

                float x = Radius * cosU * sinV;
                float y = Radius * sinU * sinV;
                float z = Radius * cosU;
                
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

    public void DrawWireframeSphere(GL gl)
    {
        var watch = Stopwatch.StartNew();

        gl.LoadIdentity();
        gl.Begin(GLEnum.Lines);
        gl.Color3(0.0f, 0.8f, 0.0f);

        var points = GeneratePoints();
        var triangles = GenerateTriangles(points);

        const float scale = 0.85f;

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