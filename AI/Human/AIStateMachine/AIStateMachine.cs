using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI_Asteria
{

	[RequireComponent( typeof( Collider ) )]
	public abstract class AIStateMachine : MonoBehaviour, IAIReactions
	{
        #region public

        public AITarget		VisualThreat	=	new AITarget();
		public AITarget		AudioThreat		=	new AITarget();
        
		#endregion


        #region internal

        internal AIPath		Path;

		internal bool		canMove			= true;

		internal bool		checkObstacles  = false;

        #endregion


        #region private

		/// <summary>
		///		The current state that the AI is in. This dictates the 
		///		state machine behavoir 
		/// </summary>
        AIStateBase currentState;

		#endregion


		#region For Reference

		/// <summary>
		///		All the states that we have on this item
		/// </summary>
		public Dictionary<AIStateType, List<AIStateBase>> States
        {
            get
            {
				return possibleStates;
            }
        }

		/// <summary>
		///		The current state that we are in. 
		///		If not set, will return "nothing"
		/// </summary>
		public string CurrentStateType
        {
            get
            {
				return currentStateType.ToString();
            }
        }

		/// <summary>
		///		Return the previous state this gameobject was in
		/// </summary>
		public string PreviousStateType 
		{
            get
            {
				return previousState.ToString();
            }
		}

		#endregion


		#region Editor
		[SerializeField] internal	AIStateType	currentStateType = AIStateType.Idle;
		[SerializeField] internal	AISensor	Sensor;
		[SerializeField] internal	bool		Logging		= true;
		[SerializeField] private	bool		canShoot	= true;
		
		[Header("Possible Actions")]
		[SerializeField] internal	bool		canJump		= false;
		[SerializeField] internal	bool		canVault	= false;
		[SerializeField] internal	bool		canCrouch	= false;

        #endregion


        #region Properties

        #endregion


        #region Protected Declarations


        // protected for derivitives
        protected Dictionary<AIStateType, List<AIStateBase>> possibleStates = new Dictionary<AIStateType, List<AIStateBase>>();
		protected Collider		_collider		= null;
		protected Animator		_animator		= null;
		protected Transform		_transform		= null;
		protected AITarget		Target			= new AITarget();
		protected AIStateType	previousState;


		protected virtual void Awake()
		{
			// Cache all frequently accessed components
			_transform	=	transform;
			_animator	=	GetComponent<Animator>();
			_collider	=	GetComponent<Collider>();

			setInitialStateMachine();
			Path.UpdatePath(new List<Vector3>());
		}

		public virtual void OnTriggerEvent(AITriggerEventType type, Collider other)
		{ 
			if (currentState)
            {
				currentState.OnTriggerEvent( eventType: type, other: other );
            }
        }

		public virtual void OnCollisionEvent(AITriggerEventType type, Collision collision)
        {
			if (currentState)
            {
				currentState.OnCollisionEvent(eventType: type, other: collision);
            }
        }

		
		// Start is called before the first frame update
		protected virtual void Start()
		{
	
		}
		
		/// <summary>
		///		The late update call. This will be called a fixed 
		///		amount of times in a second.
		/// </summary>
		protected virtual void FixedUpdate()
        {
			currentState.OnFixedUpdate();
        }

		/// <summary>
		///		The late update call for the state machine
		/// </summary>
		protected virtual void LateUpdate()
        {
			currentState.OnLateUpdate();
        }

		/// <summary>
		///		When the calling entity update gets called, this will
		///		update the AI and determine the new state
		/// </summary>
		protected virtual void Update()
		{
			UpdateStateInformation();
		}


		/// <summary>
		///		Obtain a random state [based on AIStateBase] 
		///		within the script state dictionary list of 
		///		possible states associated with the passed state type
		/// </summary>
		/// <param name="stateType"> What state the AI will be in</param>
		/// <returns>
		///		A script based on the AI state 
		/// </returns>
		protected AIStateBase GetRandomScriptForState( AIStateType stateType)
        {	
			if ( !possibleStates.ContainsKey( stateType) ) { return null; }
			var newState = possibleStates[ stateType ][ Random.Range(0, possibleStates[ stateType ].Count) ];

			if (!newState.enabled)
            {
				if (Logging) { Debug.Log($"{newState.ToString()} has been inactivated"); }
				return currentState;
            }
			return newState;
        }


		/// <summary>
		///		The object is being targeted
		///		by something
		/// </summary>
		public virtual void isBeingTargeted() { }

		#endregion


		#region Internal and Public



		bool targetSet = false;
		/// <summary>
		///		Set this items target to something
		/// </summary>
		/// <param name="t"></param>
		/// <param name="p"> The position of the target in 3D space						</param>
		/// <param name="d"> The distance to the target									</param>
		/// <param name="c"> The collider on the target if it has one: this can be null	</param>
		public void SetTarget(AITargetType t, Vector3 p, float d, Collider c = null)
		{
			if (Logging && !targetSet) { Debug.Log($"The target for {gameObject.name} has been set"); targetSet = true; }

            if (c == null)
            {
				Target.Set(t, p, d);
			}
            else
            {
				Target.Set(t, c, p, d);
            }
		}

		/// <summary>
		///		Clear this items target
		/// </summary>
		internal virtual void ClearTarget()
        {
			// If we currently have a target
			if (Target.type != AITargetType.None)
			{
				if (Logging) { Debug.Log($"The target for {gameObject.name} has been cleared"); }
			}
			targetSet = false;
			Target.Clear();
        }

		#endregion


        #region Private

        #region States


        /// <summary>
        ///		Get all behavoirs and set 
        ///		the first state in the state machine
        /// </summary>
        void setInitialStateMachine()
        {
			if (Sensor) { Sensor.StateMachine = this; }
			getAllStateScriptsAttached();
			assignCurrentState();
		}

		/// <summary>
		///		Get a script based on the current state. 
		///		This will be the first script in the list
		/// </summary>
		void assignCurrentState()
        {
			if (possibleStates.ContainsKey( currentStateType ) )
			{
				currentState = possibleStates[ currentStateType ][ 0 ];
				currentState.OnEnterState();
			}
			else
			{
				if (Logging) { Debug.LogError($"[AIStateMachine.AssignScript()] This state ({currentStateType.ToString()}) is not allowed. It is not present in the dictionary"); }
				forceState();
				return;
			}
		}

		/// <summary>
		///		Force the first state if we have an issue
		/// </summary>
		void forceState()
        {
			if (possibleStates.Count > 0)
            {
				currentState = possibleStates.ElementAt(0).Value[0];
				currentState.OnEnterState();
            }
        }

		/// <summary>
		///		Get all the scripts attached to the 
		///		game object and assign them to an organized 
		///		dictionary for extraction
		/// </summary>
		void getAllStateScriptsAttached()
        {
			var attachedScripts = GetComponents<AIStateBase>();

            possibleStates = new Dictionary<AIStateType, List<AIStateBase>>();
            foreach ( var script in attachedScripts )
            {
                if ( possibleStates.ContainsKey( script.StateAssociation ) )
                {
                    possibleStates[ script.StateAssociation ].Add( script );
                }
                else
                {
                    possibleStates.Add( key: script.StateAssociation, value: new List<AIStateBase>() { script });
                }
				// assign the current state machine to the scripts
				script.SetStateMachine( this );
            }
        }

        #endregion

		/// <summary>
		///		Determine if we need to update the 
		///		current state type we are in and set
		///		an associated script
		/// </summary>
		void UpdateStateInformation()
        {
			// Determine the state in heres
			if (currentState == null) { return; }

			AIStateType newStateType	= currentState.OnUpdate();
			if ( newStateType != currentStateType )
            {
				previousState				= currentStateType;
				AIStateBase newState		= null;
			
				// Get the script driving our state
				if ( !possibleStates.ContainsKey( newStateType) )
				{
					if (Logging) { Debug.LogError($"There are no scripts available for {newStateType.ToString()}; defaulting to idle."); }
					newStateType = AIStateType.Idle;
				}

				newState = GetRandomScriptForState(newStateType); 
				if (newState == null )
                {
					if (Logging) { Debug.LogError($"There are no states available for {newStateType.ToString()}"); }
					return;
                }
				updateCurrentState(newState);
				currentStateType			= newStateType;
            }
        }


		/// <summary>
		///		Leave the old state and enter the new state
		/// </summary>
		/// <param name="newState">	The AI script to update to	</param>
		void updateCurrentState(AIStateBase newState)
        {
			if (currentState != newState)
            {
				currentState.OnExitState();
				currentState = newState;
				currentState.OnEnterState();
            }
		}

        #endregion

        #region AIReaction Interface

		public bool CanShoot { get { return canShoot; } }

		public virtual void IsBeingAimedAt() { if (Logging) { Debug.Log($"{gameObject.name} is being aimed at"); } }

		public virtual void IsBeingHit() { }

		public virtual void IsTouchingFire() { }


		#endregion

	}

}