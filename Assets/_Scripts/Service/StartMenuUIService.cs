using _Scripts.UnityServiceLocator;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace _Scripts.Service
{
    /// <summary>
    /// スタート画面のUIを管理するサービス。
    /// メインメニューと設定（音量調整）の切り替え、ボタン操作を処理する。
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class StartMenuUIService : SceneService<StartMenuUIService>
    {
        private VisualElement m_MainMenu;
        private VisualElement m_SettingsMenu;

        private Button m_BtnStart;
        private Button m_BtnSettings;
        private Button m_BtnQuit;
        private Button m_BtnBack;

        private Slider m_SliderBgm;
        private Slider m_SliderSe;

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            Debug.Assert(uiDocument != null, "UIDocument component is missing.");

            var root = uiDocument.rootVisualElement;

            // 要素の取得
            m_MainMenu = root.Q<VisualElement>("main-menu");
            m_SettingsMenu = root.Q<VisualElement>("settings-menu");

            m_BtnStart = root.Q<Button>("btn-start");
            m_BtnSettings = root.Q<Button>("btn-settings");
            m_BtnQuit = root.Q<Button>("btn-quit");
            m_BtnBack = root.Q<Button>("btn-back");

            m_SliderBgm = root.Q<Slider>("slider-bgm");
            m_SliderSe = root.Q<Slider>("slider-se");

            // Assertions
            Debug.Assert(m_MainMenu != null, "main-menu VisualElement not found.");
            Debug.Assert(m_SettingsMenu != null, "settings-menu VisualElement not found.");
            Debug.Assert(m_BtnStart != null, "btn-start Button not found.");
            Debug.Assert(m_BtnSettings != null, "btn-settings Button not found.");
            Debug.Assert(m_BtnQuit != null, "btn-quit Button not found.");
            Debug.Assert(m_BtnBack != null, "btn-back Button not found.");
            Debug.Assert(m_SliderBgm != null, "slider-bgm Slider not found.");
            Debug.Assert(m_SliderSe != null, "slider-se Slider not found.");

            // イベント登録
            m_BtnStart.clicked += OnStartClicked;
            m_BtnSettings.clicked += OnSettingsClicked;
            m_BtnQuit.clicked += OnQuitClicked;
            m_BtnBack.clicked += OnBackClicked;

            m_SliderBgm.RegisterValueChangedCallback(evt => OnBgmVolumeChanged(evt.newValue));
            m_SliderSe.RegisterValueChangedCallback(evt => OnSeVolumeChanged(evt.newValue));

            ShowMainMenu();
        }

        private void OnDisable()
        {
            if (m_BtnStart == null) return;

            m_BtnStart.clicked -= OnStartClicked;
            m_BtnSettings.clicked -= OnSettingsClicked;
            m_BtnQuit.clicked -= OnQuitClicked;
            m_BtnBack.clicked -= OnBackClicked;
        }

        public void ShowMainMenu()
        {
            m_MainMenu.RemoveFromClassList("hidden");
            m_SettingsMenu.AddToClassList("hidden");
            m_BtnStart.Focus();
        }

        public void ShowSettings()
        {
            m_MainMenu.AddToClassList("hidden");
            m_SettingsMenu.RemoveFromClassList("hidden");
            m_BtnBack.Focus();
        }

        private void OnStartClicked()
        {
            Debug.Log("Start Game Clicked");
            // TODO: シーン遷移などの処理をここに実装
            ServiceLocator.Global.Get<SceneTransitionUIService>().LoadSceneAsync("Map").Forget();
        }

        private void OnSettingsClicked()
        {
            ShowSettings();
        }

        private void OnQuitClicked()
        {
            Debug.Log("Quit Game Clicked");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnBackClicked()
        {
            ShowMainMenu();
        }

        private void OnBgmVolumeChanged(float value)
        {
            if (ServiceLocator.Global.TryGet<AudioManager>(out var audioManager))
            {
                audioManager.SetBgmVolume(value);
            }
        }

        private void OnSeVolumeChanged(float value)
        {
            if (ServiceLocator.Global.TryGet<AudioManager>(out var audioManager))
            {
                audioManager.SetSeVolume(value);
            }
        }
    }
}
