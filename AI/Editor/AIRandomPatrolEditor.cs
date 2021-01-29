using UnityEditor;
using UnityEngine;

namespace AI_Asteria
{
    [CustomEditor(typeof(AIRandomState_PatrolWaypoint_1),true)]

    public class AIRandomPatrolEditor : AIStateBaseEditor
    {
        AIRandomState_PatrolWaypoint_1 bClass;


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            bClass = (AIRandomState_PatrolWaypoint_1)target;
            GUILayout.Space(10f);

            DrawType();                        
        }


        void DrawType()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
 
            GUILayout.Space(10f);
            GUILayout.Label("Patrol Stats", EditorStyles.boldLabel);
            GUILayout.Space(10f);
            drawHorizontal( "Current Waypoint",     bClass.GetCurrentWayPoint.ToString());
            drawHorizontal( "Next Waypoint",        bClass.GetNextWaypoint.ToString());
            drawHorizontal( "Item Position",        bClass.GetPlayerLocation.ToString());
            drawHorizontal( "Target Position",      bClass.GetTargetLocation.ToString()     ?? "");
            drawHorizontal( "Waypoint Position",    bClass.EndTargetLocation.ToString()     ?? "");
            drawHorizontal( "Straight Obstruction", bClass.straightOutHit?.name.ToString()       ?? "Nothing");
          
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        void drawHorizontal(string header, string text)
        {
            int hor = 140;

            GUILayout.BeginHorizontal(GUILayout.MinWidth(hor));
            GUILayout.Space(15);
            GUILayout.Label($"{header}: ", EditorStyles.boldLabel, GUILayout.MinWidth(hor));
            GUILayout.Label(text);
            GUILayout.Space(15);
            GUILayout.EndHorizontal();
        }
    }
}
