
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace AsteriaDialogue.Editor
{
    public class DialogueFlowEditor : EditorWindow
    {
        #region Declarations

        Dialogue selectedDialogue   = null;

        [NonSerialized]
        GUIStyle nodeStyle;

        [NonSerialized]
        DialogueNode draggedNode    = null;

        [NonSerialized]
        Vector2 dragOffset          = new Vector2();

        [NonSerialized]
        DialogueNode createNewNode  = null;
            
        [NonSerialized]
        DialogueNode deleteNode     = null;

        [NonSerialized]
        DialogueNode  linkMeNode    = null;

        [NonSerialized]
        bool dragCanvas             = false;

        [NonSerialized]
        Vector2 draggingCanvasOffset;

        /// <summary> Holds each dialogue node scroll information </summary>
        Dictionary<string, Vector2> Scrolls = new Dictionary<string, Vector2>();

        Vector2 windowScrollPos = new Vector2();
        float scrolly;
        float scrollx;

        float   canvasSize      = 4000;
        float   backgroundSize  = 50;  
        int     settingsAreaH   = 200;
        int     settingsAreaW   = 200;

        /// <summary> The nodes position relative to its parent </summary>
        enum nodeRelPosition { behind, middleRight, middeLeft, front };

        #region Properties

        /// <summary>
        ///     The file name pulled from the dialogue item
        /// </summary>
        string Filename
        {
            get
            {
                if ( selectedDialogue != null )
                {
                    return selectedDialogue.name;
                }
                return string.Empty;
            }
        }

        #endregion

        #endregion


        #region Attribute Fields

        [MenuItem("Window/Dialogue Editor")]
        public static void ShowEditorWindow()
        {
            GetWindow(typeof(DialogueFlowEditor), false, "Dialogue Editor");
        }

        [ OnOpenAsset( 1 ) ]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            Dialogue dialogue = EditorUtility.InstanceIDToObject(instanceID) as Dialogue;
            if (dialogue != null)
            {
                ShowEditorWindow();
                return true;
            }
            return false;
        }
        #endregion


        #region Unity Inherited


        private void OnSelectionChanged()
        {
            Dialogue newDialogue = Selection.activeObject as Dialogue;
            if (newDialogue != null)
            {
                selectedDialogue = newDialogue;
                Repaint();
            }
        }


        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            createStyle();
        }


        private void OnGUI()
        {
            if (selectedDialogue == null)
            {
                EditorGUILayout.LabelField("No Dialogue Selected.", EditorStyles.boldLabel);
            }
            else
            {
                // Capture the mouse actions
                mouseEvents();

                windowScrollPos = EditorGUILayout.BeginScrollView(windowScrollPos);

                var canvas      = GUILayoutUtility.GetRect(canvasSize , canvasSize);  // Default size

                drawBackgound(canvas);

                drawHeader();

                // Want to draw all connections first so they go underneath any nodes
                foreach (DialogueNode node in selectedDialogue.GetAllNodes)
                {
                    DrawNodeConnections(node);
                }

                foreach (DialogueNode node in selectedDialogue.GetAllNodes)
                {
                    DrawGUINode(node);
                }

                EditorGUILayout.EndScrollView();

                nodeUpdates();
            }
        }

        #endregion

        /// <summary>
        ///     Actions indicated by the gui buttons
        /// </summary>
        void nodeUpdates()
        {
            if (createNewNode != null)
            {
                selectedDialogue.CreateChildNode(createNewNode);
                createNewNode = null;
            }
            if (deleteNode != null)
            {
                selectedDialogue.DeleteNode(deleteNode);
                deleteNode = null;
            }
        }

        #region Styling


        /// <summary>
        ///     Set all the styling information
        /// </summary>
        /// <param name="node">Node.</param>
        void setStyleInfo(DialogueNode node)
        {
            if (nodeStyle == null) { createStyle(); }
            setStyleBackground(node);
        }

        /// <summary>
        ///     Create the style each nodew ill inherit
        /// </summary>
        void createStyle()
        {
            nodeStyle           = new GUIStyle();
            nodeStyle.padding   = new RectOffset(20,20,20,20);
            nodeStyle.border    = new RectOffset(12, 12, 12, 12);
        }

        /// <summary>
        ///     Set the background color of the node
        /// </summary>
        /// <param name="node"> The node to get information from</param>
        void setStyleBackground(DialogueNode node)
        {
            if (node == null) 
            {
                nodeStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;
                return;
            }

            if ( linkMeNode != null &&  node == linkMeNode)                         // blue
            {
                nodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
            }
            else if (node.ResponseType == ResponseTypes.TypesOfResponse.Accept)     // green
            {
                nodeStyle.normal.background = EditorGUIUtility.Load("node2") as Texture2D;
            }
            else if (node.ResponseType == ResponseTypes.TypesOfResponse.Decline)    // red
            {
                nodeStyle.normal.background = EditorGUIUtility.Load("node6") as Texture2D;
            }
            else    // Normal color
            {
                nodeStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;
            }
        }

        #endregion

        /// <summary>
        ///     Handle the mouse functions 
        /// </summary>
        void mouseEvents()
        {
            if (Event.current.type == EventType.MouseDown && draggedNode == null)
            {
                draggedNode = getNodeAtClick(Event.current.mousePosition + windowScrollPos);
                if (draggedNode != null)
                {
                    dragOffset = draggedNode.Dimensions.position - Event.current.mousePosition;
                    Selection.activeObject = draggedNode;
                }
                else    // we are not on a node
                {
                    dragCanvas = true;
                    draggingCanvasOffset = Event.current.mousePosition + windowScrollPos;
                    Selection.activeObject = selectedDialogue;
                }
            }
            else if ( Event.current.type == EventType.MouseDrag && draggedNode != null)
            {
                draggedNode.SetPosition( Event.current.mousePosition + dragOffset);
                GUI.changed = true; // We have moved an item
            }
            else if (Event.current.type == EventType.MouseDrag && dragCanvas)
            {
                windowScrollPos = draggingCanvasOffset - Event.current.mousePosition;
                GUI.changed = true;
            }
            else if (Event.current.type == EventType.MouseUp && draggedNode != null)
            {
                draggedNode = null;
            }
            else if (Event.current.type == EventType.MouseUp && dragCanvas)
            {
                dragCanvas = false; 
            }
        }

        /// <summary>
        ///     Select the node where the click occured
        /// </summary>
        /// <returns>The node at click point </returns>
        /// <param name="point">Where the mouse was clicked</param>
        private DialogueNode getNodeAtClick(Vector2 point)
        {
            DialogueNode temp = null;
            foreach (var node in selectedDialogue.GetAllNodes )
            {
                if (node.Dimensions.Contains(point))
                {
                    temp = node;
                }
            }
            return temp;
        }

        /// <summary>
        ///     Get the determined node dimensions.
        /// </summary>
        /// <returns>The node dimensions locked in with specific parameters</returns>
        /// <param name="node">Node</param>
        Rect getNodeDimensions(DialogueNode node)
        {
            //var offset  = 20;
            //var x       = 0.0f;
            //foreach(var p in selectedDialogue.GetAllParents(node.uniqueID))
            //{
            //    if (x < p.dimensions.xMax ) { x = p.dimensions.xMax; }
            //}
            //node.dimensions.x = node.dimensions.x > x
            //? node.dimensions.x : x + offset;
            return node.Dimensions;
        }

        #region Drawing

        /// <summary>
        ///     Create the background of the editor window
        /// </summary>
        /// <param name="canvas"></param>
        void drawBackgound(Rect canvas)
        {
            var bg = Resources.Load("background") as Texture2D;
            if (bg == null) { Debug.LogError($"Unable to find background file"); return; }

            Rect texCoords = new Rect(x: 0, y: 0, width: canvasSize / backgroundSize, 
                    height: canvasSize / backgroundSize);
            GUI.DrawTextureWithTexCoords(position: canvas, image: bg, texCoords: texCoords);
        }

        /// <summary>
        ///     Draw the header node
        /// </summary>
        void drawHeader()
        {
            setStyleInfo(null);
            GUILayout.BeginArea( new Rect(0,0,settingsAreaW,settingsAreaH), nodeStyle);

            drawLabel(Filename);
            //var checkname = drawEditableField("File Name:", Filename);
            //if ( !checkname.Equals (Filename) )
            //{
            //    Filename = checkname;
            //}
            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("Forward Color:", EditorStyles.whiteLabel);
            selectedDialogue.ForwardsColor = EditorGUILayout.ColorField(selectedDialogue.ForwardsColor);

            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Backwards Color:", EditorStyles.whiteLabel);
            selectedDialogue.BackwardsColor = EditorGUILayout.ColorField(selectedDialogue.BackwardsColor);

            GUILayout.EndArea();
        }

        /// <summary>
        ///     Create a new dialogue node within the gui
        ///     window
        /// </summary>
        /// <param name="node"> The dialogue node to create in the new window </param>
        private void DrawGUINode(DialogueNode node)
        {
            Vector2 scroll = getScroll(node.name);

            setStyleInfo(node);

            GUILayout.BeginArea( getNodeDimensions( node ), nodeStyle );

            checkScrollReq(node.Dimensions.x, node.Dimensions.y);
            
            drawLabel(node.name);

            node.SetSpeaker( drawEditableField("Name:", node.SpeakerName) );

            node.SetText( drawEditableScrollField("Text:", node.Text,
                    scroll, out Vector2 newScroll) );

            EditorGUILayout.LabelField("Type:", EditorStyles.whiteLargeLabel);
             node.SetType( (ResponseTypes.TypesOfResponse)EditorGUILayout.EnumPopup(
                                label: "",
                                selected: node.ResponseType) );
                                

            GUILayout.BeginHorizontal();

            // New node creation
            if (GUILayout.Button("+"))
            {
                createNewNode = node;
            }
            addConnectNodeButton(node);
            if (selectedDialogue.RootNode != node)
            {
                if (GUILayout.Button("-"))
                {
                    deleteNode = node;
                }
            }
            GUILayout.EndHorizontal();

            saveNodeScroll(newScroll, node.name);
            GUILayout.EndArea();
        }

        /// <summary>
        ///     Check our scroll requirements
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        void checkScrollReq(float x, float y)
        {
            if ( x > scrollx )
            {
                scrollx = x;
            }
            if ( y > scrolly )
            {
                scrolly = y;
            }
        }

        /// <summary>
        ///     Create the node connection buttons
        /// </summary>
        /// <param name="node"> The node to create the links to</param>
        private void addConnectNodeButton(DialogueNode node)
        {
            if ( linkMeNode == null )
            {
                if ( GUILayout.Button( "Link" ) )
                {
                    linkMeNode = node;
                    nodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
                }
            }
            else if (node == linkMeNode)
            {
                if ( GUILayout.Button( "Stop" ) )
                {
                    linkMeNode = null;
                }
            }
            else if (linkMeNode.Children.Contains(node.name))
            {
                if ( GUILayout.Button( "Break" ) )
                {
                    linkMeNode.RemoveChild(node.name);
                }
            }
            else
            {
                if ( GUILayout.Button( "Child" ) )
                {
                    linkMeNode.AddChild(node.name);
                }
            }
        }

        /// <summary>
        ///     Draw the connections between the nodes and thier child 
        ///     option
        /// </summary>
        /// <param name="node"> The paraent Node to get children from   </param>
        void DrawNodeConnections(DialogueNode node)
        {
            var start = new Vector2(node.Dimensions.xMax, node.Dimensions.center.y);

            foreach (var child in selectedDialogue.GetAllChildren(node))
            {

                Vector2 end         = new Vector2( child.Dimensions.xMin,
                                         child.Dimensions.center.y);

                nodeRelPosition pos = determineRelativePos(node.Dimensions.xMin, node.Dimensions.xMax,
                                                child.Dimensions.xMin, child.Dimensions.xMax);

                if ( pos == nodeRelPosition.behind )
                {
                    start   = new Vector2( node.Dimensions.xMin,    node.Dimensions.center.y    );
                    end     = new Vector2( child.Dimensions.xMax,   child.Dimensions.center.y   );
                }
                else if ( pos == nodeRelPosition.middleRight )
                {
                    start   = new Vector2( node.Dimensions.xMax,    node.Dimensions.center.y    );
                    end     = new Vector2( child.Dimensions.xMax,   child.Dimensions.center.y   );
                }
                var tangent = ( end - start ) ;

                var offset  = pos == nodeRelPosition.behind ? 0.75f : 0.9f;
                tangent     = new Vector2(tangent.x *= offset, 0);

                var check   = pos == nodeRelPosition.behind ? selectedDialogue.BackwardsColor 
                                    : selectedDialogue.ForwardsColor;

                Handles.DrawBezier(startPosition: start, endPosition: end,
                                startTangent: start + tangent, endTangent: end - tangent,
                                color: check,
                                texture: null, width: 3.0f);
            }
        }

        /// <summary>
        ///     Determine the parents relative position to the child
        /// </summary>
        /// <returns>The relative position of the child node to the parent</returns>
        /// <param name="parMinX">      Parent Min x value  </param>
        /// <param name="parMaxX">      Parent Max x value  </param>
        /// <param name="childMinX">    Child Min x value   </param>
        /// <param name="childMaxX">    Child Max x value   </param>
        nodeRelPosition determineRelativePos(float parMinX, float parMaxX, float childMinX, float childMaxX)
        {
            if ( (childMinX < parMaxX && childMinX > parMinX ) || ( childMaxX > parMinX && childMaxX < parMaxX))
            {
                return nodeRelPosition.middleRight;
            }
            else if (childMaxX < parMinX)
            {
                return nodeRelPosition.behind;
            }
            else if (childMinX > parMaxX)
            {
                return nodeRelPosition.front;
            }

            return nodeRelPosition.front;
        }

        #endregion


        #region Field Creations


        /// <summary>
        ///     Noneditable. 
        ///     Draw a label with a white background on unity normal skin.
        ///     Draw a label with a black background on unity pro skin
        /// </summary>
        /// <param name="text">The text to put into the label feild</param>
        void drawLabel(string text)
        {           
            GUIStyle gsAlterBg = new GUIStyle();
            
            if (EditorGUIUtility.isProSkin)
            {
                gsAlterBg.normal.background = new Color(0.0f, 0.0f, 0.0f, 0.7f).MakeTex(600, 1);
            }
            else
            {
                gsAlterBg.normal.background = new Color(1.0f, 1.0f, 1.0f, 0.7f).MakeTex(600, 1);
            }

            GUILayout.BeginHorizontal(gsAlterBg);
            EditorGUILayout.LabelField(text);
            GUILayout.EndHorizontal();

        }

        /// <summary>
        ///     Draw an editable text field
        /// </summary>
        /// <returns>The new value of the field </returns>
        /// <param name="label"> The label of the field </param>
        /// <param name="text"> The text to put into the field </param>
        string drawEditableField(string label, string text)
        {
            EditorGUILayout.LabelField(label, EditorStyles.whiteLargeLabel);
            string ret = EditorGUILayout.TextField(text);

            return ret;
        }

        /// <summary>
        ///     Draws an editable scroll field
        /// </summary>
        /// <returns>The editable scroll field.</returns>
        /// <param name="label">        The label of the field          </param>
        /// <param name="text">         The text to put into the field  </param>
        /// <param name="scroll">       The scroll position             </param>
        /// <param name="newScroll">    The new scroll position         </param>
        string drawEditableScrollField(string label, string text, Vector2 scroll, out Vector2 newScroll)
        {
            EditorGUILayout.LabelField(label, EditorStyles.whiteLargeLabel);
            newScroll = EditorGUILayout.BeginScrollView(scroll);
            string newText = GUILayout.TextArea(text, GUILayout.Height(200)); 
            EditorGUILayout.EndScrollView();
            return newText;
        }
        #endregion


        #region Scroll information

        /// <summary>
        ///     Get the location of the scroll 
        ///     for the given dialogue node.
        /// </summary>
        /// <returns>The scroll length</returns>
        /// <param name="id"> Which dialogue we are in</param>
        Vector2 getScroll(string id)
        {
            if (Scrolls.ContainsKey(id))
            {
                return Scrolls[id];
            }

            return new Vector2();
        }

        /// <summary>
        ///     Save a new scroll value 
        /// </summary>
        /// <param name="scroll">Scroll.</param>
        /// <param name="id">Identifier.</param>
        void saveNodeScroll(Vector2 scroll, string id)
        {
            if ( Scrolls.ContainsKey(id) )
            {
                Scrolls[id] = scroll;
            }
            else
            {
                Scrolls.Add(id, scroll);
            }

        }
        #endregion


    }
}