using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AsteriaEnvironment;

[CustomEditor(typeof(LookAtTarget))]
public class LookAtTargetEditor : Editor
{
        LookAtTarget baseClass;

     public override void OnInspectorGUI()
    {

        baseClass = (LookAtTarget)target;

        drawCurrent();

        GUILayout.Space(20);

        base.OnInspectorGUI();

    }

    struct item
    {
        public string header;
        public string value;

        public item(string h, string v)
        {
            header = h;
            value = v;
        }

    }
    void drawCurrent() 
    {
        DrawHeaderColumn();

        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        GUILayout.Space(5);
        List<item> items = new List<item>()
        {
            new item("Target:",             $"{baseClass.ObjectName}"),
            new item("Target Tag:",         $"{baseClass.ObjectName}"),
            new item("Current Rotation:",   $"{baseClass.CurrentRotation}"),
            new item("Target Rotation:",    $"{baseClass.TargetRotation}"),
            new item("Target Offset:",      $"{baseClass.RequiredRotation}")

        };

        DrawItems(items);
        EditorGUILayout.EndHorizontal();
    }

    void DrawHeaderColumn()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Looking Info", EditorStyles.boldLabel);
        GUILayout.Space(20);
        GUILayout.EndHorizontal();
    }

    void DrawItems(List<item> items)
    {
        int headerw = 120;
        int valuew  = 200;
        int spacing = 20;
        int height  = 20;
        // Headers
        GUILayout.BeginVertical(GUILayout.MinWidth(headerw));
        GUILayout.Space(10);
        foreach (item i in items)
        {
            GUILayout.BeginHorizontal(GUILayout.MinHeight(height));
            GUILayout.Label($"{i.header} ", EditorStyles.boldLabel);
            GUILayout.Space(spacing);
            GUILayout.EndHorizontal();

        }
        GUILayout.EndVertical();
        GUILayout.BeginVertical(GUILayout.MinWidth(valuew));
        GUILayout.Space(10);
        // values
        foreach (item i in items)
        {
            GUILayout.BeginHorizontal(GUILayout.MinHeight(height));
            GUILayout.Label($"{i.value} ");
            GUILayout.Space(spacing);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();


    }

}
