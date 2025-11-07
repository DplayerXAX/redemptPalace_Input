# redemptPalace_Input (Unity I/O Demo Project)

This is a demo project prepared for Intermediate Game Development to teach **Unity Input/Output system**.

This game is *NOT* a finished project. This means some gameplay systems and codes are intentionally simplified or removed.  
The focus of this lesson is solely on **data I/O**, not the complete narrative system or full gameplay.

> Think like you are on a team, and you are assigned ONLY the job to handle **data reading / writing**.

---

## Repository rules

You cannot and are not allowed to commit or push directly to the main branch.
Making local changes is still fine though, and if you somehow want to contribute you should create your own branch or fork the repo instead.

---

## Why learning I/O matters in Unity?

We'll talk about this in class

---

## Two scenes in this project

| Part                   | Description |
|------------------------|-------------|
| **Main Play Scene**   | Player infinitely runs on a random generated map and can dash with space. Player interacts with NPC ¡ú start dialogue (this part we¡¯ll implement in class). |
| **Map Editor Scene**  | Player draws own custom map,then can save to computer,then can load saved map back into the game. |

---

## File Formats Used

| Format | Used for | Why |
|--------|----------|-----|
| **TSV(Tab Separated Values)** | Dialogue lines | Easy for writers / designers to edit in spreadsheet |
| **JSON** | Map Data / Item Data | Supports nested structures, good for saving complex game states |

---

## Core Concepts Covered in This Lesson

- Why external I/O is needed in professional pipeline
- **Serialize / Deserialization** 
- `TextAsset`, `File`, and memory data vs storage data
- Path handling & cross platform concerns
- How to read / write files
- Basic data structures for storing text (Dictionary / Lists)
- Parsing (string to usable game objects)

---

## Learning Outcomes

By the end of this lesson, you will know how to:

- Read structured text from CSV file (dialogue)
- Parse text into usable data objects inside Unity
- Save custom map data into JSON format
- Load saved JSON and restore gameplay content

---



## Attribution / Credits

This demo project is originated based on a game idea made by **[Castpixel](https://x.com/castpixel)**,which is a downloadable unity tutorial about creating a 2D isometric map.

