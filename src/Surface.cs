#pragma warning disable CS0618

namespace CSharpSphere;

public class Sphere() : Surface(vMax: 180)
{
    public int R = 700;

    protected override Vector3 GeneratePoint(float sinU, float cosU, float sinV, float cosV)
    {
        float x = R * cosU * sinV;
        float y = R * cosV;
        float z = R * sinU * sinV;
        
        return new Vector3(x, y, z);
    }
}

public class Torus() : Surface(mode: Render.DepthTest)
{
    public int R = 450;
    public int r = 250;
    
    protected override Vector3 GeneratePoint(float sinU, float cosU, float sinV, float cosV)
    {
        float x = (R + r * cosV) * -cosU;
        float y = r * sinV;
        float z = (R + r * cosV) * sinU;
        
        return new Vector3(x, y, z);
    }
}


public enum Render
{
    Single,
    Double,
    DepthTest
}


public abstract class Surface(int vMax = 360, Render mode = Render.Double)
{
    private Vector3[,] _points = null!;
    private Triangle[] _triangles = null!;
    private bool _update = true;

    public int UMax = 360;
    public int VMax = vMax;
    public int UDiv = 20;
    public int VDiv = 20;

    public bool Shading = false;
    public Render RenderMode = mode;
    
    public Matrix4 TransformationMat = Matrix4.Identity;
    
    public Color OuterColor = new(0.8f, 0.2f, 0.2f);
    public Color InnerColor = new(0.2f, 0.2f, 0.65f);
    
    public void Update() => _update = true;
    
    public void Draw()
    {
        if (_update)
        {
            GeneratePoints();
            GenerateTriangles();
            Transform();
            if (Shading) GenerateColors();
            _update = false;
        }

        if (Shading)
            Program.DrawFlat(_triangles, _points, RenderMode);
        else
            Program.DrawWireframe(_triangles, _points);
    }

    private void GeneratePoints()
    {
        _points = new Vector3[VDiv + 1, UDiv + 1];

        Parallel.For(0, VDiv + 1, row =>
        {
            var v = (float)row / VDiv * (VMax * MathF.PI / 180f);

            var sinV = MathF.Sin(v);
            var cosV = MathF.Cos(v);

            for (int col = 0; col < UDiv + 1; col++)
            {
                var u = (float)col / UDiv * (UMax * MathF.PI / 180f);

                var sinU = MathF.Sin(u);
                var cosU = MathF.Cos(u);

                _points[row, col] = GeneratePoint(sinU, cosU, sinV, cosV);
            }
        });
    }

    protected abstract Vector3 GeneratePoint(float sinU, float cosU, float sinV, float cosV);

    private void GenerateTriangles()
    {
        _triangles = new Triangle[2 * VDiv * UDiv];

        Parallel.For(0, VDiv, i =>
        {
            for (int j = 0; j < UDiv; j++)
            {
                int index = 2 * (i * UDiv + j);

                _triangles[index] = new Triangle(
                    (i, j),
                    (i, j + 1),
                    (i + 1, j));
                _triangles[index + 1] = new Triangle(
                    (i, j + 1),
                    (i + 1, j + 1),
                    (i + 1, j));
            }
        });
    }
    
    private void Transform()
    {
        Parallel.For(0, VDiv + 1, row =>
        {
            for (int col = 0; col < UDiv + 1; col++)
                _points[row, col] *= TransformationMat;
        });
    }
    
    private void GenerateColors()
    {
        foreach (Triangle tri in _triangles)
        {
            Vector3 n = NewellNormal([
                tri.GetP1(_points),
                tri.GetP2(_points),
                tri.GetP3(_points)
            ]);
            n = Vector3.Normalize(n);
            
            float cos = n.Z;
            
            if (cos >= 0) 
                tri.SetColor(OuterColor * cos).SetFront(true);
            else
                tri.SetColor(InnerColor * MathF.Abs(cos)).SetFront(false);
        }
    }

    private static Vector3 NewellNormal(Vector3[] points)
    {
        var n = new Vector3(0, 0, 0);
        
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 p0 = points[i];
            Vector3 p1 = points[(i + 1) % points.Length];
            
            n.X += (p0.Y - p1.Y) * (p0.Z + p1.Z);
            n.Y += (p0.Z - p1.Z) * (p0.X + p1.X);
            n.Z += (p0.X - p1.X) * (p0.Y + p1.Y);
        }
        return n;
    }
}