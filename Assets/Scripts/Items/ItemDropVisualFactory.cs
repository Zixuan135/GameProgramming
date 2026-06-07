using System.Collections.Generic;
using BubbleTown.Core.Enums;
using UnityEngine;

namespace BubbleTown.Items
{
    /// <summary>
    /// Rebuilds pickup visuals at runtime so dropped battle items match the illustrated item-card style.
    /// The gameplay prefab keeps its collider, rigidbody, item effect, and feedback components.
    /// </summary>
    public static class ItemDropVisualFactory
    {
        private const string VisualRootName = "VisualRoot";
        private const float CardTopY = 0.055f;
        private const float IconY = 0.16f;

        private static readonly Dictionary<string, Material> RuntimeMaterials = new Dictionary<string, Material>();

        /// <summary>
        /// Stores the runtime material colors used to build one illustrated pickup card.
        /// </summary>
        private struct ItemPalette
        {
            public Color Card;
            public Color CardDark;
            public Color CardEdge;
            public Color Rim;
            public Color Highlight;
            public Color Icon;
            public Color IconDark;
            public Color Glow;

            /// <summary>
            /// Purpose: Packages the card, icon, rim, highlight, and glow colors for one item type.
            /// Inputs: palette colors used by card layers, icon parts, and feedback glow.
            /// Output: returns an ItemPalette value consumed by item-card mesh builders.
            /// </summary>
            /// <param name="card">Main face color of the item card.</param>
            /// <param name="cardDark">Dark backing color for depth and shadow.</param>
            /// <param name="cardEdge">Colored edge accent around the card.</param>
            /// <param name="rim">Light rim color used for sticker borders and icon highlights.</param>
            /// <param name="highlight">Soft shine color used on the card and icon.</param>
            /// <param name="icon">Primary icon color.</param>
            /// <param name="iconDark">Darker icon color used for outlines and shadows.</param>
            /// <param name="glow">Glow color used by feedback and floor accents.</param>
            public ItemPalette(
                Color card,
                Color cardDark,
                Color cardEdge,
                Color rim,
                Color highlight,
                Color icon,
                Color iconDark,
                Color glow)
            {
                Card = card;
                CardDark = cardDark;
                CardEdge = cardEdge;
                Rim = rim;
                Highlight = highlight;
                Icon = icon;
                IconDark = iconDark;
                Glow = glow;
            }
        }

        /// <summary>
        /// Purpose: Replaces only the visible item model with a richer toy-card pickup.
        /// Inputs: item object and item type.
        /// Output: creates a fresh VisualRoot and retargets animation/feedback components.
        /// </summary>
        /// <param name="itemObject">Spawned item prefab instance.</param>
        /// <param name="itemType">Gameplay item type to visualize.</param>
        public static void Rebuild(GameObject itemObject, ItemType itemType)
        {
            if (itemObject == null || itemType == ItemType.None)
            {
                return;
            }

            Transform visualRoot = RecreateVisualRoot(itemObject.transform);
            ItemPalette palette = ResolvePalette(itemType);

            BuildCard(visualRoot, palette);
            BuildIcon(visualRoot, itemType, palette);

            Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>();
            ItemVisualAnimator visualAnimator = itemObject.GetComponent<ItemVisualAnimator>();
            if (visualAnimator != null)
            {
                visualAnimator.SetVisualReferences(visualRoot, renderers, palette.Glow);
            }

            ItemPickupFeedback pickupFeedback = itemObject.GetComponent<ItemPickupFeedback>();
            if (pickupFeedback != null)
            {
                pickupFeedback.SetVisualReferences(visualRoot, renderers, palette.Glow);
            }
        }

        /// <summary>
        /// Purpose: Recreates the child transform that owns the visual-only pickup meshes.
        /// Inputs: itemTransform is the spawned item root that should keep gameplay components.
        /// Output: returns a clean VisualRoot transform parented under the item.
        /// </summary>
        /// <param name="itemTransform">Spawned item transform that receives the visual root.</param>
        /// <returns>a clean VisualRoot transform parented under the item.</returns>
        private static Transform RecreateVisualRoot(Transform itemTransform)
        {
            Transform oldRoot = itemTransform.Find(VisualRootName);
            if (oldRoot != null)
            {
                oldRoot.gameObject.SetActive(false);
                DestroyObject(oldRoot.gameObject);
            }

            GameObject rootObject = new GameObject(VisualRootName);
            Transform visualRoot = rootObject.transform;
            visualRoot.SetParent(itemTransform, false);
            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = Quaternion.identity;
            visualRoot.localScale = Vector3.one;
            return visualRoot;
        }

        /// <summary>
        /// Purpose: Builds the layered square card base shared by every pickup type.
        /// Inputs: root receives generated meshes, and palette supplies the card colors.
        /// Output: no return value; appends card, rim, shine, shadow, and glow parts.
        /// </summary>
        /// <param name="root">Visual root that receives generated card pieces.</param>
        /// <param name="palette">Color palette for the current item type.</param>
        private static void BuildCard(Transform root, ItemPalette palette)
        {
            Material shadow = GetMaterial("Card_Shadow", new Color(0.08f, 0.12f, 0.18f, 1f), false, 0f, 0.18f);
            Material dark = GetMaterial("Card_Dark_" + ColorKey(palette.CardDark), palette.CardDark, false, 0f, 0.28f);
            Material edge = GetMaterial("Card_Edge_" + ColorKey(palette.CardEdge), palette.CardEdge, false, 0f, 0.34f);
            Material rim = GetMaterial("Card_Rim_" + ColorKey(palette.Rim), palette.Rim, false, 0f, 0.18f);
            Material card = GetMaterial("Card_Face_" + ColorKey(palette.Card), palette.Card, false, 0f, 0.42f);
            Material highlight = GetMaterial("Card_Highlight_" + ColorKey(palette.Highlight), palette.Highlight, false, 0f, 0.26f);
            Material glow = GetMaterial("Card_Glow_" + ColorKey(palette.Glow), palette.Glow, true, 0.45f, 0.5f);

            CreatePart(root, "Card_Shadow", PrimitiveType.Cube, new Vector3(0.045f, -0.03f, -0.045f), new Vector3(0.88f, 0.035f, 0.88f), shadow);
            CreatePart(root, "Card_DarkBase", PrimitiveType.Cube, new Vector3(0f, -0.005f, 0f), new Vector3(0.88f, 0.075f, 0.88f), dark);
            CreatePart(root, "Card_ColorEdge", PrimitiveType.Cube, new Vector3(0f, 0.035f, 0f), new Vector3(0.82f, 0.065f, 0.82f), edge);
            CreatePart(root, "Card_WhiteRim", PrimitiveType.Cube, new Vector3(0f, 0.07f, 0f), new Vector3(0.74f, 0.05f, 0.74f), rim);
            CreatePart(root, "Card_Face", PrimitiveType.Cube, new Vector3(0f, CardTopY + 0.055f, 0f), new Vector3(0.62f, 0.045f, 0.62f), card);

            CreatePart(root, "Card_TopShine", PrimitiveType.Cube, new Vector3(0f, CardTopY + 0.09f, 0.305f), new Vector3(0.52f, 0.018f, 0.026f), highlight);
            CreatePart(root, "Card_LeftShine", PrimitiveType.Cube, new Vector3(-0.305f, CardTopY + 0.086f, 0.02f), new Vector3(0.026f, 0.018f, 0.42f), highlight);
            CreatePart(root, "Card_FloorGlow", PrimitiveType.Cylinder, new Vector3(0f, -0.08f, 0f), new Vector3(0.58f, 0.012f, 0.58f), glow);

            AddCardCorner(root, "NorthEast", 0.38f, 0.38f, edge);
            AddCardCorner(root, "NorthWest", -0.38f, 0.38f, edge);
            AddCardCorner(root, "SouthEast", 0.38f, -0.38f, edge);
            AddCardCorner(root, "SouthWest", -0.38f, -0.38f, edge);
        }

        /// <summary>
        /// Purpose: Adds one rounded corner accent to the pickup card.
        /// Inputs: root, suffix, local x/z coordinates, and the corner material.
        /// Output: no return value; appends one corner mesh to the card.
        /// </summary>
        /// <param name="root">Visual root that receives the corner piece.</param>
        /// <param name="suffix">Name suffix used to keep generated objects readable.</param>
        /// <param name="x">Local x coordinate of the corner.</param>
        /// <param name="z">Local z coordinate of the corner.</param>
        /// <param name="material">Material applied to the corner mesh.</param>
        private static void AddCardCorner(Transform root, string suffix, float x, float z, Material material)
        {
            CreatePart(root, "Card_Corner_" + suffix, PrimitiveType.Sphere, new Vector3(x, CardTopY + 0.03f, z), new Vector3(0.09f, 0.035f, 0.09f), material);
        }

        /// <summary>
        /// Purpose: Chooses the icon builder that matches the gameplay item type.
        /// Inputs: root receives generated meshes, itemType selects the icon, and palette supplies colors.
        /// Output: no return value; appends the selected icon to the item card.
        /// </summary>
        /// <param name="root">Visual root that receives generated icon pieces.</param>
        /// <param name="itemType">Gameplay item type to visualize.</param>
        /// <param name="palette">Color palette for the current item type.</param>
        private static void BuildIcon(Transform root, ItemType itemType, ItemPalette palette)
        {
            switch (itemType)
            {
                case ItemType.BombCountUp:
                    BuildBombCountIcon(root, palette);
                    break;
                case ItemType.ExplosionRangeUp:
                    BuildBlastRangeIcon(root, palette);
                    break;
                case ItemType.MoveSpeedUp:
                    BuildSpeedBootIcon(root, palette);
                    break;
                case ItemType.Shield:
                    BuildShieldIcon(root, palette);
                    break;
                case ItemType.TemporaryInvincible:
                    BuildInvincibleIcon(root, palette);
                    break;
                default:
                    BuildFallbackIcon(root, palette);
                    break;
            }
        }

        /// <summary>
        /// Purpose: Builds the bomb-count-up icon using bomb, fuse, sparkle, and plus-sign pieces.
        /// Inputs: root receives generated meshes, and palette supplies icon accent colors.
        /// Output: no return value; appends the complete bomb-count icon.
        /// </summary>
        /// <param name="root">Visual root that receives generated icon pieces.</param>
        /// <param name="palette">Color palette for the bomb-count item card.</param>
        private static void BuildBombCountIcon(Transform root, ItemPalette palette)
        {
            Material bomb = GetMaterial("Bomb_Body", new Color(0.1f, 0.24f, 0.46f, 1f), false, 0f, 0.58f);
            Material bombLight = GetMaterial("Bomb_Light", new Color(0.5f, 0.78f, 1f, 1f), false, 0f, 0.42f);
            Material fuse = GetMaterial("Bomb_Fuse", new Color(0.48f, 0.24f, 0.1f, 1f), false, 0f, 0.22f);
            Material yellow = GetMaterial("Bomb_PlusYellow", palette.Icon, true, 0.2f, 0.34f);
            Material yellowDark = GetMaterial("Bomb_PlusEdge", palette.IconDark, false, 0f, 0.22f);

            CreatePart(root, "Bomb_BodyShadow", PrimitiveType.Sphere, new Vector3(-0.13f, IconY + 0.005f, -0.04f), new Vector3(0.36f, 0.21f, 0.36f), GetMaterial("Bomb_DarkShadow", new Color(0.04f, 0.09f, 0.18f, 1f), false, 0f, 0.16f));
            CreatePart(root, "Bomb_Body", PrimitiveType.Sphere, new Vector3(-0.16f, IconY + 0.04f, 0f), new Vector3(0.34f, 0.24f, 0.34f), bomb);
            CreatePart(root, "Bomb_Highlight", PrimitiveType.Sphere, new Vector3(-0.26f, IconY + 0.11f, 0.08f), new Vector3(0.1f, 0.045f, 0.1f), bombLight);
            CreatePart(root, "Bomb_TopRing", PrimitiveType.Cylinder, new Vector3(-0.06f, IconY + 0.22f, 0.15f), new Vector3(0.11f, 0.028f, 0.11f), bombLight);

            CreatePart(root, "Bomb_Fuse_A", PrimitiveType.Cube, new Vector3(0.02f, IconY + 0.25f, 0.19f), new Vector3(0.16f, 0.035f, 0.045f), fuse, new Vector3(0f, 30f, 0f));
            CreatePart(root, "Bomb_Fuse_B", PrimitiveType.Cube, new Vector3(0.13f, IconY + 0.28f, 0.25f), new Vector3(0.14f, 0.035f, 0.045f), fuse, new Vector3(0f, 12f, 0f));

            CreateFlatSparkle(root, "FuseStar", new Vector3(0.25f, IconY + 0.3f, 0.26f), 0.17f, yellowDark, yellow);
            CreateFlatPlus(root, "BombSlotPlus", new Vector3(0.22f, IconY + 0.13f, -0.19f), 0.2f, yellowDark, yellow);
            CreatePart(root, "Bomb_WhiteSticker", PrimitiveType.Cube, new Vector3(-0.06f, IconY + 0.16f, -0.24f), new Vector3(0.2f, 0.02f, 0.045f), GetMaterial("Sticker_White", palette.Rim, false, 0f, 0.18f), new Vector3(0f, -14f, 0f));
        }

        /// <summary>
        /// Purpose: Builds the explosion-range-up icon with a burst, arrows, and dotted ring.
        /// Inputs: root receives generated meshes, and palette supplies orange blast colors.
        /// Output: no return value; appends the complete blast-range icon.
        /// </summary>
        /// <param name="root">Visual root that receives generated icon pieces.</param>
        /// <param name="palette">Color palette for the blast-range item card.</param>
        private static void BuildBlastRangeIcon(Transform root, ItemPalette palette)
        {
            Material cream = GetMaterial("Blast_Cream", palette.Rim, true, 0.2f, 0.24f);
            Material gold = GetMaterial("Blast_Gold", palette.Icon, true, 0.25f, 0.32f);
            Material orangeDark = GetMaterial("Blast_OrangeDark", palette.IconDark, false, 0f, 0.22f);

            for (int i = 0; i < 20; i++)
            {
                float angle = i * Mathf.PI * 2f / 20f;
                Vector3 position = new Vector3(Mathf.Cos(angle) * 0.31f, IconY + 0.05f, Mathf.Sin(angle) * 0.31f);
                if (i % 2 == 0)
                {
                    CreatePart(root, "Blast_Dot_" + i.ToString("00"), PrimitiveType.Cube, position, new Vector3(0.045f, 0.018f, 0.045f), cream, new Vector3(0f, -angle * Mathf.Rad2Deg, 0f));
                }
            }

            CreateFlatSparkle(root, "Blast_BurstBack", new Vector3(0f, IconY + 0.06f, 0f), 0.36f, orangeDark, gold);
            CreateFlatPlus(root, "Blast_CenterPlus", new Vector3(0f, IconY + 0.14f, 0f), 0.18f, orangeDark, GetMaterial("Blast_CenterCut", palette.Card, false, 0f, 0.28f));

            CreateArrow(root, "Blast_North", new Vector3(0f, IconY + 0.12f, 0.2f), 0f, cream, gold);
            CreateArrow(root, "Blast_East", new Vector3(0.2f, IconY + 0.12f, 0f), 90f, cream, gold);
            CreateArrow(root, "Blast_South", new Vector3(0f, IconY + 0.12f, -0.2f), 180f, cream, gold);
            CreateArrow(root, "Blast_West", new Vector3(-0.2f, IconY + 0.12f, 0f), 270f, cream, gold);
        }

        /// <summary>
        /// Purpose: Builds the move-speed-up icon with a green boot, wing, motion lines, and sparkle.
        /// Inputs: root receives generated meshes, and palette supplies speed item colors.
        /// Output: no return value; appends the complete speed-boot icon.
        /// </summary>
        /// <param name="root">Visual root that receives generated icon pieces.</param>
        /// <param name="palette">Color palette for the speed item card.</param>
        private static void BuildSpeedBootIcon(Transform root, ItemPalette palette)
        {
            Transform shoe = CreateGroup(root, "SpeedBootIcon", new Vector3(0.05f, 0f, 0.01f), new Vector3(0f, -30f, 0f));
            Material green = GetMaterial("Speed_Green", palette.Icon, false, 0f, 0.38f);
            Material darkGreen = GetMaterial("Speed_DarkGreen", palette.IconDark, false, 0f, 0.2f);
            Material cream = GetMaterial("Speed_Cream", palette.Rim, false, 0f, 0.24f);
            Material wing = GetMaterial("Speed_WingWhite", new Color(0.96f, 1f, 1f, 1f), false, 0f, 0.2f);
            Material wingShade = GetMaterial("Speed_WingShade", new Color(0.68f, 0.86f, 0.96f, 1f), false, 0f, 0.2f);

            CreatePart(shoe, "Boot_Shadow", PrimitiveType.Cube, new Vector3(0.04f, IconY + 0.005f, -0.02f), new Vector3(0.46f, 0.04f, 0.22f), darkGreen, new Vector3(0f, 0f, 0f));
            CreatePart(shoe, "Boot_Sole", PrimitiveType.Cube, new Vector3(0.04f, IconY + 0.05f, -0.02f), new Vector3(0.5f, 0.055f, 0.18f), cream);
            CreatePart(shoe, "Boot_Body", PrimitiveType.Cube, new Vector3(0.03f, IconY + 0.12f, 0.02f), new Vector3(0.42f, 0.13f, 0.22f), green);
            CreatePart(shoe, "Boot_Toe", PrimitiveType.Sphere, new Vector3(0.25f, IconY + 0.13f, 0.02f), new Vector3(0.23f, 0.12f, 0.22f), green);
            CreatePart(shoe, "Boot_Collar", PrimitiveType.Cylinder, new Vector3(-0.17f, IconY + 0.19f, 0.05f), new Vector3(0.13f, 0.035f, 0.13f), cream);
            CreatePart(shoe, "Boot_Lace_A", PrimitiveType.Cube, new Vector3(0.04f, IconY + 0.205f, 0.13f), new Vector3(0.17f, 0.02f, 0.035f), cream, new Vector3(0f, 18f, 0f));
            CreatePart(shoe, "Boot_Lace_B", PrimitiveType.Cube, new Vector3(0.12f, IconY + 0.205f, 0.1f), new Vector3(0.16f, 0.02f, 0.035f), cream, new Vector3(0f, 18f, 0f));

            CreatePart(shoe, "Wing_Back", PrimitiveType.Sphere, new Vector3(-0.31f, IconY + 0.24f, 0.08f), new Vector3(0.16f, 0.055f, 0.26f), wingShade, new Vector3(0f, -24f, 0f));
            CreatePart(shoe, "Wing_Feather_Long", PrimitiveType.Sphere, new Vector3(-0.37f, IconY + 0.26f, 0.16f), new Vector3(0.13f, 0.048f, 0.28f), wing, new Vector3(0f, -28f, 0f));
            CreatePart(shoe, "Wing_Feather_Mid", PrimitiveType.Sphere, new Vector3(-0.34f, IconY + 0.22f, 0.02f), new Vector3(0.12f, 0.04f, 0.23f), wing, new Vector3(0f, -16f, 0f));
            CreatePart(shoe, "Wing_Feather_Short", PrimitiveType.Sphere, new Vector3(-0.29f, IconY + 0.18f, -0.1f), new Vector3(0.1f, 0.035f, 0.18f), wing, new Vector3(0f, -8f, 0f));

            CreatePart(root, "Speed_Line_A", PrimitiveType.Cube, new Vector3(-0.33f, IconY + 0.12f, 0.15f), new Vector3(0.19f, 0.02f, 0.035f), cream);
            CreatePart(root, "Speed_Line_B", PrimitiveType.Cube, new Vector3(-0.36f, IconY + 0.1f, -0.02f), new Vector3(0.27f, 0.02f, 0.035f), cream);
            CreatePart(root, "Speed_Line_C", PrimitiveType.Cube, new Vector3(-0.31f, IconY + 0.08f, -0.2f), new Vector3(0.18f, 0.02f, 0.035f), cream);
            CreateFlatSparkle(
                root,
                "Speed_Spark",
                new Vector3(0.29f, IconY + 0.2f, 0.23f),
                0.12f,
                GetMaterial("Speed_SparkBack", palette.IconDark, false, 0f, 0.18f),
                GetMaterial("Speed_SparkFace", palette.Rim, true, 0.18f, 0.2f));
        }

        /// <summary>
        /// Purpose: Builds the shield icon with layered shield plates, shine, bubbles, and sparkles.
        /// Inputs: root receives generated meshes, and palette supplies shield item colors.
        /// Output: no return value; appends the complete shield icon.
        /// </summary>
        /// <param name="root">Visual root that receives generated icon pieces.</param>
        /// <param name="palette">Color palette for the shield item card.</param>
        private static void BuildShieldIcon(Transform root, ItemPalette palette)
        {
            Material rim = GetMaterial("Shield_Rim", palette.Rim, false, 0f, 0.22f);
            Material blue = GetMaterial("Shield_Blue", palette.Icon, false, 0f, 0.48f);
            Material blueDark = GetMaterial("Shield_Dark", palette.IconDark, false, 0f, 0.28f);
            Material shine = GetMaterial("Shield_Shine", palette.Highlight, false, 0f, 0.18f);

            Transform shield = CreateGroup(root, "ShieldIcon", new Vector3(0f, 0f, 0f), Vector3.zero);
            CreateShieldShape(shield, "Shield_BackRim", IconY + 0.045f, 1.08f, blueDark);
            CreateShieldShape(shield, "Shield_WhiteRim", IconY + 0.075f, 0.96f, rim);
            CreateShieldShape(shield, "Shield_Face", IconY + 0.115f, 0.72f, blue);
            CreatePart(shield, "Shield_LeftTint", PrimitiveType.Cube, new Vector3(-0.07f, IconY + 0.145f, 0.03f), new Vector3(0.17f, 0.018f, 0.36f), GetMaterial("Shield_LightBlue", new Color(0.56f, 0.9f, 1f, 1f), false, 0f, 0.34f));
            CreatePart(shield, "Shield_Highlight_Dot", PrimitiveType.Sphere, new Vector3(-0.14f, IconY + 0.18f, 0.14f), new Vector3(0.08f, 0.026f, 0.08f), shine);
            CreatePart(shield, "Shield_Highlight_Slash", PrimitiveType.Cube, new Vector3(-0.05f, IconY + 0.175f, 0.24f), new Vector3(0.18f, 0.018f, 0.035f), shine, new Vector3(0f, -20f, 0f));

            Material sparkleBack = GetMaterial("Shield_SparkBack", palette.CardEdge, false, 0f, 0.18f);
            Material sparkleFace = GetMaterial("Shield_SparkFace", palette.Rim, true, 0.18f, 0.2f);
            CreateFlatSparkle(root, "Shield_Spark_NW", new Vector3(-0.31f, IconY + 0.16f, 0.28f), 0.12f, sparkleBack, sparkleFace);
            CreateFlatSparkle(root, "Shield_Spark_SE", new Vector3(0.31f, IconY + 0.14f, -0.28f), 0.1f, sparkleBack, sparkleFace);
            CreatePart(root, "Shield_Bubble_A", PrimitiveType.Sphere, new Vector3(0.28f, IconY + 0.19f, 0.28f), new Vector3(0.06f, 0.03f, 0.06f), shine);
            CreatePart(root, "Shield_Bubble_B", PrimitiveType.Sphere, new Vector3(0.36f, IconY + 0.16f, 0.18f), new Vector3(0.045f, 0.026f, 0.045f), shine);
        }

        /// <summary>
        /// Purpose: Builds the temporary-invincibility icon with a glowing star and radiating sparkles.
        /// Inputs: root receives generated meshes, and palette supplies purple star colors.
        /// Output: no return value; appends the complete invincibility icon.
        /// </summary>
        /// <param name="root">Visual root that receives generated icon pieces.</param>
        /// <param name="palette">Color palette for the invincibility item card.</param>
        private static void BuildInvincibleIcon(Transform root, ItemPalette palette)
        {
            Material purpleEdge = GetMaterial("Invincible_PurpleEdge", palette.IconDark, false, 0f, 0.28f);
            Material white = GetMaterial("Invincible_WhiteStar", palette.Rim, true, 0.55f, 0.22f);
            Material glow = GetMaterial("Invincible_Glow", palette.Glow, true, 0.75f, 0.46f);

            CreatePart(root, "Invincible_GlowDisc", PrimitiveType.Cylinder, new Vector3(0f, IconY + 0.035f, 0f), new Vector3(0.43f, 0.018f, 0.43f), glow);
            CreateFlatStar(root, "Invincible_StarBack", new Vector3(0f, IconY + 0.08f, 0f), 0.36f, purpleEdge);
            CreateFlatStar(root, "Invincible_StarFace", new Vector3(0f, IconY + 0.13f, 0f), 0.28f, white);

            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f;
                Vector3 position = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, 0.34f);
                CreatePart(root, "Invincible_Ray_" + i.ToString("00"), PrimitiveType.Cube, new Vector3(position.x, IconY + 0.08f, position.z), new Vector3(0.02f, 0.014f, 0.12f), white, new Vector3(0f, angle, 0f));
            }

            CreateFlatSparkle(root, "Invincible_Spark_NW", new Vector3(-0.31f, IconY + 0.16f, 0.28f), 0.11f, purpleEdge, white);
            CreateFlatSparkle(root, "Invincible_Spark_NE", new Vector3(0.31f, IconY + 0.16f, 0.28f), 0.1f, purpleEdge, white);
            CreateFlatSparkle(root, "Invincible_Spark_SW", new Vector3(-0.31f, IconY + 0.13f, -0.28f), 0.09f, purpleEdge, white);
            CreateFlatSparkle(root, "Invincible_Spark_SE", new Vector3(0.31f, IconY + 0.13f, -0.28f), 0.09f, purpleEdge, white);
        }

        /// <summary>
        /// Purpose: Builds a generic plus icon when an item type has no specialized art.
        /// Inputs: root receives generated meshes, and palette supplies fallback colors.
        /// Output: no return value; appends the fallback icon to the card.
        /// </summary>
        /// <param name="root">Visual root that receives generated icon pieces.</param>
        /// <param name="palette">Color palette for the fallback item card.</param>
        private static void BuildFallbackIcon(Transform root, ItemPalette palette)
        {
            CreateFlatPlus(root, "FallbackPlus", new Vector3(0f, IconY + 0.1f, 0f), 0.28f, GetMaterial("Fallback_Dark", palette.IconDark, false, 0f, 0.2f), GetMaterial("Fallback_Icon", palette.Icon, true, 0.3f, 0.28f));
        }

        /// <summary>
        /// Purpose: Creates one flat arrow from a stem and two angled head pieces.
        /// Inputs: root, name, center, angle, stem material, and head material.
        /// Output: no return value; appends the arrow parts under root.
        /// </summary>
        /// <param name="root">Parent transform for the arrow pieces.</param>
        /// <param name="name">Base name for generated arrow parts.</param>
        /// <param name="center">Local center position for the arrow stem.</param>
        /// <param name="angle">Y-axis angle in degrees.</param>
        /// <param name="stemMaterial">Material applied to the arrow stem.</param>
        /// <param name="headMaterial">Material applied to the arrow head pieces.</param>
        private static void CreateArrow(Transform root, string name, Vector3 center, float angle, Material stemMaterial, Material headMaterial)
        {
            CreatePart(root, name + "_Stem", PrimitiveType.Cube, center, new Vector3(0.055f, 0.02f, 0.22f), stemMaterial, new Vector3(0f, angle, 0f));
            Vector3 tipOffset = Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, 0.15f);
            CreatePart(root, name + "_Head_A", PrimitiveType.Cube, center + tipOffset + Quaternion.Euler(0f, angle, 0f) * new Vector3(-0.04f, 0f, -0.035f), new Vector3(0.05f, 0.02f, 0.13f), headMaterial, new Vector3(0f, angle + 32f, 0f));
            CreatePart(root, name + "_Head_B", PrimitiveType.Cube, center + tipOffset + Quaternion.Euler(0f, angle, 0f) * new Vector3(0.04f, 0f, -0.035f), new Vector3(0.05f, 0.02f, 0.13f), headMaterial, new Vector3(0f, angle - 32f, 0f));
        }

        /// <summary>
        /// Purpose: Creates one layered shield silhouette from simple primitive parts.
        /// Inputs: parent, name, vertical position, scale, and material.
        /// Output: no return value; appends shield shape parts under parent.
        /// </summary>
        /// <param name="parent">Parent transform for the shield shape.</param>
        /// <param name="name">Base name for generated shield parts.</param>
        /// <param name="y">Local y position for the shield layer.</param>
        /// <param name="scale">Uniform size multiplier for the shield layer.</param>
        /// <param name="material">Material applied to every part in this shield layer.</param>
        private static void CreateShieldShape(Transform parent, string name, float y, float scale, Material material)
        {
            CreatePart(parent, name + "_Top", PrimitiveType.Cube, new Vector3(0f, y, 0.12f), new Vector3(0.34f * scale, 0.028f, 0.22f * scale), material);
            CreatePart(parent, name + "_Left", PrimitiveType.Cube, new Vector3(-0.12f * scale, y, -0.03f), new Vector3(0.17f * scale, 0.028f, 0.32f * scale), material, new Vector3(0f, -12f, 0f));
            CreatePart(parent, name + "_Right", PrimitiveType.Cube, new Vector3(0.12f * scale, y, -0.03f), new Vector3(0.17f * scale, 0.028f, 0.32f * scale), material, new Vector3(0f, 12f, 0f));
            CreatePart(parent, name + "_Point", PrimitiveType.Cube, new Vector3(0f, y, -0.22f * scale), new Vector3(0.21f * scale, 0.028f, 0.21f * scale), material, new Vector3(0f, 45f, 0f));
        }

        /// <summary>
        /// Purpose: Creates a flat plus sign with a darker backing and brighter face.
        /// Inputs: parent, name, center, size, backing material, and face material.
        /// Output: no return value; appends plus-sign pieces under parent.
        /// </summary>
        /// <param name="parent">Parent transform for the plus sign.</param>
        /// <param name="name">Base name for generated plus parts.</param>
        /// <param name="center">Local center position for the plus sign.</param>
        /// <param name="size">Overall plus-sign size.</param>
        /// <param name="backMaterial">Material applied to backing pieces.</param>
        /// <param name="faceMaterial">Material applied to face pieces.</param>
        private static void CreateFlatPlus(Transform parent, string name, Vector3 center, float size, Material backMaterial, Material faceMaterial)
        {
            CreatePart(parent, name + "_Back_H", PrimitiveType.Cube, center + Vector3.down * 0.012f, new Vector3(size * 1.05f, 0.024f, size * 0.34f), backMaterial);
            CreatePart(parent, name + "_Back_V", PrimitiveType.Cube, center + Vector3.down * 0.011f, new Vector3(size * 0.34f, 0.024f, size * 1.05f), backMaterial);
            CreatePart(parent, name + "_Face_H", PrimitiveType.Cube, center + Vector3.up * 0.012f, new Vector3(size * 0.86f, 0.022f, size * 0.28f), faceMaterial);
            CreatePart(parent, name + "_Face_V", PrimitiveType.Cube, center + Vector3.up * 0.013f, new Vector3(size * 0.28f, 0.022f, size * 0.86f), faceMaterial);
        }

        /// <summary>
        /// Purpose: Creates a flat sparkle by combining a plus sign with diagonal shine strokes.
        /// Inputs: parent, name, center, size, backing material, and face material.
        /// Output: no return value; appends sparkle pieces under parent.
        /// </summary>
        /// <param name="parent">Parent transform for the sparkle.</param>
        /// <param name="name">Base name for generated sparkle parts.</param>
        /// <param name="center">Local center position for the sparkle.</param>
        /// <param name="size">Overall sparkle size.</param>
        /// <param name="backMaterial">Material applied to backing pieces.</param>
        /// <param name="faceMaterial">Material applied to face pieces.</param>
        private static void CreateFlatSparkle(Transform parent, string name, Vector3 center, float size, Material backMaterial, Material faceMaterial)
        {
            CreateFlatPlus(parent, name + "_Plus", center, size, backMaterial, faceMaterial);
            CreatePart(parent, name + "_Diag_A", PrimitiveType.Cube, center + Vector3.up * 0.018f, new Vector3(size * 0.22f, 0.018f, size * 0.78f), faceMaterial, new Vector3(0f, 45f, 0f));
            CreatePart(parent, name + "_Diag_B", PrimitiveType.Cube, center + Vector3.up * 0.019f, new Vector3(size * 0.22f, 0.018f, size * 0.78f), faceMaterial, new Vector3(0f, -45f, 0f));
        }

        /// <summary>
        /// Purpose: Creates a flat toy star from a round core and five blocky points.
        /// Inputs: parent, name, center, size, and material.
        /// Output: no return value; appends star pieces under parent.
        /// </summary>
        /// <param name="parent">Parent transform for the star.</param>
        /// <param name="name">Base name for generated star parts.</param>
        /// <param name="center">Local center position for the star.</param>
        /// <param name="size">Overall star size.</param>
        /// <param name="material">Material applied to the star pieces.</param>
        private static void CreateFlatStar(Transform parent, string name, Vector3 center, float size, Material material)
        {
            CreatePart(parent, name + "_Core", PrimitiveType.Sphere, center, new Vector3(size * 0.72f, 0.045f, size * 0.72f), material);
            for (int i = 0; i < 5; i++)
            {
                float angle = i * 72f;
                Vector3 pointCenter = center + Quaternion.Euler(0f, angle, 0f) * new Vector3(0f, 0f, size * 0.22f);
                CreatePart(parent, name + "_Point_" + i.ToString("00"), PrimitiveType.Cube, pointCenter, new Vector3(size * 0.24f, 0.04f, size * 0.72f), material, new Vector3(0f, angle, 0f));
            }
        }

        /// <summary>
        /// Purpose: Creates a transform group used to rotate or offset related icon pieces together.
        /// Inputs: parent, group name, local position, and local Euler rotation.
        /// Output: returns the new child group transform.
        /// </summary>
        /// <param name="parent">Parent transform for the group.</param>
        /// <param name="name">Name assigned to the new group object.</param>
        /// <param name="localPosition">Local position assigned to the group.</param>
        /// <param name="localEulerAngles">Local Euler rotation assigned to the group.</param>
        /// <returns>the new child group transform.</returns>
        private static Transform CreateGroup(Transform parent, string name, Vector3 localPosition, Vector3 localEulerAngles)
        {
            GameObject groupObject = new GameObject(name);
            Transform group = groupObject.transform;
            group.SetParent(parent, false);
            group.localPosition = localPosition;
            group.localRotation = Quaternion.Euler(localEulerAngles);
            group.localScale = Vector3.one;
            return group;
        }

        /// <summary>
        /// Purpose: Creates one primitive mesh part for a generated pickup visual.
        /// Inputs: parent, name, primitive type, local transform values, material, and optional rotation.
        /// Output: returns the generated GameObject with rendering kept and collision removed.
        /// </summary>
        /// <param name="parent">Parent transform for the generated part.</param>
        /// <param name="name">Name assigned to the generated part.</param>
        /// <param name="primitiveType">Unity primitive mesh type to create.</param>
        /// <param name="localPosition">Local position assigned to the generated part.</param>
        /// <param name="localScale">Local scale assigned to the generated part.</param>
        /// <param name="material">Material assigned to the part renderer.</param>
        /// <param name="localEulerAngles">Optional local Euler rotation for the part.</param>
        /// <returns>the generated GameObject with rendering kept and collision removed.</returns>
        private static GameObject CreatePart(
            Transform parent,
            string name,
            PrimitiveType primitiveType,
            Vector3 localPosition,
            Vector3 localScale,
            Material material,
            Vector3 localEulerAngles = default)
        {
            GameObject child = GameObject.CreatePrimitive(primitiveType);
            child.name = name;
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            child.transform.localRotation = Quaternion.Euler(localEulerAngles);
            child.transform.localScale = localScale;

            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            Collider collider = child.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                DestroyObject(collider);
            }

            return child;
        }

        /// <summary>
        /// Purpose: Resolves the illustrated-card palette for a gameplay item type.
        /// Inputs: itemType selects the color family.
        /// Output: returns the palette used by card and icon builders.
        /// </summary>
        /// <param name="itemType">Gameplay item type to visualize.</param>
        /// <returns>the palette used by card and icon builders.</returns>
        private static ItemPalette ResolvePalette(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.BombCountUp:
                    return new ItemPalette(
                        new Color(0.08f, 0.66f, 0.9f),
                        new Color(0.02f, 0.28f, 0.48f),
                        new Color(0.05f, 0.52f, 0.78f),
                        new Color(0.96f, 1f, 1f),
                        new Color(0.84f, 0.98f, 1f),
                        new Color(1f, 0.8f, 0.18f),
                        new Color(0.58f, 0.22f, 0.04f),
                        new Color(0.24f, 0.88f, 1f));
                case ItemType.ExplosionRangeUp:
                    return new ItemPalette(
                        new Color(1f, 0.48f, 0.02f),
                        new Color(0.64f, 0.22f, 0.02f),
                        new Color(0.98f, 0.64f, 0.08f),
                        new Color(1f, 0.94f, 0.64f),
                        new Color(1f, 0.88f, 0.3f),
                        new Color(1f, 0.82f, 0.24f),
                        new Color(0.9f, 0.28f, 0.02f),
                        new Color(1f, 0.74f, 0.08f));
                case ItemType.MoveSpeedUp:
                    return new ItemPalette(
                        new Color(0.48f, 0.9f, 0.34f),
                        new Color(0.12f, 0.42f, 0.12f),
                        new Color(0.2f, 0.62f, 0.18f),
                        new Color(0.98f, 1f, 0.86f),
                        new Color(0.84f, 1f, 0.62f),
                        new Color(0.36f, 0.86f, 0.22f),
                        new Color(0.08f, 0.36f, 0.1f),
                        new Color(0.68f, 1f, 0.4f));
                case ItemType.Shield:
                    return new ItemPalette(
                        new Color(0.48f, 0.86f, 1f),
                        new Color(0.08f, 0.36f, 0.72f),
                        new Color(0.2f, 0.62f, 0.94f),
                        new Color(0.98f, 1f, 1f),
                        new Color(0.78f, 0.96f, 1f),
                        new Color(0.12f, 0.64f, 1f),
                        new Color(0.04f, 0.3f, 0.68f),
                        new Color(0.42f, 0.88f, 1f));
                case ItemType.TemporaryInvincible:
                    return new ItemPalette(
                        new Color(0.68f, 0.36f, 1f),
                        new Color(0.3f, 0.18f, 0.54f),
                        new Color(0.6f, 0.3f, 0.9f),
                        new Color(1f, 0.98f, 1f),
                        new Color(0.92f, 0.78f, 1f),
                        new Color(1f, 0.96f, 1f),
                        new Color(0.48f, 0.22f, 0.82f),
                        new Color(0.9f, 0.58f, 1f));
                default:
                    return new ItemPalette(
                        new Color(0.7f, 0.72f, 0.86f),
                        new Color(0.22f, 0.24f, 0.34f),
                        new Color(0.44f, 0.48f, 0.66f),
                        new Color(1f, 1f, 1f),
                        new Color(0.88f, 0.9f, 1f),
                        new Color(1f, 0.82f, 0.28f),
                        new Color(0.4f, 0.28f, 0.1f),
                        new Color(0.8f, 0.88f, 1f));
            }
        }

        /// <summary>
        /// Purpose: Gets or creates a cached runtime material for generated pickup meshes.
        /// Inputs: key identifies the cache entry, color configures the material, and emission/smoothness tune the finish.
        /// Output: returns a reusable material instance hidden from scene saving.
        /// </summary>
        /// <param name="key">Unique cache key for the material.</param>
        /// <param name="color">Base color assigned to the material.</param>
        /// <param name="useEmission">Whether to enable emissive color when supported.</param>
        /// <param name="emissionIntensity">Multiplier for the emission color.</param>
        /// <param name="smoothness">Smoothness value clamped into the shader's supported range.</param>
        /// <returns>a reusable material instance hidden from scene saving.</returns>
        private static Material GetMaterial(string key, Color color, bool useEmission, float emissionIntensity, float smoothness)
        {
            if (RuntimeMaterials.TryGetValue(key, out Material existingMaterial) && existingMaterial != null)
            {
                return existingMaterial;
            }

            Shader shader = Shader.Find("Standard");
            if (shader == null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Diffuse");
            }

            if (shader == null)
            {
                shader = Shader.Find("Hidden/InternalErrorShader");
            }

            Material material = new Material(shader)
            {
                name = "Mat_Runtime_ItemDrop_" + key,
                color = color,
                hideFlags = HideFlags.HideAndDontSave
            };

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Glossiness"))
            {
                material.SetFloat("_Glossiness", Mathf.Clamp01(smoothness));
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }

            if (useEmission && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * Mathf.Max(0.1f, emissionIntensity));
            }

            RuntimeMaterials[key] = material;
            return material;
        }

        /// <summary>
        /// Purpose: Converts an RGB color into a compact material-cache suffix.
        /// Inputs: color supplies the RGB channels to encode.
        /// Output: returns a six-character hexadecimal RGB key.
        /// </summary>
        /// <param name="color">Color whose RGB channels should be encoded.</param>
        /// <returns>a six-character hexadecimal RGB key.</returns>
        private static string ColorKey(Color color)
        {
            return Mathf.RoundToInt(color.r * 255f).ToString("X2") +
                   Mathf.RoundToInt(color.g * 255f).ToString("X2") +
                   Mathf.RoundToInt(color.b * 255f).ToString("X2");
        }

        /// <summary>
        /// Purpose: Destroys generated visual-only objects using the safe API for play mode or edit mode.
        /// Inputs: target is the object to remove.
        /// Output: no return value; removes the target when it exists.
        /// </summary>
        /// <param name="target">Unity object to destroy.</param>
        private static void DestroyObject(Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(target);
            }
            else
            {
                Object.DestroyImmediate(target);
            }
        }
    }
}
