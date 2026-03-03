using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.OpenGL.Legacy.Extensions.ImGui;

namespace CSharpSphere;

public static partial class Program
{
    private static void ConfigureUi(int fontSize, float fontScale = 1.0f, float scale = 1.0f)
    {
        var fontConfig = new ImGuiFontConfig(
            Path.Combine(AppContext.BaseDirectory, "fonts", "Better VCR 6.1.ttf"),
            fontSize,
            io => io.Fonts.GetGlyphRangesCyrillic());

        _controller = new ImGuiController(_gl, _window, _input, fontConfig);
        
        ImGui.GetIO().FontGlobalScale = fontScale;
        ImGui.GetStyle().ScaleAllSizes(scale);
        ImGui.StyleColorsClassic();
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 4);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 4);
    }
    
    private static void RenderUi(double deltaTime)
    {
        _controller.Update((float)deltaTime);
        // ExpectCtrlClick();
        
        ImGui.Begin("Параметры сферы");
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X * 1.0f);
        
        ImGui.Text("Радиус");
        SliderF("R", ref Sphere.R, 0.0f, 5.0f);
        ImGui.Separator();
        
        ImGui.Text("\nМаксимум U, V");
        SliderF("UMax", ref Sphere.UMax, 0.0f, 2 * MathF.PI);
        SliderF("VMax", ref Sphere.VMax, 0.0f, MathF.PI);
        ImGui.Separator();
        
        ImGui.Text("\nРазбиения U, V");
        SliderI("U", ref Sphere.UDiv, 0, 100);
        SliderI("V", ref Sphere.VDiv, 0, 100);
        ImGui.Separator();
        
        ImGui.Text(Sphere.Message);
        
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
        ImGui.SliderInt($"##{label}", ref v, vMin, vMax, "%d", ImGuiSliderFlags.AlwaysClamp);
        AddDoubleClickToEditEvent();
    }
    
    private static void AddDoubleClickToEditEvent()
    {
        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)) 
            ImGui.SetKeyboardFocusHere(-1);
    }

    /// <summary>
    /// Вызвать после <c>ImGuiController.Update();</c> для включения оригинального
    /// поведения Ctrl+ЛКМ для ввода
    /// </summary>
    [Obsolete]
    private static void ExpectCtrlClick()
    {
        var io = ImGui.GetIO();
        var keyboard = _input.Keyboards[0];
        io.AddKeyEvent(
            ImGuiKey.ModCtrl, 
            keyboard.IsKeyPressed(Key.ControlLeft) || keyboard.IsKeyPressed(Key.ControlRight));
    }
}