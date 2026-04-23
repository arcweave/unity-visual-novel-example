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

        private bool _isSaveMode;

        void Awake()
        {
            panel.SetActive(false);
            closeButton.onClick.AddListener(Close);
        }

        public void OpenForSave()
        {
            _isSaveMode = true;
            titleLabel.text = "SALVA";
            Refresh();
            panel.SetActive(true);
        }

        public void OpenForLoad()
        {
            _isSaveMode = false;
            titleLabel.text = "CARICA";
            Refresh();
            panel.SetActive(true);
        }

        public void Close() => panel.SetActive(false);

        void Refresh()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                int idx = i;
                bool has = player.HasSaveInSlot(i);
                var (label, time) = player.GetSlotInfo(i);
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
                player.LoadFromSlot(slot);
            }
        }
    }
}
