using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class TurnBasedGameManager : MonoBehaviour
{
    public Combatant player;
    public Combatant enemy;
    public DiceController diceController;
    public CombatUI ui;

    public float rollDelay = 0.8f;

    private bool playerTurn = true;
    private bool busy = false;

    // Call this from a spawner to wire runtime-spawned combatants and dice/ui.
    public void Setup(Combatant playerCombatant, Combatant enemyCombatant, DiceController dice, CombatUI combatUi)
    {
        player = playerCombatant;
        enemy = enemyCombatant;
        diceController = dice;
        ui = combatUi;

        if (player == null || enemy == null)
        {
            Debug.LogError("TurnBasedGameManager.Setup missing player or enemy Combatant.");
            enabled = false;
            return;
        }

        // Ensure UI knows about this manager
        if (ui != null && ui.manager != this)
        {
            ui.manager = this;
        }

        StopAllCoroutines();
        enabled = true;
        StartCoroutine(StartBattle());
    }

    void Start()
    {
        // If already wired in inspector, start battle; otherwise wait for Setup()
        if (player != null && enemy != null)
        {
            StartCoroutine(StartBattle());
        }
    }

    IEnumerator StartBattle()
    {
        player.currentHp = player.maxHp;
        enemy.currentHp = enemy.maxHp;
        UpdateUI();
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(BeginTurn());
    }

    IEnumerator BeginTurn()
    {
        busy = true;
        if (playerTurn)
        {
            ui.SetControlActive(true);
            yield return StartCoroutine(RollForAP(player));
            ui.SetMessage($"{player.combatantName} gains {player.ap} AP");
        }
        else
        {
            ui.SetControlActive(false);
            yield return StartCoroutine(RollForAP(enemy));
            ui.SetMessage($"{enemy.combatantName} gains {enemy.ap} AP");
            yield return new WaitForSeconds(0.4f);
            yield return StartCoroutine(EnemyTakeActions());
        }
        UpdateUI();
        busy = false;
    }

    IEnumerator RollForAP(Combatant c)
    {
        int rolled = 0;
        if (diceController != null)
        {
            yield return StartCoroutine(diceController.RollCoroutine(v => rolled = v));
        }
        else rolled = UnityEngine.Random.Range(1, 7);

        c.StartTurn(rolled);
        yield return null;
    }

    public void OnPlayerAttack()
    {
        if (!playerTurn || busy) return;
        if (player.ap < 1) { ui.SetMessage("Need 1 AP to attack"); return; }
        StartCoroutine(PlayerAttackRoutine(false));
    }

    public void OnPlayerSpecial()
    {
        if (!playerTurn || busy) return;
        if (player.ap < 2) { ui.SetMessage("Need 2 AP for Special"); return; }
        StartCoroutine(PlayerAttackRoutine(true));
    }

    public void OnPlayerDefend()
    {
        if (!playerTurn || busy) return;
        if (player.ap < 1) { ui.SetMessage("Need 1 AP to defend"); return; }
        player.ApplyDefend();
        ui.SetMessage($"{player.combatantName} defends");
        UpdateUI();
    }

    public void OnEndTurn()
    {
        if (!playerTurn || busy) return;
        StartCoroutine(EndTurn());
    }

    IEnumerator PlayerAttackRoutine(bool special)
    {
        busy = true;
        int diceVal = 0;
        if (diceController != null) yield return StartCoroutine(diceController.RollCoroutine(v => diceVal = v));
        else diceVal = UnityEngine.Random.Range(1, 7);

        int dmg = player.PerformAttack(diceVal, special);
        enemy.TakeDamage(dmg);
        ui.SetMessage($"{player.combatantName} hit for {dmg} (roll {diceVal})");
        UpdateUI();

        if (enemy.IsDead()) { ui.SetMessage($"{player.combatantName} wins!"); yield break; }

        yield return new WaitForSeconds(0.5f);
        busy = false;
    }

    IEnumerator EnemyTakeActions()
    {
        busy = true;
        // simple AI: if can kill with one attack -> attack; else if low hp defend; else attack once.
        yield return new WaitForSeconds(0.3f);
        while (enemy.ap > 0)
        {
            // simulate decision
            if (enemy.ap >= 1)
            {
                int diceVal = 0;
                if (diceController != null) yield return StartCoroutine(diceController.RollCoroutine(v => diceVal = v));
                else diceVal = UnityEngine.Random.Range(1, 7);

                int dmg = enemy.PerformAttack(diceVal, false);
                player.TakeDamage(dmg);
                ui.SetMessage($"{enemy.combatantName} hits for {dmg} (roll {diceVal})");
                UpdateUI();
                if (player.IsDead()) { ui.SetMessage($"{enemy.combatantName} wins!"); yield break; }
                yield return new WaitForSeconds(0.6f);
            }
        }
        busy = false;
        yield return StartCoroutine(EndTurn());
    }

    IEnumerator EndTurn()
    {
        player.EndTurn();
        enemy.EndTurn();
        playerTurn = !playerTurn;
        UpdateUI();
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(BeginTurn());
    }

    void UpdateUI()
    {
        if (ui != null)
        {
            ui.UpdateCombatantUI(player, enemy, playerTurn);
        }
    }
}