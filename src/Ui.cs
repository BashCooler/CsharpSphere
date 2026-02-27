using ImGuiNET;
using Silk.NET.OpenGL.Legacy.Extensions.ImGui;

namespace CSharpSphere;

public static partial class Program
{
    private static void RenderUi(double deltaTime)
    {
        _controller.Update((float)deltaTime);
        
        ImGui.Begin("Параметры сферы");
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X * 1.0f);
        
        ImGui.Text("Радиус");
        ImGui.SliderFloat("##R", ref Sphere.R, 0.0f, 5.0f);
        ImGui.Separator();
        
        ImGui.Text("\nМаксимум U, V");
        ImGui.SliderFloat("##UMax", ref Sphere.UMax, 0.0f, 2 * MathF.PI);
        ImGui.SliderFloat("##VMax", ref Sphere.VMax, 0.0f, MathF.PI);
        ImGui.Separator();
        
        ImGui.Text("\nРазбиения U, V");
        ImGui.SliderInt("##U", ref Sphere.UDiv, 0, 100);
        ImGui.SliderInt("##V", ref Sphere.VDiv, 0, 100);
        ImGui.Separator();
        
        ImGui.Text(Sphere.Message);
        
        ImGui.End();
        _controller.Render();
    }
    
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
}