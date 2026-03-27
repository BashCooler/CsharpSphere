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
                new APIVersion(3, 3))
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
        _ui = new Gui(_gl, _window, _input, Sphere, 18);
        OnResize(_window.Size);
    }

    /// <summary>
    ///     Очищает область отрисовки, заливая её сплошным цветом. 
    ///     Очищает буфер цвета. Отрисовывает сферу и интерфейс пользователя.
    /// </summary>
    /// <seealso href="https://registry.khronos.org/OpenGL-Refpages/gl4/html/glClear.xhtml">
    ///     glClear и ClearBufferMask
    /// </seealso>
    private static void OnRender(double deltaTime)
    {
        _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _gl.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
        DrawAxesXy();
        Sphere.DrawWireframeSphere();
        DrawAxesZ();
        _ui.RenderUi(deltaTime);
    }

    public static void DrawLines(Triangle[] triangles, Vector4[,] points)
    {
        _gl.Begin(GLEnum.Lines);
        _gl.LineWidth(2f);
        _gl.Color3(0.5f, 0.5f, 0.5f);
        
        foreach (var tri in triangles)
        {
            var p1 = points[tri.Point1.I, tri.Point1.J];
            var p2 = points[tri.Point2.I, tri.Point2.J];
            var p3 = points[tri.Point3.I, tri.Point3.J];
            
            DrawLine(p1, p2);
            DrawLine(p2, p3);
            DrawLine(p3, p1);
        }
        
        _gl.End();
    }

    private static void DrawLine(Vector4 p1, Vector4 p2)
    {
        _gl.Vertex2(p1.X / _windowMinSize, p1.Y / _windowMinSize);
        _gl.Vertex2(p2.X / _windowMinSize, p2.Y / _windowMinSize);
    }

    private static void DrawAxesXy()
    {
        _gl.LineWidth(3f);
        _gl.Begin(GLEnum.Lines);
        
        // X
        _gl.Color3(0.8f, 0f, 0f);
        _gl.Vertex2(0, 0);
        _gl.Vertex2(0.75f, 0);
        
        // Y
        _gl.Color3(0f, 0.8f, 0f);
        _gl.Vertex2(0, 0);
        _gl.Vertex2(0, 0.75f);
        
        _gl.End();
        _gl.LineWidth(1.0f);
    }

    private static void DrawAxesZ()
    {
        _gl.PointSize(5f);
        _gl.Begin(GLEnum.Points);
        
        // Z
        _gl.Color3(0f, 0.2f, 0.8f);
        _gl.Vertex3(0, 0, 0);
        
        _gl.End();
        _gl.PointSize(1f);
    }

    /// <summary>
    ///     Задает размер Viewport равный новому размеру окна. <br />
    ///     Переходит в режим изменения стека матриц проекций <see cref="GLEnum.Projection" />.
    ///     Загружает единичную матрицу, к которой будут применены изменения.
    ///     Задает новое соотношение сторон методом <see cref="SetAspectRatio" />.
    ///     Возвращается в основной режим <see cref="GLEnum.Modelview" />.
    /// </summary>
    /// <seealso href="https://learn.microsoft.com/ru-ru/windows/win32/opengl/glmatrixmode">
    ///     Функция glMatrixMode
    /// </seealso>
    private static void OnResize(Vector2D<int> size)
    {
        _gl.Viewport(size);
        _gl.MatrixMode(GLEnum.Projection);
        _gl.LoadIdentity();
        SetAspectRatio(size.X, size.Y);
        _gl.MatrixMode(GLEnum.Modelview);
    }

    /// <summary>
    ///     Задает видимую область так, чтобы сфера всегда была круглой. Если ширина окна 
    ///     больше высоты, увеличивает отступы по бокам. В ином случае увеличивает отступы 
    ///     сверху и снизу. Область отступов отсекается во время рендера.
    /// </summary>
    /// <param name="w">ширина</param>
    /// <param name="h">высота</param>
    /// <seealso href="https://registry.khronos.org/OpenGL-Refpages/gl2.1/xhtml/glOrtho.xml">
    ///     Метод glOrtho
    /// </seealso>
    private static void SetAspectRatio(int w, int h)
    {
        float a = (float)w / h;
        _windowMinSize = Math.Min(w, h);
        
        if (a > 1.0f)
        {
            _gl.Ortho(-1.0f * a, 1.0f * a, -1.0f, 1.0f, -10, 10);
        }
        else
        {
            _gl.Ortho(-1.0f, 1.0f, -1.0f / a, 1.0f / a, -10, 10);
        }
    }

    private static void OnClose()
    {
        _ui.Dispose();
        _input.Dispose();
        _gl.Dispose();
    }
}