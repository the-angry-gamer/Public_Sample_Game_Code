using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AsteriaWeapons;

[CustomEditor(typeof(WeaponManager), true)]
public class WeaponsManagerEditor : Editor
{
    WeaponManager baseClass;

    public override void OnInspectorGUI()
    {

        baseClass = (WeaponManager)target;

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
            new item("Current Weapon:",     $"{baseClass.CurrentWeaponName}"),
            new item("Current Type:",       $"{baseClass.CurrentWeaponType}"),
        };

        DrawItems(items);

        EditorGUILayout.EndHorizontal();
        drawButtons();

    }

    float decimal2(float f)
    {
        return Mathf.Round(f * 100f) / 100f;
    }

    void DrawHeaderColumn()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Gun Manager", EditorStyles.boldLabel);
        GUILayout.Space(20);
        GUILayout.EndHorizontal();
    }

    void DrawItems(List<item> items)
    {
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
    }

    
    void drawButtons()
    {
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();        
        var width = 250;
        if (GUILayout.Button("Take Out Weapon", GUILayout.Width(width) ) )
        {
            baseClass.TakeOut();
        }
        if (GUILayout.Button("Put Weapon Away", GUILayout.Width(width)))
        {
            baseClass.PutAway();
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        //   GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Next Weapon", GUILayout.Width(width)))
        {
            baseClass.EditorIncrement();
        }

        if (GUILayout.Button("Previous Weapon", GUILayout.Width(width)))
        {
            baseClass.EditorDecrement();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

    }

}

