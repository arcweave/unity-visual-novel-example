using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Arcweave
{
    public class SaveSlotEntry : MonoBehaviour
    {
        public TextMeshProUGUI slotLabel;
        public TextMeshProUGUI infoLabel;
        public Button actionButton;
        public TextMeshProUGUI actionButtonLabel;

        private System.Action<int> _callback;
        private int _slot;

        public void Setup(int slot, bool hasSave, string label, string time, bool isSaveMode, System.Action<int> callback)
        {
            _slot = slot;
            _callback = callback;
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => _callback?.Invoke(_slot));
            slotLabel.text = "Slot " + (slot + 1);
            infoLabel.text = hasSave ? label + "\n" + time : "— Vuoto —";
            actionButtonLabel.text = isSaveMode ? "Salva" : "Carica";
            actionButton.interactable = isSaveMode || hasSave;
        }
    }
}
