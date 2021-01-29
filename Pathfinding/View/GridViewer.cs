using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace PathFindingAsteria
{
   /// <summary>
   ///      This class will visualize multiple aspects of our grid and 
   ///      pathfinding objects. Take into consideration, this could create
   ///      a heavy load on CPU and memory if everything is loaded. It is recommended
   ///      that you turn off visualizing completely open nodes and the neighbors
   /// </summary>
    [RequireComponent( typeof( GridManager ) )]
    public class GridViewer : MonoBehaviour
    {

        #region Editor

        /// <summary>
        ///     This is a list of items that we want to show / hide when creating the node
        ///     view
        /// </summary>
        [Serializable]
        struct ShowTheseItems
        {
            [Tooltip("Create an item even if it is being hidden")]
            [SerializeField] internal bool CreateHidden;            
            [SerializeField] internal bool Everything;
            [SerializeField] internal bool ShowOpen;
            [SerializeField] internal bool ShowBorder;
            [SerializeField] internal bool ShowBlocked;
            [SerializeField] internal bool ShowTerrain;
            [SerializeField] internal bool ShowPath;
            [SerializeField] internal bool ShowExploredNodes;
            [SerializeField] internal bool ShowStartNode;
            [SerializeField] internal bool ShowEndNode;
            [Tooltip("Show the connections through the path.")]
            [SerializeField] internal bool ShowPathConnections;

            [Tooltip("A percentage of the real nodes size to display.")]
            [Range(0, 1.0f)]
            [SerializeField] internal float ObjectiveNodeSize;
            [SerializeField] internal NeighborInformation Neighbors;

            [Serializable]
            internal struct NeighborInformation{
                [Tooltip("Show neighbor connections through debug draw line")]
                [SerializeField] internal bool  showNeighborConnections;
                [Tooltip("If draw each frame is not chosen, it determines how long they will persist")]
                [SerializeField] internal float showForSeconds;
                [Tooltip("This will draw the information each frame as long as nodes are present")]
                [SerializeField] internal bool  drawEachFrame;
            }
        }

        [Header("Visualized Objects")]        
        
        [Tooltip("If no prefab is selected, use a primitive type for the node")]
        [SerializeField] PrimitiveType visualNodeType   = PrimitiveType.Cube;

        [Tooltip("The percent of its original size")]
        [Range(0, 1.0f)]
        public float NodeScale                          = .5f;

        [Tooltip("Remove the colliders if we are not using a defined node view prefab. Prevents collisions")]
        [SerializeField] bool removeColliders           = true;

        [Tooltip("Determine what nodes to show")]
        [SerializeField] ShowTheseItems ShowTheseNodes  = new ShowTheseItems();

        #endregion
        [Header("Object Colors")]

        // colors to show our various NodeViews
        [SerializeField] Color startColor       = Color.green;
        [SerializeField] Color openColor        = Color.white;
        [SerializeField] Color borderColor      = Color.yellow;
        [SerializeField] Color goalColor        = Color.red;
        [SerializeField] Color terrainColor     = Color.grey;
        [SerializeField] Color blockedColor     = Color.black;
        [SerializeField] Color exploredColor    = Color.cyan;

        [SerializeField] bool ReDraw    = false;
        [SerializeField] bool Clear     = false;


        #region Private

        Node[,,]               DrawThese;
        GameObject              nodeViewPrefab;
        GameObject[,,]          NodesVisiualized;
        private GridManager     gridManager;
        GameObject              parentObject = null;       
        List<PathFindingObject> PathObject = new List<PathFindingObject>();

        #endregion



        private void Awake()
        {
            gridManager         = GetComponent<GridManager>();
            gridManager.DrawMe += DrawGraphStart;
            NodesVisiualized    = new GameObject[gridManager.gridBase.GraphWidthIterations, gridManager.gridBase.GraphHeightIterations, gridManager.gridBase.GraphDepthIterations];
        }


        #region Testing


        public void FixedUpdate()
        {

            // Clear our graph on demand
            if (Clear)
            {
                Clear = false;
                DestroyAllObjects();
                return;
            }

            // redraw our graph on demand
            if (ReDraw)
            {
                ReDraw              = false;
                nodeViewPrefab      = null;

                DrawGraphStart(  );
            }

            // If we want to, draw the connections on each graph
            if( gridManager.gridBase.GridNodes != null && ShowTheseNodes.Neighbors.drawEachFrame)
            {
                foreach (Node node in gridManager.gridBase.GridNodes)
                {
                    if ( CheckShowCriteria( node.nodeType ) )
                    {
                        ShowNeighborConnections(node);
                    }
                }
            }

            // Show our path connections
            if (ShowTheseNodes.ShowPathConnections)
            {
                if ( PathObject.Count != gridManager.PathFindingObjects.Count )   { AssignPathNodes(); }

                // Draw the line from each path node to the next
                foreach (PathFindingObject po in PathObject)
                {
                    CreatePathArrow(po);
                }
            }

        }


        #endregion

        /// <summary>
        ///     Create a visual of my grid in world
        /// </summary>        
        internal void DrawGraphStart()
        {
            Node[,,] myNodes = gridManager.gridBase.GridNodes;
            DestroyAllObjects();
            NodesVisiualized = new GameObject[gridManager.gridBase.GraphWidthIterations, gridManager.gridBase.GraphHeightIterations, gridManager.gridBase.GraphDepthIterations];

            if (myNodes == null)
            {
                Debug.LogWarning(" GridViewer: There is no graph of nodes created");
                return;
            }

            CreateParent();

            // If we want to focus on anything to do with the path nodes
            if ( ShowTheseNodes.ShowPath || ShowTheseNodes.ShowPathConnections || ShowTheseNodes.ShowExploredNodes || ShowTheseNodes.ShowStartNode || ShowTheseNodes.ShowEndNode )
            {
                AssignPathNodes();
            }

            //  Go through all my nodes
            foreach (Node node in myNodes)
            {
                if (node != null)
                {
                    bool show = CheckShowCriteria(node.nodeType);

                    // Create the object if we need to
                    if (show)
                    {
                        CreateObjectInEditor(node, true);
                        ShowNeighborConnections( myNode: node );
                    }
                    if (!show && ShowTheseNodes.CreateHidden)
                    {
                        CreateObjectInEditor(node, false);
                    }
                }
            }
            CreatePathFindingObjects();
        }

        /// <summary>
        ///     Get all the path nodes and assign them to our referenced grid
        /// </summary>
        void AssignPathNodes()
        {
            PathObject = new List<PathFindingObject>();
            // Go through each of the path finding objects
            foreach (PathFindingObject po in gridManager.PathFindingObjects)                
            {
                if (po.PathStarted == true || po.PathFinished == true)
                {
                    PathObject.Add(po);
                }
            }
        }

        /// <summary>
        ///     Show all of our friendly neighbors 
        ///     and what type of connection they are
        /// </summary>
        /// <param name="myNode">   The node we are visualizing the neighbors for   </param>
        void ShowNeighborConnections(Node myNode)
        {
            if (ShowTheseNodes.Neighbors.showNeighborConnections)
            {
                float time = ShowTheseNodes.Neighbors.showForSeconds;
                if (ShowTheseNodes.Neighbors.drawEachFrame) { time = 0.0f; }

                foreach (Node neighbor in myNode.Neighbors)
                {
                    Debug.DrawLine( start: myNode.Position, end: neighbor.Position, color: DetermineColor( neighbor ), duration: time );
                }
            }
        }

        /// <summary>
        ///     Create the parent object if it does not exist
        ///     This will hold all the grid objects if we are drawing them
        ///     on the players field
        /// </summary>
        private void CreateParent()
        {
            if (parentObject == null)
            {
                parentObject = new GameObject("Graph Container");
            }

            // Put all stuff up one level as not to break if object moves
            parentObject.transform.parent = transform.parent;
        }


        /// <summary>
        ///     Check if we are going to show the node
        /// </summary>
        /// <param name="nodeType"> The nodetype we want to check   </param>
        /// <returns>
        ///     Whether or not we should create the node in question
        /// </returns>
        private bool CheckShowCriteria(NodeType nodeType)
        {
            if (ShowTheseNodes.Everything)
            {
                return true;
            }
            else if (!ShowTheseNodes.ShowBlocked        && nodeType == NodeType.Blocked)
            {
                return false;
            }
            else if (!ShowTheseNodes.ShowOpen           && nodeType == NodeType.Open)
            {
                return false;
            }
            else if (!ShowTheseNodes.ShowTerrain        && nodeType == NodeType.Terrain)
            {
                return false;
            }
            else if (!ShowTheseNodes.ShowBorder         && nodeType == NodeType.OpenBorder)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        ///     Create the game object in the window
        /// </summary>
        /// <param name="node">         The node to visualzie   </param>
        /// <param name="setActive">    Determines active GO    </param>
        private void CreateObjectInEditor(Node node, bool setActive, float scale = 0.0f)
        {
            GameObject go;
            var location = new Vector3(node.Position.x, node.Position.y, node.Position.z);
            if (nodeViewPrefab == null)
            {
                go = GameObject.CreatePrimitive(visualNodeType);
                go.transform.position = location;
                go.transform.rotation = Quaternion.identity;
            }
            else
            {
                go = Instantiate(nodeViewPrefab, new Vector3(node.Position.x, node.Position.y, node.Position.z),
                        Quaternion.identity) as GameObject;
            }

            // removes colliders if we are dynamically creating the node
            if ( removeColliders && nodeViewPrefab == null )
            {
                var col = go.GetComponent<Collider>();
                if (col) col.enabled = false;
            }

            go.transform.name   = node.Index.x.ToString() + " " + node.Index.y.ToString() + " " + node.Index.z.ToString();

            // parent it under this transform to be more organized
            go.transform.parent = parentObject.transform;

            var dims    = gridManager.gridBase.NodeDimensions;
            scale       = scale == 0.0f ? NodeScale : ShowTheseNodes.ObjectiveNodeSize;
            go.transform.localScale = new Vector3(x: dims.x * scale, y: dims.y * scale, z: dims.z * scale);

            NodesVisiualized[ node.Index.x, node.Index.y, node.Index.z ] = go;
                        
            ColorNode( DetermineColor( node ), go );

            go.SetActive( setActive );
        }

        /// <summary>
        ///     Create our path items
        /// </summary>
        void CreatePathFindingObjects()
        {
            if (ShowTheseNodes.ShowExploredNodes)
            {
                foreach (PathFindingObject po in PathObject)
                {
                    foreach (Node n in po.ExploredNodes)
                    {
                        RecreateNode(n, exploredColor);
                    }
                }
            }
            if (ShowTheseNodes.ShowPath)
            {
                foreach (PathFindingObject po in PathObject)
                {
                    foreach (Node n in po.PathNodes)
                    {
                        RecreateNode(n, po.pathColor);
                    }
                }
            }

            if (ShowTheseNodes.ShowEndNode)
            {
                foreach(PathFindingObject po in PathObject)
                {
                    Node n      = po.EndNode;
                    RecreateNode(n, goalColor, ShowTheseNodes.ObjectiveNodeSize);
                }
            }
            if (ShowTheseNodes.ShowStartNode)
            {
                foreach (PathFindingObject po in PathObject)
                {
                    Node n = po.StartNode;
                    RecreateNode(n, startColor, ShowTheseNodes.ObjectiveNodeSize);
                }
            }
        }

        /// <summary>
        ///     Recreate a specific node on the grid
        /// </summary>
        /// <param name="n"></param>
        private void RecreateNode(Node n, Color color, float scale = 0.0f)
        {
            var item = NodesVisiualized[(int)n.Index.x, (int)n.Index.y, (int)n.Index.z];

            if (item != null)
            {
                Destroy(item);
            }
            // Create a new object
            CreateObjectInEditor(n, true, scale);
            item = NodesVisiualized[(int)n.Index.x, (int)n.Index.y, (int)n.Index.z];
            ColorNode(color, item);
        }

        /// <summary>
        ///     Create a path arrow along the path
        /// </summary>
        /// <param name="po"> The pathfinding object that we want to draw the lines for</param>
        void CreatePathArrow( PathFindingObject po)
        {
            for (int i = 0; i < po.PathNodes.Count-1; i++)
            {
                if (i != 0)
                {
                    Debug.DrawLine(po.PathNodes[i].Position, po.PathNodes[i + 1].Position, po.pathColor) ;
                }
            }
            
        }

        /// <summary>
        ///     Determine what color the node will be basd on its criteria
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal Color DetermineColor(Node node)
        {
            if (node.nodeType       == NodeType.Blocked)
            {
                return blockedColor;
            }
            else if (node.nodeType == NodeType.Terrain)
            {
                return terrainColor;
            }
            else if (node.nodeType == NodeType.OpenBorder)
            {
                return borderColor;
            }
            else
            {
                return openColor;
            }
        }


        /// <summary>
        ///     Apply the color to the gameobject
        /// </summary>
        /// <param name="color"></param>
        /// <param name="go"></param>
        internal void ColorNode(Color color, GameObject go)
        {            
            if (go != null )
            {
                Renderer goRenderer = go.GetComponent<Renderer>();

                if (goRenderer != null)
                {
                    goRenderer.material.color = color;
                }
            }
            
        }

        /// <summary>
        ///     Destroy any created objects on the field
        ///     This allows us to recreate them without double dipping
        /// </summary>
        internal void DestroyAllObjects()
        {
            if (NodesVisiualized != null)
            {
                // Go through and destroy all the nodes
                foreach(GameObject go in NodesVisiualized)
                {
                    if (go != null)
                    {
                        Destroy(go);
                    }
                }
            }
            
            if (PathObject != null)
            {
                PathObject.Clear();
            }
        }
    }
}