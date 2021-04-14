using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Dino
{
    [CustomEditor(typeof(Ragdoll))]
    public class RagdollEditor : UnityEditor.Editor
    {

        Ragdoll baseClass;

        public override void OnInspectorGUI()
        {
            baseClass = (Ragdoll)target;

            base.OnInspectorGUI();
            drawButton();
        }

        void drawButton()
        {
            if (!baseClass.IsReadyForRagdoll) { return; }


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

}