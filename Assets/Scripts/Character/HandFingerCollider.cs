using System;
using System.Collections;
using System.Collections.Generic;
using Gym;
using UnityEngine;

public class HandFingerCollider : MonoBehaviour
{
    public FingerColliderType type;
    private void OnCollisionEnter(Collision other)
    {
      
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Pickable"))
        {
            var pickable = other.gameObject.GetComponent<Pickable>();
            VRGrabHandler.Instance.AddFingerForPickable(this, pickable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        VRGrabHandler.Instance.ReleaseFingerForPickable(type);
    }

    private void OnCollisionExit(Collision other)
    {
      

    }
}

public enum FingerColliderType
{
    R_PINKEY, R_MIDDLE, R_INDEX, R_THUMB, R_RING,
    L_PINKEY, L_MIDDLE, L_INDEX, L_THUMB, L_RING
}

public static class FingerColliderTypeExtensions
{

    public static int GetHandIndex(this FingerColliderType fingerCollider)
    {
        if (fingerCollider <= FingerColliderType.R_RING)
        {
            return 1;
        }

        return 0;
    }
}