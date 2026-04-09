#pragma warning disable CS0618 // Type or member is obsolete

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL.Legacy;

namespace CSharpSphere;

public static class Program
{
    private static GL _gl = null!;
    private static IWindow _window = null!;
    private static IInputContext _input = null!;
    private static Gui _ui = null!;
    private static readonly Sphere Sphere = new();

    private const int Font = 12;
    private static int _windowMinSize;
    
    public static void Main()
    {
        var options = WindowOptions.Default with
        {
            Title = "Сфера",
            Size = new Vector2D<int>(1280, 720),
            API = new GraphicsAPI(
                ContextAPI.OpenGL,
                ContextProfile.Compatability,
                ContextFlags.Default,
                new APIVersion(3, 3)),
            Samples = 8,
            VSync = true
        };
        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.FramebufferResize += OnResize;
        _window.Closing += OnClose;
        _window.Run();
        _window.Dispose();
    }

    private static void OnLoad()
    {
        _gl = GL.GetApi(_window);
        _input = _window.CreateInput();
        _ui = new Gui(_gl, _window, _input, Sphere, Font);
        OnResize(_window.Size);
    }

    private static void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        Sphere.Draw();
        _ui.RenderUi(deltaTime);
    }

    public static void DrawLines(Triangle[] triangles, Vector3[,] points)
    {
        DrawAxes(Axes.X | Axes.Y);
        
        _gl.Begin(GLEnum.Lines);
        _gl.Color3(0.6f, 0.6f, 0.6f);
        
        foreach (Triangle tri in triangles)
        {
            Vector3 p1 = tri.GetP1(points);
            Vector3 p2 = tri.GetP2(points);
            Vector3 p3 = tri.GetP3(points);
            
            DrawLine(p1, p2);
            DrawLine(p2, p3);
            DrawLine(p3, p1);
        }
        
        _gl.End();
        
        DrawAxes(Axes.Z);
    }

    public static void DrawPolygons(Triangle[] triangles, Vector3[,] points, bool TwoStep = true)
    {
        _gl.Begin(GLEnum.Triangles);

        foreach (Triangle tri in triangles)
        {
            if (TwoStep && tri.Front) continue;
            
            Vector3 p1 = tri.GetP1(points);
            Vector3 p2 = tri.GetP2(points);
            Vector3 p3 = tri.GetP3(points);
            
            Color color = tri.Color;
            _gl.Color3(color.R, color.G, color.B);
            
            DrawTriangle(p1, p2, p3);
        }
        _gl.End();
        
        DrawAxes(Axes.All);

        if (!TwoStep) return;
        
        _gl.Begin(GLEnum.Triangles);

        foreach (Triangle tri in triangles)
        {
            if (!tri.Front) continue;
            
            Vector3 p1 = tri.GetP1(points);
            Vector3 p2 = tri.GetP2(points);
            Vector3 p3 = tri.GetP3(points);
            
            Color color = tri.Color;
            _gl.Color3(color.R, color.G, color.B);
            
            DrawTriangle(p1, p2, p3);
        }
        
        _gl.End();
    }

    private static void DrawTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var s = _windowMinSize;
        _gl.Vertex2(p1.X / s, p1.Y / s);
        _gl.Vertex2(p2.X / s, p2.Y / s);
        _gl.Vertex2(p3.X / s, p3.Y / s);
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

    private static void DrawAxes(Axes axes)
    {
        _gl.LineWidth(3f);
        _gl.Begin(GLEnum.Lines);

        if (axes.HasFlag(Axes.X))
        {
            _gl.Color3(0.8f, 0f, 0f);
            _gl.Vertex2(0, 0);
            _gl.Vertex2(0.75f, 0);
            
            float c = 0.75f * _windowMinSize + Font;
            float s = _windowMinSize;
            _gl.Vertex2((c - Font * 0.5f) / s, -Font * 1.5f * 0.5f / s);
            _gl.Vertex2((c + Font * 0.5f) / s,  Font * 1.5f * 0.5f / s);
            _gl.Vertex2((c - Font * 0.5f) / s,  Font * 1.5f * 0.5f / s);
            _gl.Vertex2((c + Font * 0.5f) / s, -Font * 1.5f * 0.5f / s);
        }

        if (axes.HasFlag(Axes.Y))
        {
            _gl.Color3(0f, 0.8f, 0f);
            _gl.Vertex2(0, 0);
            _gl.Vertex2(0, 0.75f);
            
            float c = 0.75f * _windowMinSize + Font * 1.5f;
            float s = _windowMinSize;
            _gl.Vertex2((0 - Font * 0.5f) / s, (c + Font * 1.5f * 0.5f) / s);
            _gl.Vertex2(0, c / _windowMinSize);
            _gl.Vertex2((0 + Font * 0.5f) / s, (c + Font * 1.5f * 0.5f) / s);
            _gl.Vertex2(0, c / s);
            _gl.Vertex2(0, c / s);
            _gl.Vertex2(0, (c - Font * 1.5f * 0.5f) / s);
        }
        
        _gl.End();

        if (axes.HasFlag(Axes.Z))
        {
            _gl.PointSize(5f);
            _gl.Begin(GLEnum.Points);

            _gl.Color3(0f, 0.2f, 0.8f);
            _gl.Vertex3(0, 0, 0);

            _gl.End();
            _gl.PointSize(1f);
            
            _gl.Begin(GLEnum.Lines);
            const float c = -Font * 1.5f;
            float s = _windowMinSize;
            _gl.Vertex2((c - Font * 0.5f) / s,  Font * 1.5d * 0.5f / s);
            _gl.Vertex2((c + Font * 0.5f) / s,  Font * 1.5d * 0.5f / s);
            _gl.Vertex2((c + Font * 0.5f) / s,  Font * 1.5d * 0.5f / s);
            _gl.Vertex2((c - Font * 0.5f) / s, -Font * 1.5d * 0.5f / s);
            _gl.Vertex2((c - Font * 0.5f) / s, -Font * 1.5d * 0.5f / s);
            _gl.Vertex2((c + Font * 0.5f) / s, -Font * 1.5d * 0.5f / s);
            _gl.End();
        }
        
        _gl.LineWidth(1.0f);
    }

    private static void OnResize(Vector2D<int> size)
    {
        _gl.Viewport(size);
        _gl.MatrixMode(GLEnum.Projection);
        _gl.LoadIdentity();
        SetAspectRatio(size.X, size.Y);
        _gl.MatrixMode(GLEnum.Modelview);
    }

    private static void SetAspectRatio(int w, int h)
    {
        float a = (float)w / h;
        _windowMinSize = Math.Min(w, h);

        const float s = 1.0f;
        if (a > s)
            _gl.Ortho(-s * a, s * a, -s, s, -10, 10);
        else
            _gl.Ortho(-s, s, -s / a, s / a, -10, 10);
    }

    private static void OnClose()
    {
        _ui.Dispose();
        _input.Dispose();
        _gl.Dispose();
    }
}
