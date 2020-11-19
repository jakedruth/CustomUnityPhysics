using System;
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private ParticleManager _particleManager;

    public static ParticleManager ParticleManager
    {
        get { return instance._particleManager; }
    }

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        _particleManager = new ParticleManager(int.MaxValue, int.MaxValue);
    }

    void FixedUpdate()
    {
        ParticleManager.FixedUpdate(Time.deltaTime);
    }
}