using System.Collections.Generic;
using _Scripts.Model;
using UnityEngine;
using ZLinq;

namespace _Scripts.Behaviour
{
    public class WaveController : MonoBehaviour
    {
        [SerializeField, Min(0)] private int currentWave;
        [SerializeField] private List<WaveGroup> waveGroupList;

        public void Setup()
        {
            foreach (var waveGroup in waveGroupList.AsValueEnumerable())
            {
                waveGroup.Setup();
            }
        }

        public void StartNextWave()
        {
            waveGroupList[currentWave].WaveStart();
            currentWave++;
        }

        public bool IsEnemyRemain()
        {
            return waveGroupList.AsValueEnumerable()
                .Index()
                .Where(x => x.Index < currentWave & x.Item.IsWaveActive())
                .Any();
        }

        public void ClearEnemies()
        {
            foreach (var waveGroup in waveGroupList.AsValueEnumerable())
            {
                waveGroup.WaveEnd();
            }
        }
    }
}