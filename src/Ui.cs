using ImGuiNET;

namespace CSharpSphere;

public static partial class Program
{
    private static void RenderUi(double deltaTime)
    {
        _controller.Update((float)deltaTime);
        
        ImGui.Begin("Sphere Parameters");
        
        ImGui.Text("Radius");
        ImGui.SliderFloat("", ref Sphere.Radius, 0.1f, 5.0f);
        
        ImGui.Text("Sector");
        ImGui.SliderFloat("U max", ref Sphere.UMax, 0.1f, 2 * MathF.PI);
        ImGui.SliderFloat("V max", ref Sphere.VMax, 0.1f, MathF.PI);
        
        ImGui.Text("Divisions");
        ImGui.SliderInt("U", ref Sphere.UDiv, 0, 100);
        ImGui.SliderInt("V", ref Sphere.VDiv, 0, 100);
        
        ImGui.Text(Sphere.Messenger.Message);
        
        ImGui.End();
        
        _controller.Render();
    }
}