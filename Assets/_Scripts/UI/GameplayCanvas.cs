using TMPro;
using UnityEngine;
using static GameEvents;

public class GameplayCanvas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI globalAlertStatus_txt;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameEvents.OnGlobalAlert += OnGlobalAlert;
        GameEvents.OnPlayerDied += OnPlayerDead;
    }

    private void OnDestroy()
    {
        GameEvents.OnGlobalAlert -= OnGlobalAlert;
        GameEvents.OnPlayerDied += OnPlayerDead;
    }

    private void OnGlobalAlert(AlertMessage alert)
    {
        globalAlertStatus_txt.text = $"Global Alert : {alert.target != null}";
        globalAlertStatus_txt.color = alert.target != null ? Color.red : Color.white;
    }

    private void OnPlayerDead()
    {
        globalAlertStatus_txt.text = $"Global Alert : false";
        globalAlertStatus_txt.color = Color.white;
    }
}
