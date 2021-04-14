using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AsteriaHealth;
using UnityEditor;

[CustomEditor(typeof(SeperateRagdollDeath))]
public class RagdollDeathEditor : Editor
{
    SeperateRagdollDeath baseClass;

    public override void OnInspectorGUI()
    {
        baseClass = (SeperateRagdollDeath)target;

        base.OnInspectorGUI();
        drawButton();
    }

    void drawButton()
    {
        if ( !baseClass.isSourceSet ) { return; }

        GUILayout.Space(5f);

        if (!baseClass.isRagdoll)
        {
            if (GUILayout.Button("Force Ragdoll", GUILayout.Height(30)))
            {
                baseClass.EnableRagdoll();
            }
        }
        else 
        { 
            if (GUILayout.Button("Reanimate", GUILayout.Height(30)))
            {
                baseClass.Reanimate();
            }
        }

        GUILayout.Space(10f);
    }
}
