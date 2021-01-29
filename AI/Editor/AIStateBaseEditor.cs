using UnityEditor;
using UnityEngine;

namespace AI_Asteria
{
    [CustomEditor(typeof(AIStateBase), true)]
    public class AIStateBaseEditor : Editor
    {
        AIStateBase baseClass;
        
        
        public override void OnInspectorGUI()
        {
            baseClass = (AIStateBase)target;
            
            GUILayout.Space(10f);

            DrawType();

            DrawAction();

            // Draw the base
            base.OnInspectorGUI();
        }


        /// <summary>
        ///     Draw an action for this class
        /// </summary>
        void DrawAction()
        {
            try
            {
                if (baseClass.getWork !=null)
                {
                    string temp = baseClass.getActionName;
                    string text = temp == string.Empty ? "State Action" : temp;
                    GUILayout.Space(5f);
                    if ( GUILayout.Button(temp, GUILayout.Height(30) ) )
                    {
                        baseClass.getWork.Invoke();
                    }
                    GUILayout.Space(10f);
                }
            }
            catch { }
        }

        /// <summary>
        ///     Draw the label for what type of class it is
        /// </summary>
        void DrawType()
        {

            GUILayout.BeginVertical(EditorStyles.helpBox);

            string label = $"Script Behavior: {baseClass.getStateAssociation.ToString()}";

            GUILayout.Space(10f);

            GUILayout.BeginHorizontal();            
            GUILayout.Space(15);
            GUILayout.Label( label , EditorStyles.boldLabel);
            GUILayout.Space(15);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }
    
    }
}
