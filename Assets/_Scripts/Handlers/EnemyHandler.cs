using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyHandler : MonoBehaviour
{
    [SerializeField] private List<EnemyCore> enemies;

    public void Awake()
    {
        Init();
    }

    public void Init()
    {
        foreach (var enemy in enemies)
        {
            enemy.Init();
        }
    }
}
