using System.Numerics;
using ImGuiNET;
using Silk.NET.Input;
using Silk.NET.Windowing;
using Silk.NET.OpenGL.Legacy;
using Silk.NET.OpenGL.Legacy.Extensions.ImGui;

using static ImGuiNET.ImGui;

namespace CSharpSphere;

public class Gui
{
    private const ImGuiSliderFlags Flag = ImGuiSliderFlags.AlwaysClamp;
    
    private readonly Sphere _sphere;
    private readonly IInputContext _input;
    private readonly ImGuiController _controller;
    
    private DragAngleState _stateX;
    private DragAngleState _stateY;
    private DragAngleState _stateZ;

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
        
        GetIO().FontGlobalScale = fontScale;
        GetStyle().ScaleAllSizes(scale);
        StyleColorsClassic();
        PushStyleVar(ImGuiStyleVar.WindowRounding, 4);
        PushStyleVar(ImGuiStyleVar.FrameRounding, 4);
        PushStyleVar(ImGuiStyleVar.GrabRounding, 4);
    }

    public void RenderUi(double deltaTime)
    {
        _controller.Update((float)deltaTime);

        // float height = GetFontSize() * 25.5f;
        // SetNextWindowSizeConstraints(new Vector2(200, height), new Vector2(1000, height));
        Begin("Параметры сферы");
        
        Group("1", () =>
        {
            SliderI("R, пиксель", ref _sphere.R, 0, 3000, ref _sphere.Update);
        });
        
        Group("2", () =>
        {
            SliderI("Max U", ref _sphere.UMax, 0, 360, ref _sphere.Update);
            SliderI("Мax V", ref _sphere.VMax, 0, 180, ref _sphere.Update);
        });
        
        Group("3", () =>
        {
            SliderI("Div U", ref _sphere.UDiv, 0, 200, ref _sphere.Update);
            SliderI("Div V", ref _sphere.VDiv, 0, 200, ref _sphere.Update);
        });
        
        Group("4", () =>
        {
            Label("", "Поворот по X, Y, Z", "");
            DragAngle("AngleX", ref _stateX, Matrix4.GetRotateX, ref _sphere.Update);
            DragAngle("AngleY", ref _stateY, Matrix4.GetRotateY, ref _sphere.Update);
            DragAngle("AngleZ", ref _stateZ, Matrix4.GetRotateZ, ref _sphere.Update);
        });

        Group("5", () =>
        {
            Checkbox("Шейдер", ref _sphere.Shading);
        });
        
        End();
        _controller.Render();
    }

    private void Group(string name, Action content)
    {
        BeginChild(
            name, 
            new Vector2(0, 0), 
            ImGuiChildFlags.Border | ImGuiChildFlags.AutoResizeY);
        PushItemWidth(GetContentRegionAvail().X * 1.0f);
        content();
        EndChild();
    }

    private void DragAngle(string name, ref DragAngleState state, Func<int, Matrix4> transform, ref bool update)
    {
        const int limit = int.MaxValue;
        
        DragInt($"##{name}", ref state.Delta, 1, -limit, limit, "%d", Flag);

        SetHoverCursor(ref state.Hover);

        if (IsItemActivated()) 
            state.Initial = _sphere.TransformationMat;
        if (IsItemActive())
        {
            _sphere.TransformationMat = state.Initial * transform(state.Delta);
            update = true;
        }
        if (!IsItemDeactivated()) 
            return;
        
        state.Initial = _sphere.TransformationMat;
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

    private static void SliderI(string label, ref int v, int vMin, int vMax, ref bool update)
    {
        Label($"{vMin}", label, $"{vMax}");
        bool active = SliderInt($"##{label}", ref v, vMin, vMax, "%d", Flag);
        if (active) update = true;
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

    public void Dispose() => _controller.Dispose();

    private struct DragAngleState
    {
        public Matrix4 Initial;
        public int Delta;
        public bool Hover;
    }
}

