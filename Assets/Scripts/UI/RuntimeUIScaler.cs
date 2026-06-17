using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Shared runtime scaling rules for image-backed UI and dynamic labels.
    /// </summary>
    public static class RuntimeUIScaler
    {
        public const float ReferenceWidth = 1024f;
        public const float ReferenceHeight = 768f;

        private const float DefaultMaxScale = 2.75f;

        /// <summary>
        /// Gets the reference resolution used when calculating runtime UI scale.
        /// </summary>
        public static Vector2 ReferenceResolution => new Vector2(ReferenceWidth, ReferenceHeight);

        /// <summary>
        /// Gets the pixel-density multiplier used by exported builds.
        /// </summary>
        public static float PixelDensityScale => 1f;

        /// <summary>
        /// Gets a screen-area based multiplier so high-resolution player builds keep readable labels.
        /// </summary>
        public static float ScreenAreaScale
        {
            get
            {
                float currentArea = Mathf.Max(1f, (float)Screen.width * Screen.height);
                float referenceArea = ReferenceWidth * ReferenceHeight;
                return Mathf.Max(1f, Mathf.Sqrt(currentArea / referenceArea));
            }
        }

        /// <summary>
        /// Gets the shared fallback scale for runtime-generated UI text.
        /// </summary>
        public static float GlobalScale => Mathf.Clamp(
            Mathf.Max(ScreenAreaScale, PixelDensityScale),
            1f,
            DefaultMaxScale);

        /// <summary>
        /// Purpose: Resolves a scale that follows both the drawn artwork size and the runtime screen size.
        /// Inputs: renderedWidth is the current on-screen width; artworkWidth is the source art width; min/max clamp the result.
        /// Output: multiplier used by image-backed dynamic labels.
        /// </summary>
        /// <param name="renderedWidth">Current on-screen artwork width.</param>
        /// <param name="artworkWidth">Source artwork width.</param>
        /// <param name="minScale">Smallest allowed multiplier.</param>
        /// <param name="maxScale">Largest allowed multiplier.</param>
        /// <returns>Resolved UI text multiplier.</returns>
        public static float ResolveArtworkScale(float renderedWidth, float artworkWidth, float minScale = 1f, float maxScale = DefaultMaxScale)
        {
            float artworkScale = artworkWidth > 0f ? renderedWidth / artworkWidth : 1f;
            return Mathf.Clamp(Mathf.Max(artworkScale, GlobalScale), minScale, maxScale);
        }

        /// <summary>
        /// Purpose: Scales one authored font size by a runtime multiplier.
        /// Inputs: baseFontSize is the authored size; scale is the resolved multiplier.
        /// Output: a safe integer font size.
        /// </summary>
        /// <param name="baseFontSize">Original authored font size.</param>
        /// <param name="scale">Runtime multiplier.</param>
        /// <returns>Scaled font size.</returns>
        public static int ScaleFontSize(int baseFontSize, float scale)
        {
            return Mathf.Max(1, Mathf.RoundToInt(baseFontSize * Mathf.Max(0.01f, scale)));
        }
    }
}
