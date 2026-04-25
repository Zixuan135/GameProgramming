namespace BubbleTown.Core.Enums
{
    /// <summary>
    /// Item categories supported by the shared item system.
    /// Values are kept stable for Unity serialization compatibility.
    /// </summary>
    public enum ItemType
    {
        None = 0,
        BombCountUp = 1,
        ExplosionRangeUp = 2,
        MoveSpeedUp = 3,

        // Compatibility alias for earlier scripts/scenes that used bomb range wording.
        BombRangeUp = ExplosionRangeUp
    }
}
