# Writer Guide — Arcweave Visual Novel Conventions

This guide explains how to structure your Arcweave project so it renders correctly as a visual novel in Unity.

---

## Element cover = background

Assign a cover image to an element to set the **background** for that scene.

- The image must exist in Unity's `Assets/Resources/` folder with the same filename.
- If an element has no cover, the previous background remains on screen.
- Supported: `.png`, `.jpg` (any format Unity can load as `Texture2D`).

---

## Components = characters on stage

Each **component attached to an element** represents a character present in that scene. Their cover images appear as character sprites, stacked left-to-right in the order they are listed.

| Component position in list | Visual position on screen |
|---|---|
| First | Leftmost |
| Second | Next to the right |
| … | … |

**Tips:**
- Detach a component from an element to remove that character from stage.
- Reorder components to change who appears where.
- A component without a cover image is silently skipped (it will not occupy a slot).

---

## First component = speaker (nameplate)

The **first component** attached to an element determines whose name appears in the nameplate above the dialogue box.

- Use the component's **Name** field in Arcweave as the character's display name.
- If no component is attached, the nameplate is hidden.
- The order of components matters: whichever is listed first is considered the speaker.

---

## Image naming

Images in Arcweave are referenced by filename. Unity loads them from `Assets/Resources/` using the filename without extension.

Recommended naming for character poses:
```
maya_normal.png
maya_smile.png
alex_angry.png
```

Change the component's cover image in Arcweave to switch a character's expression between elements.

---

## Variables and scripting (Arcscript)

Use Arcweave's **Arcscript** inside element content and connection labels as normal. Variables are automatically saved and loaded by the Save/Load system.

---

## What is NOT supported (MVP)

- Audio triggered from elements (post-MVP)
- Character position overrides (characters always stack left-to-right)
- Typewriter / auto-advance mode (post-MVP)
- Multiple save slots (post-MVP)
