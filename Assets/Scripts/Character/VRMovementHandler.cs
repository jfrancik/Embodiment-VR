using System;
using UnityEngine;

namespace Character
{
    public class VRMovementHandler : MonoBehaviour
    {
        public float standingHeight = 1.822f;
        public float crouchingHeight;
        public bool isStanding = true;
        public Transform vrHeadTransform;
        public LayerMask canStandLayers;

        public Transform heightSensorTransform;
        public float headSyncSmooth = 100;


        public float GetCurrentHeight()
        {
            var ray = new Ray(heightSensorTransform.position, Vector3.down);
            var hits = new RaycastHit[5];
            var hitCount = Physics.RaycastNonAlloc(ray, hits, standingHeight + 3f, canStandLayers);
            if (hitCount > 0)
            {
                Debug.DrawRay(heightSensorTransform.position, Vector3.down *  hits[0].distance);
                return hits[0].distance;
            }
            else
            {
                // Debug.LogError("PLAYER IS IN THE AIR!");
                return -1f;
            }
        }
        private void FixedUpdate()
        {
            var currentHeight = GetCurrentHeight(); 
            if (isStanding && currentHeight > 0)
            {
                var vrHeadCurrentPos = vrHeadTransform.position;
                if (!Mathf.Approximately(currentHeight, standingHeight))
                {
                    var targetPos = new Vector3(vrHeadCurrentPos.x, (standingHeight-currentHeight) +   vrHeadCurrentPos.y , vrHeadCurrentPos.z);
                    // Debug.LogWarning("Dist:" +  (currentHeight) + " Target: " + standingHeight);
                    vrHeadTransform.position =  Vector3.Lerp(vrHeadCurrentPos, targetPos, Time.deltaTime * headSyncSmooth);
                }
            }
        }
    }
}