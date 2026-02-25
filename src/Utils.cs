namespace CSharpSphere;


public struct Triangle((int, int) point1, (int, int) point2, (int, int) point3)
{
    public Indexer Point1 = new(point1.Item1, point1.Item2);
    public Indexer Point2 = new(point2.Item1, point2.Item2);
    public Indexer Point3 = new(point3.Item1, point3.Item2);
}


public readonly struct Indexer(int i, int j)
{
    public readonly int Row = i;
    public readonly int Col = j;

    public override string ToString()
    {
        return $"({Row}, {Col})";
    }
}

public class Messenger
{
    public string Message = "";
    
    private readonly char[] spinner = { '|', '/', '-', '\\' };
    private int s = 0;

    public void Update(long ms)
    {
        // Message = $"\n{spinner[s]} Drawn sphere, took {ms} ms";
        Message = $"\nВремя кадра: {ms} ms";
        // s += 1; if (s > 3) s = 0;
    }
}