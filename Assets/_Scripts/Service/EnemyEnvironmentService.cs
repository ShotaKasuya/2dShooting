using System.Collections.Generic;
using _Scripts.UnityServiceLocator;
using UnityEngine;

namespace _Scripts.Service
{
    /// <summary>
    /// 敵の移動パターンなどの環境情報を保持し、サービスロケータ経由で提供するクラス。
    /// </summary>
    public class EnemyEnvironmentService : MonoBehaviour
    {
        [Header("Move To Coordinate Settings")]
        [SerializeField] private Vector2 targetCoordinate;
        [SerializeField] private float arrivalDistance = 0.1f;

        [Header("Move On Curve Settings")]
        [SerializeField] private Vector2 curvePivot;
        [SerializeField] private float amplitudeX = 3f;
        [SerializeField] private float amplitudeY = 1.5f;
        [SerializeField] private float frequency = 1f;

        [Header("Patrol Settings")]
        [SerializeField] private List<Transform> patrolWaypoints;

        public Vector2 TargetCoordinate => targetCoordinate;
        public float ArrivalDistance => arrivalDistance;
        public Vector2 CurvePivot => curvePivot;
        public float AmplitudeX => amplitudeX;
        public float AmplitudeY => amplitudeY;
        public float Frequency => frequency;
        public List<Transform> PatrolWaypoints => patrolWaypoints;

        private void Awake()
        {
            ServiceLocator.ForSceneOf(this).Register(this);
        }
    }
}
