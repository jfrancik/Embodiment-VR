using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRSkeletonJointFollower : MonoBehaviour
{
    public OVRSkeleton skeleton;
    public OVRSkeleton.BoneId boneId;
    public Vector3 rotationOffset;
    public Transform targetTransform;
    public bool hasOffset = false;
    
    private void LateUpdate()
    {
    //    if((int)boneId >= skeleton.Bones.Count)
    //        return;
    //    targetTransform = skeleton.Bones[(int)boneId].Transform;
    //    // transform.localRotation = targetTransform.llocalRotationocalRotation * Quaternion.Euler(rotationOffset);
    //    transform.rotation = targetTransform.rotation ;
    //    // if(hasOffset)
    //         transform.Rotate(VRIKSettings.Instance.rotationOffset);
    //    // transform.position = targetTransform.position;
    }
}
