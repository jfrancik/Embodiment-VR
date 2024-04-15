using System;
using DefaultNamespace.DesignPatterns;
using UnityEngine;

namespace DefaultNamespace
{
    public class VRGizmos : MonoSingleton<VRGizmos>
    {
        public bool isEnabled;

        public Transform sphere;
        private MeshRenderer _sphereRenderer;
        
        
        public Transform line;
        private MeshRenderer _lineRenderer;
        private void Start()
        {
            
            _sphereRenderer = sphere.GetComponent<MeshRenderer>();
            sphere.gameObject.SetActive(false);
            
            
            
            _lineRenderer = line.GetComponent<MeshRenderer>();
            line.gameObject.SetActive(false);
        }

        public void DrawSphere(Vector3 position, float radius, Color color)
        {
            if(!isEnabled)
                return;
            _sphereRenderer.material.color = color;
            sphere.gameObject.SetActive(true);
            sphere.position = position;
            sphere.localScale = Vector3.one * radius;
        }
        public void DrawRay(Vector3 from, Vector3 direction, float size, Color color)
        {
            if(!isEnabled)
                return;
            DrawLine(from, from + direction*size, color);
        }
        public void DrawLine(Vector3 from, Vector3 to, Color color)
        {
            if(!isEnabled)
                return;
            _lineRenderer.material.color = color;
            line.gameObject.SetActive(true);

            var distanceVector = to - from;
            var size = distanceVector.magnitude;
            var position = (to + from)/2;
            line.position = position;
            line.localScale = new Vector3(line.localScale.x,size/2f ,line.localScale.z);
            line.up = distanceVector;
        }
    }
}