# Arcweave Unity Visual Novel Template

A ready-to-use **visual novel template** for Unity, built on top of the
[Arcweave](https://arcweave.com) narrative engine. Drop in your story, your
character art, your music — and you have a working VN with backgrounds,
characters, dialogue, choices, save / load, dialogue history, and a main menu.

> Built on Unity **2022.3 LTS** (URP / Built-in compatible — uses only UGUI).

---

## Contents

- [What you get](#what-you-get)
- [Quick start](#quick-start)
- [Project structure](#project-structure)
- [How the story flows](#how-the-story-flows)
- [Visual conventions in Arcweave](#visual-conventions-in-arcweave)
- [Scripts overview](#scripts-overview)
- [Save / load system](#save--load-system)
- [Audio](#audio)
- [Setting up your own story](#setting-up-your-own-story)
- [Common issues](#common-issues)
- [Extending the template](#extending-the-template)

---

## What you get

| Feature                  | Where it lives                                                                  |
|--------------------------|---------------------------------------------------------------------------------|
| Narrative engine         | `Assets/Arcweave/Plugin/` (the official Arcweave Unity plugin)                  |
| Player / state machine   | `Assets/Scripts/ArcweavePlayer.cs`                                              |
| In-game UI (VN view)     | `Assets/Scripts/VisualNovelUI.cs` + `Assets/Prefabs/@ArcweavePlayerUI`          |
| Save / load (3 slots)    | `Assets/Scripts/SaveMenuUI.cs`, `SaveSlotEntry.cs`                              |
| Dialogue history (log)   | `Assets/Scripts/DialogueLogUI.cs`                                               |
| Audio (loop / one-shot)  | `Assets/Scripts/AudioManager.cs`                                                |
| Mute toggle button       | `VisualNovelUI` → `muteButton` field (optional, wired in the prefab)            |
| Main menu                | `Assets/Scripts/MainMenuController.cs` + `Assets/Scenes/MainMenuScene`          |
| Demo story               | `Assets/Arcweave/project.json` + `Assets/Arcweave/Arcweave Project Asset.asset` |
| Demo art / audio         | `Assets/Resources/` (characters, backgrounds, BGM)                              |

---

## Quick start

1. Clone the repo and open it in **Unity 2022.3 LTS** (older versions may work, untested).
2. Open **`Assets/Scenes/MainMenuScene.unity`**.
3. Hit **Play**. New Game starts the demo story; Continue / Load uses the save slots.

The demo story (a short slice-of-life set in a Japanese high school) runs
end-to-end with backgrounds, characters, music, and choices.

---

## Project structure

```
Assets/
├── Arcweave/
│   ├── Plugin/                      Arcweave runtime + interpreter (do not edit)
│   ├── project.json                 Story exported from Arcweave (replace with your own)
│   └── Arcweave Project Asset.asset ScriptableObject that imports project.json
├── Resources/                       Runtime-loaded images & audio (names referenced by Arcweave)
├── Prefabs/
│   └── @ArcweavePlayerUI            Visual novel UI prefab (Canvas, dialogue, choices, menus)
├── Scenes/
│   ├── MainMenuScene                Title screen — New Game / Continue / Load / Quit
│   └── VisualNovelScene             Game scene — prefab + ArcweavePlayer + AudioManager
├── Scripts/                         All custom MonoBehaviours
│   └── Editor/                      Editor-only utilities
└── UI/
    ├── Fonts/                       Barlow Condensed TMP SDF assets
    └── GUI/                         Sprite assets for buttons / textboxes / nameplate
```

---

## How the story flows

The Arcweave plugin parses `project.json` into a graph of **Boards → Elements → Connections**.
At runtime, `ArcweavePlayer` walks this graph one element at a time:

```
ProjectMaker → Project (boards, elements, connections, variables)
                    ↓
            ArcweavePlayer  (state machine)
                    ↓ (events)
            VisualNovelUI   (renders to screen)
```

`ArcweavePlayer` exposes events that the UI subscribes to:

| Event              | Fires when …                                                      |
|--------------------|-------------------------------------------------------------------|
| `onProjectStart`   | A new run begins (New Game or load)                               |
| `onElementEnter`   | The player advances to a new element                              |
| `onWaitInputNext`  | The element has a single outgoing path → wait for click / Space   |
| `onElementOptions` | The element has multiple outgoing paths → show choice buttons     |
| `onProjectFinish`  | Reached an element with no outgoing paths                         |
| `onBeforeLoad`     | About to restore a save (used to clear UI state)                  |

---

## Visual conventions in Arcweave

The template maps Arcweave structure to visuals with no custom plugin work — just follow these rules:

| In Arcweave                   | Renders as in Unity                                  |
|-------------------------------|------------------------------------------------------|
| **Element cover**             | Background image for that scene                      |
| **Component cover** (ordered) | Character sprites, stacked left-to-right             |
| First **Component name**      | Speaker nameplate (above dialogue text)              |
| Element **Content**           | Dialogue text (rich-text + Arcscript variables)      |
| Outgoing **Connections**      | Choice buttons (auto-advance if only one)            |
| Connection **label**          | Choice button text; empty label → `[ N/A ]` italic   |
| **Audio assets** on element   | Music / SFX triggered on enter (Once / Loop / Stop)  |

### Image naming

Unity loads images from `Assets/Resources/` by filename without extension.
Keep names consistent between Arcweave and the Resources folder:

```
maya_normal.png   → "maya_normal" in Arcweave
maya_smile.png    → "maya_smile"
alex_normal.png   → "alex_normal"
```

Change a component's cover image between elements to switch character expressions.

### Character staging tips

- **Remove a character from stage** — detach their component from the element.
- **Change position** — reorder components in the element; leftmost component = leftmost sprite.
- A component with no cover image is silently skipped (no slot reserved).
- Characters always distribute left-to-right; there is no explicit slot assignment.

### Arcscript and variables

Use Arcweave's built-in **Arcscript** in element content and connection labels as normal.
All variables are automatically saved and restored by the Save / Load system.

---

## Scripts overview

All custom scripts live in `Assets/Scripts/` and are in the `Arcweave` namespace.

### `ArcweavePlayer.cs`
The narrative state machine. Responsibilities:
- Initialize the project and walk the element graph (`PlayProject`, `Next`)
- Track the current element + bump its `Visits` counter
- Fire events for the UI to react to
- Save / load to PlayerPrefs (3 slots)

### `VisualNovelUI.cs`
The view layer. Subscribes to `ArcweavePlayer` events and updates:
- `background` (RawImage) ← element cover
- `charactersContainer` (HorizontalLayoutGroup) ← one RawImage per component cover, sized to fit
- `speakerName` (TMP) ← first component's name
- `dialogueText` (TMP) ← element content, with typewriter effect (Space / Return / click to skip)
- Choice buttons spawned from `choiceButtonTemplate`
- Save / Load / Back-to-Menu / **Mute** buttons in the QuickMenu
  (Mute is optional — assign `muteButton` and `audioManager` in the Inspector)

Configurable in the Inspector: typewriter speed, fade modes (None / FadeAlpha /
Overlay) and durations for backgrounds and characters, scene name to return to.

### `SaveMenuUI.cs` + `SaveSlotEntry.cs`
A 3-slot save / load popup. Opens via `OpenForSave()` / `OpenForLoad()`.
Shows label + timestamp per slot. Reused in both the in-game UI and the main menu
(in the menu, set the `loadOverride` callback to switch scenes instead of restoring in place).

### `DialogueLogUI.cs`
A scrollable history of every dialogue line and choice the player has seen.
Toggle open / close via a button. Cleared automatically before a save is loaded.

### `AudioManager.cs`
Hooks `ArcweavePlayer.onElementEnter` and processes the element's `AudioAsset[]`:
- **Once**: one-shot via shared `AudioSource.PlayOneShot`
- **Loop**: dedicated `AudioSource` per asset, volume cross-faded in
- **Stop**: fades out and disposes the dedicated source

Includes ContextMenu helpers (`Stop All`, `Pause All`, `Resume All`),
`SetMasterVolume(float)`, and `ToggleMute()` (remembers the previous volume on unmute).

### `MainMenuController.cs`
Title screen. New Game / Continue / Load / Quit. Continue and Load are disabled
when no save exists. Continue jumps to the most recent slot; Load opens the
SaveMenu so the player can pick a slot.

### `GameBootstrap.cs`
A static class that carries two flags across scene loads:
- `ShouldLoadSave` — true when the menu wants the game scene to restore a save
- `LoadSlot` — which slot to restore

`ArcweavePlayer.Start()` reads these in the game scene and acts accordingly.

---

## Save / load system

Each save slot is 4 PlayerPrefs keys, prefixed `arcweave_save_<slot>_`:

| Key               | Value                                                    |
|-------------------|----------------------------------------------------------|
| `_currentElement` | Element UUID                                             |
| `_variables`      | JSON-encoded variable state (Arcweave's own format)      |
| `_label`          | Element title or first component name (for slot UI)      |
| `_time`           | ISO 8601 timestamp (lex-sortable for "most recent")      |

Slots are zero-indexed; default count is `ArcweavePlayer.SAVE_SLOTS = 3`. To
change it, edit the constant and update the SaveMenu prefab to have N slots.

---

## Audio

Audio is driven entirely by Arcweave — attach audio assets to elements and
`AudioManager` handles the rest. Nothing to call in code.

| Mode   | Behaviour                                                        |
|--------|------------------------------------------------------------------|
| `Once` | Plays the clip once (fire-and-forget)                            |
| `Loop` | Starts a looping track, cross-fading in over `fadeDuration`      |
| `Stop` | Fades out and stops the track matching that asset name           |

To add music or SFX:
1. Drop the file into `Assets/Resources/` (mp3, wav, ogg — anything Unity supports).
2. In Arcweave, attach an audio asset to the element with the filename matching
   the Resources file (no extension).
3. Set the mode: `Once`, `Loop`, or `Stop`.

Volume per asset is set in Arcweave. `AudioManager.masterVolume` is a global
multiplier you can wire to a settings slider. The **Mute** button calls
`ToggleMute()` and restores the previous volume on unmute.

---

## Setting up your own story

### 1 — Import your Arcweave project

Replace `Assets/Arcweave/project.json` with your export, then select
`Assets/Arcweave/Arcweave Project Asset.asset` and click **Import** if it
doesn't refresh automatically.

Alternatively use **Import from Web** in the asset Inspector with your
Arcweave API key + project hash to pull the latest version without a manual export.

### 2 — Add your assets to Resources

All images and audio referenced in Arcweave must exist in `Assets/Resources/`
with the **exact same filename** (without extension). Case-sensitive.

### 3 — Wire the scene

1. Open `Assets/Scenes/VisualNovelScene.unity` (or create a new scene).
2. Drag `Assets/Prefabs/@ArcweavePlayerUI.prefab` into the scene.
3. Create a GameObject and add the `ArcweavePlayer` component.
4. Assign `Assets/Arcweave/Arcweave Project Asset.asset` to its **Aw** field.
5. Select `@ArcweavePlayerUI` and assign the `ArcweavePlayer` to its **Player** field.
6. *(Optional)* Add `AudioManager` to any GameObject, assign `ArcweavePlayer` to its
   **Player** field, then wire it to `VisualNovelUI`'s **Audio Manager** and **Mute Button** fields.

### 4 — Build Settings

Add both `MainMenuScene` and `VisualNovelScene` to **File → Build Settings → Scenes In Build**.

### 5 — Adjust layout

The `CharacterLayer` uses a `HorizontalLayoutGroup` — characters distribute
automatically. Resize `DialoguePanel`, `CharacterLayer`, and `ChoicesPanel`
freely in the Scene view using anchors; no code changes needed.

---

## Common issues

**Buttons don't respond to hover or clicks.**
- Each scene needs its own `EventSystem` (GameObject → UI → Event System).
- For ColorTint transitions, `Image.color` on the Button must be **white**,
  otherwise tint colors multiply by black and show no state change.

**Audio not playing.**
- Confirm `AudioManager` is in the scene with `Player` assigned.
- Verify the filename in Arcweave matches a file in `Assets/Resources/` (case-sensitive, no extension).
- Check `Active Sources` in the `AudioManager` Inspector at runtime — empty means the asset name is wrong.

**Mute button does nothing.**
- Assign both `muteButton` and `audioManager` in the `VisualNovelUI` Inspector.
  Both are optional; if either is missing the button is ignored.

**Continue / Load disabled.**
- Correct behavior when no save exists. Start a New Game, save, return to menu.

**Scene change errors.**
- Add both `MainMenuScene` and `VisualNovelScene` to **File → Build Settings → Scenes In Build**.

**Choice buttons all look the same.**
- For Sprite Swap: assign distinct sprites to Highlighted / Pressed / Selected states.
- For Color Tint: see the white `Image.color` note above.

---

## Extending the template

- **Settings menu** — wire a Slider's `OnValueChanged` to `AudioManager.SetMasterVolume`;
  the Mute button already uses `ToggleMute()` and updates its own label.
- **Auto-advance** — when `_typewriterDone` and `_pendingNextCallback != null` in
  `VisualNovelUI`, start a coroutine that auto-clicks after N seconds.
- **Skip read text** — `Element.Visits > 0` means it's been seen; speed up the typewriter.
- **Localization** — Arcweave supports locales natively via `ArcweaveProjectAsset.locale`.
- **Custom UI** — swap `VisualNovelUI` with your own MonoBehaviour that subscribes to
  the same `ArcweavePlayer` events.

---

## Credits

- [Arcweave](https://arcweave.com) — narrative authoring tool & official Unity plugin
- [Barlow Condensed](https://fonts.google.com/specimen/Barlow+Condensed) — UI font (SIL OFL)
- Demo story, art, and music: bundled for example purposes
