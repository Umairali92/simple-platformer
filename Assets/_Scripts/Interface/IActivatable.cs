using UnityEngine;

public interface IActivatable
{
    void SetActive(bool active);
    bool IsActive { get; }
}
