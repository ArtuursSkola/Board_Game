using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BoardGameManager : MonoBehaviour
{
    private static BoardGameManager instance;

    [Header("Core")]
    public DiceRollScript dice;
    public TextMeshProUGUI turnText;
    public Transform[] squares; // ordered square1..squareN
    [Tooltip("Vertical offset above square (Y only).")]
    public float pieceOffsetY = 0.5f;
    [Tooltip("Optional X/Z fine-tune offset if your square pivots are off-center. Set to zero normally.")]
    public Vector2 pieceOffsetXZ = Vector2.zero;
    [Header("Movement")]
    [Tooltip("Units per second the piece walks between squares.")]
    public float moveSpeed = 3.5f;
    [Tooltip("Animator bool parameter that is set true while walking.")]
    public string walkBoolParam = "Walk";
    [Tooltip("Optional fallback bool name to try if walkBoolParam is missing (leave empty to disable).")]
    public string walkBoolFallback = "isWalking";
    [Tooltip("Animator bool parameter that is set true while idle.")]
    public string idleBoolParam = "Idle";
    [Tooltip("Optional fallback bool name to try if idleBoolParam is missing (leave empty to disable).")]
    public string idleBoolFallback = "isIdle";
    [Tooltip("Animator bool parameter that is set true while attacking.")]
    public string attackBoolParam = "Attack";
    [Tooltip("Optional fallback bool name to try if attackBoolParam is missing (leave empty to disable).")]
    public string attackBoolFallback = "isAttacking";
    [Header("Battle")]
    [Tooltip("Seconds to pause between showing each battle roll.")]
    public float battleRollPause = 0.8f;

    [Header("Win UI")]
    [Tooltip("Panel shown when someone wins.")]
    public GameObject winPanel;
    [Tooltip("Button on the win panel to reset the game.")]
    public Button resetButton;
    [Tooltip("Button on the win panel to return to the main menu.")]
    public Button leaveButton;
    [Tooltip("Text showing who won (e.g., 'The winner is ...').")]
    public TextMeshProUGUI winOrLoseText;
    [Tooltip("Text showing stats like throws and time.")]
    public TextMeshProUGUI scoreText;
    [Tooltip("Name of the home scene to load when pressing Home.")]
    public string homeSceneName = "MainMenu";
    [Tooltip("Board index (0-based) that counts as the final winning square.")]
    public int winningSquareIndex = 29; // square30 in 0-based indexing
    [Tooltip("Optional leaderboard handler to record wins.")]
    public LeaderboardManager leaderboard;
    [Tooltip("Filename for saving wins as plain text.")]
    public string textLeaderboardFileName = "LeaderboardName.txt";
    [Header("Special Tiles")]
    [Tooltip("Prefab or object to show as a jumpscare when landing on a scare tile.")]
    public GameObject jumpScareObject;
    [Tooltip("How long to show the jumpscare.")]
    public float jumpScareDuration = 2f;
    [Tooltip("Optional audio clip to play when a jumpscare triggers.")]
    public AudioClip jumpScareClip;
    [Tooltip("Audio source used to play the jumpscare clip (falls back to PlayClipAtPoint if none).")]
    public AudioSource jumpScareAudioSource;
    [Range(0f, 1f)] public float jumpScareVolume = 1f;
    [Tooltip("How many scare tiles to pick (excluding start and winning tiles).")]
    public int scareTileCount = 2;
    [Tooltip("How many bonus tiles to pick (extra step forward).")]
    public int bonusTileCount = 2;

    [Header("Timing")]
    public float botRollDelay = 0.6f;

    private readonly List<GameObject> players = new List<GameObject>();
    private readonly List<bool> isHuman = new List<bool>();
    private Vector3[] squareCenters;
    private float[] squareTopY;
    private int[] positions;
    private Coroutine[] moveRoutines;
    private int currentTurn = 0;
    private bool waitingForRoll = false;
    private bool rollInProgress = false; // guards against duplicate dice callbacks
    private bool started = false;
    private readonly HashSet<Animator> missingWalkParamLogged = new HashSet<Animator>();
    private readonly HashSet<Animator> missingIdleParamLogged = new HashSet<Animator>();
    private readonly HashSet<Animator> missingAttackParamLogged = new HashSet<Animator>();
    private int[] throwCounts;
    private float gameStartTime;
    private bool gameOver = false;
    private HashSet<int> scareTiles = new HashSet<int>();
    private HashSet<int> bonusTiles = new HashSet<int>();

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("BoardGameManager: duplicate instance detected. Destroying extra instance on " + gameObject.name);
            Destroy(this);
            return;
        }
        instance = this;
    }

    void OnDisable()
    {
        if (dice != null) dice.OnRolled -= HandleRoll;
        if (instance == this) instance = null;

        if (resetButton != null) resetButton.onClick.RemoveListener(OnResetGameButton);
        if (leaveButton != null) leaveButton.onClick.RemoveListener(OnHomeButton);
    }

    public void SetupPlayers(List<GameObject> spawnedPlayers)
    {
        players.Clear();
        isHuman.Clear();

        if (spawnedPlayers == null || spawnedPlayers.Count == 0)
        {
            Debug.LogError("BoardGameManager.SetupPlayers called with no players.");
            return;
        }

        players.AddRange(spawnedPlayers);
        // First player is human, the rest bots by default
        for (int i = 0; i < players.Count; i++)
        {
            isHuman.Add(i == 0);
        }

        positions = new int[players.Count];
        moveRoutines = new Coroutine[players.Count];
        throwCounts = new int[players.Count];

        // Cache square centers/topY to keep placement consistent across moves
        squareCenters = new Vector3[squares.Length];
        squareTopY = new float[squares.Length];
        for (int s = 0; s < squares.Length; s++)
        {
            Vector3 center = squares[s].position;
            float topY = center.y;

            var rend = squares[s].GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                center = rend.bounds.center;
                topY = rend.bounds.max.y;
            }
            else
            {
                var col = squares[s].GetComponentInChildren<Collider>();
                if (col != null)
                {
                    center = col.bounds.center;
                    topY = col.bounds.max.y;
                }
            }

            squareCenters[s] = center;
            squareTopY[s] = topY;
        }
        for (int i = 0; i < players.Count; i++)
        {
            positions[i] = 0;
            MovePieceToSquare(i, 0);
        }

        // Hook up win panel buttons
        if (resetButton != null)
        {
            resetButton.onClick.RemoveAllListeners(); // Clear existing to prevent duplicates
            resetButton.onClick.AddListener(OnResetGameButton);
        }
        if (leaveButton != null)
        {
            leaveButton.onClick.RemoveAllListeners(); // Clear existing to prevent duplicates
            leaveButton.onClick.AddListener(OnHomeButton);
        }

        StartGame();
    }

    private void StartGame()
    {
        if (players.Count == 0 || squares == null || squares.Length == 0)
        {
            Debug.LogError("BoardGameManager missing players or squares.");
            return;
        }

        if (dice == null)
        {
            dice = FindFirstObjectByType<DiceRollScript>();
        }
        if (dice == null)
        {
            Debug.LogError("BoardGameManager could not find DiceRollScript.");
            return;
        }

        dice.OnRolled -= HandleRoll;
        dice.OnRolled += HandleRoll;    

        Debug.Log("BoardGameManager.StartGame: subscribed to dice. Players: " + players.Count);
        for (int i = 0; i < players.Count; i++)
        {
            Debug.Log("  Player " + i + ": " + players[i].name + " (human=" + isHuman[i] + ")");
        }

        currentTurn = Random.Range(0, players.Count);
        started = true;
        gameOver = false;
        gameStartTime = Time.time;
        GenerateSpecialTiles();
        Debug.Log("BoardGameManager.StartGame: random start player = " + currentTurn + " (" + players[currentTurn].name + ")");
        BeginTurn();
    }

    private void BeginTurn()
    {
        if (!started) return;
        if (gameOver) return;
        if (dice != null)
        {
            dice.OnRolled -= HandleRoll; // ensure single subscription per roll
            dice.OnRolled += HandleRoll;
        }
        rollInProgress = false;
        waitingForRoll = true;

        if (turnText != null)
        {
            turnText.text = $"Turn: {players[currentTurn].name}";
        }

        bool isPlayerTurn = isHuman[currentTurn];
        dice.SetRollPermission(isPlayerTurn); // allow clicks only on player turn
        dice.ResetDice();

        if (!isPlayerTurn)
        {
            StartCoroutine(BotTurn());
        }
    }

    private IEnumerator BotTurn()
    {
        yield return new WaitForSeconds(botRollDelay);
        // temporarily allow a programmatic roll
        dice.SetRollPermission(true);
        dice.TriggerRoll();
        dice.SetRollPermission(false);
    }

    private void HandleRoll(int faceValue)
    {
        if (rollInProgress)
        {
            Debug.LogWarning("HandleRoll duplicate event ignored.");
            return;
        }
        if (gameOver) return;
        if (dice != null)
        {
            dice.OnRolled -= HandleRoll; // drop further callbacks from this physical roll
        }
        rollInProgress = true;
        if (!waitingForRoll) { Debug.LogWarning("HandleRoll called but not waiting for roll. Ignoring."); return; }
        waitingForRoll = false;
        // Trust the UI-facing dice number if available to avoid any event/value mismatch
        if (dice != null && int.TryParse(dice.diceFaceNum, out int parsedUiFace))
        {
            faceValue = parsedUiFace;
        }
        Debug.Log("HandleRoll: face=" + faceValue + ", currentTurn=" + currentTurn + " (" + players[currentTurn].name + "), oldPos=" + positions[currentTurn]);

        // Clamp to a sane die range in case of bad collider names or fallback overshoots
        faceValue = Mathf.Clamp(faceValue, 1, 6);

        int steps = Mathf.Max(1, faceValue);
        if (throwCounts != null && currentTurn < throwCounts.Length) throwCounts[currentTurn]++;
        int startIndex = positions[currentTurn];
        int targetIndex = Mathf.Min(startIndex + steps, squares.Length - 1);
        positions[currentTurn] = targetIndex;
        Debug.Log("HandleRoll: moving player " + currentTurn + " to square " + targetIndex);

        // Prevent any extra roll processing until the next turn explicitly begins
        dice.SetRollPermission(false);

        StartCoroutine(MovePlayerAndEndTurn(currentTurn, startIndex, targetIndex));
    }

    private void EndTurn()
    {
        currentTurn = (currentTurn + 1) % players.Count;
        BeginTurn();
    }

    // Hook this to your reset dice UI button
    public void OnResetDiceButton()
    {
        waitingForRoll = true;
        rollInProgress = false;
        dice.ResetDice();
        bool isPlayerTurn = isHuman[currentTurn];
        dice.SetRollPermission(isPlayerTurn);
        if (!isPlayerTurn)
        {
            StartCoroutine(BotTurn());
        }
    }

    private void MovePieceToSquare(int playerIndex, int squareIndex)
    {
        if (squareIndex < 0 || squareIndex >= squares.Length) return;
        if (players[playerIndex] == null) return;

        Vector3 targetPos = GetSquarePlacement(squareIndex);
        players[playerIndex].transform.position = targetPos;
    }

    private Vector3 GetSquarePlacement(int squareIndex)
    {
        // Prefer cached centers from setup so every square is registered once; compute on-demand if missing
        bool hasCache = squareCenters != null && squareTopY != null &&
                        squareIndex >= 0 && squareIndex < squareCenters.Length &&
                        squareIndex < squareTopY.Length;

        Vector3 center = hasCache ? squareCenters[squareIndex] : squares[squareIndex].position;
        float topY = hasCache ? squareTopY[squareIndex] : center.y;

        // If cache is missing (e.g., hot-adding squares), compute and store a consistent center/top
        if (!hasCache)
        {
            var rend = squares[squareIndex].GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                center = rend.bounds.center;
                topY = rend.bounds.max.y;
            }
            else
            {
                var col = squares[squareIndex].GetComponentInChildren<Collider>();
                if (col != null)
                {
                    center = col.bounds.center;
                    topY = col.bounds.max.y;
                }
            }

            // Final fallback: use transform position and its scaled half-height
            if (center == squares[squareIndex].position)
            {
                float halfHeight = squares[squareIndex].lossyScale.y * 0.5f;
                topY = squares[squareIndex].position.y + halfHeight;
            }

            // Grow caches if needed so future moves reuse the same registered spot
            if (squareCenters == null || squareCenters.Length != squares.Length)
            {
                squareCenters = new Vector3[squares.Length];
                squareTopY = new float[squares.Length];
            }
            squareCenters[squareIndex] = center;
            squareTopY[squareIndex] = topY;
        }

        return new Vector3(
            center.x + pieceOffsetXZ.x,
            topY + pieceOffsetY,
            center.z + pieceOffsetXZ.y);
    }

    private IEnumerator MovePlayerAndEndTurn(int playerIndex, int startIndex, int targetIndex)
    {
        // Stop any ongoing move for this player
        if (moveRoutines != null && moveRoutines[playerIndex] != null)
        {
            StopCoroutine(moveRoutines[playerIndex]);
        }

        var routine = StartCoroutine(MovePlayerAlongSquares(playerIndex, startIndex, targetIndex));
        if (moveRoutines != null)
        {
            moveRoutines[playerIndex] = routine;
        }

        yield return routine;

        // Apply special tile effects (jumpscare/bonus) before battles
        yield return ApplyTileEffects(playerIndex);

        // Resolve any battles on the landing square before ending turn
        yield return ResolveBattlesFor(playerIndex);

        if (moveRoutines != null)
        {
            moveRoutines[playerIndex] = null;
        }

        rollInProgress = false;
        if (CheckWin(playerIndex)) yield break;
        EndTurn();
    }

    private IEnumerator MovePlayerAlongSquares(int playerIndex, int startIndex, int targetIndex)
    {
        if (players[playerIndex] == null) yield break;
        var anim = players[playerIndex].GetComponent<Animator>();
        SetWalking(anim, true);

        int step = targetIndex >= startIndex ? 1 : -1;
        int current = startIndex;
        while (current != targetIndex)
        {
            current += step;
            Vector3 targetPos = GetSquarePlacement(current);
            yield return MoveToPosition(players[playerIndex], targetPos);
        }

        SetWalking(anim, false);
    }

    private IEnumerator ResolveBattlesFor(int moverIndex)
    {
        if (positions == null) yield break;
        if (positions[moverIndex] <= 0) yield break; // start tile is safe
        // Check for opponents on the same square as mover; handle sequentially
        int opponentIndex = FindOpponentOnSameSquare(moverIndex);
        while (opponentIndex != -1)
        {
            yield return RunBattle(moverIndex, opponentIndex);
            // Re-evaluate in case the mover is still sharing a tile after battle
            opponentIndex = FindOpponentOnSameSquare(moverIndex);
        }
    }

    private int FindOpponentOnSameSquare(int moverIndex)
    {
        if (positions == null) return -1;
        int square = positions[moverIndex];
        for (int i = 0; i < positions.Length; i++)
        {
            if (i == moverIndex) continue;
            if (positions[i] == square)
            {
                return i;
            }
        }
        return -1;
    }

    private IEnumerator RunBattle(int playerA, int playerB)
    {
        // Simple dice duel: highest roll wins; ties re-roll
        int rollA, rollB;
        string nameA = players[playerA].name;
        string nameB = players[playerB].name;

        // Trigger attack animation on both fighters
        var animA = players[playerA].GetComponent<Animator>();
        var animB = players[playerB].GetComponent<Animator>();
        SetAttacking(animA, true);
        SetAttacking(animB, true);

        ShowBattleStatus($"Battle! {nameA} vs {nameB}");
        yield return new WaitForSeconds(battleRollPause);

        do
        {
            rollA = Random.Range(1, 7);
            ShowBattleStatus($"{nameA} rolls {rollA}");
            yield return new WaitForSeconds(battleRollPause);

            rollB = Random.Range(1, 7);
            ShowBattleStatus($"{nameB} rolls {rollB}");
            yield return new WaitForSeconds(battleRollPause);

            if (rollA == rollB)
            {
                ShowBattleStatus("Tie! Re-rolling...");
                yield return new WaitForSeconds(battleRollPause);
            }
        } while (rollA == rollB);

        bool aWins = rollA > rollB;
        int winner = aWins ? playerA : playerB;
        int loser = aWins ? playerB : playerA;

        ShowBattleStatus($"Winner: {players[winner].name}. Loser moves back 1.");
        Debug.Log($"Battle: {nameA} rolled {rollA} vs {nameB} rolled {rollB}. Winner: {players[winner].name}.");
        yield return new WaitForSeconds(battleRollPause);

        // End attack animation before moving loser
        SetAttacking(animA, false);
        SetAttacking(animB, false);

        int loserStart = positions[loser];
        int loserTarget = Mathf.Max(0, loserStart - 1);
        positions[loser] = loserTarget;

        // Move the loser back one square (animated if distance > 0)
        if (moveRoutines != null && moveRoutines[loser] != null)
        {
            StopCoroutine(moveRoutines[loser]);
            moveRoutines[loser] = null;
        }

        if (loserStart != loserTarget)
        {
            var backRoutine = StartCoroutine(MovePlayerAlongSquares(loser, loserStart, loserTarget));
            if (moveRoutines != null)
            {
                moveRoutines[loser] = backRoutine;
            }
            yield return backRoutine;
            if (moveRoutines != null)
            {
                moveRoutines[loser] = null;
            }
        }
        else
        {
            MovePieceToSquare(loser, loserTarget);
        }

        // After a battle move, check if mover still meets win condition
        if (CheckWin(playerA) || CheckWin(playerB)) yield break;
    }

    private void GenerateSpecialTiles()
    {
        scareTiles.Clear();
        bonusTiles.Clear();
        int maxIndex = Mathf.Max(0, squares.Length - 1);
        int winIndex = Mathf.Clamp(winningSquareIndex, 0, maxIndex);
        int protectedStart = 0;

        // helper to draw a unique random tile excluding protected ones
        int DrawTile(HashSet<int> existing)
        {
            const int maxAttempts = 200;
            for (int i = 0; i < maxAttempts; i++)
            {
                int idx = Random.Range(0, maxIndex + 1);
                if (idx == protectedStart || idx == winIndex) continue;
                if (existing.Contains(idx)) continue;
                if (scareTiles.Contains(idx)) continue;
                if (bonusTiles.Contains(idx)) continue;
                return idx;
            }
            return -1;
        }

        for (int i = 0; i < scareTileCount; i++)
        {
            int t = DrawTile(scareTiles);
            if (t >= 0) scareTiles.Add(t);
        }
        for (int i = 0; i < bonusTileCount; i++)
        {
            int t = DrawTile(bonusTiles);
            if (t >= 0) bonusTiles.Add(t);
        }
    }

    private IEnumerator ApplyTileEffects(int playerIndex)
    {
        if (positions == null || playerIndex < 0 || playerIndex >= positions.Length) yield break;
        if (gameOver) yield break;

        int tile = positions[playerIndex];
        // Scare tile: show jump scare, step back 1
        if (scareTiles.Contains(tile))
        {
            if (turnText != null) turnText.text = players[playerIndex].name + " landed on a scare tile!";
            PlayJumpScareSound();
            if (jumpScareObject != null) jumpScareObject.SetActive(true);
            yield return new WaitForSeconds(jumpScareDuration);
            if (jumpScareObject != null) jumpScareObject.SetActive(false);

            int startIdx = positions[playerIndex];
            int targetIdx = Mathf.Max(0, startIdx - 1);
            positions[playerIndex] = targetIdx;

            // animate back one tile
            var backRoutine = StartCoroutine(MovePlayerAlongSquares(playerIndex, startIdx, targetIdx));
            yield return backRoutine;
        }
        // Bonus tile: move forward 1
        else if (bonusTiles.Contains(tile))
        {
            if (turnText != null) turnText.text = players[playerIndex].name + " found a bonus tile!";
            int startIdx = positions[playerIndex];
            int targetIdx = Mathf.Min(squares.Length - 1, startIdx + 1);
            positions[playerIndex] = targetIdx;
            var fwdRoutine = StartCoroutine(MovePlayerAlongSquares(playerIndex, startIdx, targetIdx));
            yield return fwdRoutine;
        }

        // After tile effects, check win
        CheckWin(playerIndex);
    }

    private void PlayJumpScareSound()
    {
        if (jumpScareClip == null) return;
        if (jumpScareAudioSource != null)
        {
            jumpScareAudioSource.PlayOneShot(jumpScareClip, jumpScareVolume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(jumpScareClip, Camera.main != null ? Camera.main.transform.position : Vector3.zero, jumpScareVolume);
        }
    }

    private IEnumerator MoveToPosition(GameObject player, Vector3 targetPos)
    {
        if (player == null) yield break;
        Vector3 start = player.transform.position;
        float distance = Vector3.Distance(start, targetPos);
        float duration = moveSpeed > 0.01f ? distance / moveSpeed : 0.01f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            player.transform.position = Vector3.Lerp(start, targetPos, t);
            yield return null;
        }

        player.transform.position = targetPos;
    }

    private void ShowBattleStatus(string message)
    {
        if (turnText != null)
        {
            turnText.text = message;
        }
        Debug.Log(message);
    }

    private void SetWalking(Animator anim, bool isWalking)
    {
        if (anim == null) return;

        string walkParam = ResolveBool(anim, walkBoolParam, walkBoolFallback, missingWalkParamLogged, "walk");
        string idleParam = ResolveBool(anim, idleBoolParam, idleBoolFallback, missingIdleParamLogged, "idle");

        if (!string.IsNullOrEmpty(walkParam))
        {
            anim.SetBool(walkParam, isWalking);
        }
        if (!string.IsNullOrEmpty(idleParam))
        {
            anim.SetBool(idleParam, !isWalking);
        }
    }

    private void SetAttacking(Animator anim, bool isAttacking)
    {
        if (anim == null) return;
        string attackParam = ResolveBool(anim, attackBoolParam, attackBoolFallback, missingAttackParamLogged, "attack");
        if (!string.IsNullOrEmpty(attackParam))
        {
            anim.SetBool(attackParam, isAttacking);
        }
    }

    private string ResolveBool(Animator anim, string primary, string fallback, HashSet<Animator> loggedSet, string label)
    {
        if (!string.IsNullOrEmpty(primary) && AnimatorHasBool(anim, primary)) return primary;
        if (!string.IsNullOrEmpty(fallback) && AnimatorHasBool(anim, fallback)) return fallback;

        // Auto-detect common parameter names to avoid missing-animator warnings
        string[] common;
        if (label == "walk")
        {
            common = new[] { "Walk", "Walking", "IsWalking", "isWalking", "walk", "walking" };
        }
        else if (label == "idle")
        {
            common = new[] { "Idle", "isIdle", "IsIdle", "idle", "Idling" };
        }
        else // attack
        {
            common = new[] { "Attack", "Attacking", "IsAttacking", "isAttacking", "attack", "attacking" };
        }

        foreach (var name in common)
        {
            if (AnimatorHasBool(anim, name))
            {
                return name;
            }
        }

        if (loggedSet != null && !loggedSet.Contains(anim))
        {
            loggedSet.Add(anim);
            Debug.LogWarning("Animator missing " + label + " bool parameter ('" + primary + "', fallback '" + fallback + "', tried common names) on " + anim.gameObject.name + ".");
        }
        return null;
    }

    private bool AnimatorHasBool(Animator anim, string paramName)
    {
        foreach (var p in anim.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Bool && p.name == paramName)
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckWin(int playerIndex)
    {
        if (gameOver) return true;
        int winIndex = Mathf.Clamp(winningSquareIndex, 0, squares.Length - 1);
        if (positions != null && playerIndex >= 0 && playerIndex < positions.Length && positions[playerIndex] >= winIndex)
        {
            gameOver = true;
            dice.SetRollPermission(false);
            if (turnText != null)
            {
                turnText.text = players[playerIndex].name + " wins!";
            }

            if (winPanel != null) winPanel.SetActive(true);
            if (winOrLoseText != null)
            {
                winOrLoseText.text = "The winner is " + players[playerIndex].name;
            }
            if (scoreText != null)
            {
                float elapsed = Time.time - gameStartTime;
                string timeStr = string.Format("{0:00}:{1:00}", Mathf.Floor(elapsed / 60f), Mathf.Floor(elapsed % 60f));
                int throws = (throwCounts != null && playerIndex < throwCounts.Length) ? throwCounts[playerIndex] : 0;
                scoreText.text = "Throws: " + throws + "; Time: " + timeStr + " sec";
            }
            SaveWinToLeaderboard(playerIndex);
            return true;
        }
        return false;
    }

    private void SaveWinToLeaderboard(int playerIndex)
    {
        if (leaderboard == null) return;
        string winnerName = players != null && playerIndex >= 0 && playerIndex < players.Count ? players[playerIndex].name : "Unknown";
        bool isBot = isHuman != null && playerIndex >= 0 && playerIndex < isHuman.Count ? !isHuman[playerIndex] : false;
        int throws = (throwCounts != null && playerIndex < throwCounts.Length) ? throwCounts[playerIndex] : 0;
        int score = CalculateScoreFromThrows(throws);
        leaderboard.AddEntry(winnerName, score, isBot, throws, Time.time - gameStartTime);
        AppendWinToTextFile(winnerName, isBot, throws, score, Time.time - gameStartTime);
    }

    private int CalculateScoreFromThrows(int throws)
    {
        // Best score achieved at 5 throws => 10000; more throws reduce score proportionally.
        if (throws <= 0) return 0;
        const float bestThrows = 5f;
        const int bestScore = 10000;
        float factor = bestThrows / Mathf.Max(throws, 1);
        int score = Mathf.RoundToInt(bestScore * factor);
        return Mathf.Clamp(score, 1, bestScore);
    }

    private void AppendWinToTextFile(string winnerName, bool isBot, int throws, int score, float elapsedSeconds)
    {
        try
        {
            string timeStr = string.Format("{0:00}:{1:00}", Mathf.Floor(elapsedSeconds / 60f), Mathf.Floor(elapsedSeconds % 60f));
            string line = string.Format("{0}{1}, {2} Moves, {3} sec, Score: {4}",
                winnerName,
                isBot ? " (bot)" : string.Empty,
                throws,
                timeStr,
                score);

            // Primary save in persistent data path
            string path = Path.Combine(Application.persistentDataPath, textLeaderboardFileName);
            File.AppendAllText(path, line + Environment.NewLine);

#if UNITY_EDITOR
            // Mirror into Assets/Resources so you can inspect it in the Editor if desired
            string resourcesDir = Path.Combine(Application.dataPath, "Resources");
            if (!Directory.Exists(resourcesDir)) Directory.CreateDirectory(resourcesDir);
            string editorPath = Path.Combine(resourcesDir, textLeaderboardFileName);
            File.AppendAllText(editorPath, line + Environment.NewLine);
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Failed to append win to text leaderboard: " + ex.Message);
        }
    }

    /// <summary>
    /// Resets the game to its initial state without reloading the scene.
    /// </summary>
    public void ResetGame()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        if (players != null && players.Count > 0)
        {
            // Reset player positions and stats
            positions = new int[players.Count];
            throwCounts = new int[players.Count];
            for (int i = 0; i < players.Count; i++)
            {
                positions[i] = 0;
                throwCounts[i] = 0;
                if (moveRoutines != null && moveRoutines[i] != null)
                {
                    StopCoroutine(moveRoutines[i]);
                    moveRoutines[i] = null;
                }
                MovePieceToSquare(i, 0);
            }
        }

        // Reset game state
        gameOver = false;
        started = false; // Will be set to true in StartGame

        // Restart the game logic
        StartGame();
    }

    // UI buttons
    public void OnResetGameButton()
    {
        ResetGame();
    }

    public void OnHomeButton()
    {
        if (!string.IsNullOrEmpty(homeSceneName))
        {
            SceneManager.LoadScene(homeSceneName);
        }
    }
}
