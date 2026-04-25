#pragma warning disable CS0618 // Type or member is obsolete

using static CSharpSphere.Program;

namespace CSharpSphere;

public abstract class Surface
{
    private Vector3[,] _points = null!;
    private Triangle[] _triangles = null!;
    
    public int UMax = 360;
    public int VMax = 180;
    public int UDiv = 20;
    public int VDiv = 20;

    public bool Update = true;
    public bool Shading = false;
    public bool TwoStep = true;
    
    public Matrix4 TransformationMat = Matrix4.Identity;
    
    public Color OuterColor = new(0.8f, 0.2f, 0.2f);
    public Color InnerColor = new(0.2f, 0.2f, 0.65f);

    public void Draw()
    {
        if (Update)
        {
            GeneratePoints();
            GenerateTriangles();
            Transform();
            if (Shading) GenerateColors();
            Update = false;
        }

        if (Shading)
            DrawPolygons(_triangles, _points, TwoStep);
        else
            DrawLines(_triangles, _points);
    }

    private void GeneratePoints()
    {
        var points = new Vector3[VDiv + 1, UDiv + 1];

        Parallel.For(0, VDiv + 1, row =>
        {
            var v = (float)row / VDiv * (VMax * MathF.PI / 180f);

            float sinV = MathF.Sin(v);
            float cosV = MathF.Cos(v);

            for (int col = 0; col < UDiv + 1; col++)
            {
                var u = (float)col / UDiv * (UMax * MathF.PI / 180f);

                float sinU = MathF.Sin(u);
                float cosU = MathF.Cos(u);

                points[row, col] = GeneratePoint(cosU, sinV, cosV, sinU);
            }
        });
        
        _points = points;
    }

    protected abstract Vector3 GeneratePoint(float cosU, float sinV, float cosV, float sinU);

    private void GenerateTriangles()
    {
        int cols = UDiv + 1;
        int rows = VDiv + 1;
        
        var triangles = new Triangle[2 * (rows - 1) * (cols - 1)];

        Parallel.For(0, rows - 1, i =>
        {
            for (int j = 0; j < cols - 1; j++)
            {
                int index = 2 * (i * (cols - 1) + j);
                
                triangles[index] = new Triangle(
                    (i, j),
                    (i, j + 1),
                    (i + 1, j));
                triangles[index + 1] = new Triangle(
                    (i, j + 1),
                    (i + 1, j + 1),
                    (i + 1, j));
            }
        });
        
        _triangles = triangles;
    }
    
    private void Transform()
    {
        var result = new Vector3[VDiv + 1, UDiv + 1];

        Parallel.For(0, VDiv + 1, row =>
        {
            for (int col = 0; col < UDiv + 1; col++)
                result[row, col] = _points[row, col] * TransformationMat;
        });
        
        _points = result;
    }
    
    private void GenerateColors()
    {
        var lightPos = new Vector3(0f, 0f, 1f);

        foreach (Triangle tri in _triangles)
        {
            Vector3[] points = [
                tri.GetP1(_points), 
                tri.GetP2(_points), 
                tri.GetP3(_points)];
            
            Vector3 n = NewellNormal(points);
            n = Vector3.Normalize(n);
            
            float cos = n.X * lightPos.X + n.Y * lightPos.Y + n.Z * lightPos.Z;
            
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

public class Sphere : Surface
{
    public int R = 700;

    protected override Vector3 GeneratePoint(float cosU, float sinV, float cosV, float sinU)
    {
        float x = R * cosU * sinV;
        float y = R * cosV;
        float z = R * sinU * sinV;
        
        return new Vector3(x, y, z);
    }
}
