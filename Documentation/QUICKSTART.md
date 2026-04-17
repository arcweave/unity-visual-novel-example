# Quick Start

Get from a fresh project to a running visual novel in 5 steps.

## Prerequisites

- Unity 2022.3.62f3 or later
- TextMeshPro package installed (Window > Package Manager)
- An Arcweave project exported as `project.json`

---

## Step 1 — Import your Arcweave project

Replace `Assets/Arcweave/project.json` with your own export from Arcweave.

In the Project window, select `Assets/Arcweave/Arcweave Project Asset.asset` and confirm it shows your boards and elements in the Inspector. If it does not update automatically, click **Import**.

---

## Step 2 — Add your images to Resources

All images referenced in Arcweave must exist in `Assets/Resources/` with the **exact same filename** (without extension) as used in Arcweave.

Example: if an element cover is named `classroom.png` in Arcweave, place `classroom.png` inside `Assets/Resources/`.

---

## Step 3 — Add the Visual Novel UI prefab to your scene

The prefab is already built at `Assets/Arcweave/Demo/@ArcweavePlayerUI.prefab`. Its hierarchy:
- `Background` — fullscreen background image
- `CharacterLayer` — horizontal row where character sprites appear
- `DialoguePanel` — bottom panel with speaker name and dialogue text
- `ChoicesPanel` — choice buttons spawned above the dialogue box
- `QuickMenu` — Save / Load buttons (top-right)

---

## Step 4 — Set up the scene

1. Open `Assets/Arcweave/Demo/ArcweaveDemoScene.unity` (or create a new scene).
2. Drag `Assets/Arcweave/Demo/@ArcweavePlayerUI.prefab` into the scene.
3. In the scene hierarchy, find or create a **GameObject** and add the `ArcweavePlayer` component to it.
4. Assign `Assets/Arcweave/Arcweave Project Asset.asset` to the **Aw** field of `ArcweavePlayer`.
5. Select the `@ArcweavePlayerUI` object in the hierarchy and assign the `ArcweavePlayer` GameObject to its **Player** field.

---

## Step 5 — Play

Press Play. The visual novel starts from the element marked as *Starting Element* in Arcweave.

---

## Adjusting the layout

The `CharacterLayer` uses a `HorizontalLayoutGroup`. Characters are spawned and distributed automatically left-to-right based on the order of components in each Arcweave element. To change character size, select the `CharacterTemplate` child inside `CharacterLayer` and adjust the `LayoutElement` preferred width or the `AspectRatioFitter` aspect ratio.

The `DialoguePanel`, `CharacterLayer`, and `ChoicesPanel` use anchor-based layout. You can resize them freely in the Scene view without code changes.
