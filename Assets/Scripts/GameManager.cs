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
    private static GameManager instance;
    private static bool isQuitting = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        isQuitting = false;
        instance = null;
    }

    public static GameManager Instance
    {
        get
        {
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
    [SerializeField] private int cash = 200;      // Used to buy and upgrade weapons.
    [SerializeField] private int scrap = 50;      // Used to repair and reinforce the barricade.
    [SerializeField] private int rations = 1;     // Used to recruit companions.

    [Header("Score and Night")]
    [SerializeField] private int score = 0;
    [SerializeField] private int currentNight = 1;

    [Header("Game State")]
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
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Enforce the minimum starting balances on scene values.
            if (cash < 200) cash = 200;
            if (scrap < 50) scrap = 50;
            if (rations < 1) rations = 1;
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

        Debug.Log($"Gained {amount} {type}. Cash: {cash}, Scrap: {scrap}, Rations: {rations}.");
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

        Debug.LogWarning($"Insufficient {type}. Required: {amount}.");
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

