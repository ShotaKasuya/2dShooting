using _Scripts.UnityServiceLocator;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Service
{
    /// <summary>
    /// 実行環境や使用中の入力デバイスに応じて、タッチ用UI（バーチャルパッド等）の表示・非表示を切り替えるサービス。
    /// </summary>
    public class TouchInputUIService : SceneService<TouchInputUIService>
    {
        [Header("UI References")]
        [Tooltip("タッチ操作用のUIグループ（仮想スティックやボタンを含むGameObject）")]
        [SerializeField] private GameObject touchUIContainer;

        [Header("Behavior Settings")]
        [Tooltip("タッチ対応デバイス以外（PCなど）で、タッチ操作時にのみ動的にUIを表示するかどうか")]
        [SerializeField] private bool enableDynamicSwitching = true;

        [Tooltip("Unity Editor上で常にタッチUIを表示するかどうか（デバッグ用）")]
        [SerializeField] private bool forceShowInEditor = true;

        private void Start()
        {
            if (touchUIContainer == null)
            {
                Debug.LogWarning("TouchUIContainer is not assigned in TouchInputUIService.", this);
                return;
            }

            // 初期状態の表示判定
            bool shouldShow = DetermineInitialVisibility();
            touchUIContainer.SetActive(shouldShow);

            // 動的切り替えの登録
            if (enableDynamicSwitching)
            {
                InputSystem.onActionChange += OnActionChange;
            }
        }

        private void OnDestroy()
        {
            if (enableDynamicSwitching)
            {
                InputSystem.onActionChange -= OnActionChange;
            }
        }

        /// <summary>
        /// 実行環境に応じた初期の表示状態を決定します。
        /// </summary>
        private bool DetermineInitialVisibility()
        {
#if UNITY_EDITOR
            if (forceShowInEditor)
            {
                return true;
            }
#endif

            // モバイルプラットフォーム（Android, iOS）またはタッチ対応の携帯端末
            if (Application.isMobilePlatform || SystemInfo.deviceType == DeviceType.Handheld)
            {
                return true;
            }

            // タッチパネルがサポートされているPCやブラウザ等の環境
            if (Input.touchSupported)
            {
                return true;
            }

            // それ以外のデスクトップPCなどの環境では初期状態は非表示
            return false;
        }

        /// <summary>
        /// 入力イベントが発生したときにデバイスの種類を検知し、UIの表示状態を動的に切り替えます。
        /// </summary>
        private void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.ActionPerformed) return;

            var action = obj as InputAction;
            if (action == null || action.activeControl == null) return;

            var device = action.activeControl.device;

            // タッチスクリーンによる入力があればUIを表示
            if (device is Touchscreen)
            {
                SetTouchUIVisibility(true);
            }
            // キーボード、マウス、ゲームパッドによる入力があればタッチUIを非表示にする
            else if (device is Keyboard || device is Mouse || device is Gamepad)
            {
                // モバイルプラットフォームでは、ゲームパッドが接続されて使われた場合のみ非表示にし、
                // マウスやキーボードの誤検知等で消えないように保護する
                if (Application.isMobilePlatform)
                {
                    if (device is Gamepad)
                    {
                        SetTouchUIVisibility(false);
                    }
                }
                else
                {
                    // PC環境などでは、キーボードやマウスで操作し始めたらタッチUIを非表示にする
                    SetTouchUIVisibility(false);
                }
            }
        }

        /// <summary>
        /// タッチUIの表示状態を設定します。
        /// </summary>
        public void SetTouchUIVisibility(bool visible)
        {
            if (touchUIContainer != null && touchUIContainer.activeSelf != visible)
            {
                touchUIContainer.SetActive(visible);
            }
        }
    }
}
