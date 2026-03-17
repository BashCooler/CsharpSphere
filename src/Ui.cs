using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.OpenGL.Legacy;
using Silk.NET.OpenGL.Legacy.Extensions.ImGui;

namespace CSharpSphere;

public class Gui
{
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
    private int _deltaX;
    

    public void RenderUi(double deltaTime)
    {
        _controller.Update((float)deltaTime);
        
        // TODO ползунки должны сбрасываться
        // то есть после каждого преобразования
        // мы как будто нажимаем Apply All Transforms в Blender
        // Но это для поворотов именно
        
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
        ImGui.DragInt("##AngleX", ref _deltaX, 1, -180, 180, "%d", ImGuiSliderFlags.AlwaysClamp);
        if (ImGui.IsItemHovered())
        {
            _input.Mice[0].Cursor.StandardCursor = StandardCursor.HResize;
        }
        else
        {
            _input.Mice[0].Cursor.StandardCursor = StandardCursor.Arrow;
        }
        if (ImGui.IsItemActivated())
        {
            _initialX = _sphere.AngleX;
        }
        if (ImGui.IsItemActive())
        {
            _sphere.AngleX = _initialX + _deltaX;
        }
        if (ImGui.IsItemDeactivated())
        {
            _sphere.AngleX = (_initialX + _deltaX) % 360;
            _deltaX = 0;
        }
        
        // SliderI("AngleY", ref _angleY, -180, 180, () => _sphere.rotate(0, _angleY, 0));
        // SliderI("AngleZ", ref _angleZ, -180, 180, () => _sphere.rotate(0, 0, _angleZ));
        ImGui.Separator();
        
        ImGui.Text(_sphere.Message);
        
        ImGui.End();
        _controller.Render();
    }

    private static void SliderF(string label, ref float v, float vMin, float vMax)
    {
        ImGui.SliderFloat($"##{label}", ref v, vMin, vMax, "%.3f", ImGuiSliderFlags.AlwaysClamp);
        AddDoubleClickToEditEvent();
    }

    private static void SliderI(string label, ref int v, int vMin, int vMax)
    {
        var move = ImGui.SliderInt($"##{label}", ref v, vMin, vMax, "%d", ImGuiSliderFlags.AlwaysClamp);
        AddDoubleClickToEditEvent();
    }
    
    private static void AddDoubleClickToEditEvent()
    {
        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)) 
            ImGui.SetKeyboardFocusHere(-1);
    }

    public void Dispose() => _controller.Dispose();
}