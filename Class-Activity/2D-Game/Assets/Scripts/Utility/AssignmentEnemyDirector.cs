using UnityEngine;

public class AssignmentEnemyDirector : MonoBehaviour
{
    [HideInInspector] public GameObject enemyPrefab;
    [HideInInspector] public Transform target;
    [HideInInspector] public Transform projectileHolder;
    [HideInInspector] public GameManager gameManager;

    [Header("Spawn Tuning")]
    public float baseSpawnDelay = 2.35f;
    public float minimumSpawnDelay = 0.95f;
    public float speedIncreasePerKill = 0.07f;
    public float spawnMargin = 2f;
    public int baseMaxAliveEnemies = 4;

    private float nextSpawnTime;
    private bool spawningEnabled;

    public void ResetDirector()
    {
        nextSpawnTime = Time.time + 1f;
        spawningEnabled = true;
    }

    public void StopSpawning()
    {
        spawningEnabled = false;
    }

    private void Update()
    {
        if (!spawningEnabled || enemyPrefab == null || target == null || gameManager == null || gameManager.gameIsOver)
        {
            return;
        }

        if (FindObjectsOfType<Enemy>().Length >= GetMaxAliveEnemies())
        {
            return;
        }

        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + GetCurrentSpawnDelay();
        }
    }

    private float GetCurrentSpawnDelay()
    {
        float difficultyStep = gameManager.EnemiesDefeated * 0.12f;
        return Mathf.Max(minimumSpawnDelay, baseSpawnDelay - difficultyStep);
    }

    private int GetMaxAliveEnemies()
    {
        return baseMaxAliveEnemies + Mathf.FloorToInt(gameManager.EnemiesDefeated / 3f);
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.followTarget = target;
            enemy.moveSpeed += gameManager.EnemiesDefeated * speedIncreasePerKill;
        }

        ShootingController[] shootingControllers = enemyObject.GetComponentsInChildren<ShootingController>();
        foreach (ShootingController shootingController in shootingControllers)
        {
            shootingController.projectileHolder = projectileHolder;
        }
    }

    private Vector3 GetSpawnPosition()
    {
        Camera sceneCamera = Camera.main;
        if (sceneCamera == null)
        {
            return target.position + Vector3.up * 8f;
        }

        float distanceFromCamera = Mathf.Abs(sceneCamera.transform.position.z);
        Vector3 bottomLeft = sceneCamera.ViewportToWorldPoint(new Vector3(0f, 0f, distanceFromCamera));
        Vector3 topRight = sceneCamera.ViewportToWorldPoint(new Vector3(1f, 1f, distanceFromCamera));

        int edge = Random.Range(0, 4);
        switch (edge)
        {
            case 0:
                return new Vector3(bottomLeft.x - spawnMargin, Random.Range(bottomLeft.y, topRight.y), 0f);
            case 1:
                return new Vector3(topRight.x + spawnMargin, Random.Range(bottomLeft.y, topRight.y), 0f);
            case 2:
                return new Vector3(Random.Range(bottomLeft.x, topRight.x), topRight.y + spawnMargin, 0f);
            default:
                return new Vector3(Random.Range(bottomLeft.x, topRight.x), bottomLeft.y - spawnMargin, 0f);
        }
    }
}
