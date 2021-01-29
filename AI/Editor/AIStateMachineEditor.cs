using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace AI_Asteria
{
    [CustomEditor(typeof(AIStateMachine), true)]

    public class AIStateMachineEditor : Editor
    {

        AIStateMachine bClass; 
        
 
        public override void OnInspectorGUI()
        {
            if (!bClass) { bClass = (AIStateMachine)target; }
            DrawType();
            GUILayout.Space(10f);

            base.OnInspectorGUI();
         
        }

        void DrawType()
        {
            if (bClass.States == null)
            {
                return;
            }

            int c = 0;
            foreach(var state in bClass.States)
            {
                c += state.Value.Count;
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Space(10f);
            GUILayout.Label("State Machine Stats",  EditorStyles.boldLabel      );
            drawHorizontal("Current State",         bClass.CurrentStateType     );
            drawHorizontal("Previous State",        bClass.PreviousStateType    );
            
            if (bClass.States.Count > 0)
            {

                GUILayout.Space(10);
                drawHorizontal("State Count",           c.ToString() );
                foreach (var state in bClass.States)
                {               
                    drawHorizontal( state.Key.ToString(), state.Value.Count.ToString() );
                    foreach(var statebase in state.Value)
                    {
                        drawHorizontal( "", statebase.ToString() );          
                    }
                }
            }

            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        void drawHorizontal(string header, string text)
        {
            int hor = 140;

            GUILayout.BeginHorizontal(GUILayout.MinWidth(hor));
            GUILayout.Space(15);
            var t = header == string.Empty ? string.Empty : header + ":";
            GUILayout.Label($"{t} ", EditorStyles.boldLabel, GUILayout.MinWidth(hor));
            GUILayout.Label(text);
            GUILayout.Space(15);
            GUILayout.EndHorizontal();
        }

    }

}