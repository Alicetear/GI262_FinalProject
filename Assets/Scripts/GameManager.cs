using System;
using UnityEngine;

public enum CurrencyType
{
    Cash,
    Scrap,
    Rations
}

public enum GameState
{
    NightDefense,
    MorningShop,
    GameOver,
    Victory
}

public class GameManager : MonoBehaviour
{
    private static bool isQuitting = false;
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            // ป้องกันการสร้าง GameManager (Auto) ขึ้นมาใหม่ตอนที่แอปพลิเคชันกำลังปิดตัว
            if (isQuitting) return null;

            if (instance == null)
            {
                instance = FindAnyObjectByType<GameManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("GameManager (Auto)");
                    instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    [Header("ระบบเศรษฐกิจ (3 Currencies)")]
    [SerializeField] private int cash = 0;        // ใช้ซื้อปืน/อัปเกรดปืน (เริ่มต้น $0)
    [SerializeField] private int scrap = 0;       // ใช้ซ่อมแซม/อัปเกรดกำแพง
    [SerializeField] private int rations = 0;     // ใช้จ้างเพื่อน AI/ฟื้นฟูเลือด

    [Header("คะแนนและคืนปัจจุบัน")]
    [SerializeField] private int score = 0;
    [SerializeField] private int currentNight = 1;

    [Header("สถานะเกม")]
    public GameState currentState = GameState.NightDefense;

    public event Action OnCurrencyChanged;
    public event Action<int> OnScoreChanged;

    public int Cash => cash;
    public int Scrap => scrap;
    public int Rations => rations;
    public int Score => score;
    public int CurrentNight => currentNight;

    private void Awake()
    {
        isQuitting = false;

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // คืนที่ 1 เริ่มต้นเงิน (Cash) ที่ $0 เสมอ
            if (currentNight <= 1)
            {
                cash = 0;
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public void AddCurrency(CurrencyType type, int amount)
    {
        if (amount <= 0) return;

        switch (type)
        {
            case CurrencyType.Cash:
                cash += amount;
                break;
            case CurrencyType.Scrap:
                scrap += amount;
                break;
            case CurrencyType.Rations:
                rations += amount;
                break;
        }

        Debug.Log($"ได้รับ {type}: +{amount} (Cash={cash}, Scrap={scrap}, Rations={rations})");
        OnCurrencyChanged?.Invoke();
    }

    public bool SpendCurrency(CurrencyType type, int amount)
    {
        if (amount < 0) return false;

        switch (type)
        {
            case CurrencyType.Cash:
                if (cash >= amount)
                {
                    cash -= amount;
                    OnCurrencyChanged?.Invoke();
                    return true;
                }
                break;

            case CurrencyType.Scrap:
                if (scrap >= amount)
                {
                    scrap -= amount;
                    OnCurrencyChanged?.Invoke();
                    return true;
                }
                break;

            case CurrencyType.Rations:
                if (rations >= amount)
                {
                    rations -= amount;
                    OnCurrencyChanged?.Invoke();
                    return true;
                }
                break;
        }

        Debug.LogWarning($"ทรัพยากรไม่พอ! ต้องการ {type}: {amount}");
        return false;
    }

    public void AddScore(int amount)
    {
        score += amount;
        OnScoreChanged?.Invoke(score);
    }

    public void SetCurrentNight(int night)
    {
        currentNight = night;
    }

    public void SetGameState(GameState newState)
    {
        currentState = newState;
    }

    public void TriggerGameOver()
    {
        currentState = GameState.GameOver;
        Debug.Log("GameManager: Game Over!");
    }

    public void TriggerVictory()
    {
        currentState = GameState.Victory;
        Debug.Log("GameManager: Victory!");
    }
}

