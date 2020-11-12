using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private int _score;
    public static int Score
    {
        get { return instance._score; }
        set { instance._score = value; }
    }

    public float startTime;
    private float _timer;
    public static float Timer
    {
        get { return instance._timer; }
        set { instance._timer = value; }
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

        Score = 0;
        Timer = startTime;
    }

    void Update()
    {
        Timer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        ForceManager.FixedUpdate(Time.deltaTime);
    }

    void OnGUI()
    {
        // Set Gui Consts
        Vector2 margin = new Vector2(10, 10);
        Vector2 padding = new Vector2(5, 3);

        string text = $"Time: {Timer:F0}\n" +
                      $"Score: {Score}";

        GUIContent content = new GUIContent(text);
        GUIStyle style = new GUIStyle(GUI.skin.box) {alignment = TextAnchor.UpperLeft, contentOffset = Vector2.one * padding};
        Vector2 size = style.CalcSize(content);
        Rect contentRect = new Rect(
            margin.x + padding.x, margin.y + padding.y,
            size.x + padding.x * 2, size.y + padding.y * 2);

        GUI.Box(contentRect, content, style);
    }
}