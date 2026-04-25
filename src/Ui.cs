using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.OpenGL.Legacy;
using Silk.NET.OpenGL.Legacy.Extensions.ImGui;
using static CSharpSphere.Program;
using static ImGuiNET.ImGui;

namespace CSharpSphere;

public partial class Gui
{
    private const ImGuiSliderFlags Flag = ImGuiSliderFlags.AlwaysClamp;
    
    private readonly Sphere _sphere;
    private readonly Torus _torus;
    private readonly IInputContext _input;
    private readonly ImGuiController _controller;
    
    private DragAngleState _stateX;
    private DragAngleState _stateY;
    private DragAngleState _stateZ;
    private DragAngleState _stateOuterColor;
    private DragAngleState _stateInnerColor;

    public Gui(GL gl, IWindow window, IInputContext input, Sphere sphere, Torus torus,
        int fontSize, float fontScale = 1.0f, float scale = 1.0f)
    {
        _input = input;
        _sphere = sphere;
        _torus = torus;

        var fontConfig = new ImGuiFontConfig(
            Path.Combine(AppContext.BaseDirectory, "fonts", "Better VCR 6.1.ttf"),
            fontSize,
            io => io.Fonts.GetGlyphRangesCyrillic());
        
        _controller = new ImGuiController(gl, window, input, fontConfig);
        
        GetIO().FontGlobalScale = fontScale;
        GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        GetStyle().ScaleAllSizes(scale);
        StyleColorsClassic();
        PushStyleVar(ImGuiStyleVar.WindowRounding, 4);
        PushStyleVar(ImGuiStyleVar.FrameRounding, 4);
        PushStyleVar(ImGuiStyleVar.GrabRounding, 4);
    }

    public void RenderUi(double deltaTime, ref SurfaceType selectedSurfaceType)
    {
        _controller.Update((float)deltaTime);

        DockSpaceOverViewport(
            0,
            GetMainViewport(),
            ImGuiDockNodeFlags.PassthruCentralNode);
        
        Begin("Параметры сферы");

        SurfaceType surf = selectedSurfaceType;
        
        Group("0", () =>
        {
            Label("", "Поверхность", "");
            if (RadioButton("Сфера", surf == SurfaceType.Sphere)) 
                surf = SurfaceType.Sphere;
            if (RadioButton("Тор", surf == SurfaceType.Torus)) 
                surf = SurfaceType.Torus;
        });
        
        selectedSurfaceType = surf;
        Surface surface = surf switch
        {
            SurfaceType.Sphere => _sphere,
            SurfaceType.Torus  => _torus,
            _ => _sphere
        };
        
        Group("1", () =>
        {
            switch (surface)
            {
                default:
                    SliderI("R, пиксель", ref _sphere.R, 0, 3000, _sphere.Update);
                    break;
                case Torus:
                    SliderI("R, пиксель", ref _torus.R, 0, 3000, _torus.Update);
                    SliderI("r, пиксель", ref _torus.r, 0, 3000, _torus.Update);
                    break;
            }
        });

        Group("2", () =>
        {
            SliderI("Max U", ref surface.UMax, 0, 360, surface.Update);
            var vMax = surface is Sphere ? 180 : 360;
            SliderI("Мax V", ref surface.VMax, 0, vMax, surface.Update);
        });

        Group("3", () =>
        {
            SliderI("Div U", ref surface.UDiv, 0, 200, surface.Update);
            SliderI("Div V", ref surface.VDiv, 0, 200, surface.Update);
        });

        Group("4", () =>
        {
            Label("", "Поворот по X, Y, Z", "");
            DragAngle("AngleX", ref _stateX, surface, Matrix4.GetRotateX, surface.Update);
            DragAngle("AngleY", ref _stateY, surface, Matrix4.GetRotateY, surface.Update);
            DragAngle("AngleZ", ref _stateZ, surface, Matrix4.GetRotateZ, surface.Update);
        });

        Group("5", () =>
        {
            Label("", "Отрисовка", "");
            Group("Surface", () =>
            {
                if (RadioButton("Wireframe", !surface.Shading))
                {
                    surface.Shading = false;
                    surface.Update();
                }

                if (RadioButton("Flat закраска", surface.Shading))
                {
                    surface.Shading = true;
                    surface.Update();
                }
            });

            if (!surface.Shading) BeginDisabled();
            Group("Render mode", () =>
            {
                Label("", "Алгоритм", "");
                if (RadioButton("1 цикл", surface.RenderMode == Render.Single)) 
                    surface.RenderMode = Render.Single;
                if (RadioButton("2 цикла", surface.RenderMode == Render.Double)) 
                    surface.RenderMode = Render.Double;
                if (RadioButton("Z-буфер (OpenGL)", surface.RenderMode == Render.DepthTest)) 
                    surface.RenderMode = Render.DepthTest;
            });
            
            
            Group("6", () =>
            {
                Label("", "Внешний цвет", "");
                ColorEdit("OuterColor", ref surface.OuterColor, ref _stateOuterColor, surface.Update);
            });
            
            Group("7", () =>
            {
                Label("", "Внутренний цвет", "");
                ColorEdit("InnerColor", ref surface.InnerColor, ref _stateInnerColor, surface.Update);
            });
            
            if (!surface.Shading) EndDisabled();
        });
        
        End();
        _controller.Render();
    }

    public void Dispose() => _controller.Dispose();
}

