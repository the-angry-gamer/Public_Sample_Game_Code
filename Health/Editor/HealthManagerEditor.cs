using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AsteriaHealth;


[CustomEditor(typeof(HealthManager))]
public class HealthManagerEditor : Editor
{
    HealthManager baseClass;

    public override void OnInspectorGUI()
    {

        baseClass = (HealthManager)target;

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
            new item("Current Health:", $"{baseClass.CurrentHealth} / {baseClass.TotalHealth}"),
            new item("Last Hit By:",    $"{baseClass.LastObjectHitName}"),
            new item("Last Hit From:",  $"{baseClass.ConnectingItemName}"),
            new item("Last Hit Type:",  $"{baseClass.ConnectingItemName}"),
            new item("Last Hit Damage:",$"{baseClass.LastHitDamage}"),
            new item("Last Hit Type:",  $"{baseClass.LastHitTypeString}"),

        };

        DrawItems(items);
        EditorGUILayout.EndHorizontal();
        drawButton();
        drawHealth();
    }

    float decimal2(float f)
    {
        return Mathf.Round(f * 100f) / 100f;
    }

    void DrawHeaderColumn()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Character Health Stats", EditorStyles.boldLabel);
        GUILayout.Space(20);
        GUILayout.EndHorizontal();
    }

    void DrawItems(List<item> items)
    {
        EditorGUILayout.BeginHorizontal();
        int headerw = 120;
        int valuew = 200;
        int spacing = 20;
        int height = 20;
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
        EditorGUILayout.EndHorizontal();
    }


    void drawButton()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Kill Me Pls"))
        {            
            baseClass.alterHealth(baseClass.CurrentHealth, null);
        }
        EditorGUILayout.EndHorizontal();
    }

    string healthValue = "0";
    void drawHealth()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();
        healthValue = EditorGUILayout.TextField(label: "Do Damage: ", healthValue);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Do Damage"))
        {
            float.TryParse(healthValue, out float damage);
            baseClass.alterHealth(damage, null);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Add Life"))
        {
            float.TryParse(healthValue, out float damage);
            baseClass.alterHealth(-damage, null);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }
}
