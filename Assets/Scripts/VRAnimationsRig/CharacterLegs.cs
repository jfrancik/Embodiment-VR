using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class CharacterLegs : MonoBehaviour
{
    private Animator animator;

    public Transform defaultLeftAnchor;
    public Transform defaultRightAnchor;
    private Transform _leftLegTarget;
    private bool _hasLeftLegTarget;

    private Transform _rightLegTarget;
    private bool _hasRightLegTarget;

    public float footPlacementRange = 1;
    [SerializeField] private float footOffset = 0;


    public Transform rightFootTransform;
    public Transform leftFootTransform;

    public float iKSmoothTime = 20;

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

    private void Update()
    {
        UpdateLegAnchors();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        AvatarIKGoal[] feet = new AvatarIKGoal[] { AvatarIKGoal.LeftFoot, AvatarIKGoal.RightFoot };

        if (!isClimbing)
        {
            foreach (AvatarIKGoal foot in feet)
            {
                Vector3 footPosition = animator.GetIKPosition(foot);
                RaycastHit hit;
                Physics.Raycast(footPosition + Vector3.up, Vector3.down, out hit, 2, groundLayers);
                animator.SetIKPositionWeight(foot, 1);
                animator.SetIKPosition(foot, hit.point + new Vector3(0, footOffset, 0));
            }

            SetFootTarget(defaultLeftAnchor, AvatarIKGoal.LeftFoot);
            SetFootTarget(defaultRightAnchor, AvatarIKGoal.RightFoot);
        }
        else
        {
            var nearestToLeft = GetNearestFootPlacement(footPlacementRange, climbLayers, AvatarIKGoal.LeftFoot);
            if (nearestToLeft != null)
                SetFootTarget(nearestToLeft, AvatarIKGoal.LeftFoot);
            var nearestToRight = GetNearestFootPlacement(footPlacementRange, climbLayers, AvatarIKGoal.RightFoot);
            if (nearestToRight != null)
                SetFootTarget(nearestToRight, AvatarIKGoal.RightFoot);
        }

        if (_hasRightLegTarget)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            // var newPos = Vector3.Lerp(rightFootTransform.position, _rightLegTarget.position,
            //     Time.deltaTime * iKSmoothTime);
            var newPos = _rightLegTarget.position;
            animator.SetIKPosition(AvatarIKGoal.RightFoot, newPos);
        }

        if (_hasLeftLegTarget)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            // var newPos = Vector3.Lerp(leftFootTransform.position, _leftLegTarget.position,
            //     Time.deltaTime * iKSmoothTime);

            var newPos = _leftLegTarget.position;
            animator.SetIKPosition(AvatarIKGoal.LeftFoot, newPos);
        }
    }

    public Transform legBaseTransform;
    public float anchorDistanceSoftThreshold = 0.5f;
    public float anchorDistanceThreshold;
    public float legOpenness = 0.2f;
    public bool isCheckingSoft = true;

    public LegWalkState leftLegState = LegWalkState.STAY;
    public LegWalkState rightLegState = LegWalkState.STAY;


    private Vector3 _leftNextAnchorPositionTarget;
    private Vector3 _rightNextAnchorPositionTarget;
    public float distanceInFront = 0.5f;

    
    
    //TODO: make thresholds based on speed of movement
    public void UpdateLegAnchors()
    {
        var leftLegBasePos = legBaseTransform.position + -legBaseTransform.right * legOpenness;
        var rightLegBasePos = legBaseTransform.position + legBaseTransform.right * legOpenness;
        var leftNextAnchorPositionForward = leftLegBasePos + legBaseTransform.forward * distanceInFront;
        var rightNextAnchorPositionForward = rightLegBasePos + legBaseTransform.forward * distanceInFront;
        var leftNextAnchorPositionBackward = leftLegBasePos - legBaseTransform.forward * distanceInFront;
        var rightNextAnchorPositionBackward = rightLegBasePos - legBaseTransform.forward * distanceInFront;
        var leftDistVector = defaultLeftAnchor.position - leftLegBasePos;
        var rightDistVector = defaultRightAnchor.position - rightLegBasePos;
        var leftDist =
            Vector3.Distance(leftLegBasePos, defaultLeftAnchor.position);
        var rightDist = Vector3.Distance(rightLegBasePos, defaultRightAnchor.position);

        if (leftDist > anchorDistanceSoftThreshold && leftLegState == LegWalkState.STAY &&
            rightLegState == LegWalkState.STAY)
        {
            if (Vector3.Dot(leftDistVector, defaultLeftAnchor.right) > 0)
            {
                _leftNextAnchorPositionTarget = leftNextAnchorPositionForward;
            }
            else
            {
                _leftNextAnchorPositionTarget = leftNextAnchorPositionBackward;
            }

            leftLegState = LegWalkState.GOING_FORWARD;
        }
        else if (rightDist > anchorDistanceSoftThreshold && leftLegState == LegWalkState.STAY &&
                 rightLegState == LegWalkState.STAY)
        {
            if (Vector3.Dot(rightDistVector, defaultRightAnchor.right) > 0)
            {
                _rightNextAnchorPositionTarget = rightNextAnchorPositionForward;
            }
            else
            {
                _rightNextAnchorPositionTarget = rightNextAnchorPositionBackward;
            }

            rightLegState = LegWalkState.GOING_FORWARD;
        }

        defaultLeftAnchor.position = Vector3.Lerp(defaultLeftAnchor.position, _leftNextAnchorPositionTarget,
            Time.deltaTime * iKSmoothTime);
        if (Vector3.Distance(defaultLeftAnchor.position, _leftNextAnchorPositionTarget) < 0.001f)
        {
            leftLegState = LegWalkState.STAY;
        }


        defaultRightAnchor.position = Vector3.Lerp(defaultRightAnchor.position, _rightNextAnchorPositionTarget,
            Time.deltaTime * iKSmoothTime);
        if (Vector3.Distance(defaultRightAnchor.position, _rightNextAnchorPositionTarget) < 0.001f)
        {
            rightLegState = LegWalkState.STAY;
        }

        // if (leftDist> anchorDistanceSoftThreshold && isCheckingSoft)
        // {
        //     defaultLeftAnchor.position = leftNextAnchorPosition;
        //     isCheckingSoft = false;
        // }
        // else if (rightDist > anchorDistanceSoftThreshold && isCheckingSoft)
        // {
        //     defaultRightAnchor.position = rightNextAnchorPosition;
        //     isCheckingSoft = false;
        // }
        // if (leftDist> anchorDistanceThreshold)
        // {
        //     defaultLeftAnchor.position = leftNextAnchorPosition;
        //     isCheckingSoft = true;
        //
        // }
        // else if (rightDist > anchorDistanceThreshold)
        // {
        //     defaultRightAnchor.position = rightNextAnchorPosition;
        //     isCheckingSoft = true;
        //
        // }
    }

    private void OnDrawGizmos()
    {
        if (animator == null)
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

public enum LegWalkState
{
    STAY,
    GOING_FORWARD
}