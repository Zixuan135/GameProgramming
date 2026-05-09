using System.Collections;
using UnityEngine;

public class AssignmentPlayerFeedback : MonoBehaviour
{
    [HideInInspector] public AssignmentSceneController sceneController;
    [HideInInspector] public Controller playerController;
    [HideInInspector] public ShootingController shootingController;
    [HideInInspector] public Health health;
    [HideInInspector] public SpriteRenderer spriteRenderer;

    public float OverdriveTimeRemaining { get; private set; }

    private float baseMoveSpeed;
    private float baseFireRate;
    private Coroutine damageFlashRoutine;

    private void Awake()
    {
        CacheDefaults();
    }

    private void OnEnable()
    {
        TryBindHealthEvents();
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Damaged -= HandleDamaged;
            health.Died -= HandleDied;
        }
    }

    private void Update()
    {
        if (playerController == null || shootingController == null || spriteRenderer == null)
        {
            return;
        }

        if (OverdriveTimeRemaining > 0f)
        {
            OverdriveTimeRemaining -= Time.deltaTime;
            float pulse = 0.7f + Mathf.PingPong(Time.time * 2f, 0.3f);
            spriteRenderer.color = new Color(0.55f, pulse, 1f, 1f);

            if (OverdriveTimeRemaining <= 0f)
            {
                RestoreDefaults();
                if (sceneController != null)
                {
                    sceneController.ShowStatusMessage("Overdrive offline", new Color(0.8f, 0.9f, 1f), 1.2f);
                }
            }
        }
    }

    public void RefreshBindings()
    {
        CacheDefaults();
        TryBindHealthEvents();
    }

    public void ApplyOverdrive(float duration, float moveSpeedMultiplier, float fireRateMultiplier)
    {
        OverdriveTimeRemaining = Mathf.Max(OverdriveTimeRemaining, duration);
        playerController.moveSpeed = baseMoveSpeed * moveSpeedMultiplier;
        shootingController.fireRate = baseFireRate * fireRateMultiplier;

        if (sceneController != null)
        {
            if (sceneController.AudioManager != null)
            {
                sceneController.AudioManager.PlayPowerUp();
            }
            sceneController.ShowStatusMessage("Overdrive active", new Color(0.45f, 0.9f, 1f), 1.5f);
        }
    }

    private void HandleDamaged(Health damagedHealth, int damageAmount)
    {
        if (sceneController != null)
        {
            if (sceneController.AudioManager != null)
            {
                sceneController.AudioManager.PlayHit();
            }
            sceneController.TriggerDamageFlash();
            sceneController.ShowStatusMessage("Hull hit!", new Color(1f, 0.45f, 0.45f), 0.8f);
            sceneController.SetFailureReason("The ship took too much direct damage.");
        }

        if (damageFlashRoutine != null)
        {
            StopCoroutine(damageFlashRoutine);
        }
        damageFlashRoutine = StartCoroutine(DamageFlash());
    }

    private void HandleDied(Health deadHealth)
    {
        if (sceneController != null)
        {
            sceneController.SetFailureReason("Hull integrity reached zero.");
            sceneController.ShowStatusMessage("Ship destroyed", new Color(1f, 0.4f, 0.4f), 1.2f);
        }
    }

    private IEnumerator DamageFlash()
    {
        spriteRenderer.color = new Color(1f, 0.35f, 0.35f, 1f);
        yield return new WaitForSeconds(0.15f);
        if (OverdriveTimeRemaining <= 0f)
        {
            spriteRenderer.color = Color.white;
        }
    }

    private void CacheDefaults()
    {
        if (playerController != null && baseMoveSpeed <= 0f)
        {
            baseMoveSpeed = playerController.moveSpeed;
        }

        if (shootingController != null && baseFireRate <= 0f)
        {
            baseFireRate = shootingController.fireRate;
        }
    }

    private void TryBindHealthEvents()
    {
        CacheDefaults();
        if (health != null)
        {
            health.Damaged -= HandleDamaged;
            health.Died -= HandleDied;
            health.Damaged += HandleDamaged;
            health.Died += HandleDied;
        }
    }

    private void RestoreDefaults()
    {
        OverdriveTimeRemaining = 0f;
        playerController.moveSpeed = baseMoveSpeed;
        shootingController.fireRate = baseFireRate;
        spriteRenderer.color = Color.white;
    }
}
