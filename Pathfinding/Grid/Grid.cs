using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace PathFindingAsteria
{
    
    [Serializable]
    public class Grid
    {
      
        #region Editor

        [Header("Graph Information")]

        [SerializeField]
        [Tooltip("Determine what type of graph will be created")]
        internal GraphType graphType                    = GraphType.NodeBasedGrid;

        [SerializeField]
        [Tooltip("Determines if we incrementally create the grid each frame. Improves frame rate performance")]
        public bool iterateOverFrames   = false;
        
        [SerializeField]
        [Range(100, 100000)]
        int nodesToIterate              = 10000;

        [Tooltip("The start point of the graph. Will default to the transform it is sitting on")]
        [SerializeField] GameObject startObject;

        [Tooltip("Detemine nodes closest to start and target objectives if we have a target")]
        [SerializeField] bool CalculateClosestStartNode = false;

        [SerializeField]
        List<NodeLayerCosts> LayerCosts     = new List<NodeLayerCosts>();

        [Header("Collision Check")]
        [SerializeField] bool CheckUp       = true;
        [SerializeField] bool CheckDown     = true;
        [SerializeField] bool CheckForward  = true;
        [SerializeField] bool CheckBack     = true;
        [SerializeField] bool CheckLeft     = true;
        [SerializeField] bool CheckRight    = true;

        [Tooltip("What layers will be included as determined hits... Excluded layers will be ignored")]
        [SerializeField] LayerMask includeLayers = ~0;

        [Header("Graph Location and Size")]
        [Tooltip("Determine if we want to offset from the origin")]
        [SerializeField] bool offSetGraph   = true;

        // The dimensions of the graph
        [Tooltip("How many units to manually set the y axis initial position the graph is going to go if we are not using half the height")]
        [SerializeField] int graphYOffset           = 0;

        [Tooltip("Determine if we half the overall height iterations to determine y offset or use the offset below")]
        [SerializeField] bool halfYIterations       = true;

        [Tooltip("How many units up the graph is going to go")]
        [SerializeField] int graphHeightIterations  = 6;

        [Tooltip("How many iterations wide the graph is going to go")]
        [SerializeField] int graphWidthIterations   = 10;

        [Tooltip("How many iterations wide the depth is going to go")]
        [SerializeField] int graphDepthIterations   = 10;

        //Offset relates to the world positions only
        [Tooltip("One Time offset from starting point of grid on x axis")]
        [SerializeField] float offsetX          = 0;
        [Tooltip("One Time offset from starting point of grid on y axis")]
        [SerializeField] float offsetY          = 0;
        [Tooltip("One Time offset from starting point of grid on z axis")]
        [SerializeField] float offsetZ          = 0;

        [Header("Node Sizes")]
        [Tooltip("Determine x and z axis distance of nodes from each other")]
        [SerializeField] float nodeDistance     = 1.0f;
        [Tooltip("Determine the next y axis size of node")]
        [SerializeField] float heightStep       = 1.0f;

        [Tooltip("Determine how far to check for ray collision")]
        [SerializeField] float DistanceToCheck  = 0.1f;
        [Tooltip("Determine how far to check along the y for collision")]
        [SerializeField] float nodeHeight       = 1.0f;
        [Tooltip("Determine how far to check along the x for collision")]
        [SerializeField] float nodeWidth        = 1.0f;

        /// <summary>
        ///     The node object height, width amd depth that make up the graph
        /// </summary>
        internal Vector3 NodeDimensions
        {
            get { return new Vector3(nodeWidth, nodeHeight, nodeWidth); }
        }


        #endregion


        #region Private Declarations

        string errorString              = "(Namespace: Pathfinding; Class: Grid; function: {0}) - {1}. Exception:{2}";
        List<string> errors             = new List<string>();
        List<IGridObject> gridObjects   = new List<IGridObject>();

        DateTime startTime;
        DateTime endTime;

        /// <summary> The closest Node to the starting position of the grid </summary>
        Node startNode;

        /// <summary>
        ///     store the potential directions of all of the neighbors
        /// </summary>
        List<Vector3Int> neighborDirections = new List<Vector3Int>()
        {
            new Vector3Int( 0, 0, 1 ),
            new Vector3Int( 0, 1, 1 ),
            new Vector3Int( 1, 1, 1 ),
            new Vector3Int( 1, 0, 0 ),
            new Vector3Int( 1, 1, 0 ),
            new Vector3Int( 0, 1, 0 ),

            new Vector3Int( 0, 0,  -1 ),
            new Vector3Int( 0, -1, -1 ),
            new Vector3Int( -1, -1,-1 ),
            new Vector3Int( -1, 0,  0 ),
            new Vector3Int( -1, -1, 0 ),
            new Vector3Int( 0, -1,  0 ) 

        };

        /// <summary> The list of nodes </summary>
        Node[,,] Nodes;
        Node[,,] CachedNodes;

        /// <summary> Grid Start Position, this is affected by the offsets          </summary>
        Vector3 gridStartPosition;

        /// <summary> The original center of the grid location, prior to offest     </summary>
        Vector3 objectStartPosition;

        /// <summary> The cached original rotation of the grid                      </summary>
        Quaternion startObjectRotation;


        #endregion


        #region Properties

        /// <summary>
        ///     A list of all the errors that may have occured during 
        ///     the grid creation
        /// </summary>
        public List<string> ClassErrorList
        {
            get
            {
                return errors;
            }
        }

        /// <summary>
        ///     How many nodes we have completed in 
        ///     our grid creation
        /// </summary>
        public int NodesCompleted
        {
            get;
            private set;
        }

        /// <summary>
        ///     Get the time in seconds that it takes 
        ///     to complete. If the grid is not completed
        ///     it will return 0.0f.
        /// </summary>
        public double TimeTaken
        {
            get
            {
                if ( !CreationComplete)
                {
                    return 0.0f;
                }
                else
                {
                    return (endTime - startTime).TotalSeconds;
                }
            }
        }

        /// <summary>
        ///     How many nodes to iterate over during each 
        ///     update call
        /// </summary>
        public int NodesToIterate
        {
            get
            {
                return nodesToIterate;
            }
            set
            {
                nodesToIterate = value;
            }
        }

        /// <summary>
        ///     The node closest to the starting location.
        ///     If this is null, you have to enable the calculation
        ///     for Calculate Closest StartNode
        /// </summary>
        public Node StartNode
        {
            get
            {
                return startNode;
            }
        }

        /// <summary>
        ///     Alerts whether the grid has been completed or not
        /// </summary>
        public bool CreationComplete
        {
            private set;
            get;
        } = false;

        /// <summary>
        ///     Determines if we have started the graph or not
        /// </summary>
        public bool CreationStarted
        {
            get;
            private set;
        } = false;

        /// <summary>
        ///     The start position of the graph
        ///     Can override the requirement for a start object
        /// </summary>
        internal Vector3 GridStartPosition
        {
            get
            {
                return gridStartPosition;
            }
            set
            {
                gridStartPosition = value;
            }
        }

        /// <summary>
        ///     The start position of the object moving along the grid
        /// </summary>
        internal Vector3 ObjectStartPosition
        {
            get { return objectStartPosition; }    
        }

        /// <summary>
        ///     The starting object of the grid
        /// </summary>
        internal GameObject StartObject 
        {
            get
            {
                return startObject;
            }
            set
            {
                startObject         = value;
                gridStartPosition   = startObject.transform.position;
                objectStartPosition = startObject.transform.position;
            }
        }

        internal GridManager manager { get; private set; }

        #region Graph Properties

        /// <summary> How many iterations along the y axis the graph has gone   </summary>
        public int GraphHeightIterations    { get { return graphHeightIterations;   } set { graphHeightIterations = value;  } }
        
        public int GraphNegY                { get { return graphYOffset;            } set { graphYOffset = value;           } }
        
        /// <summary> How many iterations along the x axis the graph has gone   </summary>
        public int GraphWidthIterations     { get { return graphWidthIterations;    } set { graphWidthIterations = value;   } }
        
        /// <summary> How many iterations along the z axis the graph has gone   </summary>
        public int GraphDepthIterations     { get { return graphDepthIterations;    } set { graphDepthIterations = value;   } }

        /// <summary> The overall height of the graph. This is calculated with the height iterations and the nodeheight     </summary>
        public float GraphHeight            { get { return graphHeightIterations * nodeHeight; } }

        /// <summary> The overall widght of the graph. This is calculated using the width iterations and node width         </summary>
        public float GraphWidth             { get { return graphWidthIterations * nodeWidth; } }

        /// <summary> The overall depth of the graph. Graph depth iterations with node width                                </summary>
        public float GraphDepth             { get { return graphDepthIterations * nodeWidth; } }

        /// <summary> The height of each node in the graph   </summary>
        public float NodeHeight             { get { return nodeHeight; } set { nodeHeight = value;  } }

        /// <summary> The width of each node in the graph   </summary>
        public float NodeWidth              { get { return nodeWidth; } set { nodeWidth = value;    } }

        #endregion


        /// <summary>
        ///     All the nodes currently created.
        ///     Will return an empty array if the
        ///     grid has not been completed
        /// </summary>
        internal Node[,,] GridNodes
        {
            get
            {
                if (CreationComplete)
                {
                    // return our nodes being created if we have not completed anything yet
                    if (CachedNodes == null) { return Nodes; }
                    else { return CachedNodes; }
                }
                else
                {
                    return new Node[0, 0, 0];
                }
            }
        }

        /// <summary>
        ///     Determines if the grid is the active grid
        /// </summary>
        internal bool IsActive
        {
            get;
            set;
        } = true;


        #endregion


        #region Initializers

        /// <summary>
        ///     Initalize my grid and create it.
        ///     the parameters have to be specified
        ///     individually. This should be used for a copy
        /// </summary>
        internal void Init(GridManager man)
        {
            init(graphHeightIterations, graphWidthIterations, graphDepthIterations, nodeHeight, nodeWidth,
                        objectStartPosition, man);
        }

        /// <summary>
        ///     Initialize my grid and create it with specified parameters
        /// </summary>
        /// <param name="_height">      The height iterations of the grid           </param>
        /// <param name="_width">       The width Iterations of the grid            </param>
        /// <param name="_depth">       The depth iterations of the grid            </param>
        /// <param name="n_height">     The height of each node                     </param>
        /// <param name="n_width">      The width of each node                      </param>
        /// <param name="_startObject"> The game object to start the graph off of   </param>
        /// <param name="_endObject">   the end object to base the graph off of     </param>
        public void Init(int _height, int _width, int _depth, float n_height, float n_width, GameObject _startObject, GridManager man)
        {
            StartObject             = _startObject;            
            
            init(_height, _width, _depth, n_height, n_width,
                        objectStartPosition, man);
        }

        /// <param name="_height">      The height iterations of the grid           </param>
        /// <param name="_width">       The width Iterations of the grid            </param>
        /// <param name="_depth">       The depth iterations of the grid            </param>
        /// <param name="n_height">     The height of each node                     </param>
        /// <param name="n_width">      The width of each node                      </param>
        /// <param name="_endPosition"> The target vector on the grid               </param>
        /// <param name="_startPosition"> The starting position of the grid         </param>
        void init(int _height, int _width, int _depth, float n_height, float n_width, Vector3 _startPosition, GridManager man)
        {
            manager = man;
            // Cache our stuff
            graphHeightIterations   = _height;
            graphWidthIterations    = _width;
            graphDepthIterations    = _depth;

            nodeHeight              = n_height;
            nodeWidth               = n_width;

            objectStartPosition     = _startPosition;

            startObjectRotation     = startObject.transform.rotation;

            // Create a multidimensional array of nodes
            Nodes                   = new Node[ graphWidthIterations, graphHeightIterations, graphDepthIterations ];
            NodesCompleted          = 0;
            ResetStartPosition();

            createGrid();
        }

        #endregion


        #region Creation

        /// <summary>
        ///     Create the grid with the
        ///     desired dimensions.
        /// </summary>
        void createGrid()
        {
            CreationComplete    = false;
            CreationStarted     = true;
            startTime           = DateTime.Now;
            
            UpdateGrid();
        }

        /// <summary>
        ///     Update the grid. This will iterate over the 
        ///     predefined gid and either create it in full or over 
        ///     an iterated amount of nodes per call
        /// </summary>
        public void UpdateGrid()
        {
            try
            {
                iterateGridNodes(startX: 0, startY: 0, startZ: 0, maxX: graphWidthIterations,
                           maxY: graphHeightIterations, maxZ: graphDepthIterations);

                bool complete = true;
                if (iterateOverFrames && NodesCompleted < Nodes.Length)
                {
                    complete = false;
                }

                if (complete) { markCreationComplete(); }

            }
            catch(Exception exc)
            {
                string error = string.Format(errorString, "UpdateGrid", "Updating the grid failed.", exc.Message);
                Debug.LogError( error ); errors.Add(error);
            }
        }

        /// <summary>
        ///     Mark the required items that we have finished the grid
        /// </summary>
        void markCreationComplete()
        {
            assignNeighbors();

            resetRequirements();

            // This grid has been completed
            CreationComplete    = true;
            CreationStarted     = false;
            endTime             = DateTime.Now;
            
        }

        /// <summary>
        ///     Reset the requirements that make us 
        ///     be able to make the grid and more nodes
        /// </summary>
        void resetRequirements()
        {
            xtotal = 0;
            ytotal = 0;
            ztotal = 0;
            CachedNodes = Nodes;
        }


        #region Create Nodes

        int xtotal  = 0;
        int ytotal  = 0;
        int ztotal  = 0;
        int count   = 0;

        /// <summary>
        ///     Iterate through all the grid nodes 
        ///     to create the ones we want. This is where
        ///     the iteration over updates occurs
        /// </summary>
        /// <param name="startX">   the start of the X loop </param>
        /// <param name="startY">   the start of the Y loop </param>
        /// <param name="startZ">   the start of the Z loop </param>
        /// <param name="maxX">     How far to go in the X  </param> 
        /// <param name="maxY">     How far to go in the Y  </param>
        /// <param name="maxZ">     How far to go in the Z  </param>
        /// <returns>
        ///     How Many nodes were iterated this time through
        /// </returns>
        int iterateGridNodes(int startX, int startY, int startZ, int maxX, int maxY, int maxZ)
        {
            int howMany = 0;

            // Build from the ground up 
            for (int y = startY + ytotal; y < maxY; y++)
            {
                for (int x = startX + xtotal; x < maxX; x++)
                {
                    for (int z = startZ + ztotal; z < maxZ; z++)
                    {
                        ztotal++;
                        NodesCompleted++;
                        count++;
                        createIndividualNode(x, y, z); 
                        howMany++;

                        // If we want to control how many nodes we are iterating per call
                        if ( iterateOverFrames && count > NodesToIterate)
                        {
                            var temp = count;
                            count = 0;
                            return temp; // send me back
                        }
                    }
                    xtotal++;
                    ztotal = 0;
                }
                ytotal++;
                xtotal = 0;
            }
            return howMany;
        }
        
        /// <summary>
        ///     Used to create the individual node on the grid. 
        ///     This is thread safe
        /// </summary>
        /// <param name="x">    x index </param>
        /// <param name="y">    y index </param>
        /// <param name="z">    z index </param>
        void createIndividualNode(int x, int y, int z)
        {            
            // Determine our graph type
            if (graphType == GraphType.NodeBasedGrid)
            {
                AddNewNodeBoxGraph(x, y, z);
            }
            else if ( graphType == GraphType.OverlayGrid)
            {
                AddNewNodeOverlayGrid(x, y, z);
            }
            else if (graphType == GraphType.TerrainSnap)
            {

            }
            else
            {
                // Nothing was chosen
                AddNewNodeBoxGraph(x, y, z);
            }
        }

        #endregion
        
        #endregion


        #region Neighbors

        /// <summary>
        ///     Run through each node and assign its neighbor nodes.
        ///     This should be run after all the nodes are created 
        ///     so that we do not assign null neighbors
        /// </summary>
        private void assignNeighbors()
        {
            assignNeighbors(minx: 0, maxx: graphWidthIterations, miny: 0, maxy: graphHeightIterations, minz: 0, maxz: graphDepthIterations);
        }

        /// <summary>
        ///     Assign neighbors for the designated areas
        /// </summary>
        /// <param name="minx"> The min x to look for   </param>
        /// <param name="maxx"> The max x to look for   </param>
        /// <param name="miny"> The min y to look for   </param>
        /// <param name="maxy"> The max y to look for   </param>
        /// <param name="minz"> The min z to look for   </param>
        /// <param name="maxz"> The max z to look for   </param>
        private void assignNeighbors(int minx, int maxx, int miny, int maxy, int minz, int maxz)
        {
            try
            {
                // Go through and get the neighbor nodes once we are all set
                for (int x = minx; x < maxx; x++)
                {
                    for (int y = miny; y < maxy; y++)
                    {
                        for (int z = minz; z < maxz; z++)
                        {
                            Nodes[ x, y, z ] = getNeighborNodes(Nodes[ x, y, z]);  
                        }
                    }
                }
            }
            catch(Exception exc) 
            {
                string error = $"(Namespace: Pathfinding; Class: Grid; function: assignNeighbors) - Unable to build neighbors list. Exception: {exc.Message}";
                Debug.LogError( error );
                errors.Add(error);
            }            
        }

        /// <summary> Nodes that are defined as solid objects, cannot go through these  </summary> 
        List<NodeType> solidNodes = new List<NodeType>() { NodeType.Blocked, NodeType.Terrain };
        /// <summary>
        ///     Obtain all of the neighbor nodes for 
        ///     the passed node
        /// </summary>
        /// <param name="myNode">   The node to get neighbors for   </param>
        /// <returns>
        ///     A node populated with its neighbors
        /// </returns>
        /// <remarks>
        ///     Will ignore null values
        /// </remarks>
        Node getNeighborNodes(Node myNode)
        {
            // Get all of the neighbors
            foreach (Vector3Int dir in neighborDirections)
            {
                // cache the direction integers
                int x = dir.x + myNode.X;
                int z = dir.z + myNode.Z;
                int y = dir.y + myNode.Y;

                try
                {
                    // Make sure we are within the bounds of our array
                    if ( checkIfWithinGraphRange( x: x , y: y, z: z  ) )
                    {
                        Node n = Nodes[x, y, z];
                        
                        if ( n != null )
                        {
                            myNode.Neighbors.Add( n );

                            // If we are neighboring solid material but not solid ourselves...
                            if ( !solidNodes.Contains( myNode.nodeType ) && solidNodes.Contains( n.nodeType ) )
                            {
                                // Add the bordering objects cost to our border node
                                myNode.Costs.AddRange( n.Costs ); myNode.Layers.AddRange( n.Layers );
                                myNode.nodeType = NodeType.OpenBorder;
                            }
                        }
                    }
                }
                catch (Exception){ }
            }
            return myNode;
        }


        /// <summary>
        ///     Determine if the neighbor would be within the bounds of the graph
        /// </summary>
        /// <param name="x">    The current x coordinate to check   </param>
        /// <param name="y">    The current y coordinate to check   </param>
        /// <param name="z">    The current z coordinate to check   </param>
        /// <returns>
        ///     A bool of true or false
        /// </returns>
        internal bool checkIfWithinGraphRange(int x, int y, int z)
        {
            if (x > graphWidthIterations - 1 || x < 0)
            {
                return false;
            }

            if (z > graphDepthIterations -1 || z < 0)
            {
                return false;
            }

            if (y > graphHeightIterations - 1|| y < 0)
            {
                return false;
            }

            return true;

        }


        /// <summary>
        ///     Will check and lock the coordinates within the grid
        /// </summary>
        /// <param name="x">    The x coord to check    </param>
        /// <param name="y">    The y coord to check    </param>
        /// <param name="z">    The z coord to check    </param>
        /// <returns>
        ///     Will return a bool of true if all were within range. A boolean
        ///     of false will be returned and the values will be locked into the 
        ///     grid coordinates
        /// </returns>
        internal bool lockWithinGridRange(ref int x, ref int y, ref int z)
        {
            bool within = true;
            if (x < 0)
            {
                x = 0;
                within = false;
            }
            if (y < 0)
            {
                y = 0;
                within = false;
            }
            if (z < 0)
            {
                z = 0;
                within = false;
            }

            if (x > graphWidthIterations - 1 )
            {
                x = graphWidthIterations - 1;
                within = false;
            }

            if (z > graphDepthIterations - 1 ) 
            {
                z = graphDepthIterations - 1;
                within = false;
            }

            if (y > graphHeightIterations - 1 )
            {
                y = graphHeightIterations - 1;
                within = false;
            }

            return within;
        }

        #endregion


        #region Grid types

        /// <summary>
        ///     Create an overlay grid node.
        ///     This grid creates a node forward and right offset
        ///     from the central position
        /// </summary>
        /// <param name="x">    X index </param>
        /// <param name="y">    Y index </param>
        /// <param name="z">    Z Index </param>
        private void AddNewNodeOverlayGrid(int x, int y, int z)
        {
            // TODO create this grid

        }

        /// <summary>
        ///     Lock it to the terrain
        /// </summary>
        /// <param name="x">    X index </param>
        /// <param name="y">    Y index </param>
        /// <param name="z">    Z Index </param>
        private void AddNewTerrainSnap(int x, int y, int z)
        {
            //TODO Create a new one here
        }

        /// <summary>
        ///     Create a new node at the given index.
        ///     This creates a node at the given index
        ///     by creating box hits in all directions
        /// </summary>
        /// <param name="x">    X index </param>
        /// <param name="y">    Y index </param>
        /// <param name="z">    Z Index </param>
        private void AddNewNodeBoxGraph(int x, int y, int z)
        {
            //  Get the correct world space information
            float posX = gridStartPosition.x + ( x * nodeDistance    );
            float posZ = gridStartPosition.z + ( z * nodeDistance    );
            float posY = gridStartPosition.y + ( y * heightStep      );

            //Create a new node and update it's values
            Vector3 pos = new Vector3(posX, posY, posZ);

            RaycastHit[] hits = new RaycastHit[0];
            if (CheckForward)   { hits = hits.AddArray( GetBoxColliderHits(  Vector3.forward,   new Vector3( pos.x, pos.y, pos.z ) ) ); }

            if (CheckBack)      { hits = hits.AddArray( GetBoxColliderHits( -Vector3.forward,   new Vector3( pos.x, pos.y, pos.z ) ) ); }

            if (CheckDown)      { hits = hits.AddArray( GetBoxColliderHits(  Vector3.down,      new Vector3( pos.x, pos.y, pos.z ) ) ); }

            if (CheckUp)        { hits = hits.AddArray( GetBoxColliderHits( -Vector3.down,      new Vector3( pos.x, pos.y, pos.z ) ) ); }

            if (CheckLeft)      { hits = hits.AddArray( GetBoxColliderHits( -Vector3.right,     new Vector3( pos.x, pos.y, pos.z ) ) ); }

            if (CheckRight)     { hits = hits.AddArray( GetBoxColliderHits(  Vector3.right,     new Vector3( pos.x, pos.y, pos.z ) ) ); }


            // Create the node and determine it options
            Node node = new Node(x, y, z, pos, NodeType.Open);
            DetermineNodeTypeHit(hits, ref node);

            node.distanceTraveled = Vector3.Distance(node.Position, objectStartPosition);
            
            calculateStartNode(node); 

            // then place it to the grid
            Nodes[x, y, z] = node;
        }

        /// <summary>
        ///     Check if we should set a new start/end node
        /// </summary>
        /// <param name="checkNode"> The node to check proximity to the start</param>
        void calculateStartNode(Node checkNode)
        {
            if (startNode == null) { startNode = checkNode; }
            if (CalculateClosestStartNode)
            {
                startNode = checkNode.distanceTraveled < startNode.distanceTraveled ? checkNode : startNode;
            }
        }

        /// <summary>
        ///     Get the hits directions
        /// </summary>
        /// <param name="dir">      The direction to check                  </param>
        /// <param name="startPos"> The starting position of the collider   </param>
        /// <returns>
        ///     A list of the box collider hits
        /// </returns>
        private RaycastHit[] GetBoxColliderHits(Vector3 dir, Vector3 startPos)
        {
            return Physics.BoxCastAll(center: new Vector3(startPos.x, startPos.y, startPos.z), halfExtents: new Vector3(nodeWidth / 2, nodeHeight / 2, nodeWidth / 2),
                        direction: dir, orientation: startObjectRotation, maxDistance: DistanceToCheck, includeLayers);        
        }

        #endregion


        #region Utilities

        #region Public 

        /// <summary>
        ///     Update specific nodes within a range, 
        ///     bypasses recreating the whole graph
        /// </summary>
        /// <param name="centerRadius">     The center of the area to search for the closest node of    </param>
        /// <param name="width">            The width we want to re-search                              </param>
        /// <param name="height">           The height we want to re-search                             </param>
        /// <param name="depth">            The depth we want to re-search                              </param>
        /// <param name="keepIterations">   Determines if we should keep the grid iteration total       </param>
        public int UpdateNodes(Vector3 centerRadius, float width, float height, float depth, bool keepIterations)
        {
            bool hold           = iterateOverFrames;
            iterateOverFrames   = keepIterations;
            Node closest        = getClosestNode( centerRadius, new List<NodeType>(), false );

            int close_x = closest.Index.x;
            int close_y = closest.Index.y;
            int close_z = closest.Index.z;

            int max_x   = (int)Mathf.Clamp( value: ( close_x + ( ( width    ) / nodeDistance ) ), min: 0, max: graphWidthIterations     );
            int max_y   = (int)Mathf.Clamp( value: ( close_y + ( ( height   ) / heightStep   ) ), min: 0, max: graphHeightIterations    );
            int max_z   = (int)Mathf.Clamp( value: ( close_z + ( ( depth    ) / nodeDistance ) ), min: 0, max: graphDepthIterations     );

            int min_x   = (int)Mathf.Clamp( value: ( close_x - ( ( width    ) / nodeDistance ) ), min: 0, max: graphWidthIterations     );
            int min_y   = (int)Mathf.Clamp( value: ( close_y - ( ( height   ) / heightStep   ) ), min: 0, max: graphHeightIterations    );
            int min_z   = (int)Mathf.Clamp( value: ( close_z - ( ( depth    ) / nodeDistance ) ), min: 0, max: graphDepthIterations     );

            var amount = iterateGridNodes(startX: min_x, startY: min_y, startZ: min_z, maxX: max_x,
                        maxY: max_y, maxZ: max_z);

            assignNeighbors( minx: min_x, maxx: max_x, miny: min_y, maxy: max_y, minz: min_z, maxz: max_z ) ;

            resetRequirements();
            iterateOverFrames = hold;
            return amount;

        }

        /// <summary>
        ///     Get a random node on the grid
        /// </summary>
        /// <returns>   
        ///     A random node on the grid within the range
        /// </returns>        
        public Node GetRandomNodeOnGrid()
        {
            int x = UnityEngine.Random.Range( 0, graphWidthIterations   );
            int y = UnityEngine.Random.Range( 0, graphHeightIterations  );
            int z = UnityEngine.Random.Range( 0, graphWidthIterations   );

            return Nodes[ x, y, z ];
        }

        /// <summary>
        ///     Get a random node within a distance given 
        /// </summary>
        /// <param name="startPosition">    The position we want to start the search off of </param>
        /// <param name="xRange">           How far along x to go       </param>
        /// <param name="yRange">           How far along y to go       </param>
        /// <param name="zRange">           How far along z to go       </param>
        /// <param name="xOffset">          The x start position        </param>
        /// <param name="yOffset">          The y start position        </param>
        /// <param name="zOffset">          The z start position        </param>
        /// <param name="IgnoreThese">      Determines if we want to avoid blocked nodes</param>
        /// <returns>
        ///     A randomized node with the given ranges
        /// </returns>
        public Node GetRandomNodeWithinRange(Vector3 startPosition, float xRange, float yRange, float zRange, List<NodeType> IgnoreThese, float xOffset = 0.0f, float yOffset = 0.0f, float zOffset = 0.0f)
        {
            // Randomize our distance
            float x = UnityEngine.Random.Range( startPosition.x + xOffset, startPosition.x + xOffset + xRange);
            float y = UnityEngine.Random.Range( startPosition.y + yOffset, startPosition.y + yOffset + yRange); 
            float z = UnityEngine.Random.Range( startPosition.z + zOffset, startPosition.z + zOffset + zRange); 

            // Return the closes node to where we want to be
            return getClosestNode( new Vector3( x, y, z ), IgnoreThese, false );            
        }

        /// <summary>
        ///     Get a random node within a distance given 
        /// </summary>
        /// <param name="startNode">    the node to start off of    </param>
        /// <param name="xRange">       How far along x to go       </param>
        /// <param name="yRange">       How far along y to go       </param>
        /// <param name="zRange">       How far along z to go       </param>
        /// <param name="xOffset">      The x start position        </param>
        /// <param name="yOffset">      The y start position        </param>
        /// <param name="zOffset">      The z start position        </param>
        /// <param name="IgnoreThese">  Determines if we want to avoid blocked nodes</param>
        /// <returns>
        ///     A randomized node with the given ranges
        /// </returns>
        public Node GetRandomNodeWithinRange(Node startNode, float xRange, float yRange, float zRange, List<NodeType> IgnoreThese, float xOffset = 0.0f, float yOffset = 0.0f, float zOffset = 0.0f )
        {
            return GetRandomNodeWithinRange( startNode.Position , xRange: xRange, yRange: yRange, zRange: zRange, xOffset: xOffset, yOffset: yOffset, zOffset: zOffset, IgnoreThese: IgnoreThese);
        }

        /// <summary>
        ///     Get the closest Node of a certain type
        /// </summary>
        /// <param name="desiredLocation">  The location of our target          </param>
        /// <param name="type">             The type of node we are looking for </param>
        /// <returns>
        ///     the closest node of the specified type
        /// </returns>
        public Node getClosestNode(Vector3 desiredLocation, NodeType type)
        {
            Node returnNode = null;
            float distance = Mathf.Infinity;

            foreach (Node checkNode in Nodes)
            {
                // Check our avoided types
                if ( checkNode.nodeType == type )
                {
                    var checkDistance = Vector3.Distance(a: desiredLocation, b: checkNode.Position);
                    if (checkDistance < distance)
                    {
                        distance = checkDistance;
                        returnNode = checkNode;
                    }
                }
            }

            return returnNode;
        }

        /// <summary>
        ///     Get the closes node to a desired location
        /// </summary>
        /// <param name="desiredLocation">  The location of our target                                                                      </param>
        /// <param name="avoidTypes">       Avoid these Nodes                                                                               </param>
        /// <param name="closestGuess">     If we are within the height / width range of a node height / width, pull the first best guess   </param>
        /// <returns>
        ///     The closest node to the desired location
        /// </returns>
        public Node getClosestNode(Vector3 desiredLocation, List<NodeType> avoidTypes, bool closestGuess)
        {
            float distance  = Mathf.Infinity;
            Node returnNode = null;

            foreach ( Node checkNode in GridNodes )
            {
                if ( checkNode != null)
                {
                    // Check our avoided types
                    if ( !avoidTypes.Contains( checkNode.nodeType ) )
                    {
                        var checkDistance = Vector3.Distance(a: desiredLocation, b: checkNode.Position);
                        if ( checkDistance  < distance )
                        {   
                            distance    = checkDistance;
                            returnNode  = checkNode;
                        }

                        if ( closestGuess )
                        { 
                            // Check if we are within one iteration, as this will be the closest                        
                            if ( Mathf.Abs( (float)( returnNode.Position.y - (nodeHeight +.1) ) )  < 0 ) { return returnNode; }
                            if ( Mathf.Abs(checkDistance) < nodeDistance) { return returnNode; }
                        }
                    }
                }
            }

            return returnNode;
        }

        #endregion


        #region Private


        /// <summary>
        ///     Create an offset so we search all around
        ///     the gameobject
        /// </summary>
        void ResetStartPosition()
        {
            gridStartPosition = startObject.transform.position;
            
            if (offSetGraph)
            {
                if( halfYIterations )   // Use half the height downwards
                {
                    if (GraphHeightIterations % 2 != 0) { graphYOffset = (int)Mathf.Round(GraphHeightIterations / 2); }
                    else { graphYOffset = GraphHeightIterations / 2; }
                    graphYOffset = -graphYOffset;
                }

                //  Create our offset
                gridStartPosition.x = gridStartPosition.x - ( ( GraphWidthIterations * nodeDistance   ) / 2)  + offsetX;
                gridStartPosition.y = gridStartPosition.y + ( ( GraphNegY * heightStep                )    )  + offsetY;
                gridStartPosition.z = gridStartPosition.z - ( ( GraphDepthIterations * nodeDistance   ) / 2)  + offsetZ;
            }
        }

        /// <summary>
        ///     Lets determine what we are hitting
        ///     fills the reference node with relevant costs
        ///     and type information
        /// </summary>
        /// <param name="hits"> The raycast hits the node encountered   </param>
        /// <param name="node"> The node to update                      </param>
        void DetermineNodeTypeHit(RaycastHit[] hits, ref Node node)
        {
            NodeType nodeType   = NodeType.Open;
            node.hitCount       = hits.Length;

            // we have no hits, leave me alone
            if (hits.Length < 1)
            {
                node.nodeType = nodeType;
                return;
            }
            
            int checks = 0;
            // Go through all the node type hits
            for (int i = 0; i < hits.Length; i++)
            {
                //nodeType = NodeType.Blocked;
                var checkHit = hits[i];

                checkGameObjectGrid(checkHit);

                //  First check if we are on a terrain node
                if ( checkHit.collider.GetType() == typeof( TerrainCollider ) )    // if we hit the terrain
                {                    
                    nodeType            = NodeType.Terrain;
                    node.isOnTerrain    = true; 
                }
                else
                {
                    nodeType = NodeType.Blocked;    // We have hit something other than terrain with all of our hits
                }


                // prioritize the nodetype
                if ( nodeType < node.nodeType )
                {
                    node.nodeType = nodeType;
                }

                // Add each layer this node touches 
                var layer =  checkHit.transform.gameObject.layer;
                if ( !node.Layers.Contains( layer ) ) { node.Layers.Add( layer ); }

                // Costs check
                foreach ( NodeLayerCosts costs in LayerCosts)
                {
                    if ( Utilities_PF.IsLayerMatch( layer: layer, compareAgainst: costs.Layer ) )
                    {
                        checks++;
                        node.Costs.Add( costs );
                        if ( !costs.isBlocked )
                        {
                            nodeType = node.isOnTerrain ? NodeType.Terrain : NodeType.Open;   // Override if we have already determined this was a hit
                        }
                    }
                }

                // Make sure we did not hit anything outside of costs to override node type
                if ( checks > 0 && checks == hits.Length ) 
                { 
                    node.nodeType = nodeType; 
                }
                else
                {
                    // If there is on collider that is on the terrain
                    if ( checks == hits.Length - 1 && node.isOnTerrain)
                    {
                        node.nodeType = NodeType.Terrain;
                    }
                }
            }
        }


        #region Grid Object Interactions

        /// <summary>
        ///     Check if the raycast is
        ///     an interactable grid object
        /// </summary>
        /// <param name="hit"></param>
        bool checkGameObjectGrid(RaycastHit hit)
        {
    
            var temp = hit.transform.gameObject;
            if (temp)
            {
                var gridO = hit.transform.gameObject.GetComponent<IGridObject>();
                if ( gridO != null )
                {
                    return gridO.GridContact( manager );
                }
            }
            return false;
        }
        
        /// <summary>
        ///     Clear all the objects for this grid
        /// </summary>
        void ClearGridObjects()
        {
            foreach(IGridObject gridO in gridObjects)
            {
                gridO.RemoveGrid(grid: manager);
            }
        }

        #endregion
        
        #endregion


        #endregion
    }

}
