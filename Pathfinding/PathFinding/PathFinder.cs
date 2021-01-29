using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathFindingAsteria
{
    /// <summary>
    ///     The pathfinding algorithm. This will find a path on a node based grid. 
    ///     Settings and parameters are determine through a PathfindingParameters class
    ///     instance that can be passed in through the constructor
    /// </summary>
    public class PathFinder
    {
        #region Private Declarations

        /// <summary> When the pathfinder started   </summary>
        DateTime startTime  = DateTime.Now;
        /// <summary> When the pathfinder ended     </summary>
        DateTime endTime    = DateTime.Now;

        /// <summary> The Nodes being explored at each iteration    </summary>
        PathfindingPriorityQueue<Node> frontierNodes    = new PathfindingPriorityQueue<Node>();

        /// <summary> A list of already explored nodes  </summary>
        List<Node> exploredNodes                        = new List<Node>();

        /// <summary> A list of already explored nodes indexes </summary>
        List<Vector3Int>   exploredIndexes              = new List<Vector3Int>();

        /// <summary> The nodes of the full grid    </summary>
        Grid grid;

        PathfindingParameters Parameters;

        // Our Two Nodes
        Node startNode;
        Node endNode;
        Node closestToEnd;

        // The iterations of height and width, a comparison of the object dimensions with the graph iterations
        int heightIterations            = 0;
        int horizontalIterations        = 0;
        int heightTraverseIterations    = 0;
        int heightFallIterations        = 0;
        #endregion


        #region Properties

        /// <summary>
        ///     A list of errors with time stamps associated with them
        /// </summary>
        internal List<PathFindingError> Errors
        {
            get;
            set;
        } = new List<PathFindingError>();

        /// <summary>
        ///     A list of errors without the time stamps
        /// </summary>
        internal List<string> ErrorMessages
        {
            get
            {
                List<string> temp = new List<string>();
                foreach(PathFindingError error in Errors)
                {
                    temp.Add(error.Error);
                }

                return temp;
            }
        }


        /// <summary>
        ///     The amount of time that the path
        ///     took to create in seconds
        /// </summary>
        internal double TimeTaken
        {
            get
            {
                return (endTime - startTime).TotalSeconds;
            }
        }


        /// <summary>
        ///     The time the path was completed
        /// </summary>
        internal DateTime StartTime
        {
            get { return startTime; }
        }


        /// <summary>
        ///     The time the path was completed
        /// </summary>
        internal DateTime EndTime
        {
            get { return endTime; }
        }


        /// <summary>
        ///     A list of all the explored nodes 
        ///     completed during the search
        /// </summary>
        internal List<Node> ExploredNodes
        {
            get { return exploredNodes; }
        }

        /// <summary>A list of all the path coordinates searched</summary>
        public List<Vector3> PathCoordinates
        {
            get
            {
                return GetPathCoordinates();
            }
        }

        List<Vector3> pathCoordinates = new List<Vector3>();

        /// <summary> Alerts us if the path is completed    </summary>
        internal bool PathFinished
        {
            get;
            private set;
        } = false;


        /// <summary> Alerts us if the path is completed    </summary>
        internal bool PathStarted
        {
            get;
            private set;
        } = false;


        /// <summary> Alerts us if the path is completed    </summary>
        internal bool ObjectiveFound
        {
            get;
            private set;
        } = false;

        /// <summary> The chosen path                       </summary>
        internal List<Node> pathNodes
        {
            get;
            private set;
        } = new List<Node>();

        /// <summary> The node where the path started from  </summary>
        internal Node StartNode
        {
            get { return startNode; }
        }

        /// <summary> the node the path ended on            </summary>
        internal Node EndNode
        {
            get { return endNode; }
        }

        #endregion


        #region Initializers

        /// <summary>
        ///     Initialize the path with just the parameters.
        ///     This will contain a cached grid within in
        /// </summary>
        /// <param name="pathfinding">  The parameters to  use when finding a path  </param>
        public void Init(PathfindingParameters pathfinding)
        {
            Parameters  = pathfinding;
            grid        = pathfinding.gridBase;
            init();
        }

        /// <summary>
        ///     Start the path with a passed grid.
        ///     This will bypass any grid stored in the pathfinding object
        /// </summary>
        /// <param name="pathfinding">  The parameters to  use when finding a path  </param>
        /// <param name="gridBase">     The grid to use to find a path              </param>
        public void Init(PathfindingParameters pathfinding, Grid gridBase)
        {
            grid        = gridBase;
            Parameters  = pathfinding;
            init();
        }


        /// <summary>
        ///     Initialize me - the private section all Public inits point to
        /// </summary>
        void init()
        {
            if ( grid == null )
            {
                Errors.Add( new PathFindingError( error: $"[Pathfinder.Init())] There is no grid associated with the pathfinder") );
                return;
            }
            cacheItems();
            FullPathCreation();
        }

        /// <summary>
        ///     Cache items for the class
        /// </summary>
        void cacheItems()
        {
            // Total; iterations req'd for pathfinders size
            heightIterations            = (int)( Parameters.ObjectHeight        / grid.NodeHeight   ); 
            horizontalIterations        = (int)( (Parameters.ObjectWidth / 2)   / grid.NodeWidth    );
            heightTraverseIterations    = (int)( Parameters.maxHeightTraversal  / grid.NodeHeight   );
            heightFallIterations        = (int)( Parameters.maxDropHeight       / grid.NodeHeight   );
            initialFall                 = true;
        }
        #endregion


        #region Path Creation        

        /// <summary>
        ///     Start our full path creation
        /// </summary>
        void FullPathCreation()
        {
            startCreation();

            // Thread if desired 
            if ( Parameters.threadPathCreation )
            {
                Thread T = new Thread(() => createOurPath() );
                T.Name = $"PathfindingThread";
                T.Start();
            }
            else
            {
                createOurPath();
            }
        }

        /// <summary>
        ///     Create our path from start to end
        /// </summary>
        private void createOurPath()
        {
            try
            {
                // While we have objects to search and our path is not finished
                while (!PathFinished && frontierNodes != null)
                {
                    if (frontierNodes.Count > 0 && !(startTime < DateTime.Now.AddSeconds((double)-Parameters.timeAllowed)))
                    {
                        // Get the current node off of the frontier
                        Node currentNode = frontierNodes.Dequeue();

                        if ( checkFall( currentNode ) && checkNodeDimensionOverrides(currentNode) )
                        {
                            currentNode = controlNodeGroundSnap(currentNode);
                            ExploreThisNode( currentNode );

                            // Search our frontier
                            ExpandGraph(currentNode);

                            //  If we have our end node, kill it
                            if ( frontierNodes.Contains( endNode ) )
                            {
                                pathNodes           = GetPathNodes(endNode);
                                PathFinished        = true;
                                ObjectiveFound      = true;
                            }
                            else { ObjectiveFound = false; }
                        }
                    }
                    else
                    {
                        // There are no more nodes to search on the frontier and the end was not found
                        pathNodes = GetPathNodes(closestToEnd);
                        PathFinished = true;
                    }

                    // something terrible would have happened
                    if (exploredIndexes.Count + 1 > grid.GridNodes.Length)
                    {
                        Debug.LogError("Searched all the nodes without finding end");
                        break;
                    }
                }
            }
            catch
            {
                Debug.LogError("Failure in the pathfinding createOurPath() function");
                Errors.Add( new PathFindingError( error: "[Pathfinder.createOurPath()] We are unable to find our path." ) );
            }
            PathStarted     = false;
            PathFinished    = true;
            endTime         = DateTime.Now;
        }

        /// <summary>
        ///     Set the important class start information
        /// </summary>
        void startCreation()
        {
            startTime = DateTime.Now;

            // The path starts here
            PathFinished    = false;
            PathStarted     = true;
            AssignNodes();
            ClearExploredNodes();

            // Create our pathfinding queue
            frontierNodes   = new PathfindingPriorityQueue<Node>();
            frontierNodes.Enqueue(startNode);
        }

        #region Searches

        /// <summary>
        ///     Expand the path through available neighbors
        /// </summary>
        /// <param name="currentNode">  The node we are searching through   </param>
        void ExpandGraph(Node currentNode)
        {
            if (currentNode != null)
            {
                // Get distances for the node
                currentNode.distanceToTravel = getDistanceToEnd( currentNode.Position       );
                currentNode.distanceTraveled = getDistanceFromStart( currentNode.Position   );

                // Go through each node neighbor
                for (int i = 0; i < currentNode.Neighbors.Count; i++)
                {
                    // Copy an instance of the neighbor to work off of
                    Node neighborNode = currentNode.Neighbors[i].Copy();

                    // if the current neighbor has not been explored and it is within our path types
                    if ( !exploredIndexes.Contains( neighborNode.Index ) && canTraverseNode( ref neighborNode ) )//&& traverseButStop(ref neighborNode, currentNode.nodeType ) ) 
                    {
                        exploredIndexes.Add( neighborNode.Index);

                        // Find our path
                        if (Parameters.TypeOfPath == PathType.AStar)
                        {
                            AStarExpansion(currentNode, neighborNode);
                        }
                        else if (Parameters.TypeOfPath == PathType.GreedyBestFirst)
                        {
                            GreedyFirstSearch(currentNode, neighborNode);
                        }
                        else if ( Parameters.TypeOfPath == PathType.BreadthFirstSearch)
                        {
                            BreadthFirstSearch(currentNode, neighborNode);
                        }
                        else
                        {
                            AStarExpansion(currentNode, neighborNode);
                        }
                    }
                }
                DetermineIfEndNode( currentNode );
            }
        }

        /// <summary>
        ///     A star Expansion
        ///     Searches for the next node using A* algorithm
        /// </summary>
        /// <param name="currentNode">  The node currently being examined   </param>
        /// <param name="testNode">     The nodes neighbor in question      </param>
        private void AStarExpansion(Node currentNode, Node testNode)
        {
            // calculate total distance costs
            float distanceToNeighbor    = GetNodeDistances(currentNode.Position, testNode.Position);
            float newDistanceTraveled   = distanceToNeighbor + currentNode.distanceTraveled + 
                                            getTotalNodeCosts(currentNode) +
                                            getTotalHeightCosts(height: ( testNode.Y - currentNode.Y ) );

            // TODO break out the newdistance equation without costs, as then it can be used below
            // if a shorter path exists to the neighbor via this node, re-route
            if ( float.IsPositiveInfinity(testNode.distanceTraveled) || newDistanceTraveled < testNode.distanceToTravel )
            {
                testNode.previous           = currentNode;
                testNode.distanceTraveled   = newDistanceTraveled;
            }

            if ( !checkNodeAvailability( currentNode ) ) { return; }

            // if the neighbor is not part of the frontier, add this to the priority queue
            if ( !checkContainsNode( Check: testNode, checkThese: frontierNodes ) && grid != null )
            {
                // base priority, F score,  on G score (distance from start) + H score (estimated distance to goal)
                float distanceToGoal    = GetNodeDistances(testNode.Position, endNode.Position);
                testNode.priority       = testNode.distanceTraveled + distanceToGoal;

                // add to priority queue using the F score
                frontierNodes.Enqueue(testNode);
            }
        }

        /// <summary>
        ///     Greedy first search algorithm
        ///     Search based on the next closest node 
        ///     to the end
        /// </summary>
        /// <param name="currentNode">      The node currently being examined   </param>
        /// <param name="neighborNode">     The nodes neighbor in question      </param>
        private void GreedyFirstSearch(Node currentNode, Node neighborNode)
        {
            // calculate total distance costs                        
            float newDistanceToTravel       = getDistanceToEnd(neighborNode.Position);

            neighborNode.previous           = currentNode;
            neighborNode.distanceToTravel   = newDistanceToTravel;

            if ( !checkNodeAvailability( currentNode ) ) { return; }

            // if the neighbor is not part of the frontier, add this to the priority queue
            if (!checkContainsNode(Check: neighborNode, checkThese: frontierNodes) && grid != null)
            {
                neighborNode.priority = neighborNode.distanceToTravel;
                frontierNodes.Enqueue(neighborNode);
            }
        }

        /// <summary>
        ///     Pathfinding that relies on the breadth first search
        ///     spanning out on first come first serve basis for the 
        ///     search of the end node
        /// </summary>
        /// <param name="currentNode">      The node currently being examined   </param>
        /// <param name="neighborNode">     The nodes neighbor in question      </param>
        void BreadthFirstSearch( Node currentNode, Node neighborNode)
        {
            // calculate total distance costs                        
            float newDistanceToTravel       = getDistanceToEnd(neighborNode.Position);

            neighborNode.previous           = currentNode;
            neighborNode.distanceToTravel   = newDistanceToTravel;

            if ( !checkNodeAvailability( currentNode ) ) { return; }

            // if the neighbor is not part of the frontier, add this to the priority queue
            if ( !checkContainsNode(Check: neighborNode, checkThese: frontierNodes) && grid != null)
            {
                neighborNode.priority = frontierNodes.Count; // prioritize them by when they are added
                frontierNodes.Enqueue(neighborNode);
            }
        }

        #endregion

        #endregion


        #region Utilities



        /// <summary>
        ///     Determines whether or not we can
        ///     traverse this node while pathfinding
        /// </summary>
        /// <param name="node"> The node to check if we can traverse over   </param>
        /// <returns>
        ///     A bool true if we can clear it
        ///     A bool false if we cannot   
        /// </returns>
        /// <remarks>
        ///     Tread lightly, since grid and pathfinder can both create costs
        ///     they can step on each others toes
        /// </remarks>
        bool canTraverseNode( ref Node node)
        {
            // We only care about the layers we want to override, the grid would have take care of the original
            // Check cost overrides and the layers that we are on if there are any overlap
            bool overrideNode   = false;
            bool canClear       = canClearNode( ref node );
            
            foreach (int layer in node.Layers)
            {
                checkNodeLayerOverride(layer, canClear, out float cost, out bool blocked);
                if (blocked) 
                { return false; }
                else
                {
                    overrideNode = true;
                }
            }
             
            if (overrideNode) { return true; }  // we are overriding this node so we can traverse it despite the node type that the grid gave it

            if ( Parameters.pathTypes.Contains( node.nodeType ) ) { return true; }

            return false;   // no matches here
        }        

        /// <summary>
        ///     Determine if we can pass through the node 
        ///     Checks barriers, terrain, and anything 
        ///     excluded will be passed through.
        ///     This is not determined by path objects
        /// </summary>
        /// <param name="node"> The node to check if we can traverse    </param>
        /// <returns>
        ///     A boolean if we can or cannot clear through the node
        /// </returns> 
        bool canClearNode(ref Node node)
        {

            if ( Parameters.barrierTypes.Contains( node.nodeType    )   )   { return false; }

            // If we want to block terrain but its of another nodetype
            if ( Parameters.TerrainIsBarrier && node.isOnTerrain    )       { return false; }            

            return true;
        }

        /// <summary>
        ///     Determines if we can land on this type of node
        /// </summary>
        /// <param name="node">         The node we would like to check </param>
        /// <param name="comingFrom">   The nodetype we are coming from </param>
        /// <returns>
        ///     A bool determining if we can walk on this
        /// </returns>
        bool traverseButStop( ref Node node )
        {
            
            if ( Parameters.walkAndStopSearch.Contains( node.nodeType ) )
            { 
                    return true; 
            }
            return false;
        }

        /// <summary>
        ///     Need to check the index of the neighbor to see if it is the end node
        /// </summary>
        /// <param name="checkNode"> Compare this to the end node to check if we have made it   </param>
        /// <remarks>
        ///     This is required because we copied the node. The neighbor will not 
        ///     be a copy even if it is at the same index
        /// </remarks>
        private bool DetermineIfEndNode(Node checkNode)
        {
            // Set as closest Node to the end (used if we do not find the end in our pathfinding
            if (closestToEnd == null || checkNode.distanceToTravel < closestToEnd.distanceToTravel) { closestToEnd = checkNode; }

            if ( checkNode.Equals( EndNode ) )
            { 
                endNode = checkNode.Copy( KeepNeighbors: true, keepPrevious: true); 
                frontierNodes.Enqueue( endNode );
                return true;
            }
            return false;
        }

        #region Node Costs and Overrides

        /// <summary>
        ///     Check our pathfinding overrides with the layers
        ///     within a node generated during the grid creation
        /// </summary>
        /// <param name="layer">        The layer we are searching for equality         </param>
        /// <param name="canClearNode"> Whether we can inherently clear this node       </param>
        /// <param name="cost">         The costs that we are deriving from the layer   </param>
        /// <param name="isBlocked">    Determines whether or not we are blocked here   </param>
        /// <remarks>
        ///     Checks our layer overrides to determine costs and traversability
        /// </remarks>
        void checkNodeLayerOverride( int layer, bool canClearNode ,out float cost, out bool isBlocked)
        {
            isBlocked   = !canClearNode;
            cost        = 0.0f;
            bool overrideNode = false;

            // if our path is overriding what we can use
            foreach (NodeLayerCosts nlc in Parameters.OverrideCosts)
            {
                if (Utilities_PF.IsLayerMatch(layer: layer, compareAgainst: nlc.Layer))
                {
                    cost += nlc.Cost;
                    if (nlc.isBlocked) 
                    { isBlocked = true; return; }
                    else
                    {
                        overrideNode = true;
                    }
                }
            }

            // we have overriden the nodes original blockage
            if (overrideNode) { isBlocked = false; }

        }

        /// <summary>
        ///     Get the total costs of a nodes movement
        /// </summary>
        /// <param name="n"> The node of which to check costs </param>
        /// <returns>
        ///     A float of the total costs on the node
        /// </returns>
        float getTotalNodeCosts(Node n)
        {
            float costs = 0.0f;
            
            // override existing costs
            foreach (NodeLayerCosts nlc in n.Costs)  
            {
                if (Parameters.LayerCostDictionary.ContainsKey( nlc.Layer ) )
                {
                    costs += Parameters.LayerCostDictionary[ nlc.Layer ].Cost;   // get the override cost
                }
                else
                {
                    costs += nlc.Cost; // get cost from the grid
                }
            }

            // override costs not accounted for during grid creation
            bool canClear = canClearNode(ref n);
            foreach (LayerMask mask in n.Layers)
            {
                checkNodeLayerOverride(mask, canClear, out float costSum, out bool isblocked);

                costs += costSum;
            }

            return costs;
        }

        /// <summary>
        ///     Get the height offset costs
        /// </summary>
        /// <param name="height">   The height between the two nodes    </param>
        /// <returns>
        ///     A float of the multiplied value
        /// </returns>
        float getTotalHeightCosts(int height)
        {
            if (height < 0) { return 0.0f; }
            return (height * Parameters.heighIterationCost);            
        }

        #endregion
        
        
        #region Dimension Override Information

        #region Falling

        /// <summary> Determines if the object is initially falling before it finds its path</summary>
        bool initialFall = true;
        /// <summary>
        ///     Control our initial falling 
        ///     and subsuquent ability to fall
        /// </summary>
        /// <param name="node"> The node that would be falling from </param>
        /// <returns>
        ///     A bool if we can traverse over this node 
        ///     without falling to our deaths. Ignores the 
        ///     initial start fall.
        /// </returns>
        bool checkFall(Node node)
        {
            var ret = checkFallHeight(node);
            initialFall = false;

            return ret;
        }

        /// <summary>
        ///     Check if we will fall within this node
        /// </summary>
        /// <param name="node"> the node to check the ground against</param>
        /// <returns>
        ///     A bool if we can traverse this node 
        ///     because we will not fall too far
        /// </returns>
        bool checkFallHeight(Node node)
        {
            // If we are restricting the fall distance
            if ( Parameters.restrainTraverseDrop && !initialFall )
            {
                int fallTo = node.Y - ( heightFallIterations == 0 ? 1 : heightFallIterations);

                // go through each falling node
                for (int i = node.Y; i >= fallTo; i -- )
                {
                    if (i < 0) { break; }

                    Node test = grid.GridNodes[ node.X, i, node.Z];
                    if ( !canTraverseNode(ref test) && !canClearNode(ref test) )
                    {
                        return true; // we hit something solid
                    }

                }
                return false; // we have fallen too far to recovers                
            }
            return true;
        }
        
        #endregion   

        /// <summary>
        ///     Check our traversal height to make
        ///     sure we are not going too many iterations
        ///     high on the nodes
        /// </summary>
        /// <param name="node"> The node to check its previous of   </param>
        /// <returns>
        ///     A bool of true if we went to high, 
        ///     a bool of false if we went to low
        /// </returns>
        bool CheckHeightTraversal(Node node)
        {            
            // Check previous nodes to make sure we are not getting up two high
            if ( Parameters.restrainTraverseHeight )
            {
                Node heightCheck    = node;
                var checkUp         = heightTraverseIterations  == 0 ? heightTraverseIterations + 1 : heightTraverseIterations;

                // check up and down
                heightCheck = getPreviousNodeAtIteration(node, checkUp,    out int checkCompleted  );

                // Check if we have gone up too much
                if ( ( node.Y - heightCheck.Y ) >= checkUp )    { return false; }    
                
            }
            return true;
        }

        /// <summary>
        ///     Get the previous node after a certain amount of iterations
        /// </summary>
        /// <param name="n">            The node containing the previous nodes  </param>
        /// <param name="iterations">   The amount of iterations to go back     </param>
        /// <param name="completed">    A return parameter that details how many iterations were completed  </param>
        /// <returns>
        ///     A node after the passed amount of iterations. 
        ///     This will return the most recent node if there 
        ///     are more iterations than previous nodes
        /// </returns>
        Node getPreviousNodeAtIteration(Node n, int iterations, out int completed)
        {
            completed = 0;
            Node heightCheck = n;
            // Get the node at the height iterations
            for (int h = iterations; h > 0; h--)
            {
                // Get our previous nodes
                if (heightCheck.previous != null)
                {
                    completed++;
                    heightCheck = heightCheck.previous;
                }
                else
                {
                    break;
                }
            }

            return heightCheck;
        }

        /// <summary>
        ///     Check our height and width overrides. Also checks our 
        ///     ability to traverse heightwise.
        /// </summary>
        /// <param name="checkNode">    The node to verify  </param>
        /// <returns>
        ///     A bool if we can traverse this node
        /// </returns>
        /// <remarks>
        ///     This determines if our node is viable for our path
        /// </remarks>
        bool checkNodeAvailability(Node checkNode)
        {
            // Check if we are overriding width, height, and if we can actually traverse to this node
            if ( !CheckHeightTraversal( checkNode ) || !checkNodeDimensionOverrides( checkNode ) )
            {
                return false;
            }
            return true;
        }      

        /// <summary>
        ///     Check the node dimensions, h x w and see 
        ///     if we are overriding them and if we are
        ///     if we can traverse it
        /// </summary>
        /// <param name="checkNode">    The node to override the width / height of </param>
        /// <returns>
        ///     A boolean if the pathfinding object can traverse
        ///     this area of the grid
        /// </returns>
        bool checkNodeDimensionOverrides(Node checkNode)
        {
            if ( !CheckHeight(checkNode) || !CheckWidth(checkNode) )
            {
                return false;
            }
            return true;
        }

        /// <summary>
        ///     Check if we can traverse the height of this node.
        /// </summary>
        /// <param name="checkNode">    The position to check from  </param>
        /// <returns>                   
        ///     true if it is not too high
        /// </returns>
        bool CheckHeight(Node checkNode)
        {
            try
            {
                if( Parameters.overridePathHeight )
                {
                    // Check if our height is good
                    if ( Parameters.IgnoreOutOfGridDimensions || checkNodeArrayInclusion( checkNode )  )
                    {
                        // Check the heigh normally
                        for (int check = 1; check <= heightIterations; check++)
                        {
                            // if we are ignoring the outside of the grid, make sure we are still within it
                            if ( Parameters.IgnoreOutOfGridDimensions && ( checkNode.Y + check ) > grid.GraphHeightIterations  ) { break; }

                            var node = grid.GridNodes[ checkNode.X, checkNode.Y + check, checkNode.Z];

                            if ( !canClearNode(ref node ) )
                            {
                                return false;
                            }                    
                        }
                        return true;
                    }
                    else
                    {
                        return false; // out of the grid
                    }
                }
            }
            catch { }
            return true;            
        }


        /// <summary>
        ///     Check if we can traverse the width of this node
        /// </summary>
        /// <param name="checkNode">    The position to check from  </param>
        /// <returns>                   
        ///     false if we cannot traverse this node
        /// </returns>
        bool CheckWidth(Node checkNode)
        {
            try
            {
                // Check iterations x and z for this
                if( Parameters.overridePathWidth )
                {
                    // Check if our dims are good
                    if ( Parameters.IgnoreOutOfGridDimensions || checkNodeArrayInclusion( checkNode ) )
                    {
                        for(int check = 1; check  <= horizontalIterations; check++)
                        {
                            // Check if we are in the right area
                            var amount = checkNode.Y + check;
                            if (Parameters.IgnoreOutOfGridDimensions && amount > grid.GraphWidthIterations || amount > grid.GraphDepthIterations ) { break; }

                            // our height
                            if ( !canClearNode( ref grid.GridNodes[ checkNode.X + check, checkNode.Y, checkNode.Z + check] ) )
                            {
                                return false;
                            }
                            if ( !canClearNode( ref grid.GridNodes[checkNode.X - check, checkNode.Y, checkNode.Z - check] ) )
                            {
                                return false;
                            }                   
                        }
                    }
                    else
                    {
                        return false;    // out of grid
                    }
                }

                return true;
            }
            catch
            {
                return true;
            }
            
        }

        /// <summary>
        ///     Check the dimensions of the passed node in both
        ///     positive and negetive directions
        /// </summary>
        /// <param name="checkNode">    The node to add dimensions to       </param>
        /// <param name="checkYDown">   Whether or not to check downwards   </param>
        /// <returns>
        ///     A boolean if we can check the node on the graph
        /// </returns>
        bool checkNodeArrayInclusion(Node checkNode, bool checkYDown = false)
        {
            if ( checkNodeDimensionsPositive( checkNode ) && checkNodeDimensionsNegetive( checkNode, checkYDown ) )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Make sure the index is within the dimesions 
        ///     of our node array
        /// </summary>
        /// <param name="checkNode">    the node to check up and down   </param>
        /// <returns>
        ///     A bool of true if we are within the bounds
        ///     false if we are out
        /// </returns>
        /// <remarks> 
        ///     This will add the iterations, it does not need to be done prior
        /// </remarks>
        bool checkNodeDimensionsPositive(Node checkNode)
        {
            int x = checkNode.X + horizontalIterations;
            int y = checkNode.Y + heightIterations;
            int z = checkNode.Z + horizontalIterations;

            if ( grid.checkIfWithinGraphRange( x, y, z ) )
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        ///     Make sure the index is within the dimesions 
        ///     of our node array
        /// </summary>
        /// <param name="checkNode">    the node to check up and down   </param>
        /// <param name="checkY">       Determines whether we negate Y  </param>
        /// <returns>
        ///     A bool of true if we are within the bounds
        ///     false if we are out
        /// </returns>
        /// <remarks> 
        ///     This will subtract the iterations, it does not need to be done prior
        /// </remarks>
        bool checkNodeDimensionsNegetive(Node checkNode, bool checkY = false)
        {
            int yOffset = checkY ? heightIterations : 0;
            int x = checkNode.X - horizontalIterations;
            int y = checkNode.Y - yOffset;
            int z = checkNode.Z - horizontalIterations;

            if ( grid.checkIfWithinGraphRange(x, y, z) )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion


        #region Ground Check

        /// <summary> The nodes we have already fallen too  </summary>
        List<Vector3> snappedTo = new List<Vector3>();
        /// <summary>
        ///     Bring the object to the ground. This function
        ///     controls the overall searched nodes and administers
        ///     a list of nodes that have been searched through.
        /// </summary>
        /// <param name="checkNode">    The Node to bring to the ground </param>
        /// <returns>
        ///     The node that is determined by gravity
        /// </returns>
        Node controlNodeGroundSnap(Node checkNode)
        {
            Node node = SnapNodeToGround(checkNode);
            snappedTo.Add(node.Index);

            return node;
        }

        /// <summary>
        ///     Check for the lowest ground node. Essentially add gravity to
        ///     the object
        /// </summary>
        /// <param name="checkNode">    The node we want to bring to the ground </param>
        /// <returns>
        ///     A node that is closest to the ground
        /// </returns>
        Node SnapNodeToGround(Node checkNode)
        {
            if( Parameters.snapToGround )
            {
                Node returnMe = checkNode;

                if ( !canClearNode( ref returnMe ) ) { return returnMe; }
                
                // Add the index to the searched index
                if ( snappedTo.Contains( returnMe.Index ) ) { return returnMe; }

                Node previous   = checkNode;
                int y           = (int)returnMe.Index.y - 1;
                
                // Make sure we do not fall out of the grid
                while( y > 0 )
                {
                    if ( y < 0 ) { break; }
                    
                    var droppedNode = grid.GridNodes[ (int)returnMe.Index.x, y, (int)returnMe.Index.z ].Copy();
                    
                    // We have checked this node already
                    if ( snappedTo.Contains( droppedNode.Index ) ) { return previous; }

                    if ( canClearNode( ref droppedNode ) )
                    {
                        // as we fall, add in the previous node
                        droppedNode.previous    = returnMe;
                        returnMe                = droppedNode;
                    }
                    else if( traverseButStop( ref droppedNode) )
                    {
                        // as we fall, add in the previous node
                        droppedNode.previous    = returnMe;
                        returnMe                = droppedNode;
                        break;
                    }
                    else
                    {
                        break;
                    }
                    previous = returnMe;
                    y--;
                }

                return returnMe;
            }
            return checkNode;
        }
        
        #endregion


        /// <summary>
        ///     Get the distance between two vectors
        /// </summary>
        /// <param name="pointA">   Distance from this point    </param>
        /// <param name="pointB">   To this point               </param>
        /// <returns>
        ///     A float value of the distance between two nodes
        /// </returns>
        float GetNodeDistances(Vector3 pointA, Vector3 pointB)
        {
            float distance;
            distance = Vector3.Distance(pointA, pointB);
            return distance;
        }

        /// <summary>
        ///     Assign the start and end nodes based off of the grid
        /// </summary>
        private void AssignNodes()
        {
            startNode       = controlNodeGroundSnap( grid.getClosestNode( Parameters.startPosition,     
                            Parameters.barrierTypes, closestGuess: Parameters.findObjQuick ).Copy() );
            
            endNode         = controlNodeGroundSnap( grid.getClosestNode( desiredLocation: Parameters.EndPosition, 
                            avoidTypes: Parameters.barrierTypes, closestGuess: Parameters.findObjQuick).Copy() );

            closestToEnd    = null;

        }


        /// <summary>
        ///     Check if our enumerable has the node in it
        /// </summary>
        /// <param name="Check">    The node to check   </param>
        /// <returns>
        ///     A bool if we have the node in this list
        /// </returns>
        bool checkContainsNode(Node Check, IEnumerable checkThese)
        {
            foreach (Node n in checkThese)
            {
                if ( n.Equals( Check ) )
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     The nodes distance from the end position
        /// </summary>
        /// <param name="thisPosition"> The distance to check from</param>
        /// <returns>
        ///     A distance from hte passed position to the ending position
        /// </returns>
        private float getDistanceToEnd(Vector3 thisPosition)
        {
            return Vector3.Distance(thisPosition, endNode.Position);
        }

        /// <summary>
        ///     The nodes distance from the start position
        /// </summary>
        /// <param name="thisPosition"></param>
        /// <returns>
        ///     a distance from the passed position from the starting position
        /// </returns>
        private float getDistanceFromStart(Vector3 thisPosition)
        {
            return Vector3.Distance(thisPosition, startNode.Position);
        }

        /// <summary>
        ///     Create a list of coordinates
        /// </summary>
        /// <returns>
        ///     The coordinates of each node in the pathfinder
        /// </returns>
        internal List<Vector3> GetPathCoordinates()
        {
            pathCoordinates.Clear();
            if (pathNodes != null)
            {
                foreach (Node n in pathNodes)
                {
                    pathCoordinates.Add(n.Position);
                }
                return pathCoordinates;
            }
            else
            {
                return new List<Vector3>();
            }
            
        }

        /// <summary>
        ///     Get a list of the path nodes working backwards
        /// </summary>
        /// <param name="endNode">  The node that has found or is closest to the target</param>
        /// <returns>
        ///     A list of nodes working backwards from the end node
        /// </returns>
        private List<Node> GetPathNodes(Node endNode)
        {
            List<Node> path = new List<Node>();
            if (endNode == null) { return path; }

            // start at the end Node
            path.Add( endNode );

            // follow the breadcrumb trail backward until we hit a node that has no previous node (usually the start Node)
            Node currentNode = endNode.previous;
            
            while (currentNode != null)
            {
                // insert the previous node at the first position in the path
                path.Insert(0, currentNode );
                
                // continue backward through the graph
                currentNode = currentNode.previous;

                // Safely break our while loop if things get out of hand
                if (path.Count > grid.GridNodes.Length * 1.5) { break; }
            }

            // return the list of Nodes
            return path;
        }


        /// <summary>
        ///     Add the node to the explored list so we can ignore it later on
        /// </summary>
        /// <param name="addNode"> the node that is about to be explored    </param>
        void ExploreThisNode(Node addNode)
        {
            if ( !exploredNodes.Contains( addNode ) )           
            {
                exploredNodes.Add( addNode );
            }
            if (!exploredIndexes.Contains( addNode.Index ) )
            {
                exploredIndexes.Add( addNode.Index );
            }
        }

        /// <summary>
        ///     Clear the nodes that have already been explored
        ///     so we can clear the way fo another path finding
        ///     excursion
        /// </summary>
        void ClearExploredNodes()
        {
            exploredIndexes.Clear();
            exploredNodes.Clear();
            snappedTo.Clear();
        }

        #endregion

    }
}