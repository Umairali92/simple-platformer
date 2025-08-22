using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class AwarenessIndicatorUI : MonoBehaviour
{
    [SerializeField] private AwarenessModule source;
    [SerializeField] private RectTransform anchor; // optional: head bone; if null uses this transform
    [SerializeField] private Image fillImage;      // radial or horizontal fill
    [SerializeField] private Gradient colorByLevel; // 0..1 color gradient
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 2.0f, 0);

    Camera _cam;

    void Awake() { _cam = Camera.main; }
    void OnEnable()
    {
        if (source != null)
        {
            source.AwarenessChanged += OnAwareness;
            source.StageChanged += OnStageChanged;
            OnAwareness(source.Awareness01);
        }
    }
    void OnDisable()
    {
        if (source != null)
        {
            source.AwarenessChanged -= OnAwareness;
            source.StageChanged -= OnStageChanged;
        }
    }

    void LateUpdate()
    {
        if (!_cam) _cam = Camera.main;
        var a = anchor ? anchor.position : transform.parent ? transform.parent.position : transform.position;
        transform.position = a + worldOffset;
        // billboard toward camera (side-view)
        transform.forward = _cam ? _cam.transform.forward : Vector3.forward;
    }

    void OnAwareness(float v)
    {
        if (fillImage)
        {
            fillImage.fillAmount = Mathf.Clamp01(v);
            if (colorByLevel.colorKeys.Length > 0)
                fillImage.color = colorByLevel.Evaluate(v);
        }
    }

    void OnStageChanged(AwarenessStage s)
    {
        // optional: play vfx/sfx per stage
    }
}
