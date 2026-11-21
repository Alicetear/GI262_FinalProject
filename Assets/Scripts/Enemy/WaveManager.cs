using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct EnemyCatalogEntry
{
    public string enemyId; // e.g. "Walker", "Runner", "Fat", "Spitter"
    public GameObject enemyPrefab;
    public EnemyAttackType attackType; // Melee vs Range
}

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
    public string nightName = "Night 1";
    public float spawnInterval = 1.5f;

    [Header("ลำดับศัตรูที่แน่นอน (Queue Sequence - ปิดการสุ่ม)")]
    [Tooltip("ระบุ enemyId เรียงตามลำดับที่จะปล่อยออกมา เช่น Walker, Walker, Runner")]
    public List<string> enemySequence = new List<string>();

    [Header("ข้อมูลสำรองเดิม (สำหรับ Backward Compatibility)")]
    [HideInInspector] public List<EnemySpawnPool> enemyPool;
    [HideInInspector] public int totalEnemiesToSpawn = 10;
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    [Header("คลังอ้างอิงศัตรูทั้งหมด (Master Enemy Reference Catalog)")]
    [Tooltip("รายการศัตรูทั้งหมดเป็น reference ให้กับเวฟ data ตามโจทย์อาจารย์")]
    [SerializeField] private List<EnemyCatalogEntry> enemyCatalog = new List<EnemyCatalogEntry>();
    private Dictionary<string, GameObject> enemyCatalogDict = new Dictionary<string, GameObject>();

    [Header("พรีแฟบศัตรู 4 สายพันธุ์ (Direct References สำหรับ Catalog)")]
    [SerializeField] private GameObject walkerPrefab;
    [SerializeField] private GameObject runnerPrefab;
    [SerializeField] private GameObject fatPrefab;
    [SerializeField] private GameObject spitterPrefab;

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

        BuildEnemyCatalog();
    }

    private void BuildEnemyCatalog()
    {
        enemyCatalogDict.Clear();

        // 1. ลงทะเบียนจาก Direct Prefabs
        if (walkerPrefab != null) enemyCatalogDict["Walker"] = walkerPrefab;
        if (runnerPrefab != null) enemyCatalogDict["Runner"] = runnerPrefab;
        if (fatPrefab != null) enemyCatalogDict["Fat"] = fatPrefab;
        if (spitterPrefab != null) enemyCatalogDict["Spitter"] = spitterPrefab;

        // 2. ลงทะเบียนจาก EnemyCatalog entries
        foreach (var entry in enemyCatalog)
        {
            if (!string.IsNullOrEmpty(entry.enemyId) && entry.enemyPrefab != null)
            {
                enemyCatalogDict[entry.enemyId] = entry.enemyPrefab;
            }
        }

        // 3. Fallback สแกนหา Prefab จาก nightWaves เก่า (ถ้ามี)
        if (nightWaves != null)
        {
            foreach (var wave in nightWaves)
            {
                if (wave.enemyPool != null)
                {
                    foreach (var pool in wave.enemyPool)
                    {
                        if (pool.enemyPrefab != null)
                        {
                            string pName = pool.enemyPrefab.name.ToLower();
                            if (pName.Contains("walker") && !enemyCatalogDict.ContainsKey("Walker"))
                                enemyCatalogDict["Walker"] = pool.enemyPrefab;
                            else if (pName.Contains("runner") && !enemyCatalogDict.ContainsKey("Runner"))
                                enemyCatalogDict["Runner"] = pool.enemyPrefab;
                            else if (pName.Contains("fat") && !enemyCatalogDict.ContainsKey("Fat"))
                                enemyCatalogDict["Fat"] = pool.enemyPrefab;
                            else if (pName.Contains("spitter") && !enemyCatalogDict.ContainsKey("Spitter"))
                                enemyCatalogDict["Spitter"] = pool.enemyPrefab;
                        }
                    }
                }
            }
        }
    }

    // สร้างชุดข้อมูลเวฟ 7 คืนตามโครงสร้าง Queue (FIFO) ปิดการสุ่ม 100%
    public List<MixedNightWaveData> GenerateDefault7Nights()
    {
        List<MixedNightWaveData> waves = new List<MixedNightWaveData>();

        // Night 1: First Contact (5 Walkers) - เรียนรู้ระบบยิง
        waves.Add(new MixedNightWaveData
        {
            nightName = "Night 1: First Contact",
            spawnInterval = 2.5f,
            enemySequence = new List<string> { "Walker", "Walker", "Walker", "Walker", "Walker" }
        });

        // Night 2: The Fast & The Hungry (8 Enemies: Walkers + Runners) - ทดสอบความเร็ว
        waves.Add(new MixedNightWaveData
        {
            nightName = "Night 2: The Fast & The Hungry",
            spawnInterval = 2.0f,
            enemySequence = new List<string> { "Walker", "Walker", "Runner", "Walker", "Runner", "Walker", "Runner", "Runner" }
        });

        // Night 3: Toxic Sky (12 Enemies: Walkers + Runners + Spitters) - แนะนำศัตรูระยะไกล
        waves.Add(new MixedNightWaveData
        {
            nightName = "Night 3: Toxic Sky",
            spawnInterval = 1.8f,
            enemySequence = new List<string> { "Walker", "Runner", "Spitter", "Walker", "Walker", "Spitter", "Runner", "Walker", "Spitter", "Runner", "Runner", "Walker" }
        });

        // Night 4: The Juggernaut (15 Enemies: แนะนำ Fat Zombie เลือดหนาตีกำแพงแรง)
        waves.Add(new MixedNightWaveData
        {
            nightName = "Night 4: The Juggernaut",
            spawnInterval = 1.6f,
            enemySequence = new List<string> { "Walker", "Walker", "Runner", "Fat", "Walker", "Spitter", "Runner", "Fat", "Walker", "Spitter", "Runner", "Runner", "Fat", "Walker", "Spitter" }
        });

        // Night 5: Coordinated Siege (20 Enemies: บุกผสมผสาน 4 สายพันธุ์)
        waves.Add(new MixedNightWaveData
        {
            nightName = "Night 5: Coordinated Siege",
            spawnInterval = 1.4f,
            enemySequence = new List<string>
            {
                "Fat", "Walker", "Runner", "Spitter",
                "Runner", "Runner", "Walker", "Spitter",
                "Fat", "Fat", "Runner", "Spitter", "Spitter",
                "Walker", "Runner", "Runner", "Fat", "Walker", "Spitter", "Fat"
            }
        });

        // Night 6: Blood Moon Swarm (26 Enemies: ความหนาแน่นสูง เหมาะกับปืน AOE)
        waves.Add(new MixedNightWaveData
        {
            nightName = "Night 6: Blood Moon Swarm",
            spawnInterval = 1.1f,
            enemySequence = new List<string>
            {
                "Runner", "Runner", "Runner", "Runner", "Spitter", "Spitter",
                "Fat", "Fat", "Walker", "Walker", "Runner", "Runner",
                "Walker", "Walker", "Runner", "Runner", "Fat", "Fat",
                "Spitter", "Spitter", "Spitter", "Spitter", "Runner", "Runner", "Fat", "Fat"
            }
        });

        // Night 7: The Final Stand (34 Enemies: ศึกสุดท้ายรอฮ. กู้ภัย)
        waves.Add(new MixedNightWaveData
        {
            nightName = "Night 7: The Final Stand",
            spawnInterval = 0.95f,
            enemySequence = new List<string>
            {
                "Runner", "Runner", "Runner", "Runner", "Walker", "Walker", "Walker", "Walker",
                "Spitter", "Spitter", "Fat", "Spitter", "Spitter", "Spitter", "Fat", "Fat",
                "Runner", "Runner", "Runner", "Runner", "Walker", "Walker", "Walker", "Walker",
                "Spitter", "Spitter", "Fat", "Fat", "Fat", "Runner", "Runner", "Walker", "Walker", "Fat"
            }
        });

        return waves;
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

        // 1. นำข้อมูลศัตรูทั้งหมดของเวฟนี้เข้าสู่ Queue (FIFO) - ปิดการสุ่มโดยสิ้นเชิง
        spawnQueue.Clear();

        if (currentWave.enemySequence != null && currentWave.enemySequence.Count > 0)
        {
            // ดึงจาก Master Catalog ด้วย Queue ตามลำดับแน่นอน
            foreach (string enemyId in currentWave.enemySequence)
            {
                if (enemyCatalogDict.TryGetValue(enemyId, out GameObject prefab))
                {
                    spawnQueue.Enqueue(prefab);
                }
                else
                {
                    Debug.LogWarning($"[WaveManager] ไม่พบศัตรู ID '{enemyId}' ใน Catalog!");
                }
            }
        }
        else if (currentWave.enemyPool != null && currentWave.enemyPool.Count > 0)
        {
            // Fallback ปิดการสุ่ม: เรียงคิวตามลำดับแบบแน่นอน (Deterministic Queue)
            for (int i = 0; i < currentWave.totalEnemiesToSpawn; i++)
            {
                GameObject prefab = currentWave.enemyPool[i % currentWave.enemyPool.Count].enemyPrefab;
                if (prefab != null)
                {
                    spawnQueue.Enqueue(prefab);
                }
            }
        }

        Debug.Log($"[WaveManager] บรรจุศัตรูเข้าคิว Queue: {spawnQueue.Count} ตัว (ปิดการสุ่ม)");
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
        activeEnemies.RemoveAll(item => item == null || item.IsDead);
        CheckWaveEnd();
    }

    public void CheckWaveEnd()
    {
        activeEnemies.RemoveAll(item => item == null || item.IsDead);

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
    private bool isQuitting = false;

    // Unity จะเรียกฟังก์ชันนี้อัตโนมัติทันทีที่กดปุ่ม Stop หรือปิดเกม
    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    private void OnDestroy()
    {
        // ถ้ากำลังปิดเกม/เปลี่ยนฉาก ให้หยุดทำงานทันที ห้ามเสก VFX หรือดรอปของเด็ดขาด
        if (isQuitting || !gameObject.scene.isLoaded) return;

        // โค้ดเดิมของคุณใน OnDestroy (ถ้ามี)
    }
}
