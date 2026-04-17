using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Arcweave.Project;

namespace Arcweave
{
    public enum FadeMode { None, FadeAlpha, Overlay }

    public class VisualNovelUI : MonoBehaviour
    {
        [Header("References")]
        public ArcweavePlayer player;

        [Header("Background")]
        public RawImage background;

        [Header("Characters")]
        public RectTransform charactersContainer;
        public RawImage characterTemplate;

        [Header("Dialogue Box")]
        public TextMeshProUGUI speakerName;
        public TextMeshProUGUI dialogueText;
        public Button dialoguePanelButton;
        public GameObject continueArrow;
        public float charsPerSecond = 40f;

        [Header("Choices")]
        public Button choiceButtonTemplate;

        [Header("Quick Menu")]
        public Button saveButton;
        public Button loadButton;

        [Header("Fade")]
        public FadeMode backgroundFadeMode = FadeMode.Overlay;
        public float backgroundFadeTime = 0.5f;
        public FadeMode characterFadeMode = FadeMode.FadeAlpha;
        public float characterFadeTime = 0.3f;
        public Image transitionOverlay;

        private readonly List<RawImage> _characterSlots = new List<RawImage>();
        private readonly List<Button> _choiceButtons = new List<Button>();
        private float _lastContainerH = -1f;
        private float _lastContainerW = -1f;
        private System.Action _pendingNextCallback;
        private Coroutine _typewriterCoroutine;
        private bool _typewriterDone = true;

        void OnEnable()
        {
            characterTemplate.gameObject.SetActive(false);
            choiceButtonTemplate.gameObject.SetActive(false);
            if (continueArrow != null) continueArrow.SetActive(false);
            if (dialoguePanelButton != null) dialoguePanelButton.onClick.AddListener(OnDialogueClick);
            saveButton.onClick.AddListener(Save);
            loadButton.onClick.AddListener(Load);
            loadButton.gameObject.SetActive(PlayerPrefs.HasKey(ArcweavePlayer.SAVE_KEY + "_currentElement"));

            player.onElementEnter += OnElementEnter;
            player.onElementOptions += OnElementOptions;
            player.onWaitInputNext += OnWaitInputNext;
            player.onProjectFinish += OnProjectFinish;
        }

        void OnDisable()
        {
            if (dialoguePanelButton != null) dialoguePanelButton.onClick.RemoveListener(OnDialogueClick);
            player.onElementEnter -= OnElementEnter;
            player.onElementOptions -= OnElementOptions;
            player.onWaitInputNext -= OnWaitInputNext;
            player.onProjectFinish -= OnProjectFinish;
        }

        void Update()
        {
            if (_pendingNextCallback != null && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
                OnDialogueClick();
        }

        void OnDialogueClick()
        {
            if (!_typewriterDone)
            {
                SkipTypewriter();
                return;
            }
            if (_pendingNextCallback == null) return;
            var cb = _pendingNextCallback;
            _pendingNextCallback = null;
            if (continueArrow != null) continueArrow.SetActive(false);
            cb();
        }

        void SkipTypewriter()
        {
            if (_typewriterCoroutine != null) { StopCoroutine(_typewriterCoroutine); _typewriterCoroutine = null; }
            FinishTypewriter();
        }

        void FinishTypewriter()
        {
            _typewriterDone = true;
            dialogueText.maxVisibleCharacters = int.MaxValue;
            if (_pendingNextCallback != null && continueArrow != null)
                continueArrow.SetActive(true);
        }

        IEnumerator TypewriterRoutine()
        {
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.ForceMeshUpdate();
            int total = dialogueText.textInfo.characterCount;
            float delay = charsPerSecond > 0f ? 1f / charsPerSecond : 0f;

            for (int i = 0; i < total; i++)
            {
                dialogueText.maxVisibleCharacters = i + 1;
                if (delay > 0f) yield return new WaitForSeconds(delay);
            }

            _typewriterCoroutine = null;
            FinishTypewriter();
        }

        void Save()
        {
            player.Save();
            loadButton.gameObject.SetActive(true);
        }

        void Load()
        {
            ClearChoiceButtons();
            player.Load();
        }

        //----------------------------------------------------------------------

        void LateUpdate()
        {
            if (_characterSlots.Count == 0) return;
            var rt = (RectTransform)charactersContainer;
            float h = rt.rect.height;
            float w = rt.rect.width;
            if (Mathf.Approximately(h, _lastContainerH) && Mathf.Approximately(w, _lastContainerW)) return;
            _lastContainerH = h;
            _lastContainerW = w;
            RefreshCharacterLayout();
        }

        void RefreshCharacterLayout()
        {
            if (_characterSlots.Count == 0) return;
            var containerRT = (RectTransform)charactersContainer;
            float containerH = containerRT.rect.height;
            float containerW = containerRT.rect.width;
            if (containerH <= 0 || containerW <= 0) return;

            var hlg = charactersContainer.GetComponent<HorizontalLayoutGroup>();
            float spacing = hlg != null ? hlg.spacing : 0f;
            float padH = hlg != null ? hlg.padding.left + hlg.padding.right : 0f;
            float totalGaps = padH + spacing * Mathf.Max(0, _characterSlots.Count - 1);
            float maxSlotW = (containerW - totalGaps) / _characterSlots.Count;

            for (int i = 0; i < _characterSlots.Count; i++)
            {
                var tex = _characterSlots[i].texture;
                if (tex == null || tex.height == 0) continue;
                float aspect = (float)tex.width / tex.height;
                float slotW = Mathf.Min(containerH * aspect, maxSlotW);
                float slotH = slotW / aspect;
                var le = _characterSlots[i].GetComponent<LayoutElement>();
                if (le != null)
                {
                    le.preferredWidth = slotW;
                    le.preferredHeight = slotH;
                }
            }
        }

        //----------------------------------------------------------------------

        void OnElementEnter(Element e)
        {
            UpdateBackground(e);
            UpdateCharacters(e);
            UpdateDialogue(e);
        }

        void UpdateBackground(Element e)
        {
            var tex = e.GetCoverImage();
            switch (backgroundFadeMode)
            {
                case FadeMode.None:
                    background.texture = tex;
                    SetAlpha(background, tex != null ? 1f : 0f);
                    break;
                case FadeMode.FadeAlpha:
                    if (tex != null) { background.texture = tex; FadeIn(background, backgroundFadeTime); }
                    else FadeOut(background, backgroundFadeTime);
                    break;
                case FadeMode.Overlay:
                    StartCoroutine(OverlayTransition(() =>
                    {
                        background.texture = tex;
                        SetAlpha(background, tex != null ? 1f : 0f);
                    }, backgroundFadeTime));
                    break;
            }
        }

        void UpdateCharacters(Element e)
        {
            var comps = e.Components;

            // Build list of textures for components that have a cover image
            var textures = new List<Texture2D>();
            if (comps != null)
            {
                foreach (var c in comps)
                {
                    var tex = c.GetCoverImage();
                    if (tex != null) textures.Add(tex);
                }
            }

            // Grow pool
            while (_characterSlots.Count < textures.Count)
            {
                var img = Instantiate(characterTemplate, charactersContainer);
                img.gameObject.SetActive(true);
                _characterSlots.Add(img);
            }

            // Shrink pool — destroy excess slots
            while (_characterSlots.Count > textures.Count)
            {
                var last = _characterSlots[_characterSlots.Count - 1];
                _characterSlots.RemoveAt(_characterSlots.Count - 1);
                Destroy(last.gameObject);
            }

            for (int i = 0; i < textures.Count; i++)
            {
                var slot = _characterSlots[i];
                if (slot.texture == textures[i]) continue;
                slot.texture = textures[i];
                switch (characterFadeMode)
                {
                    case FadeMode.None:    SetAlpha(slot, 1f); break;
                    case FadeMode.FadeAlpha: FadeIn(slot, characterFadeTime); break;
                    case FadeMode.Overlay:
                        StartCoroutine(OverlayTransition(() => SetAlpha(slot, 1f), characterFadeTime));
                        break;
                }
            }

            _lastContainerH = -1f; // force RefreshCharacterLayout on next LateUpdate
            RefreshCharacterLayout();
        }

        void UpdateDialogue(Element e)
        {
            var comps = e.Components;
            var speaker = comps != null && comps.Count > 0 ? comps[0].Name : string.Empty;
            speakerName.text = speaker;
            speakerName.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(speaker));

            if (_typewriterCoroutine != null) { StopCoroutine(_typewriterCoroutine); _typewriterCoroutine = null; }
            _typewriterDone = false;
            _pendingNextCallback = null;
            if (continueArrow != null) continueArrow.SetActive(false);

            if (e.HasContent())
            {
                e.RunContentScript();
                dialogueText.text = e.RuntimeContent;
            }
            else
            {
                dialogueText.text = string.Empty;
            }

            SetAlpha(dialogueText, 1f);
            _typewriterCoroutine = StartCoroutine(TypewriterRoutine());
        }

        //----------------------------------------------------------------------

        void OnElementOptions(Options options, System.Action<int> callback)
        {
            for (int i = 0; i < options.Paths.Count; i++)
            {
                var index = i;
                var label = !string.IsNullOrEmpty(options.Paths[i].label)
                    ? options.Paths[i].label
                    : "<i>[ N/A ]</i>";
                SpawnChoiceButton(label, () => callback(index));
            }
        }

        void OnWaitInputNext(System.Action callback)
        {
            _pendingNextCallback = callback;
            // Arrow shown only after typewriter finishes (FinishTypewriter checks this)
            if (_typewriterDone && continueArrow != null)
                continueArrow.SetActive(true);
        }

        void OnProjectFinish(Project.Project p)
        {
            SpawnChoiceButton("Restart", player.PlayProject);
        }

        Button SpawnChoiceButton(string label, System.Action onClick)
        {
            var btn = Instantiate(choiceButtonTemplate, choiceButtonTemplate.transform.parent);
            _choiceButtons.Add(btn);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = label;
            btn.gameObject.SetActive(true);

            var btnGraphic = btn.GetComponent<Graphic>();
            if (btnGraphic != null) FadeIn(btnGraphic, characterFadeTime);

            btn.onClick.AddListener(() =>
            {
                ClearChoiceButtons();
                onClick();
            });
            return btn;
        }

        void ClearChoiceButtons()
        {
            foreach (var b in _choiceButtons) Destroy(b.gameObject);
            _choiceButtons.Clear();
        }

        //----------------------------------------------------------------------

        void FadeIn(Graphic g, float duration) => StartCoroutine(FadeRoutine(g, 0f, 1f, duration));
        void FadeOut(Graphic g, float duration) => StartCoroutine(FadeRoutine(g, g.color.a, 0f, duration));

        IEnumerator FadeRoutine(Graphic g, float from, float to, float duration)
        {
            if (g == null) yield break;
            SetAlpha(g, from);
            if (duration <= 0f) { SetAlpha(g, to); yield break; }
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                SetAlpha(g, Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }
            SetAlpha(g, to);
        }

        static void SetAlpha(Graphic g, float a)
        {
            var c = g.color;
            c.a = a;
            g.color = c;
        }

        IEnumerator OverlayTransition(System.Action swapContent, float totalDuration)
        {
            if (transitionOverlay == null) { swapContent(); yield break; }
            float half = totalDuration * 0.5f;
            yield return FadeRoutine(transitionOverlay, 0f, 1f, half);
            swapContent();
            yield return FadeRoutine(transitionOverlay, 1f, 0f, half);
        }
    }
}
