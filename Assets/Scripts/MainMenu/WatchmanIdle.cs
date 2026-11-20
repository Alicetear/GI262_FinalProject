using UnityEngine;
using UnityEngine.UI;

// Side-view cycle with a planted foot for 62% of each stride.
[RequireComponent(typeof(RawImage))]
public sealed class WatchmanIdle : MonoBehaviour
{
    private const int FrameCount = 20;
    private const float RouteCycles = 6f;
    private const float StrideInHeights = 30f / (0.62f * 144f);

    [SerializeField] private float cycleDuration = 1.8f;
    [SerializeField] private float phase;
    [SerializeField] private bool walkLeft;
    private RawImage figure;
    private Vector2 origin;
    private Color tint;
    private float elapsed;

    private void Awake()
    {
        figure = GetComponent<RawImage>();
        origin = figure.rectTransform.anchoredPosition;
        tint = figure.color;
    }

    private void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        float cycles = elapsed / Mathf.Max(0.1f, cycleDuration) + phase * RouteCycles;
        int frame = Mathf.FloorToInt(Mathf.Repeat(cycles, 1f) * FrameCount);
        int direction = walkLeft ? -1 : 1;
        float route = Mathf.Repeat(cycles, RouteCycles);
        float displacement = (route - RouteCycles * 0.5f) * StrideInHeights * figure.rectTransform.rect.height;
        figure.rectTransform.anchoredPosition = origin + Vector2.right * (displacement * direction);
        figure.uvRect = new Rect((frame + (walkLeft ? 1f : 0f)) / FrameCount, 0f, direction / (float)FrameCount, 1f);
        // Disappear into the mist before looping back to the start of the route.
        float mist = Mathf.Clamp01(Mathf.Min(route, RouteCycles - route) / 0.65f);
        Color color = tint;
        color.a *= Mathf.SmoothStep(0f, 1f, mist);
        figure.color = color;
    }
}
