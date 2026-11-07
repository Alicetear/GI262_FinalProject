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

    [Header("ตั้งค่าเวฟ (Night Waves - มี 2 คืน)")]
    [SerializeField] private List<MixedNightWaveData> nightWaves;
    private int currentWaveIndex = 0;

    [Header("จุดเกิดศัตรู")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("ตั้งค่าหน้าจอตอนเช้า (Morning Screen)")]
    [SerializeField] private GameObject morningScreenPanel;
    [SerializeField] private Button nextNightButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    private int enemiesRemainingToSpawn = 0;
    private int enemiesAliveInScene = 0;
    private bool isSpawningWave = false;

    [Header("ตั้งค่าหน่วงเวลาก่อนจบเวฟ")]
    [SerializeField] private float delayAfterWave = 2.0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (morningScreenPanel != null) morningScreenPanel.SetActive(false);

        if (nextNightButton != null)
        {
            nextNightButton.onClick.RemoveAllListeners();
            nextNightButton.onClick.AddListener(OnProceedToNextNight);
        }

        StartCoroutine(StartWave(currentWaveIndex));
    }

    private IEnumerator StartWave(int waveIndex)
    {
        if (waveIndex >= nightWaves.Count)
        {
            yield break;
        }

        MixedNightWaveData currentWave = nightWaves[waveIndex];
        Debug.Log($"--- เริ่มต้น {currentWave.nightName} ---");

        enemiesRemainingToSpawn = currentWave.totalEnemiesToSpawn;
        enemiesAliveInScene = currentWave.totalEnemiesToSpawn;
        isSpawningWave = true;

        while (enemiesRemainingToSpawn > 0)
        {
            GameObject prefabToSpawn = GetRandomEnemyFromPool(currentWave.enemyPool);
            SpawnEnemy(prefabToSpawn);

            enemiesRemainingToSpawn--;

            if (enemiesRemainingToSpawn > 0)
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
        if (spawnPoints.Length == 0 || enemyPrefab == null) return;

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Instantiate(enemyPrefab, spawnPoints[randomIndex].position, Quaternion.identity);
    }

    public void OnEnemyKilled()
    {
        enemiesAliveInScene--;
        Debug.Log($"ซอมบี้ตาย! เหลืออีก: {enemiesAliveInScene} ตัวในฉาก");
        CheckWaveEnd();
    }

    public void CheckWaveEnd()
    {
        if (enemiesAliveInScene <= 0 && !isSpawningWave)
        {
            StartCoroutine(WaitAndEndWaveCoroutine());
        }
    }

    private IEnumerator WaitAndEndWaveCoroutine()
    {
        Debug.Log("เคลียร์ซอมบี้หมดแล้ว! รอหน่วงเวลาแป๊บนึง...");
        yield return new WaitForSeconds(delayAfterWave);

        // 💡 เช็กว่าเป็นคืนสุดท้าย (Night 2 / Index 1) หรือยัง
        if (currentWaveIndex >= nightWaves.Count - 1)
        {
            // ถ้าจบ Night 2 แล้ว ไม่ต้องเปิดหน้าจอตอนเช้า แค่ปริ้นต์บอกใน Console ว่าชนะ
            Debug.Log("🎉 VICTORY! ชนะเกมแล้ว! ผ่านครบทุกเวฟเรียบร้อย!");
        }
        else
        {
            // ถ้ายังเป็น Night 1 อยู่ ให้เปิดหน้าจอตอนเช้าเพื่อไปต่อ Night 2 ปกติ
            ShowMorningScreen();
        }
    }

    private void ShowMorningScreen()
    {
        if (morningScreenPanel != null) morningScreenPanel.SetActive(true);

        // 💡 สูตรคำนวณเลขเวฟถัดไปที่ถูกต้องเป๊ะ:
        // - จบ Index 0 (คืน 1) -> currentWaveIndex (0) + 2 = แสดงผล "Night 2"
        // - จบ Index 1 (คืน 2) -> currentWaveIndex (1) + 2 = แสดงผล "Night 3"
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

        // ขยับไปเวฟถัดไป (Night 2)
        currentWaveIndex++;

        StartCoroutine(StartWave(currentWaveIndex));
    }
}