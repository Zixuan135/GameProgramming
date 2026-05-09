using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AssignmentSceneController : MonoBehaviour
{
    private const string EnemyPrefabPath = "Assignment/ChaserEnemy";
    private const string ProjectilePrefabPath = "Assignment/Player_Projectile";
    private const string PlayerHitEffectPath = "Assignment/PlayerHitEffect";
    private const string PlayerDeathEffectPath = "Assignment/PlayerDeathEffect";
    private const string VictoryEffectPath = "Assignment/VictoryEffect";
    private const string GameOverEffectPath = "Assignment/GameOverEffect";

    private readonly Color hudColor = new Color(0.92f, 0.97f, 1f, 1f);
    private readonly Color panelColor = new Color(0.06f, 0.1f, 0.18f, 0.9f);

    private GameObject enemyPrefab;
    private GameObject projectilePrefab;
    private GameObject playerHitEffect;
    private GameObject playerDeathEffect;
    private GameObject victoryEffect;
    private GameObject gameOverEffect;

    private GameObject playerObject;
    private Controller playerController;
    private ShootingController playerShooter;
    private Health playerHealth;
    private AssignmentPlayerFeedback playerFeedback;
    private AssignmentAudioManager audioManager;
    private SpriteRenderer playerSpriteRenderer;
    private CameraController cameraController;
    private GameManager gameManager;
    private AssignmentEnemyDirector enemyDirector;
    private Transform projectileHolder;

    private Canvas uiCanvas;
    private GameObject menuVisualsRoot;
    private GameObject mainMenuPanel;
    private GameObject instructionsPanel;
    private GameObject hudPanel;
    private GameObject gameOverPanel;
    private GameObject victoryPanel;
    private Text scoreText;
    private Text healthText;
    private Text progressText;
    private Text highScoreText;
    private Text timerText;
    private Text objectiveText;
    private Text powerupText;
    private Text statusText;
    private Text resultsSummaryText;
    private Text victorySummaryText;
    private Image damageFlashImage;
    private Coroutine statusMessageRoutine;
    private Coroutine healthFlashRoutine;
    private string failureReason = "Hull integrity reached zero.";

    private bool gameStarted;
    private float runTimer;
    private int lastScoreValue;
    private int lastPowerupWave = -1;

    private static Sprite whiteSprite;
    private static Font defaultFont;

    public AssignmentAudioManager AudioManager => audioManager;

    private void Awake()
    {
        LoadAssets();
        EnsureEventSystem();
        EnsurePlayer();
        EnsureGameManager();
        EnsureCamera();
        EnsureHelpers();
        BuildUI();
        SubscribeEvents();
        ShowMainMenu();
    }

    private void OnDestroy()
    {
        GameManager.ScoreChanged -= HandleScoreChanged;
        GameManager.ObjectiveProgressChanged -= HandleObjectiveProgressChanged;
        GameManager.GameOverTriggered -= HandleGameOver;
        GameManager.LevelClearedTriggered -= HandleLevelCleared;
    }

    private void Update()
    {
        if (gameStarted && gameManager != null && !gameManager.gameIsOver)
        {
            runTimer += Time.deltaTime;
        }

        UpdateHud();
        FadeDamageFlash();
    }

    public void StartGame()
    {
        GameManager.ResetScore();
        ClearSpawnedActors();
        ResetPlayer();

        gameStarted = true;
        runTimer = 0f;
        lastScoreValue = 0;
        lastPowerupWave = -1;
        failureReason = "Hull integrity reached zero.";
        gameManager.gameIsOver = false;
        playerObject.SetActive(true);
        enemyDirector.ResetDirector();
        audioManager.PlayGameMusic();
        SetMenuVisualsActive(false);

        mainMenuPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        hudPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        victoryPanel.SetActive(false);

        ShowStatusMessage("Objective live: destroy 12 drones and stay intact.", hudColor, 2f);
    }

    public void ShowInstructions()
    {
        SetMenuVisualsActive(true);
        mainMenuPanel.SetActive(false);
        instructionsPanel.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        Debug.Log("Exit button pressed.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void TriggerDamageFlash()
    {
        if (damageFlashImage != null)
        {
            Color color = damageFlashImage.color;
            color.a = 0.35f;
            damageFlashImage.color = color;
        }

        if (healthFlashRoutine != null)
        {
            StopCoroutine(healthFlashRoutine);
        }
        healthFlashRoutine = StartCoroutine(FlashHealthText());
    }

    public void ShowStatusMessage(string message, Color color, float duration)
    {
        if (statusText == null)
        {
            return;
        }

        if (statusMessageRoutine != null)
        {
            StopCoroutine(statusMessageRoutine);
        }
        statusText.text = message;
        statusText.color = color;
        statusMessageRoutine = StartCoroutine(ClearStatusMessage(duration));
    }

    private IEnumerator ClearStatusMessage(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (statusText != null)
        {
            statusText.text = string.Empty;
        }
    }

    private void LoadAssets()
    {
        enemyPrefab = Resources.Load<GameObject>(EnemyPrefabPath);
        projectilePrefab = Resources.Load<GameObject>(ProjectilePrefabPath);
        playerHitEffect = Resources.Load<GameObject>(PlayerHitEffectPath);
        playerDeathEffect = Resources.Load<GameObject>(PlayerDeathEffectPath);
        victoryEffect = Resources.Load<GameObject>(VictoryEffectPath);
        gameOverEffect = Resources.Load<GameObject>(GameOverEffectPath);

        if (enemyPrefab == null || projectilePrefab == null)
        {
            Debug.LogError("AssignmentSceneController could not load required prefabs from Resources/Assignment.");
        }
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    private void EnsureGameManager()
    {
        GameObject managerObject = new GameObject("GameManager");
        gameManager = managerObject.AddComponent<GameManager>();
        gameManager.player = playerObject;
        gameManager.gameIsWinnable = true;
        gameManager.enemiesToDefeat = 12;
        gameManager.printDebugOfWinnableStatus = false;
        gameManager.victoryEffect = victoryEffect;
        gameManager.gameOverEffect = gameOverEffect;
    }

    private void EnsurePlayer()
    {
        playerObject = GameObject.Find("Player Sprites_0");
        if (playerObject == null)
        {
            playerObject = new GameObject("Player");
            playerObject.AddComponent<SpriteRenderer>();
        }

        playerObject.name = "Player";
        playerObject.tag = "Player";
        playerObject.transform.position = Vector3.zero;

        playerSpriteRenderer = playerObject.GetComponent<SpriteRenderer>();
        CircleCollider2D collider = playerObject.GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = playerObject.AddComponent<CircleCollider2D>();
        }
        collider.isTrigger = false;
        collider.radius = 0.45f;

        Rigidbody2D rigidbody2D = playerObject.GetComponent<Rigidbody2D>();
        if (rigidbody2D == null)
        {
            rigidbody2D = playerObject.AddComponent<Rigidbody2D>();
        }
        rigidbody2D.gravityScale = 0f;
        rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        playerController = playerObject.GetComponent<Controller>();
        if (playerController == null)
        {
            playerController = playerObject.AddComponent<Controller>();
        }
        playerController.moveSpeed = 5.8f;
        playerController.movementMode = Controller.MovementModes.FreeRoam;
        playerController.aimMode = Controller.AimModes.AimTowardsMouse;
        playerController.moveAction = CreateMoveAction();
        playerController.lookAction = CreateLookAction();
        playerController.moveAction.Enable();
        playerController.lookAction.Enable();

        playerShooter = playerObject.GetComponent<ShootingController>();
        if (playerShooter == null)
        {
            playerShooter = playerObject.AddComponent<ShootingController>();
        }
        playerShooter.isPlayerControlled = true;
        playerShooter.projectilePrefab = projectilePrefab;
        playerShooter.fireRate = 0.22f;
        playerShooter.projectileSpread = 1.1f;
        playerShooter.fireAction = CreateFireAction();
        playerShooter.fireAction.Enable();

        playerHealth = playerObject.GetComponent<Health>();
        if (playerHealth == null)
        {
            playerHealth = playerObject.AddComponent<Health>();
        }
        playerHealth.teamId = 0;
        playerHealth.defaultHealth = 5;
        playerHealth.maximumHealth = 5;
        playerHealth.currentHealth = 5;
        playerHealth.invincibilityTime = 0.8f;
        playerHealth.isAlwaysInvincible = false;
        playerHealth.useLives = false;
        playerHealth.hitEffect = playerHitEffect;
        playerHealth.deathEffect = playerDeathEffect;

        playerFeedback = playerObject.GetComponent<AssignmentPlayerFeedback>();
        if (playerFeedback == null)
        {
            playerFeedback = playerObject.AddComponent<AssignmentPlayerFeedback>();
        }
        playerFeedback.sceneController = this;
        playerFeedback.playerController = playerController;
        playerFeedback.shootingController = playerShooter;
        playerFeedback.health = playerHealth;
        playerFeedback.spriteRenderer = playerSpriteRenderer;
        playerFeedback.RefreshBindings();
    }

    private void EnsureCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 6.5f;
        mainCamera.backgroundColor = new Color(0.03f, 0.05f, 0.09f);

        cameraController = mainCamera.GetComponent<CameraController>();
        if (cameraController == null)
        {
            cameraController = mainCamera.gameObject.AddComponent<CameraController>();
        }
        cameraController.target = playerObject.transform;
        cameraController.cameraMovementStyle = CameraController.CameraStyles.Overhead;
        cameraController.freeCameraMouseTracking = 0.1f;
        cameraController.maxDistanceFromTarget = 1f;
        cameraController.cameraZCoordinate = -10f;
        cameraController.lookAction = CreateLookAction();
        cameraController.lookAction.Enable();
    }

    private void EnsureHelpers()
    {
        GameObject audioManagerObject = new GameObject("AudioManager");
        audioManager = audioManagerObject.AddComponent<AssignmentAudioManager>();

        GameObject projectileHolderObject = new GameObject("ProjectileHolder");
        projectileHolder = projectileHolderObject.transform;

        CreateStarfield();

        GameObject directorObject = new GameObject("EnemyDirector");
        enemyDirector = directorObject.AddComponent<AssignmentEnemyDirector>();
        enemyDirector.enemyPrefab = enemyPrefab;
        enemyDirector.target = playerObject.transform;
        enemyDirector.projectileHolder = projectileHolder;
        enemyDirector.gameManager = gameManager;
        enemyDirector.enabled = true;

        playerShooter.projectileHolder = projectileHolder;
        gameManager.player = playerObject;
    }

    private void BuildUI()
    {
        GameObject canvasObject = new GameObject("MainMenuCanvas");
        uiCanvas = canvasObject.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        menuVisualsRoot = new GameObject("MenuVisualsRoot");
        menuVisualsRoot.transform.SetParent(canvasObject.transform, false);
        BuildBackgroundDecor(menuVisualsRoot.transform);
        mainMenuPanel = CreatePanel("MainMenuPanel", canvasObject.transform, new Vector2(640f, 620f), panelColor);
        instructionsPanel = CreatePanel("InstructionsPanel", canvasObject.transform, new Vector2(760f, 640f), panelColor);
        hudPanel = CreatePanel("HUDCanvas", canvasObject.transform, new Vector2(0f, 0f), Color.clear);
        gameOverPanel = CreatePanel("GameOverPanel", canvasObject.transform, new Vector2(680f, 400f), panelColor);
        victoryPanel = CreatePanel("VictoryPanel", canvasObject.transform, new Vector2(680f, 400f), panelColor);

        BuildMainMenu();
        BuildInstructions();
        BuildHud();
        BuildResultsPanels();
    }

    private void BuildMainMenu()
    {
        CreatePanelAccent("MainMenuTitleGlow", mainMenuPanel.transform, new Vector2(0f, 226f), new Vector2(520f, 84f), new Color(0.18f, 0.38f, 0.62f, 0.22f));
        CreatePanelAccent("MainMenuAccentTop", mainMenuPanel.transform, new Vector2(0f, 286f), new Vector2(480f, 6f), new Color(0.38f, 0.74f, 1f, 0.9f));
        CreatePanelAccent("MainMenuAccentBottom", mainMenuPanel.transform, new Vector2(0f, -286f), new Vector2(480f, 6f), new Color(0.38f, 0.74f, 1f, 0.45f));
        CreateText("Title", mainMenuPanel.transform, "SPACE BLASTER", 56, TextAnchor.MiddleCenter, new Vector2(0f, 230f), new Vector2(560f, 86f), Color.white);
        CreateText("Subtitle", mainMenuPanel.transform, "A refined arcade survival assignment build", 22, TextAnchor.MiddleCenter, new Vector2(0f, 180f), new Vector2(540f, 42f), hudColor);
        CreateText("MenuBrief", mainMenuPanel.transform, "Destroy drones, survive incoming damage,\nand use Overdrive pickups to keep control of the arena.", 24, TextAnchor.MiddleCenter, new Vector2(0f, 108f), new Vector2(560f, 86f), new Color(0.82f, 0.9f, 1f));
        CreateButton("NewGameButton", mainMenuPanel.transform, "Start Mission", new Vector2(0f, 10f), new Vector2(320f, 64f), StartGame);
        CreateButton("InstructionsButton", mainMenuPanel.transform, "Instructions", new Vector2(0f, -72f), new Vector2(320f, 64f), ShowInstructions);
        CreateButton("ExitButton", mainMenuPanel.transform, "Exit", new Vector2(0f, -154f), new Vector2(320f, 64f), ExitGame);
        CreateText("MenuHint", mainMenuPanel.transform, "Assignment objective: destroy 12 enemy drones.\nBlue Overdrive boosts increase movement and fire rate for a short time.", 22, TextAnchor.MiddleCenter, new Vector2(0f, -258f), new Vector2(580f, 96f), hudColor);
    }

    private void BuildInstructions()
    {
        CreatePanelAccent("InstructionsGlow", instructionsPanel.transform, new Vector2(0f, 245f), new Vector2(520f, 72f), new Color(0.18f, 0.38f, 0.62f, 0.18f));
        CreatePanelAccent("InstructionsAccentTop", instructionsPanel.transform, new Vector2(0f, 294f), new Vector2(560f, 5f), new Color(0.38f, 0.74f, 1f, 0.8f));
        CreateText("InstructionsTitle", instructionsPanel.transform, "Mission Briefing", 46, TextAnchor.MiddleCenter, new Vector2(0f, 252f), new Vector2(520f, 64f), Color.white);
        CreateText(
            "InstructionsBody",
            instructionsPanel.transform,
            "Objective: Destroy 12 enemy drones.\n\nControls: Move with WASD or Arrow Keys.\nAim with the mouse.\nFire with Left Click or Space.\n\nPower-up: Blue Overdrive boosts movement and fire rate.\n\nWin: Reach the drone target.\nLose: Let your hull drop to zero.",
            25,
            TextAnchor.UpperLeft,
            new Vector2(0f, 56f),
            new Vector2(620f, 340f),
            hudColor);
        CreateText("InstructionsTip", instructionsPanel.transform, "Tip: blue Overdrive pickups temporarily boost your movement and fire rate.", 22, TextAnchor.MiddleCenter, new Vector2(0f, -168f), new Vector2(620f, 54f), new Color(0.45f, 0.9f, 1f));
        CreateButton("BackButton", instructionsPanel.transform, "Back", new Vector2(0f, -256f), new Vector2(240f, 58f), ShowMainMenu);
    }

    private void BuildHud()
    {
        RectTransform hudRect = hudPanel.GetComponent<RectTransform>();
        hudRect.anchorMin = Vector2.zero;
        hudRect.anchorMax = Vector2.one;
        hudRect.offsetMin = Vector2.zero;
        hudRect.offsetMax = Vector2.zero;

        CreateHudCard("LeftHudCard", hudPanel.transform, new Vector2(-690f, 450f), new Vector2(360f, 142f), new Color(0.04f, 0.08f, 0.14f, 0.78f));
        CreateHudCard("RightHudCard", hudPanel.transform, new Vector2(690f, 450f), new Vector2(360f, 142f), new Color(0.04f, 0.08f, 0.14f, 0.78f));
        CreateHudCard("ObjectiveHudCard", hudPanel.transform, new Vector2(0f, 480f), new Vector2(700f, 92f), new Color(0.04f, 0.08f, 0.14f, 0.7f));

        scoreText = CreateText("ScoreText", hudPanel.transform, "Score: 0", 28, TextAnchor.MiddleLeft, new Vector2(-760f, 488f), new Vector2(300f, 40f), Color.white);
        healthText = CreateText("HealthText", hudPanel.transform, "Hull: 5/5", 28, TextAnchor.MiddleLeft, new Vector2(-760f, 448f), new Vector2(300f, 40f), hudColor);
        progressText = CreateText("ProgressText", hudPanel.transform, "Progress: 0/12", 26, TextAnchor.MiddleLeft, new Vector2(-760f, 410f), new Vector2(320f, 36f), hudColor);
        highScoreText = CreateText("HighScoreText", hudPanel.transform, "High Score: 0", 28, TextAnchor.MiddleRight, new Vector2(760f, 488f), new Vector2(320f, 40f), hudColor);
        timerText = CreateText("TimerText", hudPanel.transform, "Time: 0.0s", 28, TextAnchor.MiddleRight, new Vector2(760f, 448f), new Vector2(320f, 40f), hudColor);
        objectiveText = CreateText("ObjectiveText", hudPanel.transform, "Objective: destroy 12 drones and avoid enemy collisions!", 28, TextAnchor.MiddleCenter, new Vector2(0f, 496f), new Vector2(660f, 36f), Color.white);
        powerupText = CreateText("PowerupText", hudPanel.transform, string.Empty, 24, TextAnchor.MiddleCenter, new Vector2(0f, 454f), new Vector2(480f, 34f), new Color(0.45f, 0.9f, 1f));
        statusText = CreateText("StatusText", hudPanel.transform, string.Empty, 30, TextAnchor.MiddleCenter, new Vector2(0f, -482f), new Vector2(760f, 54f), Color.white);

        GameObject damageObject = new GameObject("DamageFlash");
        damageObject.transform.SetParent(hudPanel.transform, false);
        damageFlashImage = damageObject.AddComponent<Image>();
        damageFlashImage.color = new Color(1f, 0f, 0f, 0f);
        RectTransform damageRect = damageObject.GetComponent<RectTransform>();
        damageRect.anchorMin = Vector2.zero;
        damageRect.anchorMax = Vector2.one;
        damageRect.offsetMin = Vector2.zero;
        damageRect.offsetMax = Vector2.zero;
    }

    private void BuildResultsPanels()
    {
        CreateText("GameOverTitle", gameOverPanel.transform, "Mission Failed", 46, TextAnchor.MiddleCenter, new Vector2(0f, 124f), new Vector2(520f, 60f), Color.white);
        resultsSummaryText = CreateText("ResultsSummary", gameOverPanel.transform, string.Empty, 26, TextAnchor.MiddleCenter, new Vector2(0f, 30f), new Vector2(560f, 80f), hudColor);
        CreateText("GameOverHint", gameOverPanel.transform, "Restart to try again, or return to the menu.", 22, TextAnchor.MiddleCenter, new Vector2(0f, -30f), new Vector2(520f, 40f), new Color(0.88f, 0.9f, 0.96f));
        CreateButton("RestartButton", gameOverPanel.transform, "Restart", new Vector2(-140f, -106f), new Vector2(220f, 58f), RestartGame);
        CreateButton("ReturnToMenuButton", gameOverPanel.transform, "Return To Menu", new Vector2(140f, -106f), new Vector2(260f, 58f), ReturnToMainMenu);

        CreateText("VictoryTitle", victoryPanel.transform, "Mission Complete", 46, TextAnchor.MiddleCenter, new Vector2(0f, 124f), new Vector2(520f, 60f), Color.white);
        victorySummaryText = CreateText("VictorySummary", victoryPanel.transform, string.Empty, 26, TextAnchor.MiddleCenter, new Vector2(0f, 28f), new Vector2(560f, 92f), hudColor);
        CreateText("VictoryHint", victoryPanel.transform, "You cleared the objective. Restart for a higher score or return to menu.", 22, TextAnchor.MiddleCenter, new Vector2(0f, -34f), new Vector2(560f, 42f), new Color(0.88f, 0.9f, 0.96f));
        CreateButton("VictoryRestartButton", victoryPanel.transform, "Restart", new Vector2(-140f, -106f), new Vector2(220f, 58f), RestartGame);
        CreateButton("VictoryMenuButton", victoryPanel.transform, "Return To Menu", new Vector2(140f, -106f), new Vector2(260f, 58f), ReturnToMainMenu);
    }

    private void SubscribeEvents()
    {
        GameManager.ScoreChanged += HandleScoreChanged;
        GameManager.ObjectiveProgressChanged += HandleObjectiveProgressChanged;
        GameManager.GameOverTriggered += HandleGameOver;
        GameManager.LevelClearedTriggered += HandleLevelCleared;
    }

    private void HandleScoreChanged(int score, int highScore)
    {
        if (gameStarted && score > lastScoreValue)
        {
            ShowStatusMessage("Score +" + (score - lastScoreValue), new Color(1f, 0.9f, 0.45f), 0.8f);
        }

        lastScoreValue = score;
        StartCoroutine(FlashScoreText());
    }

    private IEnumerator FlashScoreText()
    {
        if (scoreText == null)
        {
            yield break;
        }

        scoreText.color = new Color(1f, 0.95f, 0.4f);
        yield return new WaitForSeconds(0.2f);
        scoreText.color = Color.white;
    }

    private void HandleObjectiveProgressChanged(int defeated, int target)
    {
        int pickupWave = defeated / 4;
        if (gameStarted && defeated > 0 && defeated % 4 == 0 && pickupWave != lastPowerupWave)
        {
            lastPowerupWave = pickupWave;
            SpawnOverdrivePickup();
        }
    }

    private void HandleGameOver()
    {
        enemyDirector.StopSpawning();
        SetMenuVisualsActive(true);
        hudPanel.SetActive(false);
        gameOverPanel.SetActive(true);
        resultsSummaryText.text = "Failure reason: " + failureReason + "\nFinal Score: " + GameManager.score + "\nEnemies Defeated: " + gameManager.EnemiesDefeated;
        audioManager.PlayGameOver();
    }

    private void HandleLevelCleared()
    {
        enemyDirector.StopSpawning();
        SetMenuVisualsActive(true);
        hudPanel.SetActive(false);
        victoryPanel.SetActive(true);
        if (victorySummaryText != null)
        {
            victorySummaryText.text = "Objective cleared in " + runTimer.ToString("F1") + "s\nFinal Score: " + GameManager.score + "\nDrones Destroyed: " + gameManager.EnemiesDefeated;
        }
        audioManager.PlayWin();
    }

    private void ShowMainMenu()
    {
        gameStarted = false;
        if (enemyDirector != null)
        {
            enemyDirector.StopSpawning();
        }

        if (playerObject != null)
        {
            playerObject.SetActive(false);
        }

        ClearSpawnedActors();
        SetMenuVisualsActive(true);
        mainMenuPanel.SetActive(true);
        instructionsPanel.SetActive(false);
        hudPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        victoryPanel.SetActive(false);
        audioManager.PlayMenuMusic();
    }

    private void UpdateHud()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + GameManager.score;
        }

        if (highScoreText != null && gameManager != null)
        {
            highScoreText.text = "High Score: " + gameManager.highScore;
        }

        if (playerHealth != null && healthText != null)
        {
            healthText.text = "Hull: " + playerHealth.currentHealth + "/" + playerHealth.maximumHealth;
        }

        if (progressText != null && gameManager != null)
        {
            progressText.text = "Progress: " + gameManager.EnemiesDefeated + "/" + gameManager.enemiesToDefeat;
        }

        if (timerText != null)
        {
            timerText.text = "Time: " + runTimer.ToString("F1") + "s";
        }

        if (powerupText != null)
        {
            if (playerFeedback != null && playerFeedback.OverdriveTimeRemaining > 0f)
            {
                powerupText.text = "Speed Boost: Active (" + playerFeedback.OverdriveTimeRemaining.ToString("F1") + "s)";
            }
            else
            {
                powerupText.text = string.Empty;
            }
        }
    }

    private void FadeDamageFlash()
    {
        if (damageFlashImage == null)
        {
            return;
        }

        Color color = damageFlashImage.color;
        color.a = Mathf.MoveTowards(color.a, 0f, Time.deltaTime * 1.8f);
        damageFlashImage.color = color;
    }

    private void SpawnOverdrivePickup()
    {
        Vector2 spawnOffset = Random.insideUnitCircle.normalized * Random.Range(2.5f, 4.5f);
        GameObject pickup = new GameObject("OverdrivePickup");
        pickup.transform.position = playerObject.transform.position + (Vector3)spawnOffset;

        SpriteRenderer renderer = pickup.AddComponent<SpriteRenderer>();
        renderer.sprite = GetWhiteSprite();
        renderer.drawMode = SpriteDrawMode.Simple;
        renderer.color = new Color(0.35f, 0.9f, 1f);
        pickup.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

        CircleCollider2D collider = pickup.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;

        AssignmentPickup assignmentPickup = pickup.AddComponent<AssignmentPickup>();
        assignmentPickup.playerFeedback = playerFeedback;
        assignmentPickup.sceneController = this;

        ShowStatusMessage("Overdrive pickup spawned", new Color(0.45f, 0.9f, 1f), 1.1f);
    }

    private void ClearSpawnedActors()
    {
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            Destroy(enemy.gameObject);
        }

        foreach (Projectile projectile in FindObjectsOfType<Projectile>())
        {
            Destroy(projectile.gameObject);
        }

        foreach (AssignmentPickup pickup in FindObjectsOfType<AssignmentPickup>())
        {
            Destroy(pickup.gameObject);
        }
    }

    private void ResetPlayer()
    {
        playerObject.transform.position = Vector3.zero;
        playerObject.transform.rotation = Quaternion.identity;
        playerHealth.currentHealth = playerHealth.maximumHealth;
        playerSpriteRenderer.color = Color.white;
        playerFeedback.RefreshBindings();
        playerObject.SetActive(true);
    }

    public void RegisterPickupCollected()
    {
        audioManager.PlayPickup();
        GameManager.AddScore(15);
        ShowStatusMessage("Overdrive collected: speed and fire rate boosted.", new Color(0.45f, 0.9f, 1f), 1.35f);
    }

    public void SetFailureReason(string reason)
    {
        if (!string.IsNullOrWhiteSpace(reason))
        {
            failureReason = reason;
        }
    }

    private GameObject CreatePanel(string name, Transform parent, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = Vector2.zero;
        return panel;
    }

    private Text CreateText(string name, Transform parent, string content, int fontSize, TextAnchor alignment, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.font = GetDefaultFont();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        Outline outline = textObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.55f);
        outline.effectDistance = new Vector2(1f, -1f);
        RectTransform rectTransform = text.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;
        return text;
    }

    private Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.18f, 0.27f, 0.4f, 1f);
        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.18f, 0.27f, 0.4f, 1f);
        colors.highlightedColor = new Color(0.28f, 0.42f, 0.62f, 1f);
        colors.pressedColor = new Color(0.12f, 0.19f, 0.3f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        button.onClick.AddListener(action);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;

        AssignmentButtonFeedback feedback = buttonObject.AddComponent<AssignmentButtonFeedback>();
        feedback.targetImage = image;
        feedback.audioManager = audioManager;

        Text labelText = CreateText("Label", buttonObject.transform, label, 24, TextAnchor.MiddleCenter, Vector2.zero, size, Color.white);
        labelText.raycastTarget = false;
        return button;
    }

    private void BuildBackgroundDecor(Transform parent)
    {
        CreateFullScreenBackdrop(parent, new Color(0.015f, 0.02f, 0.05f, 1f));
        CreateDecorationCircle("DecorA", parent, new Vector2(-780f, 310f), new Vector2(250f, 250f), new Color(0.2f, 0.45f, 0.72f, 0.15f));
        CreateDecorationCircle("DecorB", parent, new Vector2(820f, -260f), new Vector2(320f, 320f), new Color(0.16f, 0.62f, 0.82f, 0.1f));
        CreateDecorationCircle("DecorC", parent, new Vector2(620f, 260f), new Vector2(120f, 120f), new Color(0.68f, 0.82f, 1f, 0.12f));
        CreateDecorationCircle("DecorD", parent, new Vector2(-640f, -290f), new Vector2(150f, 150f), new Color(0.5f, 0.72f, 1f, 0.08f));
        CreateDecorationCircle("DecorNebulaA", parent, new Vector2(-520f, 180f), new Vector2(520f, 320f), new Color(0.16f, 0.32f, 0.6f, 0.12f));
        CreateDecorationCircle("DecorNebulaB", parent, new Vector2(540f, -140f), new Vector2(620f, 360f), new Color(0.1f, 0.48f, 0.56f, 0.08f));
        CreateDecorationCircle("DecorNebulaC", parent, new Vector2(0f, -310f), new Vector2(860f, 220f), new Color(0.12f, 0.18f, 0.34f, 0.1f));
        CreateMenuStars(parent, 90);
    }

    private void CreateStarfield()
    {
        if (cameraController == null)
        {
            return;
        }

        GameObject starfieldRoot = new GameObject("StarfieldBackground");
        starfieldRoot.transform.SetParent(cameraController.transform, false);
        starfieldRoot.transform.localPosition = new Vector3(0f, 0f, 15f);

        CreateWorldNebula("NebulaBackA", starfieldRoot.transform, new Vector3(-3.8f, 2.2f, 0f), new Vector3(7.2f, 4.8f, 1f), new Color(0.12f, 0.24f, 0.46f, 0.12f), -110);
        CreateWorldNebula("NebulaBackB", starfieldRoot.transform, new Vector3(4.1f, -1.5f, 0f), new Vector3(6.4f, 4.2f, 1f), new Color(0.08f, 0.42f, 0.52f, 0.08f), -109);
        CreateWorldNebula("NebulaBackC", starfieldRoot.transform, new Vector3(0.6f, 0.4f, 0f), new Vector3(9.5f, 5.8f, 1f), new Color(0.22f, 0.18f, 0.4f, 0.05f), -111);

        for (int index = 0; index < 95; index++)
        {
            GameObject star = new GameObject("Star_" + index);
            star.transform.SetParent(starfieldRoot.transform, false);
            star.transform.localPosition = new Vector3(Random.Range(-11f, 11f), Random.Range(-7f, 7f), 0f);

            SpriteRenderer renderer = star.AddComponent<SpriteRenderer>();
            renderer.sprite = GetWhiteSprite();
            renderer.sortingOrder = -100 + Random.Range(-3, 3);

            float size = Random.Range(0.02f, 0.12f);
            star.transform.localScale = new Vector3(size, size, 1f);

            float brightness = Random.Range(0.45f, 0.95f);
            renderer.color = new Color(brightness, brightness, brightness, Random.Range(0.45f, 0.9f));

            AssignmentStarfieldStar starBehaviour = star.AddComponent<AssignmentStarfieldStar>();
            starBehaviour.spriteRenderer = renderer;
            starBehaviour.minAlpha = Random.Range(0.2f, 0.5f);
            starBehaviour.maxAlpha = Random.Range(0.65f, 1f);
            starBehaviour.pulseSpeed = Random.Range(0.6f, 1.8f);
            starBehaviour.driftSpeed = Random.Range(0.2f, 0.6f);
            starBehaviour.driftDirection = new Vector3(Random.Range(-1f, 1f), Random.Range(-0.4f, 0.4f), 0f);
        }
    }

    private void CreateFullScreenBackdrop(Transform parent, Color color)
    {
        GameObject backdrop = new GameObject("MenuBackdrop");
        backdrop.transform.SetParent(parent, false);
        Image image = backdrop.AddComponent<Image>();
        image.color = color;
        RectTransform rectTransform = backdrop.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private void SetMenuVisualsActive(bool active)
    {
        if (menuVisualsRoot != null)
        {
            menuVisualsRoot.SetActive(active);
        }
    }

    private void CreateDecorationCircle(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject decor = new GameObject(name);
        decor.transform.SetParent(parent, false);
        Image image = decor.AddComponent<Image>();
        image.color = color;
        RectTransform rectTransform = decor.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;
    }

    private void CreateMenuStars(Transform parent, int count)
    {
        GameObject starRoot = new GameObject("MenuStarLayer");
        starRoot.transform.SetParent(parent, false);

        for (int index = 0; index < count; index++)
        {
            GameObject star = new GameObject("MenuStar_" + index);
            star.transform.SetParent(starRoot.transform, false);
            Image image = star.AddComponent<Image>();

            float brightness = Random.Range(0.65f, 1f);
            image.color = new Color(brightness, brightness, brightness, Random.Range(0.35f, 0.95f));

            RectTransform rectTransform = star.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(Random.Range(-960f, 960f), Random.Range(-540f, 540f));

            float size = Random.Range(2f, 6f);
            rectTransform.sizeDelta = new Vector2(size, size);
        }
    }

    private void CreateWorldNebula(string name, Transform parent, Vector3 localPosition, Vector3 scale, Color color, int sortingOrder)
    {
        GameObject nebula = new GameObject(name);
        nebula.transform.SetParent(parent, false);
        nebula.transform.localPosition = localPosition;
        nebula.transform.localScale = scale;

        SpriteRenderer renderer = nebula.AddComponent<SpriteRenderer>();
        renderer.sprite = GetWhiteSprite();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
    }

    private void CreatePanelAccent(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject accent = new GameObject(name);
        accent.transform.SetParent(parent, false);
        Image image = accent.AddComponent<Image>();
        image.color = color;
        RectTransform rectTransform = accent.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;
    }

    private IEnumerator FlashHealthText()
    {
        if (healthText == null)
        {
            yield break;
        }

        healthText.color = new Color(1f, 0.45f, 0.45f, 1f);
        healthText.transform.localScale = new Vector3(1.08f, 1.08f, 1f);
        yield return new WaitForSeconds(0.2f);
        healthText.color = hudColor;
        healthText.transform.localScale = Vector3.one;
    }

    private GameObject CreateHudCard(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject card = new GameObject(name);
        card.transform.SetParent(parent, false);
        Image image = card.AddComponent<Image>();
        image.color = color;
        RectTransform rectTransform = card.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;
        return card;
    }

    private InputAction CreateMoveAction()
    {
        InputAction action = new InputAction("Move");
        action.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        action.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");
        return action;
    }

    private InputAction CreateLookAction()
    {
        return new InputAction("Look", binding: "<Mouse>/position");
    }

    private InputAction CreateFireAction()
    {
        InputAction action = new InputAction("Fire");
        action.AddBinding("<Mouse>/leftButton");
        action.AddBinding("<Keyboard>/space");
        return action;
    }

    private static Font GetDefaultFont()
    {
        if (defaultFont == null)
        {
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        return defaultFont;
    }

    private static Sprite GetWhiteSprite()
    {
        if (whiteSprite == null)
        {
            whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), new Vector2(0.5f, 0.5f), 100f);
        }
        return whiteSprite;
    }
}
