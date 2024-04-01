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
        public List<FingerColliderType> typesNeededToPickLeft;
        public List<FingerColliderType> typesNeededToPickRight;
        public Rigidbody rb;
        public bool isMovable = true;

        public Color defaultColor = Color.white;
        
        public Color snapColor = Color.green;
        private MeshRenderer _meshRenderer;

        private void Start()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
        }

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
            _meshRenderer.material.color = defaultColor;
            if (isMovable)
            {
                
                rb.isKinematic = false;
            }

            isSnapped = false;
        }
        public void SnapToHand(List<Transform> fingersTransforms)
        {
            _meshRenderer.material.color = snapColor;
            _fingersTransforms = fingersTransforms;
            if (isMovable)
            {
                
                rb.isKinematic = true;
            }
            // Debug.LogError( $"Count: {fingersTransforms.Count} / Median: " + FindMedianOfFingers() );
            _offset = transform.position - FindMedianOfFingers();
            isSnapped = true;
        }

        private void Update()
        {
            if (isSnapped)
            {
                if(isMovable)
                    transform.position = FindMedianOfFingers() + _offset;
                else
                {
                    var newPos = FindMedianOfFingers() + _offset;
                    var movement = newPos - transform.position ;
                    GymEnv.Instance.MoveEnv(movement, gameObject.GetInstanceID());
                }
            }
            else if(!isMovable)
            {
                GymEnv.Instance.StopEnv(gameObject.GetInstanceID());

            }
        }
    }
}