using UnityEngine;

public class SessionManager : MonoBehaviour
{
    private void OnDestroy()
    {
        GameEvents.ClearAll();
    }
}
