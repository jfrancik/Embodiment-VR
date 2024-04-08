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

    [SerializeField] private float footOffset = 0;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private Collider[] _sphereHits = new Collider[20];
    public bool isClimbing;
    public LayerMask climbLayers;
    
    
    [CanBeNull]
    public Transform GetNearestFootPlacement(float range, LayerMask layerMask, AvatarIKGoal ikGoal)
    {
        Vector3 footPosition = animator.GetIKPosition(ikGoal);

        int hitsNum = Physics.OverlapSphereNonAlloc(footPosition, range, _sphereHits);
        if (hitsNum == 0)
            return null;
        var minDistCollider = _sphereHits[0];
        var minDist = Vector3.Distance(minDistCollider.transform.position, footPosition);
        for (var i = 1; i < hitsNum; i++)
        {
            var currentDistance = Vector3.Distance(_sphereHits[i].transform.position, footPosition);
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
            Physics.Raycast(footPosition + Vector3.up, Vector3.down, out hit);
            animator.SetIKPositionWeight(foot, 1);
            animator.SetIKPosition(foot, hit.point + new Vector3(0, footOffset, 0));
        }

        if (isClimbing)
        {
            var nearestToLeft = GetNearestFootPlacement(3, climbLayers, AvatarIKGoal.LeftFoot);
            if(nearestToLeft!= null)
                SetFootTarget(nearestToLeft, AvatarIKGoal.LeftFoot);
            var nearestToRight = GetNearestFootPlacement(3, climbLayers, AvatarIKGoal.RightFoot);
            if(nearestToRight!= null)
                SetFootTarget(nearestToRight, AvatarIKGoal.RightFoot);
            
        }

        if (_hasRightLegTarget)
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