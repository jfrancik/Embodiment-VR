using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class CharacterLegs : MonoBehaviour
{
    private Animator animator;

    private Transform _leftLegTarget;
    private bool _hasLeftLegTarget;

    private Transform _rightLegTarget;
    private bool _hasRightLegTarget;

    public float footPlacementRange = 1;
    [SerializeField] private float footOffset = 0;


    public Transform rightFootTransform;
    public Transform leftFootTransform;
    
    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private Collider[] _sphereHits = new Collider[20];
    public bool isClimbing;
    public LayerMask climbLayers;
    public LayerMask groundLayers;
    
    
    [CanBeNull]
    public Transform GetNearestFootPlacement(float range, LayerMask layerMask, AvatarIKGoal ikGoal)
    {
        Vector3 footPosition =
            ikGoal == AvatarIKGoal.LeftFoot ? leftFootTransform.position : rightFootTransform.position;

        Transform otherFootTarget = ikGoal == AvatarIKGoal.LeftFoot ? _rightLegTarget : _leftLegTarget;
        int hitsNum = Physics.OverlapSphereNonAlloc(footPosition, range, _sphereHits, layerMask);
        if (hitsNum == 0)
            return null;
        var minDistCollider = _sphereHits[0];
        var minDist = Vector3.Distance(minDistCollider.transform.position, footPosition);
        for (var i = 1; i < hitsNum; i++)
        {
            var currentDistance = Vector3.Distance(_sphereHits[i].transform.position, footPosition);
            if (_sphereHits[i].transform == otherFootTarget)
                continue;
            if (currentDistance < minDist)
            {
                minDist = currentDistance;
                minDistCollider = _sphereHits[i];
            }
        }

        return minDistCollider.transform;
    }

    public void SetFootTarget(Transform target, AvatarIKGoal ikGoal)
    {
        if (ikGoal == AvatarIKGoal.LeftFoot)
        {
            _hasLeftLegTarget = true;
            _leftLegTarget = target;
        }
        else if (ikGoal == AvatarIKGoal.RightFoot)
        {
            _hasRightLegTarget = true;
            _rightLegTarget = target;
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        AvatarIKGoal[] feet = new AvatarIKGoal[] { AvatarIKGoal.LeftFoot, AvatarIKGoal.RightFoot };
        foreach (AvatarIKGoal foot in feet)
        {
            Vector3 footPosition = animator.GetIKPosition(foot);
            RaycastHit hit;
            Physics.Raycast(footPosition + Vector3.up, Vector3.down, out hit, groundLayers);
            animator.SetIKPositionWeight(foot, 1);
            animator.SetIKPosition(foot, hit.point + new Vector3(0, footOffset, 0));
        }

        if (isClimbing)
        {
            var nearestToLeft = GetNearestFootPlacement(footPlacementRange, climbLayers, AvatarIKGoal.LeftFoot);
            if(nearestToLeft!= null)
                SetFootTarget(nearestToLeft, AvatarIKGoal.LeftFoot);
            var nearestToRight = GetNearestFootPlacement(footPlacementRange, climbLayers, AvatarIKGoal.RightFoot);
            if(nearestToRight!= null)
                SetFootTarget(nearestToRight, AvatarIKGoal.RightFoot);
            
            if (_hasRightLegTarget )
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                animator.SetIKPosition(AvatarIKGoal.RightFoot, _rightLegTarget.position);
            }

            if (_hasLeftLegTarget)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, _leftLegTarget.position);
            }
            
        }

        
    }

    private void OnDrawGizmos()
    {
        if(animator == null)
            animator = GetComponent<Animator>();
        Gizmos.color = Color.cyan;
        AvatarIKGoal[] feet = new AvatarIKGoal[] { AvatarIKGoal.LeftFoot, AvatarIKGoal.RightFoot };
        foreach (AvatarIKGoal foot in feet)
        {
            Vector3 footPosition = animator.GetIKPosition(foot);
            Gizmos.DrawSphere(footPosition, footPlacementRange);
        }

    }
}