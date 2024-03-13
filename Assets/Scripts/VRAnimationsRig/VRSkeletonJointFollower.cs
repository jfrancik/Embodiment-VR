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
       if((int)boneId >= skeleton.Bones.Count)
           return;
       var targetTransform = skeleton.Bones[(int)boneId].Transform;
       transform.rotation = targetTransform.rotation * Quaternion.Euler(VRIKSettings.Instance.rotationOffset);
    }
}
