using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using OVRSimpleJSON;
using UnityEngine;

namespace Gym
{
    public class Pickable : MonoBehaviour
    {
        private Vector3 _offset;
        private Dictionary<int, List<Transform>> _fingersTransforms = new Dictionary<int, List<Transform>>();
        public List<FingerColliderType> typesNeededToPickLeft;
        public List<FingerColliderType> typesNeededToPickRight;
        public Rigidbody rb;
        public bool isMovable = true;
        private Dictionary<int, bool> _handSnapped = new Dictionary<int, bool>();
        public Color defaultColor = Color.white;

        public Color snapColor = Color.green;

        public Color bothSnapColor = Color.blue;
        private MeshRenderer _meshRenderer;

        private void Start()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            _fingersTransforms.Add(0, null);
            _fingersTransforms.Add(1, null);
            _handSnapped.Add(0, false);
            _handSnapped.Add(1, false);
        }

        public bool IsAttached(float threshold, int hand)
        {
            var median = FindMedianOfFingers(hand);
            return (Vector3.Distance(transform.position,median ) > threshold);
        }

        private Vector3 FindMedianOfFingers(int hand)
        {
            var sum = Vector3.zero;
            var count = 0;

            foreach (var ft in _fingersTransforms[hand])
            {
                sum += ft.position;
                count++;
            }

            return new Vector3(sum.x / count, sum.y / count, sum.z / count);
        }

        private Vector3 FindMedianOfFingers(bool debug = false)
        {
            var debugStr = "";
            var sum = Vector3.zero;
            var count = 0;
            foreach (var handPair in _handSnapped)
            {
                debugStr += " / " + handPair.Value;
          
                if (!handPair.Value)
                    continue;
           
                foreach (var ft in _fingersTransforms[handPair.Key])
                {
                    sum += ft.position;
                    count++;
                }
                
                debugStr +=  " h:"+handPair.Key+ " c:" + _fingersTransforms[handPair.Key].Count;
            }

            if (debug)
                Debug.LogError(count + debugStr);

            return new Vector3(sum.x / count, sum.y / count, sum.z / count);
        }

        public bool IsSnapped()
        {
            return _handSnapped.Values.Any(sn => sn);
        }

        public void Release(int hand)
        {
            _handSnapped[hand] = false;

            _fingersTransforms[hand] = null;
            if (isMovable && !IsSnapped())
            {
                rb.isKinematic = false;
            }

            var lines = string.Join(Environment.NewLine,
                _handSnapped.Select(kvp => kvp.Key + ": " + kvp.Value.ToString()));
            Debug.LogError($"Hand {hand} released!\n {lines}");
            _offset = transform.position - FindMedianOfFingers(true);
        }

        public void SnapToHand(List<Transform> fingersTransforms, int hand)
        {
            _handSnapped[hand] = true;
            _fingersTransforms[hand] = fingersTransforms;
            if (isMovable)
            {
                rb.isKinematic = true;
            }

            // Debug.LogError( $"Count: {fingersTransforms.Count} / Median: " + FindMedianOfFingers() );
            _offset = transform.position - FindMedianOfFingers();
        }

        private void Update()
        {
            if (IsSnapped())
            {
                if (isMovable)
                    transform.position = FindMedianOfFingers() + _offset;
                else
                {
                    var median = FindMedianOfFingers();
                    VRGizmos.Instance.DrawSphere(median, 0.03f, Color.magenta);
                    // Debug.DrawRay(median , Vector3.up, Color.red);
                    var newPos = median + _offset;
                    var movement = newPos - transform.position;

                    GymEnv.Instance.MoveEnv(movement, gameObject.GetInstanceID());
                }


                _meshRenderer.material.color = snapColor;
                if (_handSnapped.Values.All(v => v))
                    _meshRenderer.material.color = bothSnapColor;
            }
            else if (!isMovable)
            {
                GymEnv.Instance.StopEnv(gameObject.GetInstanceID());

                _meshRenderer.material.color = defaultColor;
            }
            else
            {
                _meshRenderer.material.color = defaultColor;
            }
        }
    }
}