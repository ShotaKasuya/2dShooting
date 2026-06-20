using System;
using System.Collections.Generic;
using System.Threading;
using _Scripts.Service;
using _Scripts.UnityServiceLocator;
using Cysharp.Threading.Tasks;
using Module.EditorExtension.Runtime.Attribute;
using UnityEngine;
using UnityEngine.Playables;

namespace _Scripts.Behaviour
{
    /// <summary>
    /// プロパティにフローのリストを持ち、これに応じてゲームの流れを管理する。
    /// </summary>
    public class GameFlowController : MonoBehaviour
    {
        [Serializable]
        public enum FlowType
        {
            k_AdvanceWave,
            k_ExecuteTimeline,
            k_EndGame,
        }

        [Serializable]
        public class FlowElement
        {
            public string name;
            public FlowType type;

            [Header("Wave Settings")] [Tooltip("現在Waveの生きている敵について、すべて破壊するか")]
            public bool destroyExistingEnemies = true;

            [Tooltip("全滅を待つ最大時間（秒）。0以下の場合は全滅まで無限に待つ")]
            public float waveTimeout = 60f;

            [Tooltip("Wave開始前のカウントダウン秒数")] public int countdownSeconds = 0;

            [Header("Timeline Settings")] public PlayableDirector timeline;

            [Header("Game End Settings")] public bool isWin = true;
        }

        [SerializeField] private FlowElement endGame;
        [SerializeField] private List<FlowElement> flowList = new List<FlowElement>();
        [SerializeField, NullAssert] private WaveController waveController;

        private int m_CurrentFlowIndex = -1;
        private int m_CurrentWaveCount = 0;
        private bool m_IsRunning = true;
        private PlayerController m_PlayerController;

        private void Start()
        {
            waveController.Setup();
            m_PlayerController = ServiceLocator.ForSceneOf(this).Get<PlayerController>();
            if (flowList.Count > 0)
            {
                FlowRoutine(destroyCancellationToken).Forget();
            }
        }

        private void Update()
        {
            if (!m_PlayerController.IsLiving & m_IsRunning)
            {
                m_IsRunning = false;
                HandleEndGame(endGame);
            }
        }

        private async UniTask FlowRoutine(CancellationToken token)
        {
            // シーン遷移のフェードアウトを待つ
            var sceneTransition = ServiceLocator.Global.Get<SceneTransitionUIService>();
            await sceneTransition.WaitUntilTransitionFinishedAsync();

            while (m_IsRunning && m_CurrentFlowIndex < flowList.Count - 1)
            {
                m_CurrentFlowIndex++;
                var element = flowList[m_CurrentFlowIndex];

                Debug.Log($"Executing Flow Element: {element.name} ({element.type})");

                var task = ExecuteFlowElement(element, token);

                while (task.Status != UniTaskStatus.Succeeded & m_IsRunning)
                {
                    await UniTask.Yield(token);
                }
            }

            Debug.Log("Game Flow Manager stopped.");
        }

        private UniTask ExecuteFlowElement(FlowElement element, CancellationToken token)
        {
            switch (element.type)
            {
                case FlowType.k_AdvanceWave:
                    return HandleAdvanceWave(element, token);
                case FlowType.k_ExecuteTimeline:
                    return HandleExecuteTimeline(element, token);
                case FlowType.k_EndGame:
                    HandleEndGame(element);
                    m_IsRunning = false;
                    break;
            }

            return UniTask.CompletedTask;
        }

        private async UniTask HandleAdvanceWave(FlowElement element, CancellationToken token)
        {
            if (element.destroyExistingEnemies)
            {
                waveController.ClearEnemies();
            }

            m_CurrentWaveCount++;

            // WaveAnimationController サービスを取得
            var waveAnimation = ServiceLocator.ForSceneOf(this).Get<WaveUIService>();

            // 1. "WAVE X" 表示演出
            await waveAnimation.ShowWaveStartAsync(m_CurrentWaveCount, token);

            // 2. カウントダウン（設定されている場合）
            if (element.countdownSeconds > 0)
            {
                await waveAnimation.ShowCountdownAsync(element.countdownSeconds, token);
            }

            // 3. 敵の出現・移動開始
            waveController.StartNextWave();

            // Waveが開始されるのを待つ、叉は敵が現れるのを待つ必要があるかもしれないが、
            // ここではドキュメントに従い「全滅、叉は、一定時間の経過」を条件とする。

            float timer = 0;
            while (true)
            {
                // 敵がいないことを確認
                if (!waveController.IsEnemyRemain())
                {
                    await waveAnimation.ShowWaveClearAsync(destroyCancellationToken);
                    break;
                }

                // タイムアウトチェック
                if (element.waveTimeout > 0)
                {
                    timer += Time.deltaTime;
                    if (timer >= element.waveTimeout)
                    {
                        Debug.Log("Wave timeout reached. Advancing to next flow.");
                        break;
                    }
                }

                await UniTask.Yield(token);
            }
        }

        private async UniTask HandleExecuteTimeline(FlowElement element, CancellationToken token)
        {
            if (element.timeline == null)
            {
                Debug.LogWarning("Timeline is not assigned for ExecuteTimeline flow element.");
                return;
            }

            element.timeline.Play();

            // タイムラインの内容が終わり次第、次のフローへ進む。
            while (element.timeline.state == PlayState.Playing)
            {
                await UniTask.Yield(token);
            }
        }

        private void HandleEndGame(FlowElement element)
        {
            Debug.Log($"Game Ended. Result: {(element.isWin ? "Victory" : "Defeat")}");

            // プレイヤーの動きを止める
            var playerController = ServiceLocator.ForSceneOf(this).Get<PlayerController>();
            if (playerController != null)
            {
                playerController.StopPlayer();
            }

            // GameResultController サービスを取得して表示
            var gameResult = ServiceLocator.ForSceneOf(this).Get<GameResultUIService>();
            if (element.isWin)
            {
                gameResult.ShowVictory();
            }
            else
            {
                gameResult.ShowDefeat();
            }
        }
    }
}