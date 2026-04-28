using System.Collections.Generic;
using BubbleTown.Core.Enums;
using BubbleTown.Managers;
using UnityEngine;

namespace BubbleTown.UI
{
    /// <summary>
    /// Result screen display and callbacks.
    /// Shows the latest battle summary stored by GameManager.
    /// </summary>
    public class ResultUI : MonoBehaviour
    {
        private const int TextureSize = 64;
        private const int MaxStars = 3;

        private static readonly Dictionary<string, Texture2D> RoundedTextureCache = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Texture2D> CircleTextureCache = new Dictionary<string, Texture2D>();

        [Header("Result Colors")]
        [SerializeField] private Color victoryColor = new Color(0.2f, 0.86f, 1f, 1f);
        [SerializeField] private Color defeatColor = new Color(1f, 0.45f, 0.3f, 1f);
        [SerializeField] private Color drawColor = new Color(1f, 0.72f, 0.22f, 1f);
        [SerializeField] private Color neutralColor = new Color(0.66f, 0.48f, 1f, 1f);

        private GUIStyle cardTitleStyle;
        private GUIStyle cardValueStyle;
        private GUIStyle resultIconStyle;
        private GUIStyle resultTitleStyle;
        private GUIStyle resultBodyStyle;
        private GUIStyle scoreStyle;
        private GUIStyle starStyle;
        private bool resultAudioPlayed;

        private enum ResultOutcome
        {
            Pending,
            Victory,
            Defeat,
            Draw,
            Complete
        }

        private struct ResultViewModel
        {
            public bool HasResult;
            public string Title;
            public string Detail;
            public string Winner;
            public string ModeName;
            public string MapName;
            public string OutcomeLabel;
            public string ScoreLabel;
            public string MatchScoreLabel;
            public string RewardLabel;
            public string IconText;
            public string MoodText;
            public int StarCount;
            public ResultOutcome Outcome;
            public Color AccentColor;
        }

        private void OnEnable()
        {
            resultAudioPlayed = false;
        }

        private void OnGUI()
        {
            EnsureStyles();
            SimpleUIFactory.DrawCandyBackground();

            ResultViewModel viewModel = BuildResultViewModel();
            PlayResultAudioOnce(viewModel);
            Rect panel = SimpleUIFactory.CenteredRect(840f, 660f);
            SimpleUIFactory.BeginPanel(panel);

            SimpleUIFactory.LabelPill(viewModel.HasResult ? "ROUND COMPLETE" : "RESULT WAITING");
            SimpleUIFactory.Title(viewModel.Title);
            DrawHeroResultCard(viewModel);
            DrawSummaryCards(viewModel);
            DrawRewardPanel(viewModel);

            SimpleUIFactory.FlexibleSpace();
            if (SimpleUIFactory.PrimaryButton("RETRY"))
            {
                OnClickRematch();
            }

            if (SimpleUIFactory.SecondaryButton("MAIN MENU"))
            {
                OnClickBackToMenu();
            }

            SimpleUIFactory.SmallBody("Result data is currently passed through GameManager's latest battle result fields.");
            SimpleUIFactory.EndPanel();
        }

        public void OnClickRematch()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            GameManager gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.ClearBattleResult();
                if (gameManager.CurrentGameMode == GameMode.LocalVS)
                {
                    gameManager.ResetLocalVsMatch();
                }
            }

            SceneFlowManager.Instance?.LoadBattle();
        }

        public void OnClickBackToMenu()
        {
            AudioManager.Instance?.PlayButtonClickSFX();
            GameManager.Instance?.ResetSessionData();
            SceneFlowManager.Instance?.LoadMainMenu();
        }

        private ResultViewModel BuildResultViewModel()
        {
            GameManager gameManager = GameManager.Instance;
            bool hasResult = gameManager != null && gameManager.HasBattleResult;
            string title = hasResult ? gameManager.LastResultTitle : "No Result Yet";
            string detail = hasResult ? gameManager.LastResultDetail : "No completed battle result was found. Start or retry a battle to create one.";
            string winner = hasResult ? gameManager.LastResultWinner : "None";
            GameMode mode = gameManager != null ? gameManager.CurrentGameMode : GameMode.SinglePlayer;
            BattleMapType mapType = gameManager != null ? gameManager.CurrentMapType : BattleMapType.Default;
            ResultOutcome outcome = hasResult ? ResolveOutcome(title, winner) : ResultOutcome.Pending;
            int score = CalculateScore(outcome, mode);
            int starCount = CalculateStarCount(outcome);

            return new ResultViewModel
            {
                HasResult = hasResult,
                Title = title,
                Detail = detail,
                Winner = string.IsNullOrEmpty(winner) ? "None" : winner,
                ModeName = FormatModeName(mode),
                MapName = FormatMapName(mapType),
                OutcomeLabel = FormatOutcome(outcome),
                ScoreLabel = score.ToString("0000"),
                MatchScoreLabel = FormatMatchScore(gameManager, mode),
                RewardLabel = hasResult ? "Candy Coins +" + Mathf.Max(0, score / 10) : "Candy Coins +0",
                IconText = FormatIconText(outcome),
                MoodText = FormatMoodText(outcome),
                StarCount = starCount,
                Outcome = outcome,
                AccentColor = ResolveAccentColor(outcome)
            };
        }

        private void PlayResultAudioOnce(ResultViewModel viewModel)
        {
            if (resultAudioPlayed || !viewModel.HasResult)
            {
                return;
            }

            resultAudioPlayed = true;
            switch (viewModel.Outcome)
            {
                case ResultOutcome.Victory:
                    AudioManager.Instance?.PlayVictorySFX();
                    break;
                case ResultOutcome.Defeat:
                    AudioManager.Instance?.PlayDefeatSFX();
                    break;
            }
        }

        private ResultOutcome ResolveOutcome(string title, string winner)
        {
            string normalizedTitle = string.IsNullOrEmpty(title) ? string.Empty : title.ToLowerInvariant();
            string normalizedWinner = string.IsNullOrEmpty(winner) ? string.Empty : winner.ToLowerInvariant();

            if (normalizedTitle.Contains("defeat") || normalizedTitle.Contains("game over"))
            {
                return ResultOutcome.Defeat;
            }

            if (normalizedTitle.Contains("draw") || normalizedWinner == "none")
            {
                return ResultOutcome.Draw;
            }

            if (normalizedTitle.Contains("victory") || normalizedTitle.Contains("wins"))
            {
                return ResultOutcome.Victory;
            }

            return ResultOutcome.Complete;
        }

        private int CalculateScore(ResultOutcome outcome, GameMode mode)
        {
            switch (outcome)
            {
                case ResultOutcome.Victory:
                    return 1200 + GetModeBonus(mode);
                case ResultOutcome.Draw:
                    return 550 + GetModeBonus(mode) / 2;
                case ResultOutcome.Defeat:
                    return 200;
                case ResultOutcome.Complete:
                    return 800;
                default:
                    return 0;
            }
        }

        private int GetModeBonus(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.AIBattle:
                    return 200;
                case GameMode.LocalVS:
                    return 250;
                default:
                    return 100;
            }
        }

        private int CalculateStarCount(ResultOutcome outcome)
        {
            switch (outcome)
            {
                case ResultOutcome.Victory:
                    return 3;
                case ResultOutcome.Draw:
                case ResultOutcome.Complete:
                    return 2;
                case ResultOutcome.Defeat:
                    return 1;
                default:
                    return 0;
            }
        }

        private Color ResolveAccentColor(ResultOutcome outcome)
        {
            switch (outcome)
            {
                case ResultOutcome.Victory:
                    return victoryColor;
                case ResultOutcome.Defeat:
                    return defeatColor;
                case ResultOutcome.Draw:
                    return drawColor;
                default:
                    return neutralColor;
            }
        }

        private string FormatOutcome(ResultOutcome outcome)
        {
            switch (outcome)
            {
                case ResultOutcome.Victory:
                    return "VICTORY";
                case ResultOutcome.Defeat:
                    return "DEFEAT";
                case ResultOutcome.Draw:
                    return "DRAW";
                case ResultOutcome.Complete:
                    return "ROUND COMPLETE";
                default:
                    return "NO RESULT";
            }
        }

        private string FormatIconText(ResultOutcome outcome)
        {
            switch (outcome)
            {
                case ResultOutcome.Victory:
                    return ":D";
                case ResultOutcome.Defeat:
                    return ":(";
                case ResultOutcome.Draw:
                    return ":|";
                case ResultOutcome.Complete:
                    return ":)";
                default:
                    return "?";
            }
        }

        private string FormatMoodText(ResultOutcome outcome)
        {
            switch (outcome)
            {
                case ResultOutcome.Victory:
                    return "Bubble champ energy!";
                case ResultOutcome.Defeat:
                    return "Pop, dust off, retry.";
                case ResultOutcome.Draw:
                    return "Both sides popped together.";
                case ResultOutcome.Complete:
                    return "Round wrapped up.";
                default:
                    return "Play a round to reveal the result.";
            }
        }

        private string FormatModeName(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.AIBattle:
                    return "AI Battle";
                case GameMode.LocalVS:
                    return "Local VS";
                default:
                    return "Single Player";
            }
        }

        private string FormatMapName(BattleMapType mapType)
        {
            switch (mapType)
            {
                case BattleMapType.OpenField:
                    return "Open Field";
                case BattleMapType.Maze:
                    return "Jelly Maze";
                default:
                    return "Candy Park";
            }
        }

        private void DrawHeroResultCard(ResultViewModel viewModel)
        {
            Rect rect = GUILayoutUtility.GetRect(520f, 130f, GUILayout.ExpandWidth(true));
            Rect cardRect = new Rect(rect.x + rect.width * 0.06f, rect.y, rect.width * 0.88f, rect.height);
            DrawPanel(cardRect, new Color(1f, 0.98f, 0.82f, 0.98f), viewModel.AccentColor, 24, 5);

            Rect iconCircle = new Rect(cardRect.x + 26f, cardRect.y + 24f, 82f, 82f);
            GUI.DrawTexture(iconCircle, GetCircleTexture(viewModel.AccentColor));
            GUI.Label(iconCircle, viewModel.IconText, resultIconStyle);

            GUI.Label(new Rect(cardRect.x + 128f, cardRect.y + 24f, cardRect.width - 154f, 38f), viewModel.OutcomeLabel, resultTitleStyle);
            GUI.Label(new Rect(cardRect.x + 128f, cardRect.y + 62f, cardRect.width - 154f, 28f), viewModel.MoodText, resultBodyStyle);
            GUI.Label(new Rect(cardRect.x + 128f, cardRect.y + 90f, cardRect.width - 154f, 28f), viewModel.Detail, resultBodyStyle);
            GUILayout.Space(12f);
        }

        private void DrawSummaryCards(ResultViewModel viewModel)
        {
            GUILayout.BeginHorizontal();
            DrawInfoCard("MODE", viewModel.ModeName, new Color(0.12f, 0.72f, 1f, 1f));
            GUILayout.Space(12f);
            DrawInfoCard("MAP", viewModel.MapName, new Color(0.48f, 0.9f, 0.34f, 1f));
            GUILayout.Space(12f);
            DrawInfoCard("WINNER", viewModel.Winner, viewModel.AccentColor);
            if (!string.IsNullOrEmpty(viewModel.MatchScoreLabel))
            {
                GUILayout.Space(12f);
                DrawInfoCard("VS SCORE", viewModel.MatchScoreLabel, new Color(0.52f, 0.9f, 0.35f, 1f));
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(14f);
        }

        private string FormatMatchScore(GameManager gameManager, GameMode mode)
        {
            if (gameManager == null || mode != GameMode.LocalVS)
            {
                return string.Empty;
            }

            return gameManager.LocalVsScoreLabel;
        }

        private void DrawInfoCard(string label, string value, Color accentColor)
        {
            Rect rect = GUILayoutUtility.GetRect(160f, 82f, GUILayout.ExpandWidth(true));
            DrawPanel(rect, new Color(1f, 0.95f, 0.72f, 0.97f), accentColor, 18, 3);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 12f, rect.width - 24f, 24f), label, cardTitleStyle);
            GUI.Label(new Rect(rect.x + 12f, rect.y + 40f, rect.width - 24f, 30f), value, cardValueStyle);
        }

        private void DrawRewardPanel(ResultViewModel viewModel)
        {
            Rect rect = GUILayoutUtility.GetRect(560f, 126f, GUILayout.ExpandWidth(true));
            Rect rewardRect = new Rect(rect.x + rect.width * 0.08f, rect.y, rect.width * 0.84f, rect.height);
            DrawPanel(rewardRect, new Color(1f, 0.98f, 0.86f, 0.98f), new Color(1f, 0.68f, 0.22f, 1f), 22, 4);

            GUI.Label(new Rect(rewardRect.x + 22f, rewardRect.y + 16f, 170f, 28f), "SCORE", cardTitleStyle);
            GUI.Label(new Rect(rewardRect.x + 22f, rewardRect.y + 44f, 170f, 54f), viewModel.ScoreLabel, scoreStyle);

            GUI.Label(new Rect(rewardRect.x + 218f, rewardRect.y + 16f, rewardRect.width - 240f, 26f), viewModel.RewardLabel, cardValueStyle);
            DrawStarSlots(new Rect(rewardRect.x + 220f, rewardRect.y + 52f, rewardRect.width - 250f, 50f), viewModel.StarCount, viewModel.AccentColor);
            GUILayout.Space(12f);
        }

        private void DrawStarSlots(Rect rect, int starCount, Color activeColor)
        {
            float slotSize = 46f;
            float gap = 10f;
            float startX = rect.x + (rect.width - slotSize * MaxStars - gap * (MaxStars - 1)) * 0.5f;
            for (int i = 0; i < MaxStars; i++)
            {
                bool isActive = i < starCount;
                Color fill = isActive ? activeColor : new Color(0.78f, 0.82f, 0.86f, 1f);
                Rect slotRect = new Rect(startX + i * (slotSize + gap), rect.y + 2f, slotSize, slotSize);
                DrawPanel(slotRect, fill, Color.white, 18, 2);
                GUI.Label(slotRect, isActive ? "*" : "-", starStyle);
            }
        }

        private void EnsureStyles()
        {
            if (cardTitleStyle != null)
            {
                return;
            }

            Color textPrimary = new Color(0.11f, 0.28f, 0.42f, 1f);
            Color textSecondary = new Color(0.23f, 0.45f, 0.55f, 1f);

            cardTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textSecondary }
            };

            cardValueStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = textPrimary }
            };

            resultIconStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 34,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };

            resultTitleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textPrimary }
            };

            resultBodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 15,
                wordWrap = true,
                normal = { textColor = textSecondary }
            };

            scoreStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 38,
                fontStyle = FontStyle.Bold,
                normal = { textColor = textPrimary }
            };

            starStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
        }

        private void DrawPanel(Rect rect, Color fill, Color border, int radius, int borderSize)
        {
            GUI.DrawTexture(
                new Rect(rect.x + 5f, rect.y + 7f, rect.width, rect.height),
                GetRoundedTexture(new Color(0.04f, 0.22f, 0.34f, 0.28f), new Color(0.04f, 0.22f, 0.34f, 0.28f), radius, 0));
            GUI.DrawTexture(rect, GetRoundedTexture(fill, border, radius, borderSize));
        }

        private Texture2D GetCircleTexture(Color color)
        {
            string key = ColorKey(color);
            if (CircleTextureCache.TryGetValue(key, out Texture2D texture))
            {
                return texture;
            }

            texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            float center = (TextureSize - 1) * 0.5f;
            float radius = center;
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = Mathf.Clamp01(radius - distance + 1f);
                    Color pixel = color;
                    pixel.a *= alpha;
                    texture.SetPixel(x, y, pixel);
                }
            }

            texture.Apply();
            CircleTextureCache[key] = texture;
            return texture;
        }

        private Texture2D GetRoundedTexture(Color fill, Color border, int radius, int borderSize)
        {
            string key = ColorKey(fill) + ColorKey(border) + radius + "_" + borderSize;
            if (RoundedTextureCache.TryGetValue(key, out Texture2D texture))
            {
                return texture;
            }

            texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    bool insideOuter = IsInsideRoundedRect(x, y, TextureSize, TextureSize, radius);
                    bool insideInner = borderSize <= 0 || IsInsideRoundedRect(
                        x - borderSize,
                        y - borderSize,
                        TextureSize - borderSize * 2,
                        TextureSize - borderSize * 2,
                        Mathf.Max(1, radius - borderSize));

                    Color pixel = Color.clear;
                    if (insideOuter)
                    {
                        pixel = insideInner ? fill : border;
                    }

                    texture.SetPixel(x, y, pixel);
                }
            }

            texture.Apply();
            RoundedTextureCache[key] = texture;
            return texture;
        }

        private bool IsInsideRoundedRect(int x, int y, int width, int height, int radius)
        {
            if (width <= 0 || height <= 0)
            {
                return false;
            }

            int clampedRadius = Mathf.Min(radius, Mathf.Min(width, height) / 2);
            int left = clampedRadius;
            int right = width - clampedRadius - 1;
            int bottom = clampedRadius;
            int top = height - clampedRadius - 1;

            int closestX = Mathf.Clamp(x, left, right);
            int closestY = Mathf.Clamp(y, bottom, top);
            int deltaX = x - closestX;
            int deltaY = y - closestY;
            return deltaX * deltaX + deltaY * deltaY <= clampedRadius * clampedRadius;
        }

        private string ColorKey(Color color)
        {
            return ColorUtility.ToHtmlStringRGBA(color);
        }
    }
}
