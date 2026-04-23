using UnityEngine;
using Arcweave.Project;

namespace Arcweave
{
    public class ArcweavePlayer : MonoBehaviour
    {
        public delegate void OnProjectStart(Project.Project project);
        public delegate void OnProjectFinish(Project.Project project);
        public delegate void OnElementEnter(Element element);
        public delegate void OnElementOptions(Options options, System.Action<int> next);
        public delegate void OnWaitingInputNext(System.Action next);

        public const string SAVE_KEY = "arcweave_save";
        public const int SAVE_SLOTS = 3;

        public Arcweave.ArcweaveProjectAsset aw;
        public bool autoStart = true;

        private Element currentElement;

        public event OnProjectStart onProjectStart;
        public event OnProjectFinish onProjectFinish;
        public event OnElementEnter onElementEnter;
        public event OnElementOptions onElementOptions;
        public event OnWaitingInputNext onWaitInputNext;
        public event System.Action onBeforeLoad;

        void Start()
        {
            if (!autoStart) return;
            if (GameBootstrap.ShouldLoadSave)
            {
                GameBootstrap.ShouldLoadSave = false;
                LoadFromSlot(GameBootstrap.LoadSlot);
            }
            else
            {
                PlayProject();
            }
        }

        public void PlayProject()
        {
            if (aw == null)
            {
                Debug.LogError("There is no Arcweave Project assigned in the inspector of Arcweave Player");
                return;
            }
            aw.Project.Initialize();
            if (onProjectStart != null) onProjectStart(aw.Project);
            Next(aw.Project.StartingElement);
        }

        void Next(Path path)
        {
            path.ExecuteAppendedConnectionLabels();
            Next(path.TargetElement);
        }

        void Next(Element element)
        {
            currentElement = element;
            currentElement.Visits++;
            if (onElementEnter != null) onElementEnter(element);
            var currentState = currentElement.GetOptions();
            if (currentState.hasPaths)
            {
                if (currentState.hasOptions)
                {
                    if (onElementOptions != null)
                        onElementOptions(currentState, (index) => Next(currentState.Paths[index]));
                    return;
                }
                if (onWaitInputNext != null) onWaitInputNext(() => Next(currentState.Paths[0]));
                return;
            }
            currentElement = null;
            if (onProjectFinish != null) onProjectFinish(aw.Project);
        }

        // -------------------------------------------------------------------------

        public void SaveToSlot(int slot)
        {
            if (currentElement == null) return;
            var prefix = SlotKey(slot);
            PlayerPrefs.SetString(prefix + "_currentElement", currentElement.Id);
            PlayerPrefs.SetString(prefix + "_variables", aw.Project.SaveVariables());
            var label = !string.IsNullOrEmpty(currentElement.Title)
                ? currentElement.Title
                : (currentElement.Components != null && currentElement.Components.Count > 0
                    ? currentElement.Components[0].Name
                    : currentElement.Id);
            PlayerPrefs.SetString(prefix + "_label", label);
            // ISO 8601 string is lexicographically sortable
            PlayerPrefs.SetString(prefix + "_time", System.DateTime.Now.ToString("s"));
            PlayerPrefs.Save();
        }

        public void LoadFromSlot(int slot)
        {
            var prefix = SlotKey(slot);
            if (!PlayerPrefs.HasKey(prefix + "_currentElement")) return;
            var id = PlayerPrefs.GetString(prefix + "_currentElement");
            var variables = PlayerPrefs.GetString(prefix + "_variables");
            var element = aw.Project.ElementWithId(id);
            if (element == null) return;

            if (onBeforeLoad != null) onBeforeLoad();
            aw.Project.Initialize();
            aw.Project.LoadVariables(variables);
            if (onProjectStart != null) onProjectStart(aw.Project);
            Next(element);
        }

        public bool HasSaveInSlot(int slot) =>
            PlayerPrefs.HasKey(SlotKey(slot) + "_currentElement");

        public (string label, string time) GetSlotInfo(int slot)
        {
            var prefix = SlotKey(slot);
            return (
                PlayerPrefs.GetString(prefix + "_label", string.Empty),
                PlayerPrefs.GetString(prefix + "_time", string.Empty)
            );
        }

        public static bool HasAnySave()
        {
            for (int i = 0; i < SAVE_SLOTS; i++)
                if (PlayerPrefs.HasKey(SAVE_KEY + "_" + i + "_currentElement")) return true;
            return false;
        }

        public static int MostRecentSlot()
        {
            int best = 0;
            string bestTime = string.Empty;
            for (int i = 0; i < SAVE_SLOTS; i++)
            {
                var key = SAVE_KEY + "_" + i + "_time";
                if (!PlayerPrefs.HasKey(key)) continue;
                var t = PlayerPrefs.GetString(key);
                if (string.CompareOrdinal(t, bestTime) > 0) { bestTime = t; best = i; }
            }
            return best;
        }

        static string SlotKey(int slot) => SAVE_KEY + "_" + slot;
    }
}
