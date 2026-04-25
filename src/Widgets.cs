using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;

namespace CSharpSphere;

public partial class Gui
{
    private static void Group(string name, Action content)
    {
        ImGui.BeginChild(
            name,
            new Vector2(0, 0),
            ImGuiChildFlags.Border | ImGuiChildFlags.AutoResizeY);
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X * 1.0f);
        content();
        ImGui.EndChild();
    }

    private void ColorEdit(string name, ref System.Numerics.Vector3 rgb, ref DragAngleState state, Action update)
    {
        ImGui.ColorEdit3($"##{name}", ref rgb);
        SetHoverCursor(ref state.Hover);
        if (ImGui.IsItemActive()) update.Invoke();
    }

    private void DragAngle(string name, ref DragAngleState state, Surface surface, Func<int, Matrix4> transform, Action update)
    {
        const int limit = int.MaxValue;
        
        ImGui.DragInt($"##{name}", ref state.Delta, 1, -limit, limit, "%d", Flag);

        SetHoverCursor(ref state.Hover);

        if (ImGui.IsItemActivated()) 
            state.Initial = surface.TransformationMat;
        if (ImGui.IsItemActive())
        {
            surface.TransformationMat = state.Initial * transform(state.Delta);
            update.Invoke();
        }
        if (!ImGui.IsItemDeactivated()) 
            return;
        
        state.Initial = surface.TransformationMat;
        state.Delta = 0;
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

    private static void SliderI(string label, ref int v, int vMin, int vMax, Action update)
    {
        Label($"{vMin}", label, $"{vMax}");
        bool active = ImGui.SliderInt($"##{label}", ref v, vMin, vMax, "%d", Flag);
        if (active) update.Invoke();
        AddDoubleClickToEditEvent();
    }

    private static void Label(string left, string center, string right)
    {
        float maxWidth = ImGui.GetContentRegionAvail().X;
        float padding = ImGui.GetStyle().WindowPadding.X;
        ImGui.Text(left);
        ImGui.SameLine((maxWidth - ImGui.CalcTextSize(center).X) * 0.5f + padding); 
        ImGui.Text(center);
        ImGui.SameLine(maxWidth - ImGui.CalcTextSize(right).X + padding); 
        ImGui.Text(right);
    }

    private static void AddDoubleClickToEditEvent()
    {
        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)) 
            ImGui.SetKeyboardFocusHere(-1);
    }
    
    private struct DragAngleState
    {
        public Matrix4 Initial;
        public int Delta;
        public bool Hover;
    }
}