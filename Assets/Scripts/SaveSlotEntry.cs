using System.Text.RegularExpressions;
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

        private static readonly Regex TagRegex = new Regex("<.*?>", RegexOptions.Compiled);

        private System.Action<int> _callback;
        private int _slot;

        public void Setup(int slot, bool hasSave, string label, string time, bool isSaveMode, System.Action<int> callback)
        {
            _slot = slot;
            _callback = callback;
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => _callback?.Invoke(_slot));
            slotLabel.text = "Slot " + (slot + 1);
            infoLabel.text = hasSave ? StripTags(label) + "\n" + time : "— Empty —";
            actionButtonLabel.text = isSaveMode ? "Save" : "Load";
            actionButton.interactable = isSaveMode || hasSave;
        }

        static string StripTags(string s) => string.IsNullOrEmpty(s) ? s : TagRegex.Replace(s, string.Empty);
    }
}
