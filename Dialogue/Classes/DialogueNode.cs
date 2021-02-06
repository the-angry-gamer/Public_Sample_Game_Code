using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AsteriaDialogue
{
    public class DialogueNode : ScriptableObject
    {

        #region Public dialogue information

        [SerializeField]
        [Tooltip("The name of the character / player that is speaking this dialogue line")]
        string       speakerName;

        [SerializeField]
        [TextArea(2,10)]
        [Tooltip("What the player is saying")]
        string       text;

        [SerializeField]
        [Tooltip("The type of dialogue that this is")]
        /// <summary> How this response will effect the dialogue </summary>
        ResponseTypes.TypesOfResponse responseType = ResponseTypes.TypesOfResponse.Forward;

        [SerializeField]
        Rect dimensions = new Rect(0, 0, 300, 250);

        public List<string>  children = new List<string>();


        #endregion


        #region Properties

        public bool HasSpoken { get; set; }

        public List<string> Children
        {
            get
            {
                return children;
            }
        }

        /// <summary>
        ///     The name of the persone speaking
        /// </summary>
        /// <value>The name of the speaker.</value>
        public string SpeakerName
        {
            get
            {
                return speakerName;
            }
        }

        /// <summary>
        ///     The text that is to be said
        /// </summary>
        /// <value>The text</value>
        public string Text
        {
            get
            {
                return text;
            }
        }

        /// <summary>
        ///     The type of response we have
        /// </summary>
        /// <value>The type of the response.</value>
        public ResponseTypes.TypesOfResponse ResponseType
        {
            get
            {
                return responseType;
            }
        }

        /// <summary>
        ///     The rectangle of this node (size and pos)
        /// </summary>
        /// <value>The dimensions.</value>
        public Rect Dimensions
        {
            get
            {
                return dimensions;
            }
        }

        #endregion


        #region Getters

        /// <summary>
        ///     Get the dimensions of the node 
        ///     for the gui
        /// </summary>
        /// <returns>
        ///     A rect of the node dimensions
        /// </returns>
        public Rect GetRect()
        {
            return dimensions;
        }

        #endregion


        #region Unity Editor Specific

#if UNITY_EDITOR

        #region Setters
        // Have this seperated for the undo functionality

        /// <summary>
        ///  Add a child to this node
        /// </summary>
        /// <param name="value"> Add a child id </param>
        public void AddChild(string value)
        {
            Undo.RecordObject(this, $"Add child");
            children.Add(value);
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Remove a child from this node
        /// </summary>
        /// <param name="value"> Remove a child account </param>
        public void RemoveChild(string value)
        {
            Undo.RecordObject(this, $"Remove node");
            children.Remove(value);
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Set the dimensions of this node
        /// </summary>
        /// <param name="rect"> The rectangle to set </param>
        public void SetDimensions(Rect rect)
        {
            Undo.RecordObject(this, "Node Rect");

            dimensions = rect;
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Set the position of the node
        /// </summary>
        /// <param name="pos">Position.</param>
        public void SetPosition(Vector2 pos)
        {
            Undo.RecordObject(this, "Node Position");

            dimensions.x = pos.x;
            dimensions.y = pos.y;
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Set the text value and record the change
        /// </summary>
        /// <param name="value">Value.</param>
        public void SetText(string value)
        {
            if (text == value) { return; }
            Undo.RecordObject(this, "Node Text");
            text = value;
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Set the new type of the dialogue
        /// </summary>
        /// <param name="type"> The new type to make it</param>
        public void SetType(string type)
        {
            if (Enum.TryParse(type, out ResponseTypes.TypesOfResponse e))
            {
                SetType(e);
            }
        }


        public void SetType(ResponseTypes.TypesOfResponse value)
        {
            if (responseType == value) { return; }

            Undo.RecordObject(this, "Node Response");
            responseType = value;

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     Set the name of the person speaking
        /// </summary>
        /// <param name="value">Value.</param>
        public void SetSpeaker(string value)
        {
            if (speakerName == value) { return; }
            Undo.RecordObject(this, "Node Speaker");
            speakerName = value;

            EditorUtility.SetDirty(this);
        }
      

         #endregion


#endif

        #endregion

    }
}