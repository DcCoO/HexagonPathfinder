using System.Collections.Generic;

public interface IPathFinder
{
    public void ClearPathFinderData(IMap map);

    public List<ICell> FindPathOnMap(ICell cellStart, ICell cellEnd, IMap map);
}
