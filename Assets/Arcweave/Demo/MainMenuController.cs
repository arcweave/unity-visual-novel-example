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
        public Button quitButton;

        [Header("Settings")]
        public string gameSceneName = "ArcweaveDemoScene";

        void Start()
        {
            continueButton.interactable = ArcweavePlayer.HasAnySave();
            newGameButton.onClick.AddListener(OnNewGame);
            continueButton.onClick.AddListener(OnContinue);
            if (quitButton != null) quitButton.onClick.AddListener(Application.Quit);
        }

        void OnNewGame()
        {
            GameBootstrap.ShouldLoadSave = false;
            SceneManager.LoadScene(gameSceneName);
        }

        void OnContinue()
        {
            GameBootstrap.ShouldLoadSave = true;
            GameBootstrap.LoadSlot = ArcweavePlayer.MostRecentSlot();
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
