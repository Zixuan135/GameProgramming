namespace BubbleTown.Core.Enums
{
    /// <summary>
    /// Item categories supported by the shared item system.
    /// Values are kept stable for Unity serialization compatibility.
    /// </summary>
    public enum ItemType
    {
        /// <summary>No item effect is assigned.</summary>
        None = 0,

        /// <summary>Increases the number of bombs a character may have active at once.</summary>
        BombCountUp = 1,

        /// <summary>Increases the number of grid cells reached by bomb explosions.</summary>
        ExplosionRangeUp = 2,

        /// <summary>Increases a character's grid movement speed.</summary>
        MoveSpeedUp = 3,

        /// <summary>Blocks one explosion hit before the character can be defeated.</summary>
        Shield = 4,

        /// <summary>Grants a short timed invincibility window after pickup.</summary>
        TemporaryInvincible = 5,

        /// <summary>Reserved item type for future bomb-kicking gameplay.</summary>
        KickBomb = 6,

        /// <summary>Reserved item type for future explosions that can pass through selected blockers.</summary>
        PierceExplosion = 7,

        // Compatibility alias for earlier scripts/scenes that used bomb range wording.
        BombRangeUp = ExplosionRangeUp
    }
}
