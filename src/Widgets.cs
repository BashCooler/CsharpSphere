using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using static ImGuiNET.ImGui;

namespace CSharpSphere;

public partial class Gui
{
    private static void Group(string name, Action content)
    {
        BeginChild(
            name,
            new Vector2(0, 0),
            ImGuiChildFlags.Border | ImGuiChildFlags.AutoResizeY);
        PushItemWidth(GetContentRegionAvail().X * 1.0f);
        content();
        EndChild();
    }

    private void ColorEdit(string name, ref System.Numerics.Vector3 rgb, ref DragAngleState state, Action update)
    {
        ColorEdit3($"##{name}", ref rgb);
        SetHoverCursor(ref state.Hover);
        if (IsItemActive()) update.Invoke();
    }

    private void DragAngle(string name, ref DragAngleState state, Surface surface, Func<int, Matrix4> transform, Action update)
    {
        const int limit = int.MaxValue;
        
        DragInt($"##{name}", ref state.Delta, 1, -limit, limit, "%d", Flag);

        SetHoverCursor(ref state.Hover);

        if (IsItemActivated()) 
            state.Initial = surface.TransformationMat;
        if (IsItemActive())
        {
            surface.TransformationMat = state.Initial * transform(state.Delta);
            update.Invoke();
        }
        if (!IsItemDeactivated()) 
            return;
        
        state.Initial = surface.TransformationMat;
        state.Delta = 0;
    }

    private void SetHoverCursor(ref bool hover)
    {
        if (IsItemHovered())
        {
            hover = true;
            _input.Mice[0].Cursor.StandardCursor = StandardCursor.HResize;
        }
        if (hover && !IsItemHovered())
        {
            _input.Mice[0].Cursor.StandardCursor = StandardCursor.Arrow;
            hover = false;
        }
        hover = IsItemHovered();
    }

    private static void SliderI(string label, ref int v, int vMin, int vMax, Action update)
    {
        Label($"{vMin}", label, $"{vMax}");
        bool active = SliderInt($"##{label}", ref v, vMin, vMax, "%d", Flag);
        if (active) update.Invoke();
        AddDoubleClickToEditEvent();
    }

    private static void Label(string left, string center, string right)
    {
        float maxWidth = GetContentRegionAvail().X;
        float padding = GetStyle().WindowPadding.X;
        Text(left);
        SameLine((maxWidth - CalcTextSize(center).X) * 0.5f + padding); 
        Text(center);
        SameLine(maxWidth - CalcTextSize(right).X + padding); 
        Text(right);
    }

    private static void AddDoubleClickToEditEvent()
    {
        if (IsItemHovered() && IsMouseDoubleClicked(ImGuiMouseButton.Left)) 
            SetKeyboardFocusHere(-1);
    }
    
    private struct DragAngleState
    {
        public Matrix4 Initial;
        public int Delta;
        public bool Hover;
    }
}