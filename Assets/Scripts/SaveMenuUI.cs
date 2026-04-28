using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Arcweave
{
    public class SaveMenuUI : MonoBehaviour
    {
        [Header("References")]
        public ArcweavePlayer player;
        public GameObject panel;
        public Button closeButton;
        public TextMeshProUGUI titleLabel;
        public SaveSlotEntry[] slots;

        // Optional: when set, replaces the default in-game load behavior.
        // Used by the main menu to load via scene switch instead of in-place restore.
        public System.Action<int> loadOverride;

        private bool _isSaveMode;

        void Awake()
        {
            panel.SetActive(false);
            closeButton.onClick.AddListener(Close);
        }

        public void OpenForSave()
        {
            _isSaveMode = true;
            titleLabel.text = "SAVE";
            Refresh();
            panel.SetActive(true);
        }

        public void OpenForLoad()
        {
            _isSaveMode = false;
            titleLabel.text = "LOAD";
            Refresh();
            panel.SetActive(true);
        }

        public void Close() => panel.SetActive(false);

        void Refresh()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                int idx = i;
                bool has = HasSaveInSlot(i);
                var (label, time) = GetSlotInfo(i);
                slots[i].Setup(idx, has, label, time, _isSaveMode, OnSlotAction);
            }
        }

        void OnSlotAction(int slot)
        {
            if (_isSaveMode)
            {
                player.SaveToSlot(slot);
                Refresh();
            }
            else
            {
                Close();
                if (loadOverride != null) loadOverride(slot);
                else player.LoadFromSlot(slot);
            }
        }

        // Static-ish accessors so the menu works in scenes without an ArcweavePlayer (e.g. main menu).
        bool HasSaveInSlot(int i) => player != null
            ? player.HasSaveInSlot(i)
            : ArcweavePlayer.HasSaveInSlotStatic(i);

        (string label, string time) GetSlotInfo(int i) => player != null
            ? player.GetSlotInfo(i)
            : ArcweavePlayer.GetSlotInfoStatic(i);
    }
}
