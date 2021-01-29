using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AI_Asteria
{
     #region Enums

    /// <summary>
    ///		The various types of states that the AI can be in
    /// </summary>
    public enum AIStateType			{ None, Idle, Alerted, Patrol, Attack, Pursuit, Dead, Fleeing }
	
	/// <summary>
	///		The target type that an AI can take
	/// </summary>
	public enum AITargetType		{ None, Node, Player, Visual_Light, Visual_Food, Audio, Friendly }

	public enum AITriggerEventType	{ Enter, Stay, Exit }


	#endregion




	#region Structs


	/// <summary>
	///     An object that can hold tha path and its parameter information
	/// </summary>
	internal struct AIPath
	{
		//internal PathFindingObject PO;
		List<Vector3> PathObjectives;

		/// <summary> The last time the path was created for this object    </summary>
		internal float lastChecked;

		/// <summary> Move along our path									</summary>
		internal MoveAlongPath move;

		/// <summary>
		///		Set a new path and begin movement along it	
		/// </summary>
		/// <param name="stoppingDistanceXZ">	The distance to check for the xz	</param>
		/// <param name="stoppingDistanceY">	The distance to check for the y		</param>
		/// <param name="path">					The path we are traversing			</param>
		/// <returns>
		///		a new path we are traversing
		/// </returns>
		internal MoveAlongPath SetPath(float stoppingDistanceXZ, float stoppingDistanceY, List<Vector3> path)
        {
			setPath(path);
			if (PathObjectives != null)
            {
				move = new MoveAlongPath(distanceBufferXZ: stoppingDistanceXZ, distanceBuffY: stoppingDistanceY, path: PathObjectives);
            }
			return move; 
        }

		/// <summary>
		///		Update the current path
		/// </summary>
		/// <param name="path"></param>
		internal void UpdatePath(List<Vector3> path)
        {
			setPath(path);
			if (move != null)
            {
				move.PathNodes = path;
            }
        }

		/// <summary>
		///		Set the path with include params
		/// </summary>
		/// <param name="path"></param>
		void setPath(List<Vector3> path)
        {
			lastChecked		= Time.time;
			PathObjectives	= path;
        }
		

	}

    // ----------------------------------------------------------------------
    // Class	:	AITarget
    // Desc		:	Describes a potential target to the AI System
    // ----------------------------------------------------------------------
	/// <summary>
	///		A structure to hold the AI target and its essential components
	/// </summary>
	public struct AITarget
	{
		private		AITargetType 	_type;			// The type of target
		private		Collider		_collider;		// The collider
		private		Vector3			_position;		// Current position in the world
		private		float			_distance;		// Distance from player
		private		float			_time;			// Time the target was last ping'd
		

		public AITargetType	type 		{ get{ return _type;}}
		public Collider		collider 	{ get{ return _collider;}}
		public Vector3		position	{ get{ return _position;}}
		public float		distance	{ get{ return _distance;} set {_distance = value;}}
		public float		time		{ get{ return _time;}}


		/// <summary>
		///		Set the target within the structure
		/// </summary>
		/// <param name="t"></param>
		/// <param name="c"></param>
		/// <param name="p"></param>
		/// <param name="d"></param>
		public void Set( AITargetType t, Collider c, Vector3 p, float d )
		{
			_type		=	t;
			_collider	=	c;
			_position	=	p;
			_distance	=	d;
			_time		=	Time.time;
		}

		/// <summary>
		///		Set a target that has no collider
		/// </summary>
		/// <param name="t"></param>
		/// <param name="p">	position			</param>
		/// <param name="d">	distance to target	</param>
		public void Set(AITargetType t, Vector3 p, float d)
        {
			_type		=	t;
			_position	=	p;
			_distance	=	d;
			_time		=	Time.time;
        }

		/// <summary>
		///		Clear the values of the current target
		/// </summary>
		public void Clear()
		{
			_type		=	AITargetType.None;
			_collider	=	null;
			_position	=	Vector3.zero;
			_time		=	0.0f;
			_distance	=	Mathf.Infinity;
		}
	}

    #endregion
	
}
