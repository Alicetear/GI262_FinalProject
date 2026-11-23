using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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

    [SerializeField] private int maxCorpses = 20;
    private readonly Queue<GameObject> activeCorpses = new Queue<GameObject>();

    [Header("Night Waves")]
    [SerializeField] private List<MixedNightWaveData> nightWaves;
    public int currentWaveIndex = 0;
    public bool RunStarted => runStarted;
    private bool runStarted;
    private bool endingWave;
    private bool shopOpen;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private TextMeshProUGUI pauseSaveHint;
    private CanvasGroup shopInput;
    private float previousTimeScale;
    private GameObject previousSelection;
    private int resumeFrame = -1;
    public bool PauseBlocksInput => (pausePanel != null && pausePanel.activeSelf) || Time.frameCount == resumeFrame;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) TogglePause();
    }

    public void TogglePause()
    {
        if (!runStarted || pausePanel == null) return;
        var state = GameManager.Instance.currentState;
        if (state != GameState.NightDefense && state != GameState.MorningShop) return;
        var events = UnityEngine.EventSystems.EventSystem.current;
        if (pausePanel.activeSelf)
        {
            pausePanel.SetActive(false);
            if (shopOpen && shopInput != null) shopInput.interactable = true;
            Time.timeScale = previousTimeScale;
            resumeFrame = Time.frameCount;
            if (events != null) events.SetSelectedGameObject(previousSelection != null && previousSelection.activeInHierarchy ? previousSelection : null);
        }
        else
        {
            previousTimeScale = Time.timeScale;
            previousSelection = events != null ? events.currentSelectedGameObject : null;
            if (shopOpen && shopInput != null) shopInput.interactable = false;
            Time.timeScale = 0f;
            pauseSaveHint.text = shopOpen ? "Continue will reopen this shop with your purchases." :
                $"Continue will restart Night {currentWaveIndex + 1} from its checkpoint.";
            pausePanel.SetActive(true);
            resumeButton.Select();
        }
    }


    [Header("Enemy Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Morning Shop")]
    [SerializeField] private GameObject morningScreenPanel;
    [SerializeField] private TextMeshProUGUI morningHeading;
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
        if (morningScreenPanel != null)
        {
            shopInput = morningScreenPanel.GetComponent<CanvasGroup>();
            if (shopInput == null) shopInput = morningScreenPanel.AddComponent<CanvasGroup>();
            morningScreenPanel.SetActive(false);
        }
        if (victoryPanel != null) victoryPanel.SetActive(false);

        if (nextNightButton != null)
        {
            nextNightButton.onClick.RemoveAllListeners();
            nextNightButton.onClick.AddListener(OnProceedToNextNight);
        }

        if (RunCheckpoint.ResumeRequested)
        {
            RunCheckpoint.ResumeRequested = false;
            if (RunCheckpoint.TryLoad(out var saved)) { ResumeRun(saved); return; }
        }
        BeginRun();
    }

    private RunCheckpoint nightStartCheckpoint;

    public void SaveAndReturnToMenu()
    {
        if (!runStarted || pausePanel == null || !pausePanel.activeSelf) return;
        RunCheckpoint.Store(shopOpen ? CaptureCheckpoint() : nightStartCheckpoint);
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private RunCheckpoint CaptureCheckpoint()
    {
        var gm = GameManager.Instance;
        var wall = Barricade.Instance;
        var data = new RunCheckpoint {
            nights = RunCheckpoint.CampaignNights, wave = currentWaveIndex, shopping = shopOpen,
            cash = gm.Cash, scrap = gm.Scrap, rations = gm.Rations, score = gm.Score,
            wallHP = wall.CurrentHealth, wallMax = wall.MaxHealth
        };
        ShopManager.Instance.Capture(data);
        return data;
    }

    private void ResumeRun(RunCheckpoint data)
    {
        runStarted = true;
        currentWaveIndex = data.wave;
        var gm = GameManager.Instance;
        gm.RestoreBalances(data.cash, data.scrap, data.rations, data.score);
        gm.SetCurrentNight(currentWaveIndex + 1);
        Barricade.Instance.RestoreHealth(data.wallHP, data.wallMax);
        ShopManager.Instance.Restore(data);
        if (data.shopping)
        {
            endingWave = true;
            gm.SetGameState(GameState.MorningShop);
            ShowMorningScreen();
        }
        else
        {
            nightStartCheckpoint = data;
            gm.SetGameState(GameState.NightDefense);
            Time.timeScale = 1f;
            StartCoroutine(StartWave(currentWaveIndex));
        }
    }

    private void BeginRun()
    {
        if (runStarted) return;
        runStarted = true;
        currentWaveIndex = 0;
        GameManager.Instance.ResetForNewRun();
        GameManager.Instance.SetCurrentNight(1);
        GameManager.Instance.SetGameState(GameState.NightDefense);
        Time.timeScale = 1f;
        nightStartCheckpoint = CaptureCheckpoint();
        StartCoroutine(StartWave(0));
    }

    public IEnumerator StartWave(int waveIndex)
    {
        if (nightWaves == null || nightWaves.Count == 0 || waveIndex >= RunCheckpoint.CampaignNights)
        {
            yield break;
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

        if (currentWaveIndex >= RunCheckpoint.CampaignNights - 1)
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
        if (morningHeading != null) morningHeading.text = $"MORNING - NIGHT {currentWaveIndex + 1} CLEARED";
        int nextNightNumber = currentWaveIndex + 2;

        if (buttonText != null)
        {
            buttonText.text = $"START NIGHT {nextNightNumber}";
        }

        Time.timeScale = 0f;
    }

    public void OnProceedToNextNight()
    {
        if (!shopOpen || PauseBlocksInput) return;
        shopOpen = false;
        endingWave = false;
        if (morningScreenPanel != null) morningScreenPanel.SetActive(false);
        Time.timeScale = 1f;

        ClearAllCorpses();
        currentWaveIndex++;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentNight(currentWaveIndex + 1);
            GameManager.Instance.SetGameState(GameState.NightDefense);
        }

        nightStartCheckpoint = CaptureCheckpoint();
        StartCoroutine(StartWave(currentWaveIndex));
    }


}
