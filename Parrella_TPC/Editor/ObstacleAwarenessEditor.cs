using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Human_Controller
{

    [CustomEditor(typeof(ObstacleAwareness))]

    public class ObstacleAwarenessEditor : Editor
    {

        ObstacleAwareness baseClass;

        bool hold   = false;
        bool manualUpdate = false;

        public override void OnInspectorGUI()
        {
            baseClass = (ObstacleAwareness)target;

            
            // Draw the base
            base.OnInspectorGUI();
            
            GUILayout.Space(20f);

            drawCurrentObject();
          
            GUILayout.Space(20f);

        }

        struct items
        {
            public items(string n, string h, string m, string l, bool item3Header = false)
            {
                name        = n;
                valueHigh   = h;
                valueMed    = m;
                valueLow    = l;
                Makebold    = item3Header;
            }
            internal string name;
            internal string valueHigh;
            internal string valueMed;
            internal string valueLow;

            internal bool Makebold;
        }

        bool showPosition = false;
        List<items> cached = new List<items>();
        string mode = "Constant";
        /// <summary>
        ///     Draw the current objects stats
        /// </summary>
        void drawCurrentObject()
        {
            GUILayout.Label("Current Object Awareness Info", EditorStyles.boldLabel);
            GUILayout.Space(5);

            // Make this section collapsable
            showPosition =  EditorGUILayout.Foldout(showPosition, showPosition ? "Hide" : "Show");
            if (showPosition)
            {
                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Space(15);

                // A truly roundabout way to show this information in the editor
                // It is devised of four variables that make up columns.
                var high    = baseClass.currentChecks.High.obstacles;
                var med     = baseClass.currentChecks.MedHigh.obstacles;
                var medLow  = baseClass.currentChecks.MedLow.obstacles;
                var low     = baseClass.currentChecks.Low.obstacles;
                var crouch  = baseClass.currentChecks.crouchObstacles;
                var ground  = baseClass.currentChecks.groundDropObst;
                var ledge   = baseClass.currentChecks.ledgeObstacle;
                List<items> Items = new List<items>()
                {
                    { new items( "Current:",            baseClass.currentChecks.type.ToString(), "", "") },
                    { new items( "Mode:",             mode, "","" )},
                    { new items( "", "","","") },
                    { new items( "", "Left","Front","Right") },
                    { new items( "HIGH", "", "","") },
                    { new items( "Obstacle:",          high.left.firstObjHit?.name ?? "None", high.front.firstObjHit?.name ?? "None", high.right.firstObjHit?.name ?? "None")   },
                    { new items( "Position:",          high.left.firstObjHit?.transform.position.ToString() ?? "None", high.front.firstObjHit?.transform.position.ToString() ?? "None", high.right.firstObjHit?.transform.position.ToString() ?? "None")   },
                    { new items( "Distance:",          high.left.distance.ToString(), high.front.distance.ToString(), high.right.distance.ToString()  ) },
                    { new items( "Angle:",             high.left.angleOfAttack.ToString(), high.front.angleOfAttack.ToString(), high.right.angleOfAttack.ToString()  ) },
                    { new items( "", "","","") },

                    { new items( "MIDDLE HIGH",               "", "","")                                      },
                    { new items( "Obstacle:",          med.left.firstObjHit?.name ?? "None", med.front.firstObjHit?.name ?? "None", med.right.firstObjHit?.name ?? "None")   },
                    { new items( "Position:",          med.left.firstObjHit?.transform.position.ToString() ?? "None", med.front.firstObjHit?.transform.position.ToString() ?? "None", med.right.firstObjHit?.transform.position.ToString() ?? "None")   },
                    { new items( "Distance:",          med.left.distance.ToString(), med.front.distance.ToString(), med.right.distance.ToString() )  },
                    { new items( "Angle:",             med.left.angleOfAttack.ToString(), med.front.angleOfAttack.ToString(), med.right.angleOfAttack.ToString() )  },
                    { new items( "", "","","") },
                    { new items( "MIDDLE LOW",               "", "","")                                      },
                    { new items( "Obstacle:",          medLow.left.firstObjHit?.name ?? "None", medLow.front.firstObjHit?.name ?? "None", medLow.right.firstObjHit?.name ?? "None")   },
                    { new items( "Position:",          medLow.left.firstObjHit?.transform.position.ToString() ?? "None", medLow.front.firstObjHit?.transform.position.ToString() ?? "None", medLow.right.firstObjHit?.transform.position.ToString() ?? "None")   },
                    { new items( "Distance:",          medLow.left.distance.ToString(), medLow.front.distance.ToString(), medLow.right.distance.ToString() )  },
                    { new items( "Angle:",              medLow.left.angleOfAttack.ToString(), medLow.front.angleOfAttack.ToString(), medLow.right.angleOfAttack.ToString() )  },
                    { new items( "", "","","") },
                    { new items( "LOW",               "", "","")                                     },
                    { new items( "Obstacle:",          low.left.firstObjHit?.name ?? "None", low.front.firstObjHit?.name ?? "None", low.right.firstObjHit?.name ?? "None")   },
                    { new items( "Position:",          low.left.firstObjHit?.transform.position.ToString() ?? "None", low.front.firstObjHit?.transform.position.ToString() ?? "None", low.right.firstObjHit?.transform.position.ToString() ?? "None")   },
                    { new items( "Distance:",          low.left.distance.ToString(), low.front.distance.ToString(), low.right.distance.ToString() )  },
                    { new items( "Angle:",             low.left.angleOfAttack.ToString(), low.front.angleOfAttack.ToString(), low.right.angleOfAttack.ToString() )  },
                    { new items( "", "","","") },
                    { new items( "GROUND",               "", "","")                                     },
                    { new items( "Obstacle:",          ground.left.firstObjHit?.name ?? "None", ground.front.firstObjHit?.name ?? "None", ground.right.firstObjHit?.name ?? "None")   },
                    { new items( "Position:",          ground.left.firstObjHit?.transform.position.ToString() ?? "None", ground.front.firstObjHit?.transform.position.ToString() ?? "None", low.right.firstObjHit?.transform.position.ToString() ?? "None")   },
                    { new items( "Distance:",          ground.left.distance.ToString(), ground.front.distance.ToString(), ground.right.distance.ToString() )  },
                    { new items( "Angle:",             ground.left.angleOfAttack.ToString(), ground.front.angleOfAttack.ToString(), ground.right.angleOfAttack.ToString() )  },
                    { new items( "","","","") },
                    { new items( "CROUCH",               "", "LEDGE","", true)                                     },
                    { new items( "Obstacle:",          crouch.top.firstObjHit?.name ?? "None", "Obstacle", ledge.front.firstObjHit?.name ?? "None", true )   },
                    { new items( "Position:",          crouch.top.firstObjHit?.transform.position.ToString() ?? "None", "Position", ledge.front.firstObjHit?.transform.position.ToString() ?? "None", true)   },
                    { new items( "Distance:",          crouch.top.distance.ToString(),"Distance", ledge.front.distance.ToString(), true )  },
                    { new items( "Angle:",             crouch.top.angleOfAttack.ToString(),"Angle", ledge.front.angleOfAttack.ToString(), true )  },

                };

                // check what values we would like to use
                if (!hold || manualUpdate) { cached = Items; }
                else 
                { 
                    if (cached.Count > 0)
                    {
                        cached[1] = new items(n: cached[1].name, h: "Manual", "", ""); 
                    }
                }

                drawColumns(cached);
            
                GUILayout.EndHorizontal();
                drawButtons();

                GUILayout.Space(10);
            }


        }
        /// <summary>
        ///     Draw our interaction buttons
        /// </summary>
        void drawButtons()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Constant")) { onCont(); }
            if (GUILayout.Button("Manual")) { onHold(); }

            GUILayout.EndHorizontal();

            string rays = baseClass.DrawRays ? "Off" : "On";
            if (GUILayout.Button($"Toggle Rays - {rays}")) { baseClass.DrawRays = !baseClass.DrawRays; }
        }

        /// <summary>
        ///     Draw the statistics columns
        /// </summary>
        /// <param name="values"></param>
        void drawColumns(List<items> values)
        {
            int vert = 5;
            int spacing = 5;
            GUILayout.BeginVertical(GUILayout.MinWidth(120));

            GUILayout.Space(vert);
            foreach (items item in values)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20));
                GUILayout.Label($"{item.name} ", EditorStyles.boldLabel);
                GUILayout.Space(spacing);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.MinWidth(100));
            GUILayout.Space(vert);

            foreach (items item in values)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20));
                GUILayout.Label($"{item.valueHigh}");
                GUILayout.Space(spacing);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.MinWidth(100));
            GUILayout.Space(vert);

            foreach (items item in values)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20));
                
                if (item.Makebold)
                {
                    GUILayout.Label($"{item.valueMed}", EditorStyles.boldLabel);
                }
                else
                {
                    GUILayout.Label($"{item.valueMed}");
                }
                GUILayout.Space(spacing);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.MinWidth(100));
            GUILayout.Space(vert);

            foreach (items item in values)
            {
                GUILayout.BeginHorizontal(GUILayout.Height(20));
                GUILayout.Label($"{item.valueLow}");
                GUILayout.Space(spacing);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

        }



        // Pause for manual update
        void onHold()
        {
            hold            = true;
            manualUpdate    = true;
            drawCurrentObject();
            manualUpdate    = false;
        }

        // continous update
        void onCont()
        {
            hold    = false;
        }
    }
    

}