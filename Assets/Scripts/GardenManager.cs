using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class GardenManager : MonoBehaviour
{
    public float nightDuration = 180f;

    public static GardenManager Instance { get; private set; }

    private readonly List<Vegetable> _vegetables = new List<Vegetable>();
    private bool _isFinished;
    private float _timeRemaining;

    public int TotalVegetables => _vegetables.Count;
    public int SavedVegetables => CountAliveVegetables();
    public float TimeRemaining => _timeRemaining;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _timeRemaining = Mathf.Max(1f, nightDuration);
        GameData.NightDuration = _timeRemaining;
        GameData.NightTimeRemaining = _timeRemaining;
    }

    private void Update()
    {
        if (_isFinished)
        {
            return;
        }

        _timeRemaining = Mathf.Max(0f, _timeRemaining - Time.deltaTime);
        SyncGameData();

        if (_timeRemaining <= 0f)
        {
            if (SavedVegetables > 0)
            {
                CompleteVictory();
            }
            else
            {
                CompleteDefeat("All vegetables were eaten before dawn.");
            }
        }
    }

    public void RegisterVegetable(Vegetable vegetable)
    {
        if (vegetable == null || _vegetables.Contains(vegetable))
        {
            return;
        }

        _vegetables.Add(vegetable);
        SyncGameData();
    }

    public void NotifyVegetableDestroyed(Vegetable vegetable)
    {
        SyncGameData();

        if (!_isFinished && TotalVegetables > 0 && SavedVegetables <= 0)
        {
            CompleteDefeat("All vegetables were eaten.");
        }
    }

    public Vegetable GetNearestVegetable(Vector3 position)
    {
        Vegetable nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Vegetable vegetable in _vegetables)
        {
            if (vegetable == null || !vegetable.IsAlive)
            {
                continue;
            }

            float distance = (vegetable.transform.position - position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = vegetable;
            }
        }

        return nearest;
    }

    public void CompleteVictory()
    {
        FinishRun(true, "Dawn arrived. The garden survived.");
    }

    public void CompleteDefeat(string reason)
    {
        FinishRun(false, reason);
    }

    private void FinishRun(bool victory, string reason)
    {
        if (_isFinished)
        {
            return;
        }

        _isFinished = true;
        SyncGameData();
        GameData.SetRunResult(victory, reason);
        Time.timeScale = 1f;
        SceneManager.LoadScene("GameOver");
    }

    private int CountAliveVegetables()
    {
        int count = 0;
        foreach (Vegetable vegetable in _vegetables)
        {
            if (vegetable != null && vegetable.IsAlive)
            {
                count++;
            }
        }
        return count;
    }

    private void SyncGameData()
    {
        GameData.TotalVegetables = TotalVegetables;
        GameData.SavedVegetables = SavedVegetables;
        GameData.NightTimeRemaining = _timeRemaining;
    }
}
