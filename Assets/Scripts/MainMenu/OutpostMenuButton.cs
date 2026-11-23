using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public sealed class OutpostMenuButton : Button
{
    [SerializeField] private RectTransform emblem;
    [SerializeField] private RawImage impact;
    [SerializeField] private float entranceDelay;
    private CanvasGroup visibility;
    private CanvasGroup menuInput;
    private float elapsed;
    private float focus;
    private float strike;
    private bool hovered;
    private bool selected;
    private bool committing;

    protected override void Awake()
    {
        base.Awake();
        visibility = GetComponent<CanvasGroup>();
        menuInput = transform.parent.GetComponent<CanvasGroup>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        elapsed = focus = 0f;
        hovered = selected = committing = false;
        strike = 1f;
    }

    protected override void OnDisable()
    {
        StopAllCoroutines();
        if (committing && menuInput) menuInput.interactable = true;
        committing = false;
        base.OnDisable();
    }

    private void Update()
    {
        if (!Application.isPlaying || !emblem || !impact) return;
        elapsed += Time.unscaledDeltaTime;
        focus = Mathf.MoveTowards(focus, hovered || selected ? 1f : 0f, Time.unscaledDeltaTime * 8f);
        float reveal = Mathf.SmoothStep(0f, 1f, (elapsed - entranceDelay) / 0.28f);
        visibility.alpha = reveal;
        if (committing) return;
        strike += Time.unscaledDeltaTime;
        float ring = Mathf.Sin(28f * strike) * Mathf.Exp(-11f * strike);
        emblem.localScale = Vector3.one * (0.92f + 0.08f * reveal + 0.11f * focus + 0.12f * ring);
        emblem.localRotation = Quaternion.Euler(0f, 0f, 5f * ring - (1f - reveal) * 10f);
        SetImpact(0.32f * Mathf.Max(0f, 1f - strike / 0.2f), 1f + 0.22f * Mathf.Clamp01(strike / 0.2f));
    }

    private void SetImpact(float alpha, float scale)
    {
        impact.color = new Color(0.9f, 0.76f, 0.69f, alpha);
        impact.rectTransform.localScale = Vector3.one * scale;
    }

    public override void OnPointerEnter(PointerEventData e) { base.OnPointerEnter(e); hovered = true; strike = 0f; }
    public override void OnPointerExit(PointerEventData e) { base.OnPointerExit(e); hovered = false; }
    public override void OnSelect(BaseEventData e) { base.OnSelect(e); selected = true; strike = 0f; }
    public override void OnDeselect(BaseEventData e) { base.OnDeselect(e); selected = false; }
    public override void OnPointerClick(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Left) Equip();
    }
    public override void OnSubmit(BaseEventData e) => Equip();

    private void Equip()
    {
        if (IsActive() && IsInteractable() && !committing) StartCoroutine(SeatEmblem());
    }

    private IEnumerator SeatEmblem()
    {
        committing = true;
        menuInput.interactable = false;
        float startScale = emblem.localScale.x;
        for (float t = 0f; t < 0.32f; t += Time.unscaledDeltaTime)
        {
            float lift = Mathf.SmoothStep(0f, 1f, t / 0.09f);
            float hit = Mathf.SmoothStep(0f, 1f, (t - 0.09f) / 0.055f);
            float settle = Mathf.SmoothStep(0f, 1f, (t - 0.145f) / 0.15f);
            emblem.localScale = Vector3.one * (Mathf.Lerp(startScale, 1.32f, lift) - 0.45f * hit + 0.13f * settle);
            emblem.localRotation = Quaternion.Euler(0f, 0f, -12f * lift + 15f * hit - 3f * settle);
            float burst = Mathf.Clamp01((t - 0.11f) / 0.20f);
            SetImpact(hit * (1f - burst), 0.9f + 0.55f * burst);
            yield return null;
        }
        SetImpact(0f, 1f);
        menuInput.interactable = true;
        committing = false;
        onClick.Invoke();
    }
}
