using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace AsteriaDialogue
{
    [CreateAssetMenu(fileName = "New Dialogue Item", menuName = "Asteria/New Dialogue", order = 0)]
    public class Dialogue : ScriptableObject, ISerializationCallbackReceiver
    {

        #region Declarations

        #region Editor

        [Header("Node Information")]
        [SerializeField]
        Vector2 defaultSize         = new Vector2(300, 250);

        [SerializeField]
        List<DialogueNode> nodes    = new List<DialogueNode>();

        [Header("Line Colors")]
        [SerializeField]
        Color forwardsColor  = Color.white;

        [SerializeField]
        Color backwardsColor = Color.red;


        #endregion

        #region Private Dec

        /// <summary> Register an action with a type completion</summary>
        Dictionary<ResponseTypes.TypesOfResponse, List<Action>> actions
                        = new Dictionary<ResponseTypes.TypesOfResponse, List<Action>>();

        Dictionary<string, DialogueNode>        nodeReference;
        Dictionary<string, List<DialogueNode>>  childReference;

        #endregion

        #endregion


        #region Properties

        /// <summary>
        ///     The color of the line going forward
        /// </summary>
        /// <value>The color of the forwards.</value>
        public Color ForwardsColor
        {
            get
            {
                return forwardsColor;
            }
            set
            {
                if (forwardsColor == value) { return; }
                Undo.RecordObject(this, "Undo Color Change");
                forwardsColor = value;
            }
        }

        /// <summary>
        ///     The color of the line going backwards
        /// </summary>
        /// <value>The color of the backwards.</value>
        public Color BackwardsColor
        {
            get
            {
                return backwardsColor;
            }
            set
            {
                if (backwardsColor == value) { return; }
                Undo.RecordObject(this, "Undo Color Change");
                backwardsColor = value;
            }
        }


        /// <summary>
        ///     The name of the dialogue script
        /// </summary>
        /// <value>The name string</value>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        ///     The first node in the list
        /// </summary>
        public DialogueNode RootNode
        {
            get
            {
                return nodes[0];
            }
        }

        /// <summary>
        ///     Return all the dialogue options
        /// </summary>
        /// <returns>The all nodes</returns>
        public IEnumerable<DialogueNode> GetAllNodes
        {
            get
            {
                return nodes;
            }
        }


        #endregion
       

        #region Unity Calls


#if UNITY_EDITOR

        private void Awake()
        {
            setupRoot();
            RebuildDictionaries();
        }

#endif

        /// <summary>
        ///     Run the validates... Unity call
        /// </summary>
        private void OnValidate()
        {
            setupRoot();
            RebuildDictionaries();
        }

        #endregion


        #region Add and Remove


#if UNITY_EDITOR

        /// <summary>
        ///     Delete the node and any references to it
        ///     in the dialogue
        /// </summary>
        /// <param name="node"> The node to remove </param>
        public void DeleteNode(DialogueNode node)
        {
            Undo.RecordObject(this, "Deleted dialogue Node");

            nodes.Remove(node);

            removeDisconnectedChildren(node.name);
            OnValidate();

            Undo.DestroyObjectImmediate(node);
        }

        /// <summary>
        ///     Create a new child node for a designated parent
        /// </summary>
        /// <returns>The child node</returns>
        /// <param name="parent">Parent node</param>
        public DialogueNode CreateChildNode(DialogueNode parent)
        {
            var newnode = createNode(addTolist: true);

            // Auto position it next to our parent node
            var x = newnode.Dimensions.x + parent.Dimensions.x + parent.Dimensions.width;
            var y = newnode.Dimensions.y + parent.Dimensions.y;// + parent.Dimensions.height;
            
            newnode.SetDimensions( new Rect( x, y, newnode.Dimensions.width, newnode.Dimensions.height ) );
            parent.AddChild( newnode.name );

            OnValidate();
            return newnode;
        }

        /// <summary>
        ///     Create a new default node
        /// </summary>
        /// <param name="addTolist"> Determines whether we want to add this node to the total list of nodes </param>
        /// <returns>The created node</returns>
        DialogueNode createNode(bool addTolist = true)
        {
            DialogueNode node   = CreateInstance<DialogueNode>(); //new DialogueNode();
            node.name           = Guid.NewGuid().ToString();

            Undo.RegisterCreatedObjectUndo(node, "Create New Node");
            Undo.RecordObject(this, "Add Node");

            node.SetDimensions( new Rect( 0, 0, defaultSize.x, defaultSize.y ) );

            if ( addTolist ) { nodes.Add( node ); }
            return node;
        }

        /// <summary>
        ///     Remove any children from thier parent
        /// </summary>
        /// <param name="childID">Child identifier.</param>
        void removeDisconnectedChildren(string childID)
        {
            foreach ( var n in GetAllParents( childID ) )
            {
                n.RemoveChild(childID);

            }
        }

#endif

#endregion

        /// <summary>
        ///     Setup our root node if one does not exist
        /// </summary>
        void setupRoot()
        {
            if (nodes == null) { nodes = new List<DialogueNode>(); }
            if ( nodes.Count == 0 )
            {
                var newNode = createNode();

                var r = new Rect(100, 100, newNode.Dimensions.width, newNode.Dimensions.height);
                newNode.SetDimensions(r);                
            }
        }

        /// <summary>
        ///     Rebuild our reference dictionary
        /// </summary>
        void RebuildDictionaries()
        {
            if ( nodeReference == null  )   { nodeReference     = new Dictionary<string, DialogueNode>(); }
            if ( childReference == null )   { childReference    = new Dictionary<string, List<DialogueNode>>(); }
           
            nodeReference.Clear();
            childReference.Clear();

            foreach( var node in nodes )
            {
                nodeReference.Add( key: node.name, value: node );

                foreach( var child in node.Children )
                {
                    if ( childReference.ContainsKey( child ) )
                    {
                        childReference[child].Add( node );
                    }
                    else
                    {
                        childReference.Add( key: child, 
                                value: new List<DialogueNode>() { node });
                    }
                }
            }

        }


        /// <summary>
        ///     Register an action with its associated response type
        /// </summary>
        /// <param name="a">    The action to register                           </param>
        /// <param name="type"> The type of response to register the action for  </param>
        public void RegisterAction(Action a, ResponseTypes.TypesOfResponse type)
        {
            if ( actions.ContainsKey( type ) )
            {
                actions[ type ].Add( a );
            }
            else
            {
                actions.Add(type, new List<Action>() { a });
            }
        }


        #region Gets

        /// <summary>
        ///     Returns the node at a specific point. If a number
        ///     requested is too high, the last node will be 
        ///     returned.
        /// </summary>
        /// <param name="i">The index to get the node at</param>
        /// <returns>       
        ///     The specified node in the list 
        /// </returns>

        public DialogueNode GetNodeAt(int i)
        {
            if (i >= nodes.Count)
            {
                return nodes[nodes.Count - 1];
            }
            else
            {
                return nodes[ i ];
            }
        }

        /// <summary>
        ///     Get all the children of a specified node
        /// </summary>
        /// <returns>The node children </returns>
        /// <param name="parent"> The node we want the children of  </param>
        public IEnumerable<DialogueNode> GetAllChildren(DialogueNode parent)
        {
            if (nodeReference == null || parent == null) { RebuildDictionaries(); }

            foreach ( string child in parent. Children )
            {
                if ( nodeReference.ContainsKey( child ) )
                {
                    yield return nodeReference[child];
                }
            }

        }

        /// <summary>
        ///     Return a list of all of the parent nodes
        /// </summary>
        /// <returns> a list of dialogue nodes related to the child </returns>
        /// <param name="childID">Child identifier</param>
        public IEnumerable<DialogueNode> GetAllParents(string childID)
        {
            if (childReference == null) { RebuildDictionaries(); }

            if (childReference.ContainsKey(childID))
            {
                return childReference[childID];
            }
            return new List<DialogueNode>();
        }


        #endregion


        #region I Serialization call back reciever


        /// <summary>
        ///     Before we save an object to the hard drive
        /// </summary>
        public void OnBeforeSerialize()
        {

#if UNITY_EDITOR

            //if (nodes.Count == 0)
            //{
            //    createNode();
            //}

            // Check if the parent is serialized then if the node is
            if ( AssetDatabase.GetAssetPath(this) != string.Empty )
            {
                foreach (var node in GetAllNodes )
                {
                    if ( AssetDatabase.GetAssetPath(node) == string.Empty)
                    {
                        AssetDatabase.AddObjectToAsset(node, this);
                    }
                }
            }
#endif

        }

        public void OnAfterDeserialize()
        {

        }

        #endregion


    }
}