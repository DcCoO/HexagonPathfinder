public class PathFinderData
{
    public int g;
    public int h;
    public int f;
    public ICell parent;

    public PathFinderData() => Reset();

    public void Reset()
    {
        g = int.MaxValue;
        h = int.MaxValue;
        f = int.MaxValue;
        parent = null;
    }

    public void Setup(int g, int h, ICell parent)
    {
        this.g = g;
        this.h = h;
        f = g + h;
        this.parent = parent;
    }
}