using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelHandler : MonoBehaviour
{
    public Transform geometryHolder;
    public MyRigidBody golfBall;
    public float gravity;
    private bool _displayGUIControls = true;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < geometryHolder.childCount; i++)
        {
            Transform child = geometryHolder.GetChild(i);
            Primitive.Plane plane = new Primitive.Plane(child.up, child.position);
            WorldManager.StaticPlanes.Add(plane);
        }

        GravityFG gravityFG = new GravityFG(Vector3.down * gravity);
        ForceManager.Add(gravityFG, golfBall);
    }

    public GUIStyle guiStyle;

    private void OnGUI()
    {
        const int margin = 10;
        const int padding = 5;
        const int rowHeight = 35;

        int width = 520;
        int height = 125;

        if (!_displayGUIControls)
        {
            width = 250;
            height = 35;
        }

        GUIStyle boxStyle = new GUIStyle(GUI.skin.box) {alignment = TextAnchor.UpperLeft, fontSize = 24};
        GUIStyle contentStyle = new GUIStyle(GUI.skin.label) {fontSize = 20};

        GUI.Box(new Rect(margin, margin, width, height), "Golf Controls", boxStyle);

        // A button to toggle the displays
        string hideOrShowText = _displayGUIControls ? "[Hide]" : "[Expand]";
        if (GUI.Button(new Rect(160, margin, 100, 35), hideOrShowText,
            new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleLeft, fontSize = 24}))
        {
            _displayGUIControls = !_displayGUIControls;
        }

        // return if the displays are not being shown
        if (!_displayGUIControls)
            return;

        Rect controlsRect = new Rect(margin + padding * 2, margin + padding + 35, 500, rowHeight * 3);
        string msg = "Set Power Level:\tHold Left Mouse and Aim\n" +
                     "Hit Ball:\t\tRelease Left Mouse\n" +
                     "Move Camera:\tHold Right Mouse Down and Move";
        GUI.Label(controlsRect, msg, contentStyle);

    }
}
