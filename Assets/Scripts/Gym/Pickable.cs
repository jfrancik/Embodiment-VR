using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gym
{
    public class Pickable : MonoBehaviour
    {
        private Vector3 _offset;
        private List<Transform> _fingersTransforms = new List<Transform>();
        private bool isSnapped = false;
        public List<FingerColliderType> typesNeededToPick;
        public Rigidbody rb;

        private Vector3 FindMedianOfFingers()
        {
            var sum = Vector3.zero;
            foreach (var ft in _fingersTransforms)
            {
                sum += ft.position;
            }
            return new Vector3(sum.x / _fingersTransforms.Count, sum.y / _fingersTransforms.Count,sum.z / _fingersTransforms.Count);
        }
        public void Release()
        {
            rb.isKinematic = false;

            isSnapped = false;
        }
        public void SnapToHand(List<Transform> fingersTransforms)
        {
            _fingersTransforms = fingersTransforms;
            rb.isKinematic = true;
            // Debug.LogError( $"Count: {fingersTransforms.Count} / Median: " + FindMedianOfFingers() );
            _offset = transform.position - FindMedianOfFingers();
            isSnapped = true;
        }

        private void Update()
        {
            if (isSnapped)
            {
                transform.position = FindMedianOfFingers() + _offset;
            }
        }
    }
}