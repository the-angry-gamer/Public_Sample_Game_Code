using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace PathFindingAsteria
{

    [CustomEditor(typeof(UpdateGridAgent))]

    public class UpdateGridAgentEditor : Editor
    {
        UpdateGridAgent agent;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            agent = (UpdateGridAgent)target;
            drawStats();

        }

        /// <summary>
        ///     Draw how much time this is taking
        /// </summary>
        void drawStats()
        {

            GUILayout.Space(10);

            GUILayout.Label("Grid Carving Stats", EditorStyles.boldLabel);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Space(15);

            Dictionary<string, string> mappings = new Dictionary<string, string>()
            {
                { "Grid Manager",       agent.GridName },
                { "Object Location",    agent.PreviuosPosition.ToString() },
                { "Last Created",       $"{agent.changeTime} in game time" },
                { "Creation Time",      $"{agent.timeTaken} in game time" },
                { "Nodes Created",      $"{agent.nodesCreated}" },
                { "Assigned Grids",     $"{agent.gridsAssigned}" }
            };

            drawColumns(mappings);

            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
        }

        /// <summary>
        ///     Draw the statistics columns
        /// </summary>
        /// <param name="values"></param>
        void drawColumns(Dictionary<string, string> values)
        {
            int vert = 5;
            int spacing = 5;

            GUILayout.BeginVertical(GUILayout.MinWidth(110));
            GUILayout.Space(vert);
            foreach (KeyValuePair<string,string> value in values)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20));

                GUILayout.Label($"{value.Key}: ", EditorStyles.boldLabel);
                GUILayout.Space(spacing);
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Space(vert);

            foreach (KeyValuePair<string,string> value in values)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20));

                GUILayout.Label($"{value.Value}");
                GUILayout.Space(spacing);
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();

        }

    }
}
