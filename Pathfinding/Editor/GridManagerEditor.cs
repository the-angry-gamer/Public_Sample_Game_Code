using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PathFindingAsteria
{
    [CustomEditor(typeof(GridManager))]
    public class GridManagerEditor : Editor
    {
        GridManager gm;
        public override void OnInspectorGUI()
        {
            gm = (GridManager)target;

            drawStats();
            DrawButtons();

            base.OnInspectorGUI();
        }

        /// <summary>
        ///     Draw our creation button
        /// </summary>
        void DrawButtons()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate Grid"))
            {
                gm.RecreateGraph();
            }

            if (GUILayout.Button("Generate Paths"))
            {
                gm.RecreatePaths();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(15);

        }

        /// <summary>
        ///     Draw how much time this is taking
        /// </summary>
        void drawStats()
        {

            GUILayout.Space(10);

            GUILayout.Label("Path Statistics", EditorStyles.boldLabel);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Space(15);

            Dictionary<string, string> mappings = new Dictionary<string, string>()
            {
                {"Grid Time", $"{gm.GridTime.ToString()} seconds" },
                {"Nodes Created", gm.NodeCount.ToString() }
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

            GUILayout.BeginVertical(GUILayout.MinWidth(150));
            GUILayout.Space(vert);
            foreach (KeyValuePair<string,string> value in values)
            {
                GUILayout.Label($"{value.Key}: ", EditorStyles.boldLabel);
                GUILayout.Space(spacing);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Space(vert);

            foreach (KeyValuePair<string,string> value in values)
            {
                GUILayout.Label($"{value.Value}");
                GUILayout.Space(spacing);
            }
            GUILayout.EndVertical();

        }
    }
}
