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

    public float handReleaseThreshold = 1;

    private void Update()
    {
        var isClimbing = _currentPickableLeft != null || _currentPickableRight != null;
        movementHandler.SetIsClimbing(isClimbing);

        if (_currentPickableLeft)
        {
            var isLeftAttached = _currentPickableLeft.IsAttached(handReleaseThreshold, 0);
            if (isLeftAttached)
            {
                Debug.LogError("RELEASING LEFT BY UPDATE");
                _currentPickableLeft.Release(0);
                _currentPickableLeft = null;
            }
        }

        if (_currentPickableRight)
        {
            var isRightAttached = _currentPickableRight.IsAttached(handReleaseThreshold, 1);
            if (isRightAttached)
            {
                Debug.LogError("RELEASING RIGHT BY UPDATE");
                _currentPickableRight.Release(1);
                _currentPickableRight = null;
            }
        }
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
            Debug.LogError("RELEASING LEFT BY OPEN");
            _currentPickableLeft.Release(0);
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
            Debug.LogError("RELEASING RIGHT BY OPEN");
            _currentPickableRight.Release(1);
            _currentPickableRight = null;
        }
    }

    public void AddFingerForPickable(HandFingerCollider fingerCollider, Pickable pickable)
    {
        var pair = new KeyValuePair<HandFingerCollider, Pickable>(fingerCollider, pickable);
        collidersPlacements.TryAdd(fingerCollider.type, pair);

        // PrintColliders();


        if (IsPickablePickedRight(pickable))
        {
            var fingerTransforms = collidersPlacements.Where(
                cp => cp.Key.GetHandIndex() == 1 && cp.Value.Value.Equals(pickable)
            ).Select(cp => cp.Value.Key.transform).ToList();
            _currentPickableRight = pickable;
            pickable.SnapToHand(fingerTransforms, 1);
        }
        else if (IsPickablePickedLeft(pickable))
        {
            var fingerTransforms = collidersPlacements.Where(
                cp => cp.Key.GetHandIndex() == 0 && cp.Value.Value.Equals(pickable)
            ).Select(cp => cp.Value.Key.transform).ToList();
            _currentPickableLeft = pickable;
            pickable.SnapToHand(fingerTransforms, 0);
        }
    }

    public bool IsPickableReleasedLeft(Pickable pickable)
    {
        return _currentPickableLeft != null &&
               pickable.typesNeededToPickLeft.Exists(type =>
                   !(collidersPlacements.ContainsKey(type) &&
                     collidersPlacements[type].Value.Equals(pickable))
               );
    }

    public bool IsPickableReleasedRight(Pickable pickable)
    {
        return _currentPickableRight != null &&
               pickable.typesNeededToPickRight.Exists(type =>
                   !(collidersPlacements.ContainsKey(type) &&
                     collidersPlacements[type].Value.Equals(pickable))
               );
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
    }
}