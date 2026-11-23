using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public struct EnemyCatalogEntry
{
    public string enemyId;
    public GameObject enemyPrefab;
    public EnemyAttackType attackType;
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
    public GameObject[] spawnOrder;
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

    [Header("Night Waves")]
    [SerializeField] private List<MixedNightWaveData> nightWaves;
    public int currentWaveIndex = 0;
    [SerializeField] private GameObject runSetupPanel;
    [SerializeField] private TMP_InputField nightsInput;
    [SerializeField] private Button beginButton;
    [SerializeField] private TextMeshProUGUI setupHint;
    private int totalNights = 7;
    private bool runStarted;
    private bool endingWave;
    private bool shopOpen;


    [Header("Enemy Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Morning Shop")]
    [SerializeField] private GameObject morningScreenPanel;
    [SerializeField] private Button nextNightButton;
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("Victory Screen")]
    [SerializeField] private GameObject victoryPanel;

    [Header("Wave End Delay")]
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

        if (runSetupPanel != null)
        {
            Time.timeScale = 0f;
            runSetupPanel.SetActive(true);
            nightsInput.SetTextWithoutNotify("7");
            nightsInput.onValueChanged.AddListener(ValidateNightCount);
            ValidateNightCount(nightsInput.text);
        }
        else BeginRun();
    }

    private void ValidateNightCount(string value)
    {
        bool valid = int.TryParse(value, out int count) && count > 0;
        beginButton.interactable = valid;
        setupHint.text = valid ? "Survive the final night to win." : "Enter a whole number of at least 1.";
    }

    public void BeginRun()
    {
        if (runStarted) return;
        if (nightsInput != null && (!int.TryParse(nightsInput.text, out totalNights) || totalNights < 1)) return;
        runStarted = true;
        currentWaveIndex = 0;
        if (runSetupPanel != null) runSetupPanel.SetActive(false);
        GameManager.Instance.SetCurrentNight(1);
        GameManager.Instance.SetGameState(GameState.NightDefense);
        Time.timeScale = 1f;
        StartCoroutine(StartWave(0));
    }

    public IEnumerator StartWave(int waveIndex)
    {
        if (nightWaves == null || nightWaves.Count == 0 || waveIndex >= totalNights)
        {
            yield break;
        }

        MixedNightWaveData currentWave = nightWaves[Mathf.Min(waveIndex, nightWaves.Count - 1)];
        Debug.Log($"--- เริ่มต้น {currentWave.nightName} (Wave {waveIndex + 1}/{totalNights}) ---");

        spawnQueue.Clear();

        if (currentWave.enemySequence != null && currentWave.enemySequence.Count > 0)
        {
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
        else if (currentWave.spawnOrder != null && currentWave.spawnOrder.Length > 0)
        {
            foreach (GameObject prefab in currentWave.spawnOrder)
            {
                if (prefab != null) spawnQueue.Enqueue(prefab);
            }
        }
        else if (currentWave.enemyPool != null && currentWave.enemyPool.Count > 0)
        {
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

        // Spawn enemies from the queue at the configured interval.
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

        Debug.Log($"Enemy defeated. Active: {activeEnemies.Count}, queued: {spawnQueue.Count}.");
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

        if (runStarted && !endingWave && spawnQueue.Count == 0 && activeEnemies.Count == 0 && !isSpawningWave)
        {
            endingWave = true;
            StartCoroutine(WaitAndEndWaveCoroutine());
        }
    }

    private IEnumerator WaitAndEndWaveCoroutine()
    {
        Debug.Log("Wave cleared. Waiting for the end-of-wave transition.");
        yield return new WaitForSeconds(delayAfterWave);

        if (currentWaveIndex >= totalNights - 1)
        {
            Debug.Log("Victory! All nights survived.");
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
            // Award a supply drop before opening the morning shop.
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

        shopOpen = true;
        int nextNightNumber = currentWaveIndex + 2;

        if (buttonText != null)
        {
            buttonText.text = $"Night {nextNightNumber} / {totalNights}";
        }

        Time.timeScale = 0f;
    }

    public void OnProceedToNextNight()
    {
        if (!shopOpen) return;
        shopOpen = false;
        endingWave = false;
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

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    private void OnDestroy()
    {
        if (isQuitting || !gameObject.scene.isLoaded) return;
    }
}
