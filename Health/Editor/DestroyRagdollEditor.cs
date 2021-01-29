
using UnityEngine;
using UnityEditor;

namespace AsteriaHealth
{
    [CustomEditor(typeof(DestroyRagdoll))]

    public class DestroyRagdollEditor : Editor
    {
        DestroyRagdoll baseClass;

        public override void OnInspectorGUI()
        {
            baseClass = (DestroyRagdoll)target;

            base.OnInspectorGUI();
            drawButton();
        }

        void drawButton()
        {
            GUILayout.Space(5f);

            if (baseClass.isRagdoll)
            {
                if (GUILayout.Button("Undo Ragdoll", GUILayout.Height(30)))
                {
                    baseClass.Reanimate();
                }
            }
            GUILayout.Space(10f);
        }
    }
}
