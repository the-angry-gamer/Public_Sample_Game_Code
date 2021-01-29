using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AsteriaWeapons;

[CustomEditor(typeof(WeaponBase), true)]
public class WeaponBaseEditor : Editor
{
    WeaponBase baseClass;

    public override void OnInspectorGUI()
    {

        baseClass = (WeaponBase)target;

        drawCurrent();
        drawButton();

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
            new item("Current Ammo:",       $"{baseClass.CurrentClip}/{baseClass.MaxClip}  ({baseClass.CurrentAmmo})"),
            new item("Last Fire Time:",     $"{baseClass.LastFireTime}"),
            //new item("Can Fire:",           $"{baseClass.FiringConditionsMet}"),
            new item("Fire Intervals (s):", $"{baseClass.FireRateIntervals}"),
            new item("Recoil (x,y):",       $"x: {decimal2( baseClass.CurrentRecoil.x)} / y: {decimal2( baseClass.CurrentRecoil.y)}"),
            new item("Is Reloading: ",      $"{baseClass.IsReloading}"),
        };

        DrawItems(items);

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    ///     Create a float to the second decimal
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    float decimal2(float f)
    {
        return Mathf.Round(f * 100f) / 100f;
    }

    void DrawHeaderColumn()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Gun Stats", EditorStyles.boldLabel);
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

    string ammo = "0";
    void drawButton()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginVertical();
        ammo = EditorGUILayout.TextField(label: "Add Ammo: ", ammo);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("Add Specific Ammo"))
        {
            int.TryParse(ammo, out int damage);
            baseClass.AddAmmo(damage);
        }
        if (GUILayout.Button("Add Max Ammo"))
        {
            baseClass.AddAmmo(baseClass.AmmoLimit);
        }

        EditorGUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

}
