# Unity Dialog Maker

A custom Unity Editor tool for creating branching dialog as connected nodes and exporting/importing it as JSON.

## What It Does

This project adds a node-based dialog editor window inside Unity:

- Create dialog nodes with a title and body text.
- Connect nodes to define dialog flow.
- Mark and keep a non-deletable start node.
- Save dialogs to JSON files.
- Load existing JSON dialogs back into the editor.

The main editor window is implemented in `Assets/Editor/DialogWriterWIndow.cs` and node behavior/data is in `Assets/Editor/DialogNode.cs`.

## How To Open

In Unity, open:

`Custom Tools > Dialog Writer`

## Basic Workflow

1. Open the Dialog Writer window.
2. Click `New Dialog` to start with a default start node.
3. Right-click empty space to add a dialog node.
4. Right-click a node for options:
   - `Add Connected Node`
   - `Add New Connection`
   - `Disconnect From ...`
   - `Remove Node` (disabled for start node)
5. Enter a file name in the top bar and click `Save Dialog`.
6. Select a file from the dropdown and click `Load Dialog` to edit an existing dialog.

## Dialog File Location

Dialog JSON files are saved under:

`Assets/Dialogs/`

Examples already in the project:

- `Assets/Dialogs/default.json`
- `Assets/Dialogs/SampleConversation.json`

## JSON Structure

Each dialog file is a JSON array of nodes. Every node contains:

- `title` (string)
- `fullText` (string)
- `id` (int)
- `position` (`x`, `y`)
- `isStarting` (bool)
- `isDeletable` (bool)
- `connectedNodes` (array of target node ids)

Minimal example:

```json
[
  {
    "title": "Start Node",
    "fullText": "Hello",
    "id": 0,
    "position": { "x": 200.0, "y": 200.0 },
    "isStarting": true,
    "isDeletable": false,
    "connectedNodes": []
  }
]
```

## Notes

- Connections are stored as node ids (`connectedNodes`).
- The start node is created automatically for a new dialog.
- This repository currently focuses on editor-side authoring and JSON persistence.
