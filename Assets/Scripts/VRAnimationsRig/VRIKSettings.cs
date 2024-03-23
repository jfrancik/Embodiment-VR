using System.Collections;
using System.Collections.Generic;
using DefaultNamespace.DesignPatterns;
using UnityEngine;

public class VRIKSettings : MonoSingleton<VRIKSettings>
{
    public Vector3 PositionOffset;
    public Vector3 rotationOffset;

    public int num;
}
