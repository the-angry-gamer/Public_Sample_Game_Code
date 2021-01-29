using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathFindingAsteria
{

    public class UpdateGridAgent : MonoBehaviour, IGridObject
    {
        #region Editor

        //[SerializeField]        
        //[Tooltip("This is the GameObject that has the grid manager component on it.")]
        //GridManager manager;

        [Tooltip("Turn on / off debug logging")]
        [SerializeField]
        bool Logging = true;
        
        [Tooltip("How long to wait after the transformation change before the update occurs")]
        [SerializeField]
        [Range(0,60)]
        float seconds = 0.5f;

        [SerializeField]
        [Tooltip("How much space as compared to the grid we should update the grid")]
        [Range(0,20)]
        float multiplySize = 2.0f;

        [SerializeField]
        [Tooltip("Determines whether to follow the iteration amount set off in the grid class")]
        bool keepIterations = false;
        #endregion


        /// <summary>
        ///     Get the active grid from the list of grids
        /// </summary>
        Grid activeGrid
        {
            get
            {
                foreach (GridManager m in grids)
                {
                    if ( m.gridBase.IsActive )
                    {
                        return m.gridBase;
                    }
                }
                return null;
            }
        }

        List<GridManager> grids = new List<GridManager>();

        #region Editor Script Exposure

        public  Vector3     PreviuosPosition    { get; private set; }
        public  float       changeTime          { get; private set; }
        public  float       timeTaken           { get; private set; }
        public  int         nodesCreated        { get; private set; } = 0;
        public  int         gridsAssigned       
        { 
            get 
            {
                if (grids != null)
                {
                    return grids.Count; 
                }
                return 0;
            } 
        }


        public string GridName
        {
            get
            {
                foreach (GridManager m in grids)
                {
                    if (m.gridBase.IsActive)
                    {
                        return m.name;
                    }
                }
                return "No Manager Assigned";
            }
        }

        /// <summary>
        ///     The manager of the currently active grid
        /// </summary>
        public GridManager manager
        {
            get
            {
                foreach (GridManager m in grids)
                {
                    if (m.gridBase.IsActive)
                    {
                        return m;
                    }
                }
                return null;
            }
        }

        #endregion
        
        
        // Start is called before the first frame update
        void Start()
        {
            PreviuosPosition    = gameObject.transform.position;
            changeTime          =  Time.time;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (activeGrid == null) { return; }
            CallUpdateGrid();
        }

        /// <summary>
        ///     Update the grid when we change our position
        /// </summary>
        void CallUpdateGrid()
        {
            if (activeGrid == null)
            {
                if (Logging) { Debug.LogError($"There is no grid to update for the movement of {gameObject.name}"); }
                return;
            }

            if ( (Time.time - changeTime) > seconds)
            {
                if (PreviuosPosition != gameObject.transform.position)
                {
                    //if (manager) { grid = manager.gridBase; }
                    changeTime          = Time.time;
                    float width         = gameObject.transform.lossyScale.x * multiplySize;
                    float height        = gameObject.transform.lossyScale.y * multiplySize;
                    float depth         = gameObject.transform.lossyScale.z * multiplySize;

                    float start         = Time.time;
                    nodesCreated        = activeGrid.UpdateNodes( centerRadius: gameObject.transform.position,    width: width, height: height, depth: depth, keepIterations );
                    nodesCreated       += activeGrid.UpdateNodes( centerRadius: PreviuosPosition,                 width: width, height: height, depth: depth, keepIterations );

                    timeTaken           = Time.time - start;
                    PreviuosPosition    = gameObject.transform.position;
                    
                    if (manager) { manager.TryDraw(); }
                    
                    if (Logging) { Debug.Log($"The grid has been updated based on {gameObject.name}. {nodesCreated.ToString()} nodes were updated"); }
                }
            }
        }

        #region Interfaces 

        /// <summary>
        ///     What happens when grid making makes
        ///     contact with this item
        /// </summary>
        /// <param name="grid"> the grid to assign to the object</param>
        /// <returns>
        ///     A boolean of success
        /// </returns>
        public bool GridContact(GridManager _grid)
        {
            if (!grids.Contains(_grid)) { grids.Add(_grid); }

            return true;
        }

        public void RemoveGrid(GridManager _grid)
        {
            grids.Remove(_grid);
        }

        #endregion
    }
}
