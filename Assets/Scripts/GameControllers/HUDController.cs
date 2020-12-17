using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public static HUDController instance;
    public Transform powerLevelTransform;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public static void SetPowerLevel(float t, Color color)
    {
        instance.powerLevelTransform.localScale = new Vector3(t, 1, 1);
        instance.powerLevelTransform.GetComponent<Image>().color = color;
    }

}
