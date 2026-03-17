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
        ConfigureUi(12);
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
        _gl.ClearColor(0.1f, 0.1f, 0.1f, 1.0f);
        DrawAxes();
        Sphere.DrawWireframeSphere(_gl);
        RenderUi(deltaTime);
    }

    private static void DrawAxes()
    {
        _gl.LineWidth(3f);
        _gl.Begin(GLEnum.Lines);
        
        const int l = 1;
        
        // X
        _gl.Color3(0.8f, 0f, 0f);
        _gl.Vertex2(0, 0);
        _gl.Vertex2(l, 0);
        
        // Y
        _gl.Color3(0f, 0.8f, 0f);
        _gl.Vertex2(0, 0);
        _gl.Vertex2(0, l);
        
        _gl.End();
        _gl.LineWidth(1.0f);
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
    /// <param name="margin">отступ, определяет расстояние от края окна до сферы</param>
    /// <seealso href="https://registry.khronos.org/OpenGL-Refpages/gl2.1/xhtml/glOrtho.xml">
    ///     Метод glOrtho
    /// </seealso>
    private static void SetAspectRatio(int w, int h, float margin = 1.5f)
    {
        float a = (float)w / h;
        
        if (a > 1.0f)
        {
            _gl.Ortho(-margin * a, margin * a, -margin, margin, -10, 10);
        }
        else
        {
            _gl.Ortho(-margin, margin, -margin / a, margin / a, -10, 10);
        }
    }

    private static void OnClose()
    {
        _controller.Dispose();
        _input.Dispose();
        _gl.Dispose();
    }
}