using System.Collections.Generic;
using DefaultNamespace.DesignPatterns;
using UnityEngine;

namespace Gym
{
    public class GymEnv : MonoSingleton<GymEnv>
    {
        private HashSet<int> _sources = new HashSet<int>();

        public void MoveEnv(Vector3 movement, int sourceId)
        {
            _sources.Add(sourceId);
            if(_sources.Count > 1)
                return;
            
            transform.position += movement;
        }
        public void StopEnv(int sourceId)
        {
            _sources.Remove(sourceId);
        }
    }
}