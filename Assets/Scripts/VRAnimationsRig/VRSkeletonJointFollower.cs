using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRSkeletonJointFollower : MonoBehaviour
{
    public OVRSkeleton skeleton;
    public OVRSkeleton.BoneId boneId;

    private void LateUpdate()
    {
       // transform.position = skeleton.Bones[(int)boneId].Transform.position;
       var targetTransform = skeleton.Bones[(int)boneId].Transform;
       // transform.position = targetTransform.TransformPoint(VRIKSettings.Instance.PositionOffset);
       transform.rotation = targetTransform.rotation * Quaternion.Euler(VRIKSettings.Instance.rotationOffset);
    }
}
