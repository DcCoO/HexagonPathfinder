using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Sprite Provider", menuName = "ScriptableObjects/Sprite Provider", order = 1)]
public class SpriteProvider : ScriptableObject
{
    [SerializeField] private SpriteGroup[] walkableSprites;
    [SerializeField] private SpriteGroup[] nonWalkableSprites;
    [SerializeField] private SpriteGroup startSprite;
    [SerializeField] private SpriteGroup endSprite;

    public SpriteGroup GetSpriteGroup(ECellType cellType) => cellType switch
    {
        ECellType.WALKABLE => walkableSprites[Random.Range(0, walkableSprites.Length)],
        ECellType.NON_WALKABLE => nonWalkableSprites[Random.Range(0, nonWalkableSprites.Length)],
        ECellType.SOURCE => startSprite,
        ECellType.TARGET => endSprite,
        _ => null
    };
}