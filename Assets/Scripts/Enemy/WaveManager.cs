using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("ตั้งค่าเวฟผสมซอมบี้ (โปรโตไทป์ 2 คืน)")]
    [SerializeField] private List<MixedNightWaveData> nightWaves;
    private int currentWaveIndex = 0;

    [Header("จุดเกิดศัตรู")]
    [SerializeField] private Transform[] spawnPoints;

    private int enemiesRemainingToSpawn = 0;
    private int enemiesAliveInScene = 0;
    private bool isSpawningWave = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(StartWave(currentWaveIndex));
    }

    private IEnumerator StartWave(int waveIndex)
    {
        if (waveIndex >= nightWaves.Count)
        {
            Debug.Log("🎉 ชนะเกมแล้ว! ผ่านครบทุกเวฟ");
            yield break;
        }

        MixedNightWaveData currentWave = nightWaves[waveIndex];
        Debug.Log($"--- เริ่มต้น {currentWave.nightName} ---");

        enemiesRemainingToSpawn = currentWave.totalEnemiesToSpawn;
        // กำหนดจำนวนซอมบี้ทั้งหมดที่จะเกิดในเวฟนี้ เพื่อเอาไว้เช็กตอนตาย
        enemiesAliveInScene = currentWave.totalEnemiesToSpawn;
        isSpawningWave = true;

        while (enemiesRemainingToSpawn > 0)
        {
            GameObject prefabToSpawn = GetRandomEnemyFromPool(currentWave.enemyPool);
            SpawnEnemy(prefabToSpawn);

            enemiesRemainingToSpawn--;
            yield return new WaitForSeconds(currentWave.spawnInterval);
        }

        isSpawningWave = false;
    }

    private GameObject GetRandomEnemyFromPool(List<EnemySpawnPool> pool)
    {
        if (pool == null || pool.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var spawn in pool)
        {
            totalWeight += spawn.spawnWeight;
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var spawn in pool)
        {
            currentWeight += spawn.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return spawn.enemyPrefab;
            }
        }

        return pool[0].enemyPrefab;
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnPoints.Length == 0 || enemyPrefab == null) return;

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Instantiate(enemyPrefab, spawnPoints[randomIndex].position, Quaternion.identity);
    }

    // ฟังก์ชันนี้ถูกเรียกจาก BaseEnemy ตอนซอมบี้ตาย
    public void OnEnemyKilled()
    {
        enemiesAliveInScene--;
        Debug.Log($"ซอมบี้ตาย! เหลืออีก: {enemiesAliveInScene} ตัวในฉาก");

        // เงื่อนไข: สปอว์นตัวสุดท้ายหมดแล้ว และซอมบี้ในฉากตายจนเกลี้ยงเป็น 0
        if (!isSpawningWave && enemiesRemainingToSpawn <= 0 && enemiesAliveInScene <= 0)
        {
            currentWaveIndex++;

            if (currentWaveIndex < nightWaves.Count)
            {
                Debug.Log($"✨ เคลียร์เวฟ {currentWaveIndex} สำเร็จ! กำลังไปคืนถัดไป...");
                // หน่วงเวลาสัก 2 วินาทีก่อนเริ่มเวฟใหม่ ให้ผู้เล่นได้พักหายใจ
                StartCoroutine(WaitAndStartNextWave(2f));
            }
            else
            {
                Debug.Log("🏆 VICTORY! ชนะเกมโปรโตไทป์สมบูรณ์แบบ!");
                // ตรงนี้สามารถเปิด UI Victory Panel ขึ้นมาได้
            }
        }
    }

    private IEnumerator SystemWaitAndStartNextWave(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(StartWave(currentWaveIndex));
    }

    // เผื่อชื่อฟังก์ชันหน่วงเวลาด้านบน
    private IEnumerator WaitAndStartNextWave(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(StartWave(currentWaveIndex));
    }

}