using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace.DesignPatterns;
using Gym;
using UnityEngine;

public class VRGrabHandler : MonoSingleton<VRGrabHandler>
{
    private Dictionary<FingerColliderType, KeyValuePair<HandFingerCollider, Pickable>> collidersPlacements;
    public Transform handTransform;

    private void Start()
    {
        collidersPlacements = new Dictionary<FingerColliderType, KeyValuePair<HandFingerCollider, Pickable>>();
    }

    private void PrintColliders()
    {
        var str = "";
        foreach (var collidersPlacementsKey in collidersPlacements.Keys)
        {
            str += $"{collidersPlacementsKey} - {collidersPlacements[collidersPlacementsKey].Value.name}\n";
        }
    }

    private Pickable _currentPickable = null;
    private bool _isHandOpen;

    public void OnHandOpenDeActive()
    {
        _isHandOpen = false;
        Debug.LogError("HAND NOT OPEN");
    }

    public void OnHandOpenActive()
    {
        _isHandOpen = true;
        if (_currentPickable != null)
        {
            _currentPickable.Release();
            _currentPickable = null;
        }

        Debug.LogError("HAND OPEN");
    }

    public void AddFingerForPickable(HandFingerCollider fingerCollider, Pickable pickable)
    {
        var pair = new KeyValuePair<HandFingerCollider, Pickable>(fingerCollider, pickable);
        collidersPlacements.TryAdd(fingerCollider.type, pair);

        PrintColliders();

        if (IsPickablePicked(pickable) && _currentPickable == null)
        {
            _currentPickable = pickable;
            pickable.SnapToHand(collidersPlacements.Where(cp => cp.Value.Value.Equals(pickable)
            ).Select(cp => cp.Value.Key.transform).ToList());
        }
    }

    public bool IsPickablePicked(Pickable pickable)
    {
        return !_isHandOpen && pickable.typesNeededToPick.TrueForAll(type =>
            collidersPlacements.ContainsKey(type) && collidersPlacements[type].Value.Equals(pickable)
        );
    }

    public void ReleaseFingerForPickable(FingerColliderType colliderType)
    {
        if (!collidersPlacements.ContainsKey(colliderType))
            return;
        var pickable = collidersPlacements[colliderType].Value;
        collidersPlacements.Remove(colliderType);
        // if (!IsPickablePicked(pickable))
        //     pickable.Release();
        // PrintColliders();
    }
}