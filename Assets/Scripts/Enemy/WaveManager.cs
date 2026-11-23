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
    private Dictionary<string, GameObject> enemyCatalogDict = new Dictionary<string, GameObject>(System.StringComparer.OrdinalIgnoreCase);

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
    [SerializeField] private float delayAfterWave = 2.5f;

    [Header("ระบบจัดการศพค้างในฉาก (Corpse Management)")]
    [SerializeField] private int maxCorpses = 20;

    // --- DATA STRUCTURES (Queue FIFO + List) ---
    private Queue<GameObject> spawnQueue = new Queue<GameObject>();
    private List<BaseEnemy> activeEnemies = new List<BaseEnemy>();
    private Queue<GameObject> activeCorpses = new Queue<GameObject>();

    public List<BaseEnemy> ActiveEnemies => activeEnemies;
    public int SpawnQueueCount => spawnQueue.Count;
    public int ActiveEnemyCount => activeEnemies.Count;

    private bool isSpawningWave = false;
    private bool isEndingWave = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        BuildEnemyCatalog();
    }

    private void BuildEnemyCatalog()
    {
        enemyCatalogDict.Clear();

        // 1. ลงทะเบียนจาก Direct Prefabs พร้อม Alias (ไม่สนตัวพิมพ์เล็ก-ใหญ่)
        RegisterDirectEnemy(walkerPrefab, "Walker", "Walk", "WalkZombie");
        RegisterDirectEnemy(runnerPrefab, "Runner", "Run", "RunnerZombie");
        RegisterDirectEnemy(fatPrefab, "Fat", "FatZombie");
        RegisterDirectEnemy(spitterPrefab, "Spitter", "Spit", "SpitterZombie");

        // 2. ผสานข้อมูลเพิ่มเติมจาก EnemyCatalog entries โดยไม่ทับค่าของ 4 ตัวหลักจาก Direct References
        if (enemyCatalog != null)
        {
            foreach (var entry in enemyCatalog)
            {
                if (!string.IsNullOrWhiteSpace(entry.enemyId) && entry.enemyPrefab != null)
                {
                    string cleanId = entry.enemyId.Trim();
                    if (!enemyCatalogDict.ContainsKey(cleanId))
                    {
                        enemyCatalogDict[cleanId] = entry.enemyPrefab;
                    }
                }
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
                                RegisterDirectEnemy(pool.enemyPrefab, "Walker", "Walk", "WalkZombie");
                            else if (pName.Contains("runner") && !enemyCatalogDict.ContainsKey("Runner"))
                                RegisterDirectEnemy(pool.enemyPrefab, "Runner", "Run", "RunnerZombie");
                            else if (pName.Contains("fat") && !enemyCatalogDict.ContainsKey("Fat"))
                                RegisterDirectEnemy(pool.enemyPrefab, "Fat", "FatZombie");
                            else if (pName.Contains("spitter") && !enemyCatalogDict.ContainsKey("Spitter"))
                                RegisterDirectEnemy(pool.enemyPrefab, "Spitter", "Spit", "SpitterZombie");
                        }
                    }
                }
            }
        }
    }

    private void RegisterDirectEnemy(GameObject prefab, params string[] ids)
    {
        if (prefab == null) return;
        foreach (string id in ids)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                enemyCatalogDict[id.Trim()] = prefab;
            }
        }
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

        if (enemyCatalogDict.Count == 0)
        {
            BuildEnemyCatalog();
        }

        MixedNightWaveData currentWave = nightWaves[waveIndex];
        Debug.Log($"--- เริ่มต้น {currentWave.nightName} (Wave {waveIndex + 1}/{nightWaves.Count}) ---");

        // 1. นำข้อมูลศัตรูทั้งหมดของเวฟนี้เข้าสู่ Queue (FIFO) - ปิดการสุ่มโดยสิ้นเชิง
        spawnQueue.Clear();

        if (currentWave.enemySequence != null && currentWave.enemySequence.Count > 0)
        {
            // ดึงจาก Master Catalog ด้วย Queue ตามลำดับแน่นอน (ตัดช่องว่างหัวท้ายและไม่สนตัวพิมพ์เล็ก-ใหญ่)
            foreach (string rawEnemyId in currentWave.enemySequence)
            {
                if (string.IsNullOrWhiteSpace(rawEnemyId)) continue;
                string cleanId = rawEnemyId.Trim();

                if (enemyCatalogDict.TryGetValue(cleanId, out GameObject prefab))
                {
                    spawnQueue.Enqueue(prefab);
                }
                else
                {
                    Debug.LogWarning($"[WaveManager] ไม่พบศัตรู ID '{cleanId}' ใน Catalog!");
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
        isEndingWave = false;

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

    public void RegisterCorpse(GameObject corpse)
    {
        if (corpse == null) return;

        activeCorpses.Enqueue(corpse);

        // หากจำนวนศพในฉากเกิน maxCorpses ให้ลบศพที่เก่าที่สุดออกจาก Queue และทำลายทิ้ง
        while (activeCorpses.Count > maxCorpses)
        {
            GameObject oldestCorpse = activeCorpses.Dequeue();
            if (oldestCorpse != null)
            {
                Destroy(oldestCorpse);
            }
        }
    }

    public void ClearAllCorpses()
    {
        while (activeCorpses.Count > 0)
        {
            GameObject corpse = activeCorpses.Dequeue();
            if (corpse != null)
            {
                Destroy(corpse);
            }
        }
    }

    public void CheckWaveEnd()
    {
        activeEnemies.RemoveAll(item => item == null);

        if (spawnQueue.Count == 0 && activeEnemies.Count == 0 && !isSpawningWave && !isEndingWave)
        {
            isEndingWave = true;
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

        // เคลียร์ศพเก่าทั้งหมดก่อนเริ่มปล่อยซอมบี้ชุดใหม่
        ClearAllCorpses();

        currentWaveIndex++;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentNight(currentWaveIndex + 1);
            GameManager.Instance.SetGameState(GameState.NightDefense);
        }

        StartCoroutine(StartWave(currentWaveIndex));
    }
    
}
