using System.Numerics;
using ImGuiNET;

namespace CSharpSphere;

public static partial class Program
{
    private static void RenderUi(double deltaTime)
    {
        _controller.Update((float)deltaTime);
        
        ImGui.Begin("Параметры сферы");
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X * 1.0f);
        
        ImGui.Text("Радиус");
        ImGui.SliderFloat("##R", ref Sphere.Radius, 0.0f, 5.0f);
        
        Separator();
        ImGui.Text("Максимум U, V");
        ImGui.SliderFloat("##UMax", ref Sphere.UMax, 0.0f, 2 * MathF.PI);
        ImGui.SliderFloat("##VMax", ref Sphere.VMax, 0.0f, MathF.PI);
        
        Separator();
        ImGui.Text("Разбиения U, V");
        ImGui.SliderInt("##U", ref Sphere.UDiv, 0, 100);
        ImGui.SliderInt("##V", ref Sphere.VDiv, 0, 100);
        
        Separator();
        ImGui.Text(Sphere.Messenger.Message);
        
        ImGui.End();
        _controller.Render();
    }

    private static void Separator()
    {
        ImGui.Dummy(new Vector2(0, 10));
        ImGui.Separator();
        ImGui.Dummy(new Vector2(0, 10));
    }
}