# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 2022.3.62f3 visual novel built on the **Arcweave** narrative engine. Arcweave is an external story-authoring tool that exports a `project.json` file consumed by the Unity plugin.

## Development Commands

There is no build CLI for this project — all builds and test runs go through the Unity Editor. Open the project in Unity Hub targeting Unity **2022.3.62f3**, then use the Editor's Build Settings or Play Mode. There are no unit tests configured.

To update narrative content: replace `Assets/Arcweave/project.json` with a fresh export from Arcweave (or re-import via `ArcweaveProjectAsset` with `ImportSource.FromWeb`).

## Architecture

### Data flow

```
Assets/Arcweave/project.json
        ↓  (parsed by)
ProjectMaker.cs  →  Project (graph of Boards, Elements, Connections, Variables)
        ↓  (driven by)
ArcweavePlayer.cs  (MonoBehaviour — narrative controller)
        ↓  (events)
VisualNovelUI.cs  (MonoBehaviour — visual novel view layer)
```

### Custom scripts (`Assets/Arcweave/Demo/`)

- **`ArcweavePlayer.cs`** — narrative state machine. Calls `Project.Initialize()`, advances via `Next(Element)` / `Next(Path)`, saves/loads state with `PlayerPrefs` (`arcweave_save_currentElement`, `arcweave_save_variables`). Fires Unity events: `onProjectStart`, `onProjectFinish`, `onElementEnter`, `onElementOptions`, `onWaitInputNext`.
- **`VisualNovelUI.cs`** — visual novel view layer (renamed from `ArcweavePlayerUI.cs`, same GUID preserved). Subscribes to `ArcweavePlayer` events and updates:
  - `background` (RawImage) ← `element.GetCoverImage()` (element cover = scene background)
  - `CharacterLayer` — instantiates/destroys `RawImage` children from a `characterTemplate`, one per component with a cover, left-to-right in component list order. Uses `HorizontalLayoutGroup` so any number of characters distributes automatically.
  - `speakerName` (TextMeshProUGUI) ← `element.Components[0].Name` (first component = speaker)
  - `dialogueText` (TextMeshProUGUI) ← `element.RuntimeContent`
  - Choice buttons and Save/Load wired via `SpawnChoiceButton`. All text uses **TextMeshPro**.

> Note: `Arcweave.Project.Component` clashes with `UnityEngine.Component` — always use `var` in foreach loops over `element.Components` to avoid the ambiguity error.

### Prefab hierarchy (`Assets/Arcweave/Demo/@ArcweavePlayerUI.prefab`)

```
@ArcweavePlayerUI  (Canvas 1920×1080 ScaleWithScreenSize, VisualNovelUI script)
├── Background         RawImage  anchors 0,0 → 1,1  (scene background)
├── CharacterLayer     HorizontalLayoutGroup  anchors 0,0.3 → 1,1
│   └── CharacterTemplate  RawImage + LayoutElement + AspectRatioFitter  (inactive, pool template)
├── DialoguePanel      Image (dark)  anchors 0,0 → 1,0.3
│   ├── NameplateContainer  Image  (hidden when no speaker)
│   │   └── SpeakerName   TextMeshProUGUI
│   └── DialogueText       TextMeshProUGUI
├── ChoicesPanel       VerticalLayoutGroup + ContentSizeFitter  (pivot bottom, anchored at y=0.3)
│   └── ChoiceButton   Button + TextMeshProUGUI  (inactive, spawn template)
└── QuickMenu          HorizontalLayoutGroup  top-right corner
    ├── Save           Button + TextMeshProUGUI
    └── Load           Button + TextMeshProUGUI
```

After dragging the prefab into a scene, assign the **ArcweavePlayer** GameObject to the `Player` field on the `VisualNovelUI` component — this is the only manual wiring required.

### VN conventions (Arcweave side)

- **Element cover** → background image for that scene.
- **Component cover (ordered)** → character sprites stacked left-to-right. A component without a cover is silently skipped.
- **First component name** → speaker nameplate. If no component is attached, nameplate is hidden.
- Image filenames in Arcweave must match filenames in `Assets/Resources/` (without extension).

### Arcweave plugin (`Assets/Arcweave/Plugin/Runtime/`)

**Project/** — core domain model:
- `Project.cs` — root container; holds boards, components, variables; owns `Initialize()` and `SetVariable()`.
- `Element.cs` — a story node. `RunContentScript()` executes Arcscript to produce `RuntimeContent`; `GetOptions()` evaluates outgoing connections and returns an `Options` object.
- `Connection.cs` — directed edge; `ResolvePath()` / `RunLabelScript()` execute Arcscript labels on traversal.
- `Branch.cs` / `Condition.cs` — conditional fork; `Branch.GetTrueCondition()` returns the first condition whose Arcscript evaluates truthy.
- `Jumper.cs` — unconditional redirect to another `Element`.
- `Component.cs` / `Attribute.cs` — reusable metadata (characters, locations) attached to elements.
- `Variable.cs` — typed game-state variable (int/double/bool/string) with type-prefixed serialization (`i/d/b/s` prefix).
- `ProjectMaker.cs` — deserializes `project.json` (via Full Serializer) into the full graph.

**Interpreter/** — Arcscript execution:
- `AwInterpreter.cs` / `ArcscriptTranspiler.cs` — entry point; returns `TranspilerOutput` (rendered text, variable changes, boolean result).
- `ArcscriptVisitor.cs` — ANTLR4 tree visitor that executes narrative sections, assignments, and conditionals.
- `ArcscriptState.cs` — holds mutable interpreter state (variable reads/writes, output accumulation).
- `ArcscriptFunctions.cs` — built-ins: `sqrt`, `abs`, `random`, `roll`, `show`, `reset`, `resetAll`, `visits`, `min`, `max`, `round`.
- `ArcscriptOutputs.cs` — output node tree (Paragraph, Blockquote); converts HTML to Unity rich text.
- `Utils.cs` — maps `<strong>`→`<b>`, `<em>`→`<i>`, `<code>`→colored span.
- `Generated/` — ANTLR4-generated lexer/parser; do not edit by hand.

**INodes/** — interfaces: `INode` (Id, Project, ResolvePath), `IElement`, `IProject`, `IVariable`, `IBoard`, `IComponent`, `IAttribute`, `IConnection`, `IHasAttributes`. All node types implement `INode`; polymorphic `ResolvePath()` drives navigation.

### Path resolution (how choices work)

1. `Element.GetOptions()` iterates `Outputs` (connections). Each connection calls `RunLabelScript()` — variable changes execute; the label becomes the button text.
2. Branches and Jumpers implement `ResolvePath()` transparently, so the caller always receives an `Element` target.
3. `Options` carries `Paths[]` (each `Path` has a label and target `Element`). If only one path exists, `ArcweavePlayer` auto-advances; otherwise it fires `onElementOptions` so the UI can present buttons.

### Assets

Runtime images and audio live in `Assets/Resources/` and are loaded via `Resources.Load<T>()` using the file name stored in each `Cover` object. Characters: `alex`, `jake`, `maya` (normal/smile variants). Locations: `classroom`, `home`, `school`, `sunset`, `festival`, `ending`. BGM: `home`, `school`, `festival`, `ending`.

### Save system

`ArcweavePlayer.Save()` writes two `PlayerPrefs` keys:
- `arcweave_save_currentElement` — element UUID string
- `arcweave_save_variables` — JSON blob of typed variable values

`Load()` restores both; the variable JSON uses the same type-prefix encoding as `Variable.cs`.
