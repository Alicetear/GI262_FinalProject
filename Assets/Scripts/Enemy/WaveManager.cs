using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance; // ประกาศให้สคริปต์อื่นเรียกใช้ได้ง่าย

    [System.Serializable]
    public class WaveData
    {
        public string waveName; // เช่น "Night 1"
        public List<GameObject> enemiesInWave; // ลาก Prefab ซอมบี้มาใส่เรียงตัว
        public float spawnDelay = 2f; // ระยะเวลาเกิดแต่ละตัว
    }

    [Header("Wave Configuration")]
    [SerializeField] private List<WaveData> allWaves;
    [SerializeField] private Transform spawnPoint; // จุดเกิดด้านบนจอ

    private Queue<GameObject> enemyQueue = new Queue<GameObject>();
    private int currentWaveIndex = 0;
    private int activeEnemies = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // สั่งให้เริ่มเทสเวฟแรกทันทีที่กด Play
        StartWave(0);
    }

    public void StartWave(int index)
    {
        currentWaveIndex = index;
        enemyQueue.Clear();

        // นำศัตรูเข้า Queue (Data Structure)
        foreach (GameObject enemy in allWaves[index].enemiesInWave)
        {
            enemyQueue.Enqueue(enemy);
        }

        activeEnemies = enemyQueue.Count;
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (enemyQueue.Count > 0)
        {
            GameObject nextEnemy = enemyQueue.Dequeue();
            Instantiate(nextEnemy, spawnPoint.position, Quaternion.identity);

            yield return new WaitForSeconds(allWaves[currentWaveIndex].spawnDelay);
        }
    }

    // ฟังก์ชันรับสัญญาณเมื่อซอมบี้ตาย
    public void OnEnemyDied()
    {
        activeEnemies--;
        if (activeEnemies <= 0 && enemyQueue.Count == 0)
        {
            Debug.Log($"เคลียร์เวฟ {currentWaveIndex + 1} สำเร็จ! เปิด UI ร้านค้าได้เลย");
        }
    }
}
