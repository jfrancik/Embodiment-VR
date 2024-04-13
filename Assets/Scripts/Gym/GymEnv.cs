using System;
using System.Collections.Generic;
using DefaultNamespace.DesignPatterns;
using UnityEngine;

namespace Gym
{
    public class GymEnv : MonoSingleton<GymEnv>
    {
        private Dictionary<int, Vector3> _sources = new Dictionary<int, Vector3>();
        public float sourceMovementThreshold = 0.03f;
        public float envMovementSmoothTime = 0.1f;

        private Vector3 _target;
        private bool _isFollowingByInput = false;
        private bool _reachedTarget = false;
        public void MoveEnv(Vector3 movement, int sourceId)
        {
            
            _sources.TryAdd(sourceId, movement);
            _sources[sourceId] = movement;
            if (_sources.Count > 2)
            {
                _isFollowingByInput = false;
                Debug.LogError("MULTIPLE GAME WORLD MOVEMENTS DETECTED");
                return;
            }
            _isFollowingByInput = true;
            _reachedTarget = false;
            if (_sources.Count == 1)
            {
                _target = transform.position + movement;
               
                return;
            }
            else
            {
               
                Vector3 minMovement = movement;
                foreach (var sourcePair in _sources)
                {
                    if (minMovement.magnitude > sourcePair.Value.magnitude)
                        minMovement = sourcePair.Value;
                }
                // if(minMovement.magnitude< sourceMovementThreshold)
                //     return;
                _target = transform.position + minMovement;
                return;
            }
        }

        public void StopEnv(int sourceId)
        {
            _sources.Remove(sourceId);
            if (_sources.Count == 0)
                _isFollowingByInput = false;
        }


        private void Update()
        {
            if(!_isFollowingByInput || _reachedTarget)
                return;

            transform.position = Vector3.Lerp(transform.position, _target, Time.deltaTime * envMovementSmoothTime);
            
            if (Vector3.Distance(transform.position, _target) < 0.001f)
                _reachedTarget = true;
        }
    }
}