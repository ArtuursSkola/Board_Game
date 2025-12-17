# Board_Game
2D Board game with multiple charecters

**Todo List**
- [x] Multiple curssors
- [x] Main menu animations
- [x] Main menu button (start, quit, setting, leaderboard)
- [x] Character selection screeen with animations
- [x] Settings scene :)
- [x] Board scene with throwable dice
- [x] Game logic with multiple players :)
- [x] Game camera :)
- [x] Leaderboard scene :)

---

## Overview
- Casual 2D board game built in Unity with throwable physics dice, animated characters, and special tiles.
- Single human player plus bots (2–6 total). Player name and desired player count are entered on the main menu; bots auto-fill the remaining seats.
- Win by reaching the final square first; battles, scares, and bonuses add swingy moments.

## How to Play
- In the main menu: enter your name (≥3 chars) and player count (2–6). Pick a character and press Play.
- Turns: the current player rolls; pieces walk tile by tile to the destination square.
- Battles: landing on an occupied square triggers a duel—both roll; higher wins; loser steps back 1 tile. Ties re-roll.
- Special tiles: scare tiles show a jump-scare and push you back 1; bonus tiles push you forward 1.
- Win: first to reach or pass the last board tile.

## Controls
- Mouse: click the dice to roll on your turn; use UI buttons for menu actions (reset game, home, settings, pause).
- Pause/Settings: adjust music/SFX volume, resolution/fullscreen, and resume.

## Dice & Movement
- Dice uses physics; result is clamped to 1–6.
- Movement animates per tile; walking/idle/attack animator bools auto-detect common parameter names if the configured one is missing.

## Battles
- Each fighter rolls 1–6; higher wins; loser moves back 1 tile (animated if distance > 0).
- Attack animation bool toggles during the duel.

## Special Tiles
- Scare tiles: play jump-scare (audio + visual) then move back 1 tile.
- Bonus tiles: move forward 1 tile.

## Scoring & Leaderboard
- Score is based on throws: best score 10000 at 5 throws; more throws reduce score proportionally.
- On win, results save to a JSON leaderboard and a plain-text log (`LeaderboardName.txt`).
- Leaderboard entries show rank, name (bots labeled), moves, time, and score.

## Data & Persistence
- Player name, selected character index, and player count are saved via PlayerPrefs.
- Leaderboard JSON and text log are stored under `Application.persistentDataPath` (mirrored to `Assets/Resources` in the Editor for inspection).

## Scenes & Flow
- Main Menu: enter name, choose player count, select character, start game, open settings/leaderboard/quit.
- Board Scene: roll dice, move, resolve tiles and battles, win UI, and save results.
- Leaderboard Scene: scrollable view of past wins.

## Quick Notes
- If a TMP input is empty/invalid (name too short or player count outside 2–6), play is blocked until fixed.
- Bots draw names from `Resources/PlayerName.txt`; if depleted, they are named "Bot N".
