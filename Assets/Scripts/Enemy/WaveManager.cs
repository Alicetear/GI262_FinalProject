using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class EnemySpawnPool
{
    public GameObject enemyPrefab;
    [Range(0f, 100f)]
    public float spawnWeight = 50f;
}

[System.Serializable]
public class MixedNightWaveData
{
    public string nightName;
    public List<EnemySpawnPool> enemyPool;
    public int totalEnemiesToSpawn = 10;
    public float spawnInterval = 1.5f;
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("ตั้งค่าเวฟ (Night Waves)")]
    [SerializeField] private List<MixedNightWaveData> nightWaves;
    public int currentWaveIndex = 0;

    [Header("จุดเกิดศัตรู")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("ตั้งค่าหน้าจอตอนเช้า (Morning Screen)")]
    [SerializeField] private GameObject morningScreenPanel;
    [SerializeField] private Button nextNightButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("ตั้งค่าหน้าจอชนะเกม (Victory Screen)")]
    [SerializeField] private GameObject victoryPanel;

    [Header("ตั้งค่าหน่วงเวลาก่อนจบเวฟ")]
    [SerializeField] private float delayAfterWave = 2.0f;

    // --- DATA STRUCTURES (Queue FIFO + List) ---
    private Queue<GameObject> spawnQueue = new Queue<GameObject>();
    private List<BaseEnemy> activeEnemies = new List<BaseEnemy>();

    public List<BaseEnemy> ActiveEnemies => activeEnemies;
    public int SpawnQueueCount => spawnQueue.Count;
    public int ActiveEnemyCount => activeEnemies.Count;

    private bool isSpawningWave = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (morningScreenPanel != null) morningScreenPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        if (nextNightButton != null)
        {
            nextNightButton.onClick.RemoveAllListeners();
            nextNightButton.onClick.AddListener(OnProceedToNextNight);
        }

        StartCoroutine(StartWave(currentWaveIndex));
    }

    public IEnumerator StartWave(int waveIndex)
    {
        if (nightWaves == null || waveIndex >= nightWaves.Count)
        {
            yield break;
        }

        MixedNightWaveData currentWave = nightWaves[waveIndex];
        Debug.Log($"--- เริ่มต้น {currentWave.nightName} (Wave {waveIndex + 1}/{nightWaves.Count}) ---");

        // 1. นำข้อมูลศัตรูทั้งหมดของเวฟนี้เข้าสู่ Queue (FIFO)
        spawnQueue.Clear();
        for (int i = 0; i < currentWave.totalEnemiesToSpawn; i++)
        {
            GameObject prefab = GetRandomEnemyFromPool(currentWave.enemyPool);
            if (prefab != null)
            {
                spawnQueue.Enqueue(prefab);
            }
        }

        isSpawningWave = true;

        // 2. ทยอยดึงจาก Queue (Dequeue) แล้วปล่อยออกมาตาม Interval
        while (spawnQueue.Count > 0)
        {
            GameObject prefabToSpawn = spawnQueue.Dequeue();
            SpawnEnemy(prefabToSpawn);

            if (spawnQueue.Count > 0)
            {
                yield return new WaitForSeconds(currentWave.spawnInterval);
            }
        }

        isSpawningWave = false;
        CheckWaveEnd();
    }

    private GameObject GetRandomEnemyFromPool(List<EnemySpawnPool> pool)
    {
        if (pool == null || pool.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var spawn in pool) totalWeight += spawn.spawnWeight;

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var spawn in pool)
        {
            currentWeight += spawn.spawnWeight;
            if (randomValue <= currentWeight) return spawn.enemyPrefab;
        }

        return pool[0].enemyPrefab;
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnPoints == null || spawnPoints.Length == 0 || enemyPrefab == null) return;

        int randomIndex = Random.Range(0, spawnPoints.Length);
        GameObject spawned = Instantiate(enemyPrefab, spawnPoints[randomIndex].position, Quaternion.identity);

        BaseEnemy enemyComp = spawned.GetComponent<BaseEnemy>();
        if (enemyComp != null)
        {
            activeEnemies.Add(enemyComp);
        }
    }

    public void OnEnemyKilled(BaseEnemy enemy)
    {
        if (enemy != null && activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
        else
        {
            activeEnemies.RemoveAll(item => item == null || item == enemy);
        }

        Debug.Log($"ซอมบี้ตาย! เหลือในสนาม: {activeEnemies.Count} ตัว (ในคิวเกิด: {spawnQueue.Count})");
        CheckWaveEnd();
    }

    public void OnEnemyKilled()
    {
        activeEnemies.RemoveAll(item => item == null);
        CheckWaveEnd();
    }

    public void CheckWaveEnd()
    {
        activeEnemies.RemoveAll(item => item == null);

        if (spawnQueue.Count == 0 && activeEnemies.Count == 0 && !isSpawningWave)
        {
            StartCoroutine(WaitAndEndWaveCoroutine());
        }
    }

    private IEnumerator WaitAndEndWaveCoroutine()
    {
        Debug.Log("เคลียร์ซอมบี้หมดแล้ว! รอหน่วงเวลาเปลี่ยนรอบ...");
        yield return new WaitForSeconds(delayAfterWave);

        if (currentWaveIndex >= nightWaves.Count - 1)
        {
            Debug.Log("🎉 VICTORY! ชนะเกมแล้ว! เอาชีวิตรอดครบทุกคืน!");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TriggerVictory();
            }

            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
                Time.timeScale = 0f;
            }
        }
        else
        {
            // ได้รับกล่องเสบียงสนับสนุน (Supply Drop) ทุกครั้งที่รอดพ้นจบแต่ละคืน
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddCurrency(CurrencyType.Rations, 1);
                GameManager.Instance.SetGameState(GameState.MorningShop);
            }

            ShowMorningScreen();
        }
    }

    private void ShowMorningScreen()
    {
        if (morningScreenPanel != null) morningScreenPanel.SetActive(true);

        int nextNightNumber = currentWaveIndex + 2;

        if (buttonText != null)
        {
            buttonText.text = $"Night {nextNightNumber}";
        }

        Time.timeScale = 0f;
    }

    public void OnProceedToNextNight()
    {
        if (morningScreenPanel != null) morningScreenPanel.SetActive(false);
        Time.timeScale = 1f;

        currentWaveIndex++;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentNight(currentWaveIndex + 1);
            GameManager.Instance.SetGameState(GameState.NightDefense);
        }

        StartCoroutine(StartWave(currentWaveIndex));
    }
}
