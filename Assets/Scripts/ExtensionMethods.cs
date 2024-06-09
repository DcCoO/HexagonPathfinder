using UnityEngine;

public static class ExtensionMethods
{
    public static (int, int) ToTuple(this Vector2Int vector) => (vector.x, vector.y); 
}
