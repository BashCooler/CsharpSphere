#pragma warning disable CS0618

using Silk.NET.OpenGL.Legacy;

namespace CSharpSphere;

public static partial class Program
{
    public static void DrawWireframe(Triangle[] triangles, Vector3[,] points)
    {
        DrawAxes(Axes.X | Axes.Y);

        _gl.Begin(GLEnum.Lines);
        _gl.Color3(0.5f, 0.5f, 0.5f);

        foreach (Triangle tri in triangles)
        {
            Vector3 p1 = tri.P1(points);
            Vector3 p2 = tri.P2(points);
            Vector3 p3 = tri.P3(points);

            DrawLine(p1, p2);
            DrawLine(p2, p3);
            DrawLine(p3, p1);
        }

        _gl.End();

        DrawAxes(Axes.Z);
    }

    public static void DrawFlat(Triangle[] triangles, Vector3[,] points, Render mode)
    {
        switch (mode)
        {
            case Render.Single:
                DrawTriangles(triangles, points, Sides.All);
                DrawAxes(Axes.All);
                break;
            default:
            case Render.Double:
                DrawTriangles(triangles, points, Sides.Inner);
                DrawAxes(Axes.All);
                DrawTriangles(triangles, points, Sides.Outer);
                break;
            case Render.DepthTest:
                DrawTriangles(triangles, points, Sides.All, true);
                DrawAxes(Axes.All, true);
                break;
        }
    }
    
    [Flags]
    private enum Sides
    {
        Outer = 1 << 0,
        Inner = 1 << 1,
        All = Outer | Inner
    }

    private static void DrawTriangles(Triangle[] triangles, Vector3[,] points, Sides sides, bool depthTest = false)
    {
        if (depthTest) _gl.Enable(EnableCap.DepthTest);
        _gl.Begin(GLEnum.Triangles);

        foreach (Triangle tri in triangles)
        {
            switch (tri.Outer)
            {
                case true when sides.HasFlag(Sides.Outer):
                case false when sides.HasFlag(Sides.Inner):
                    break;
                default:
                    continue;
            }
            
            Vector3 p1 = tri.P1(points);
            Vector3 p2 = tri.P2(points);
            Vector3 p3 = tri.P3(points);
            
            Color color = tri.Color;
            _gl.Color3(color.R, color.G, color.B);

            if (depthTest)
                DrawTriangle3D(p1, p2, p3);
            else 
                DrawTriangle(p1, p2, p3);
        }
        
        _gl.End();
        if (depthTest) _gl.Disable(EnableCap.DepthTest);
    }

    private static void DrawTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var s = _windowMinSize;
        _gl.Vertex2(p1.X / s, p1.Y / s);
        _gl.Vertex2(p2.X / s, p2.Y / s);
        _gl.Vertex2(p3.X / s, p3.Y / s);
    }

    private static void DrawTriangle3D(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var s = _windowMinSize;
        _gl.Vertex3(p1.X / s, p1.Y / s, p1.Z / s);
        _gl.Vertex3(p2.X / s, p2.Y / s, p2.Z / s);
        _gl.Vertex3(p3.X / s, p3.Y / s, p3.Z / s);
    }

    private static void DrawLine(Vector3 p1, Vector3 p2)
    {
        var s = _windowMinSize;
        _gl.Vertex2(p1.X / s, p1.Y / s);
        _gl.Vertex2(p2.X / s, p2.Y / s);
    }
    
    [Flags]
    private enum Axes
    {
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,
        All = X | Y | Z
    }

    private static void DrawAxes(Axes axes, bool depthTest = false)
    {
        if (depthTest) _gl.Enable(EnableCap.DepthTest);
        _gl.LineWidth(3f);
        
        if (axes.HasFlag(Axes.X))
        {
            _gl.Begin(GLEnum.Lines);
            
            _gl.Color3(0.8f, 0f, 0f);
            _gl.Vertex2(0, 0);
            _gl.Vertex2(0.75f, 0);
            
            _gl.End();
            DrawLabelX();
        }

        if (axes.HasFlag(Axes.Y))
        {
            _gl.Begin(GLEnum.Lines);
            
            _gl.Color3(0f, 0.8f, 0f);
            _gl.Vertex2(0, 0);
            _gl.Vertex2(0, 0.75f);
            
            _gl.End();
            
            DrawLabelY();
        }

        if (axes.HasFlag(Axes.Z))
        {
            _gl.PointSize(5f);
            _gl.Begin(GLEnum.Points);

            _gl.Color3(0f, 0.2f, 0.8f);
            _gl.Vertex3(0, 0, 0.75f);

            _gl.End();
            _gl.PointSize(1f);
            
            DrawLabelZ();
        }
        
        _gl.LineWidth(1.0f);
        if (depthTest) _gl.Disable(EnableCap.DepthTest);
    }

    private static void DrawLabelX()
    {
        float c = 0.75f * _windowMinSize + Font;
        float s = _windowMinSize;
        
        _gl.Begin(GLEnum.Lines);
        _gl.Vertex2((c - Font * 0.5f) / s, -Font * 1.5f * 0.5f / s);
        _gl.Vertex2((c + Font * 0.5f) / s,  Font * 1.5f * 0.5f / s);
        _gl.Vertex2((c - Font * 0.5f) / s,  Font * 1.5f * 0.5f / s);
        _gl.Vertex2((c + Font * 0.5f) / s, -Font * 1.5f * 0.5f / s);
        _gl.End();
    }
    
    private static void DrawLabelY()
    {
        float c = 0.75f * _windowMinSize + Font * 1.5f;
        float s = _windowMinSize;
        
        _gl.Begin(GLEnum.Lines);
        _gl.Vertex2((0 - Font * 0.5f) / s, (c + Font * 1.5f * 0.5f) / s);
        _gl.Vertex2(0, c / _windowMinSize);
        _gl.Vertex2((0 + Font * 0.5f) / s, (c + Font * 1.5f * 0.5f) / s);
        _gl.Vertex2(0, c / s);
        _gl.Vertex2(0, c / s);
        _gl.Vertex2(0, (c - Font * 1.5f * 0.5f) / s);
        _gl.End();
    }
    
    private static void DrawLabelZ()
    {
        const float c = -Font * 1.5f;
        float s = _windowMinSize;
        
        _gl.Begin(GLEnum.Lines);
        _gl.Vertex3((c - Font * 0.5f) / s,  Font * 1.5d * 0.5f / s, 0.75f);
        _gl.Vertex3((c + Font * 0.5f) / s,  Font * 1.5d * 0.5f / s, 0.75f);
        _gl.Vertex3((c + Font * 0.5f) / s,  Font * 1.5d * 0.5f / s, 0.75f);
        _gl.Vertex3((c - Font * 0.5f) / s, -Font * 1.5d * 0.5f / s, 0.75f);
        _gl.Vertex3((c - Font * 0.5f) / s, -Font * 1.5d * 0.5f / s, 0.75f);
        _gl.Vertex3((c + Font * 0.5f) / s, -Font * 1.5d * 0.5f / s, 0.75f);
        _gl.End();
    }
}