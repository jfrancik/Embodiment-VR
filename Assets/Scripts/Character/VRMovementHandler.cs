using System;
using System.Linq;
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
        public Transform cameraLookTransform;
        public Transform forwardWallSensor;

        private bool _isMovingForward = false;

        private Vector3 _currentMovement;

        private bool _isClimbing = false;

        public float turningAxis = 0;

        
        public CharacterLegs characterLegs;


        public void SetIsClimbing(bool value)
        {
            _isClimbing = value;
            characterLegs.isClimbing = value;
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(forwardWallSensor.position, forwardWallSensor.up * forwardIntersectDistance);
        }

        public void TurnRight()
        {
            turningAxis = 1f;
        }

        public void TurnLeft()
        {
            turningAxis = -1f;
        }

        public void StopTurning()
        {
            turningAxis = 0f;
        }

        public void MoveForward()
        {
            print("MOVING FORWARD");
            _isMovingForward = true;
        }

        public void StopMoveForward()
        {
            print("STOP MOVING FORWARD");
            _isMovingForward = false;
        }

        public float forwardIntersectDistance = 1;

        public Vector3 GetForwardWithWalls()
        {
            var ray = new Ray(forwardWallSensor.position, forwardWallSensor.up);
            var hits = new RaycastHit[5];
            var hitCount = Physics.RaycastNonAlloc(ray, hits, forwardIntersectDistance, canStandLayers);
            if (hitCount > 0)
            {
                Debug.DrawRay(forwardWallSensor.position, forwardWallSensor.up * hits[0].distance);
                var minHit = hits[0];
                for (int i = 1; i < hitCount; i++)
                {
                    if (hits[i].distance < minHit.distance)
                    {
                        minHit = hits[i];
                    }
                }

                return Vector3.Project(forwardWallSensor.up , Vector3.Cross(Vector3.up, minHit.normal).normalized);
            }
            else
            {
                // Debug.LogError("PLAYER IS IN THE AIR!");
                return cameraLookTransform.forward;
            }
        }

        public float GetCurrentHeight()
        {
            var ray = new Ray(heightSensorTransform.position, Vector3.down);
            var hits = new RaycastHit[5];
            var hitCount = Physics.RaycastNonAlloc(ray, hits, standingHeight + 3f, canStandLayers);
            if (hitCount > 0)
            {
                // print(hitCount);
                // print(hits[0].distance);
                Debug.DrawRay(heightSensorTransform.position, Vector3.down * hits[0].distance);
                var minDist = Mathf.Infinity;
                for (int i = 0; i < hitCount; i++)
                {
                    if (hits[i].distance < minDist)
                    {
                        minDist = hits[i].distance;
                    }
                }

                return minDist;
            }
            else
            {
                Debug.LogError("PLAYER IS IN THE AIR!");
                return standingHeight * 2;
            }
        }

        public float speed = 1;

        private void FixedUpdate()
        {
            UpdateVelocity();
            if(_isClimbing)
                return;
            var vrHeadCurrentPos = vrHeadTransform.position;
            if (_isMovingForward)
            {
                _currentMovement = Vector3.Scale(GetForwardWithWalls(), new Vector3(1, 0, 1)).normalized;
                vrHeadCurrentPos =
                    Vector3.Lerp(vrHeadCurrentPos, vrHeadCurrentPos + _currentMovement, Time.deltaTime * speed);
                vrHeadTransform.position = vrHeadCurrentPos;
            }

            var currentHeight = GetCurrentHeight();
            if (isStanding && currentHeight > 0)
            {
                if (!Mathf.Approximately(currentHeight, standingHeight))
                {
                    var targetPos = new Vector3(vrHeadCurrentPos.x,
                        (standingHeight - currentHeight) + vrHeadCurrentPos.y, vrHeadCurrentPos.z);
                    vrHeadTransform.position =
                        Vector3.Lerp(vrHeadCurrentPos, targetPos, Time.deltaTime * headSyncSmooth);
                }
            }

            if (Mathf.Abs(turningAxis) > Mathf.Epsilon)
            {
                vrHeadTransform.Rotate(Vector3.up, turningAxis * Time.fixedDeltaTime * 45);
            }
        }
        
        
        
        public Vector3 VRHeadVelocity { get; private set; }
        private Vector3 _posInLastFrame;
        private void UpdateVelocity()
        {
            var posInCurrentFrame = transform.position;
            VRHeadVelocity = (posInCurrentFrame - _posInLastFrame) / Time.fixedDeltaTime;
            _posInLastFrame = posInCurrentFrame;
        }
    }
}