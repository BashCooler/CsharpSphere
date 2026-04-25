using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.OpenGL.Legacy;
using Silk.NET.OpenGL.Legacy.Extensions.ImGui;

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

    public void RenderUi(double deltaTime, ref Program.Surface selectedSurface)
    {
        _controller.Update((float)deltaTime);

        DockSpaceOverViewport(
            0,
            GetMainViewport(),
            ImGuiDockNodeFlags.PassthruCentralNode);
        
        Begin("Параметры сферы");

        Program.Surface surf = selectedSurface;
        
        Group("0", () =>
        {
            Label("", "Поверхность", "");
            if (RadioButton("Сфера", surf == Program.Surface.Sphere)) 
                surf = Program.Surface.Sphere;
            if (RadioButton("Тор", surf == Program.Surface.Torus)) 
                surf = Program.Surface.Torus;
        });
        
        selectedSurface = surf;
        Surface surface = surf switch
        {
            Program.Surface.Sphere => _sphere,
            Program.Surface.Torus  => _torus,
            _ => _sphere
        };
        
        Group("1", () =>
        {
            switch (surface)
            {
                default:
                    SliderI("R, пиксель", ref _sphere.R, 0, 3000, ref _sphere.Update);
                    break;
                case Torus:
                    SliderI("R, пиксель", ref _torus.R, 0, 3000, ref _torus.Update);
                    SliderI("r, пиксель", ref _torus.r, 0, 3000, ref _torus.Update);
                    break;
            }
        });

        Group("2", () =>
        {
            SliderI("Max U", ref surface.UMax, 0, 360, ref surface.Update);
            var vMax = surface is Sphere ? 180 : 360;
            SliderI("Мax V", ref surface.VMax, 0, vMax, ref surface.Update);
        });

        Group("3", () =>
        {
            SliderI("Div U", ref surface.UDiv, 0, 200, ref surface.Update);
            SliderI("Div V", ref surface.VDiv, 0, 200, ref surface.Update);
        });

        Group("4", () =>
        {
            Label("", "Поворот по X, Y, Z", "");
            DragAngle("AngleX", ref _stateX, surface, Matrix4.GetRotateX, ref surface.Update);
            DragAngle("AngleY", ref _stateY, surface, Matrix4.GetRotateY, ref surface.Update);
            DragAngle("AngleZ", ref _stateZ, surface, Matrix4.GetRotateZ, ref surface.Update);
        });

        Group("5", () =>
        {
            Label("", "Отрисовка", "");
            bool shadingChanged = Checkbox("Flat закраска", ref surface.Shading);
            if (shadingChanged) surface.Update = true;

            if (!surface.Shading) BeginDisabled();
            Checkbox("Отрисовка в 2 этапа", ref surface.TwoStep);
            
            Group("6", () =>
            {
                Label("", "Внешний цвет", "");
                ColorEdit("OuterColor", ref surface.OuterColor.Rgb, ref _stateOuterColor, ref surface.Update);
            });
            
            Group("7", () =>
            {
                Label("", "Внутренний цвет", "");
                ColorEdit("InnerColor", ref surface.InnerColor.Rgb, ref _stateInnerColor, ref surface.Update);
            });
            
            if (!surface.Shading) EndDisabled();
        });
        
        End();
        _controller.Render();
    }

    public void Dispose() => _controller.Dispose();
}

