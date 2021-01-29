using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Human_Controller
{

    public struct BodyParts
    {
        HumanBodyBones bone;                

        public BodyParts( HumanBodyBones boneRef, Transform t)
        {
            bone = boneRef;
            transform = t;
        }

        public Transform transform
        {
            get;
            private set;
        }

    }

    /// <summary>
    ///     Capture all the human body parts within
    ///     the animator
    /// </summary>
    internal class HumanoidBodyParts
    {
        internal Transform PlayerChest;
        internal Transform PlayerSpine;
        internal Transform PlayerHead;
        internal Transform PlayerRightEye;
        internal Transform PlayerLeftEye;
        internal Transform UpperChest;
        internal Transform LeftShoulder;
        internal Transform RightShoulder;
        internal Transform LowerLeftArm;
        internal Transform LowerRightArm;
        internal Transform UpperLeftArm;
        internal Transform UpperRightArm;
        internal Transform LeftHand;
        internal Transform RightHand;
        internal Transform LeftFoot;
        internal Transform RightFoot;


        public HumanoidBodyParts(Animator animator)
        {
            // Get the bodyparts transform
            PlayerChest         = animator.GetBoneTransform( HumanBodyBones.Chest);
            PlayerSpine         = animator.GetBoneTransform( HumanBodyBones.Spine);
            PlayerHead          = animator.GetBoneTransform( HumanBodyBones.Head);
            PlayerLeftEye       = animator.GetBoneTransform( HumanBodyBones.LeftEye);
            PlayerRightEye      = animator.GetBoneTransform( HumanBodyBones.RightEye);
            UpperChest          = animator.GetBoneTransform( HumanBodyBones.UpperChest);
            LowerLeftArm        = animator.GetBoneTransform( HumanBodyBones.LeftLowerArm);
            UpperLeftArm        = animator.GetBoneTransform( HumanBodyBones.LeftUpperArm);
            LowerRightArm       = animator.GetBoneTransform( HumanBodyBones.RightLowerArm);
            UpperRightArm       = animator.GetBoneTransform( HumanBodyBones.RightUpperArm);
            RightHand           = animator.GetBoneTransform( HumanBodyBones.RightHand);
            LeftHand            = animator.GetBoneTransform( HumanBodyBones.LeftHand);
        }

    }

}
