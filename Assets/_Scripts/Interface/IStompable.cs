using UnityEngine;

public interface IStompable
{
    bool CanBeStompKilled { get; }
    void OnStomped();
}
