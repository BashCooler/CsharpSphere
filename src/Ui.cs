using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.OpenGL.Legacy;
using Silk.NET.OpenGL.Legacy.Extensions.ImGui;

namespace CSharpSphere;

public class Gui
{
    private const ImGuiSliderFlags Flag = ImGuiSliderFlags.AlwaysClamp;
    
    private readonly Sphere _sphere;
    private readonly IInputContext _input;
    private readonly ImGuiController _controller;

    public Gui(GL gl, IWindow window, IInputContext input, Sphere sphere,
        int fontSize, float fontScale = 1.0f, float scale = 1.0f)
    {
        _input = input;
        _sphere = sphere;
        
        var fontConfig = new ImGuiFontConfig(
            Path.Combine(AppContext.BaseDirectory, "fonts", "Better VCR 6.1.ttf"),
            fontSize,
            io => io.Fonts.GetGlyphRangesCyrillic());
        
        _controller = new ImGuiController(gl, window, input, fontConfig);
        
        ImGui.GetIO().FontGlobalScale = fontScale;
        ImGui.GetStyle().ScaleAllSizes(scale);
        ImGui.StyleColorsClassic();
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 4);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 4);
    }

    private int _initialX;
    private int _initialY;
    private int _initialZ;
    private int _deltaX;
    private int _deltaY;
    private int _deltaZ;
    private bool _hoverX;
    private bool _hoverY;
    private bool _hoverZ;

    public void RenderUi(double deltaTime)
    {
        _controller.Update((float)deltaTime);
        
        ImGui.Begin("Параметры сферы");
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X * 1.0f);
        
        // TODO сделать понятные единицы измерения не 1, а например в пикселях
        ImGui.Text("Радиус");
        SliderF("R", ref _sphere.R, 0.0f, 5.0f);
        ImGui.Separator();
        
        ImGui.Text("\nМаксимум U, V");
        SliderF("UMax", ref _sphere.UMax, 0.0f, 2 * MathF.PI);
        SliderF("VMax", ref _sphere.VMax, 0.0f, MathF.PI);
        ImGui.Separator();
        
        ImGui.Text("\nРазбиения U, V");
        SliderI("U", ref _sphere.UDiv, 0, 100);
        SliderI("V", ref _sphere.VDiv, 0, 100);
        ImGui.Separator();
        
        ImGui.Text("\nПоворот по X, Y, Z");
        DragAngle("AngleX", ref _deltaX, ref _sphere.AngleX, ref _initialX, ref _hoverX);
        DragAngle("AngleY", ref _deltaY, ref _sphere.AngleY, ref _initialY, ref _hoverY);
        DragAngle("AngleZ", ref _deltaZ, ref _sphere.AngleZ, ref _initialZ, ref _hoverZ);
        ImGui.Separator();
        
        ImGui.Text(_sphere.Message);
        
        ImGui.End();
        _controller.Render();
    }

    private void DragAngle(string name, ref int delta, ref int angle, ref int initial, ref bool hover)
    {
        int limit = Int32.MaxValue;
        
        ImGui.DragInt($"##{name}", ref delta, 1, -limit, limit, "%d", Flag);

        SetHoverCursor(ref hover);

        if (ImGui.IsItemActivated()) initial = angle;
        if (ImGui.IsItemActive()) angle = initial + delta;
        if (!ImGui.IsItemDeactivated()) return;
        
        angle = (initial + delta) % 360;
        delta = 0;
    }

    private void SetHoverCursor(ref bool hover)
    {
        if (ImGui.IsItemHovered())
        {
            hover = true;
            _input.Mice[0].Cursor.StandardCursor = StandardCursor.HResize;
        }
        if (hover && !ImGui.IsItemHovered())
        {
            _input.Mice[0].Cursor.StandardCursor = StandardCursor.Arrow;
            hover = false;
        }
        hover = ImGui.IsItemHovered();
    }

    private static void SliderF(string label, ref float v, float vMin, float vMax)
    {
        ImGui.SliderFloat($"##{label}", ref v, vMin, vMax, "%.3f", Flag);
        AddDoubleClickToEditEvent();
    }

    private static void SliderI(string label, ref int v, int vMin, int vMax)
    {
        ImGui.SliderInt($"##{label}", ref v, vMin, vMax, "%d", Flag);
        AddDoubleClickToEditEvent();
    }
    
    private static void AddDoubleClickToEditEvent()
    {
        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)) 
            ImGui.SetKeyboardFocusHere(-1);
    }

    public void Dispose() => _controller.Dispose();
}