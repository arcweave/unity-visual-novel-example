using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Arcweave
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Buttons")]
        public Button newGameButton;
        public Button continueButton;
        public Button loadButton;
        public Button quitButton;

        [Header("Optional: pick a slot to load")]
        public SaveMenuUI loadMenu;

        [Header("Settings")]
        public string gameSceneName = "ArcweaveDemoScene";

        void Start()
        {
            bool hasSave = ArcweavePlayer.HasAnySave();
            continueButton.interactable = hasSave;
            newGameButton.onClick.AddListener(OnNewGame);
            continueButton.onClick.AddListener(OnContinue);
            if (loadButton != null)
            {
                loadButton.interactable = hasSave;
                loadButton.onClick.AddListener(OnLoad);
            }
            if (quitButton != null) quitButton.onClick.AddListener(Application.Quit);

            if (loadMenu != null) loadMenu.loadOverride = LoadSlotAndSwitchScene;
        }

        void OnNewGame()
        {
            GameBootstrap.ShouldLoadSave = false;
            SceneManager.LoadScene(gameSceneName);
        }

        void OnContinue() => LoadSlotAndSwitchScene(ArcweavePlayer.MostRecentSlot());

        void OnLoad()
        {
            if (loadMenu != null) loadMenu.OpenForLoad();
            else LoadSlotAndSwitchScene(ArcweavePlayer.MostRecentSlot()); // fallback

            
        }

        

        void LoadSlotAndSwitchScene(int slot)
        {
            GameBootstrap.ShouldLoadSave = true;
            GameBootstrap.LoadSlot = slot;
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
