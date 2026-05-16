#pragma warning disable CS0618

using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.OpenGL.Legacy;

namespace CSharpSphere;

public static partial class Program
{
    private static GL _gl = null!;
    private static IWindow _window = null!;
    private static IInputContext _input = null!;
    private static Gui _ui = null!;

    private static SurfaceType _selectedSurfaceType = SurfaceType.Sphere;
    
    private static readonly Sphere Sphere = new();
    private static readonly Torus Torus = new();

    private const int Font = 18;
    private static int _windowMinSize;
    
    public static void Main()
    {
        var options = WindowOptions.Default with
        {
            Title = "Построение поверхности",
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
        _ui = new Gui(_gl, _window, _input, Sphere, Torus, Font);
        OnResize(_window.Size);
    }

    public enum SurfaceType
    {
        Sphere,
        Torus
    }

    private static void OnRender(double deltaTime)
    {
        _gl.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        switch (_selectedSurfaceType)
        {
            default:
            case SurfaceType.Sphere:
                Sphere.Draw();
                break;
            case SurfaceType.Torus:
                Torus.Draw();
                break;
        }
        _ui.RenderUi(deltaTime, ref _selectedSurfaceType);
    }

    private static void OnResize(Vector2D<int> size)
    {
        _gl.Viewport(size);
        _gl.MatrixMode(GLEnum.Projection);
        _gl.LoadIdentity();
        SetAspectRatio(size.X, size.Y);
        _gl.MatrixMode(GLEnum.Modelview);
        _gl.LoadIdentity();
        _gl.Translate(0.3f, 0.0f, 0.0f);
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
