using System.Diagnostics;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL.Legacy;
using Silk.NET.OpenGL.Legacy.Extensions.ImGui;
using Silk.NET.Windowing;


#pragma warning disable CS0618 // Type or member is obsolete

namespace CSharpSphere;

public static partial class Program
{
    private static GL _gl = null!;
    private static IWindow _window = null!;
    private static IInputContext _input = null!;
    private static ImGuiController _controller = null!;
    
    private static readonly Sphere Sphere = new();
    
    public static void Main()
    {
        var options = WindowOptions.Default with
        {
            Title = "Silk.NET Sphere",
            Size = new Vector2D<int>(800, 600),
            API = new GraphicsAPI(
                ContextAPI.OpenGL,
                ContextProfile.Compatability,
                ContextFlags.Default,
                new APIVersion(3, 3))
        };
        _window = Window.Create(options);
        _window.Load += OnLoad;
        _window.Update += OnUpdate;
        _window.Render += OnRender;
        _window.FramebufferResize += s => _gl.Viewport(s);
        _window.Closing += OnClose;
        _window.Run();
        _window.Dispose();
    }

    private static void OnLoad()
    {
        _gl = GL.GetApi(_window);
        _input = _window.CreateInput();
        _controller = new ImGuiController(_gl, _window, _input);
    }

    private static void OnUpdate(double deltaTime) { }

    private static void OnRender(double deltaTime)
    {
        _gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        DrawWireframeSphere();
        RenderUI(deltaTime);
    }

    private static void OnClose()
    {
        _controller.Dispose();
        _input.Dispose();
        _gl.Dispose();
    }
    
    private static void DrawWireframeSphere()
    {
        var watch = Stopwatch.StartNew();
        
        _gl.MatrixMode(GLEnum.Projection);
        _gl.LoadIdentity();
        _gl.Ortho(-1.5, 1.5, -1.5, 1.5, -10, 10);

        _gl.MatrixMode(GLEnum.Modelview);
        _gl.LoadIdentity();

        _gl.Begin(GLEnum.Lines);
        _gl.Color3(0.0f, 0.8f, 0.0f);

        var points = Sphere.GeneratePoints();
        var triangles = Sphere.GenerateTriangles(points);

        float scale = 0.85f;

        foreach (var tri in triangles)
        {
            var p1 = points[tri.Point1.Row, tri.Point1.Col];
            var p2 = points[tri.Point2.Row, tri.Point2.Col];
            var p3 = points[tri.Point3.Row, tri.Point3.Col];
            
            _gl.Vertex2(p1.X * scale, p1.Y * scale);
            _gl.Vertex2(p2.X * scale, p2.Y * scale);

            _gl.Vertex2(p2.X * scale, p2.Y * scale);
            _gl.Vertex2(p3.X * scale, p3.Y * scale);

            _gl.Vertex2(p3.X * scale, p3.Y * scale);
            _gl.Vertex2(p1.X * scale, p1.Y * scale);
        }

        _gl.End();
        watch.Stop();
        Console.WriteLine($"Drawn sphere, took {watch.ElapsedMilliseconds} ms");
    }
}