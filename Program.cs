#pragma warning disable CS0618 // Type or member is obsolete

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL.Legacy;
using Silk.NET.OpenGL.Legacy.Extensions.ImGui;
using Silk.NET.Windowing;

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
        _window.FramebufferResize += OnResize;
        _window.Closing += OnClose;
        _window.Run();
        _window.Dispose();
    }

    private static void OnLoad()
    {
        _gl = GL.GetApi(_window);
        _input = _window.CreateInput();
        _controller = new ImGuiController(_gl, _window, _input);
        OnResize(_window.Size);
    }

    private static void OnUpdate(double deltaTime) { }

    private static void OnRender(double deltaTime)
    {
        _gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        Sphere.DrawWireframeSphere(_gl);
        RenderUi(deltaTime);
    }

    private static void OnResize(Vector2D<int> size)
    {
        _gl.Viewport(size);
        _gl.MatrixMode(GLEnum.Projection);
        _gl.LoadIdentity();
        SetAspectRatio(size.X, size.Y);
        _gl.MatrixMode(GLEnum.Modelview);
    }

    private static void SetAspectRatio(int x, int y)
    {
        float a = (float)x / y;
        if (a > 1.0f)
        {
            _gl.Ortho(-1.5f * a, 1.5f * a, -1.5f, 1.5f, -10, 10);
        }
        else
        {
            _gl.Ortho(-1.5f, 1.5f, -1.5f / a, 1.5f / a, -10, 10);
        }
    }

    private static void OnClose()
    {
        _controller.Dispose();
        _input.Dispose();
        _gl.Dispose();
    }
}