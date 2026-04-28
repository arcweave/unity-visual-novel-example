using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Arcweave
{
    public class DialogueLogUI : MonoBehaviour
    {
        [Header("References")]
        public GameObject panel;
        public Button openButton;
        public Button closeButton;
        public RectTransform logContent;
        public GameObject entryTemplate;

        private readonly List<GameObject> _entries = new List<GameObject>();
        private ScrollRect _scroll;

        void Awake()
        {
            panel.SetActive(false);
            entryTemplate.SetActive(false);

            var vlg = logContent.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) vlg = logContent.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = 16f;
            vlg.padding = new RectOffset(12, 12, 8, 8);

            var csf = logContent.GetComponent<ContentSizeFitter>();
            if (csf == null) csf = logContent.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _scroll = panel.GetComponentInChildren<ScrollRect>(true);
            if (_scroll != null) _scroll.scrollSensitivity = 40f;

            if (openButton != null) openButton.onClick.AddListener(Open);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        void Update()
        {
            if (panel.activeSelf && Input.GetKeyDown(KeyCode.H))
                Close();
        }

        public void AddEntry(string speaker, string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            var body = string.IsNullOrEmpty(speaker) ? text : "<b>" + speaker + "</b>\n" + text;
            SpawnEntry(body, FontStyles.Normal);
        }

        public void AddChoice(string label)
        {
            if (string.IsNullOrEmpty(label)) return;
            SpawnEntry("> " + label, FontStyles.Italic);
        }

        void SpawnEntry(string body, FontStyles style)
        {
            var go = Instantiate(entryTemplate, logContent);
            go.SetActive(true);

            var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp == null) { _entries.Add(go); return; }

            var rt = tmp.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.fontStyle = style;
            tmp.text = body + "\n";

            var entryCsf = go.GetComponent<ContentSizeFitter>();
            if (entryCsf == null) entryCsf = go.AddComponent<ContentSizeFitter>();
            entryCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _entries.Add(go);
            StartCoroutine(ScrollToBottom());
        }

        public void Clear()
        {
            foreach (var e in _entries) Destroy(e);
            _entries.Clear();
        }

        public void Open()
        {
            panel.SetActive(true);
            StartCoroutine(ScrollToBottom());
        }

        public void Close() => panel.SetActive(false);

        IEnumerator ScrollToBottom()
        {
            yield return null;
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(logContent);
            if (_scroll != null) _scroll.verticalNormalizedPosition = 0f;
        }
    }
}
