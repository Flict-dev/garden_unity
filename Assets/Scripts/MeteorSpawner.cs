using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class MeteorSpawner : MonoBehaviour
{
    public float spawnInterval = 2.5f;
    public float spawnRadius = 20f;
    public int maxAliveBugs = 12;
    public float difficultyRampTime = 120f;

    private const float EdgeSpawnInset = 1.5f;

    public static MeteorSpawner Instance { get; private set; }

    private readonly List<MeteorMover> _aliveBugs = new List<MeteorMover>();

    private PlayerController _player;
    private EnvironmentBuilder _environment;
    private float _timer;
    private float _sessionTime;

    private void Awake()
    {
        Instance = this;
        _player = FindFirstObjectByType<PlayerController>();
        _environment = EnvironmentBuilder.Active != null
            ? EnvironmentBuilder.Active
            : FindFirstObjectByType<EnvironmentBuilder>();
        _timer = 1f;
        GameFeedback.EnsureInScene();
    }

    private void Update()
    {
        CleanupDestroyedBugs();
        GameData.AliveMeteorites = _aliveBugs.Count;
        _sessionTime += Time.deltaTime;

        if (_player == null)
        {
            _player = FindFirstObjectByType<PlayerController>();
        }

        _timer -= Time.deltaTime;
        if (_timer > 0f)
        {
            return;
        }

        // Difficulty ramps up over time: faster spawns, more bugs
        float difficulty = Mathf.Clamp01(_sessionTime / difficultyRampTime);
        float currentInterval = Mathf.Lerp(spawnInterval, spawnInterval * 0.35f, difficulty);
        int currentMaxBugs = maxAliveBugs + Mathf.FloorToInt(difficulty * 8f);

        _timer = currentInterval;
        if (_aliveBugs.Count >= currentMaxBugs)
        {
            return;
        }

        SpawnBug();
    }

    public void NotifyBugDestroyed(MeteorMover bug)
    {
        _aliveBugs.Remove(bug);
    }

    private void SpawnBug()
    {
        Vector3 spawnPos = GetEdgeSpawnPosition();
        GameObject bugGo = new GameObject("Bug");
        bugGo.transform.position = spawnPos;

        // Look toward player
        if (_player != null)
        {
            Vector3 dir = _player.transform.position - spawnPos;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                bugGo.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            }
        }

        // Randomize bug type
        BugType type = (BugType)Random.Range(0, 3);
        float moveSpeed = 3.2f;
        float health = 2f;
        float damage = 4f;

        switch (type)
        {
            case BugType.Ant:
                moveSpeed = 3.2f;
                health = 2f;
                damage = 4f;
                break;
            case BugType.Beetle:
                moveSpeed = 1.8f;
                health = 5f;
                damage = 8f;
                break;
            case BugType.Spider:
                moveSpeed = 4.5f;
                health = 1.5f;
                damage = 12f;
                break;
        }

        MeteorMover bug = bugGo.AddComponent<MeteorMover>();
        bug.Configure(type, moveSpeed, health, damage);

        if (type == BugType.Spider && _player != null)
        {
            bug.SetTarget(_player.transform);
        }
        else
        {
            Vegetable vegetable = GardenManager.Instance?.GetNearestVegetable(spawnPos);
            if (vegetable != null)
            {
                bug.SetTarget(vegetable.transform);
            }
        }

        _aliveBugs.Add(bug);
        GameFeedback.PlayBugSpawn(spawnPos, type);
    }

    private Vector3 GetEdgeSpawnPosition()
    {
        if (_environment == null)
        {
            _environment = EnvironmentBuilder.Active != null
                ? EnvironmentBuilder.Active
                : FindFirstObjectByType<EnvironmentBuilder>();
        }

        float effectiveSpawnRadius = spawnRadius;
        if (_environment != null)
        {
            effectiveSpawnRadius = Mathf.Max(spawnRadius, _environment.ArenaRadius - EdgeSpawnInset);
        }

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float x = Mathf.Cos(angle) * effectiveSpawnRadius;
        float z = Mathf.Sin(angle) * effectiveSpawnRadius;

        float groundY = _environment != null
            ? _environment.SampleGroundSurface(x, z)
            : EnvironmentBuilder.SampleGeneratedGroundHeight(x, z, effectiveSpawnRadius + 7f);
        return new Vector3(x, groundY + 0.25f, z);
    }

    private void CleanupDestroyedBugs()
    {
        _aliveBugs.RemoveAll(bug => bug == null);
    }
}
