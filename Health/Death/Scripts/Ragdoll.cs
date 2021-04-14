using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{

    [SerializeField] bool cleanUpOnStart = true;

    Collider    _collider;
    Animator    _animator;
    Rigidbody   _rigidBody;
    int         _getUpHash  = Animator.StringToHash("StandUpFront");



    public bool IsReadyForRagdoll
    {
        get
        {
            return (_collider != null && _animator != null);
        }
    }

    public bool isRagdoll
    {
        get; private set;
    }

    private void Awake()
    {
        _collider   = GetComponent<Collider>();
        _animator   = GetComponent<Animator>();
        _rigidBody  = GetComponent<Rigidbody>();
        if (_animator   == null ) { }
        if (_collider   == null ) { }
        if (_rigidBody  == null ) { }
    }

    private void Start()
    {
        if (cleanUpOnStart) { Reanimate(false); }
    }

    /// <summary>
    ///     Turn on ragdoll physics
    /// </summary>
    public void EnableRagdoll()
    {
        activateRagdoll(true);
    }


    /// <summary>
    ///     Get back up
    /// </summary>
    public void Reanimate(bool getUp = true)
    {
        if (getUp)
        {
            _animator.SetBool(_getUpHash, true);
        }

        activateRagdoll(false);
    }

    void activateRagdoll(bool activate)
    {
        // turn stuff on / off
        _animator.enabled = !activate;
        _collider.enabled = !activate;

        accessChildren(activate);

        isRagdoll = activate;
    }

    /// <summary>
    ///     Go through the children iteratively.
    ///     This is a recursive function and will set all
    ///     children of children with rigid bodies and colliders
    ///     until we are out of children.
    /// </summary>
    /// <param name="activate"></param>
    /// <param name="targetChildren"></param>
    void accessChildren(bool activate, Transform targetChildren = null)
    {
        if (targetChildren == null) { targetChildren = transform; }
        for(int i = 0; i < targetChildren.childCount; i++)
        {
            var destination = targetChildren.GetChild(i);

            var rb  = destination.GetComponent<Rigidbody>();
            var col = destination.GetComponent<Collider>();
            if (rb != null && col != null)
            {
                rb.isKinematic  = false;
                rb.velocity     = _rigidBody.velocity;

                col.isTrigger = !activate;
            }
            accessChildren(activate, destination);
        }
    }


}
