using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace PathFindingAsteria
{
    [CustomEditor(typeof(PathFindingObject))]
    public class PathFindingObjectEditor : Editor
    {

        PathFindingObject pfo;
        public override void OnInspectorGUI()
        {
            pfo = (PathFindingObject)target;
            
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

            if (GUILayout.Button("Recreate Path"))
            {
                pfo.RecreatePath = true;
            }

            if (GUILayout.Button("Random Color"))
            {
                pfo.RandomPathColor();
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(15);

        }


        /// <summary>
        ///     Draw how much time this is taking
        /// </summary>
        void drawStats()
        {

            if (pfo.isCreated)
            {
                GUILayout.Space(10);

                GUILayout.Label("Grid Statistics", EditorStyles.boldLabel);
                GUILayout.Space(5);

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Space(15);
                    
                Dictionary<string, string> mappings = new Dictionary<string, string>()
                {
                    {"Path Time",           $"{pfo.TimeTaken} seconds" },
                    {"Path Start Time",     $"{pfo.PathStartTime}" },
                    {"Path End Time",       $"{pfo.PathCompletionDateTime}" },
                    {"Path Finished",       $"{pfo.PathFinished}" },
                    {"Nodes Traversed",     pfo.PathCoordinates?.Count.ToString() ?? "0" }
                };

                drawColumns(mappings);

                GUILayout.EndHorizontal();
            
                GUILayout.Space(10);
            }
        }

        /// <summary>
        ///     Draw the statistics columns
        /// </summary>
        /// <param name="values"></param>
        void drawColumns(Dictionary<string, string> values)
        {
            int vert = 5;
            int spacing = 5;
            int h = 20;

            GUILayout.BeginVertical(GUILayout.MinWidth(150));
            GUILayout.Space(vert);
            foreach (KeyValuePair<string,string> value in values)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(h));

                GUILayout.Label($"{value.Key}: ", EditorStyles.boldLabel);
                GUILayout.Space(spacing);
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Space(vert);

            foreach (KeyValuePair<string,string> value in values)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(h));

                GUILayout.Label($"{value.Value}");
                GUILayout.Space(spacing);
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();

        }

    }

}