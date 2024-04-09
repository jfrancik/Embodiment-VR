using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Character;
using DefaultNamespace.DesignPatterns;
using Gym;
using UnityEngine;

public class VRGrabHandler : MonoSingleton<VRGrabHandler>
{
    private Dictionary<FingerColliderType, KeyValuePair<HandFingerCollider, Pickable>> collidersPlacements;
    public Transform handTransform;
    public VRMovementHandler movementHandler;
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

    private Pickable _currentPickableLeft = null;
    private Pickable _currentPickableRight = null;
    private bool _isLeftHandOpen;
    private bool _isRightHandOpen;

    private void Update()
    {
        var isClimbing = _currentPickableLeft != null || _currentPickableRight != null;
        movementHandler.SetIsClimbing(isClimbing);
       
       
    }

    public void OnLeftHandOpenDeActive()
    {
        _isLeftHandOpen = false;
    }

    public void OnLeftHandOpenActive()
    {
        _isLeftHandOpen = true;
        if (_currentPickableLeft != null)
        {
            _currentPickableLeft.Release();
            _currentPickableLeft = null;
        }
    }
    public void OnRightHandOpenDeActive()
    {
        _isRightHandOpen = false;
    }

    public void OnRightHandOpenActive()
    {
        _isRightHandOpen = true;
        if (_currentPickableRight != null)
        {
            _currentPickableRight.Release();
            _currentPickableRight = null;
        }
    }

    public void AddFingerForPickable(HandFingerCollider fingerCollider, Pickable pickable)
    {
        var pair = new KeyValuePair<HandFingerCollider, Pickable>(fingerCollider, pickable);
        collidersPlacements.TryAdd(fingerCollider.type, pair);

        PrintColliders();

        if (IsPickablePickedRight(pickable))
        {
            _currentPickableRight = pickable;
            pickable.SnapToHand(collidersPlacements.Where(cp => cp.Value.Value.Equals(pickable)
            ).Select(cp => cp.Value.Key.transform).ToList());
        }
        else if(IsPickablePickedLeft(pickable))
        {
            _currentPickableLeft = pickable;
            pickable.SnapToHand(collidersPlacements.Where(cp => cp.Value.Value.Equals(pickable)
            ).Select(cp => cp.Value.Key.transform).ToList());
        }
    }

    public bool IsPickablePickedRight(Pickable pickable)
    {
        var isRightPickable = !_isRightHandOpen && _currentPickableRight == null &&
                              pickable.typesNeededToPickRight.TrueForAll(type =>
                                  collidersPlacements.ContainsKey(type) &&
                                  collidersPlacements[type].Value.Equals(pickable)
                              );
        return isRightPickable;
    }

    public bool IsPickablePickedLeft(Pickable pickable)
    {
        var isLeftPickable = !_isLeftHandOpen && _currentPickableLeft == null &&
                              pickable.typesNeededToPickLeft.TrueForAll(type =>
                                  collidersPlacements.ContainsKey(type) &&
                                  collidersPlacements[type].Value.Equals(pickable)
                              );
        return isLeftPickable;
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