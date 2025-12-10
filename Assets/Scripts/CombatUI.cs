using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour
{
    public TextMeshProUGUI playerHpText;
    public TextMeshProUGUI enemyHpText;
    public TextMeshProUGUI playerApText;
    public TextMeshProUGUI messageText;

    public Button attackButton;
    public Button defendButton;
    public Button specialButton;
    public Button endTurnButton;

    public TurnBasedGameManager manager;

    void Awake()
    {
        if (manager == null)
        {
            manager = FindFirstObjectByType<TurnBasedGameManager>();
        }
    }

    public void UpdateCombatantUI(Combatant player, Combatant enemy, bool playerTurn)
    {
        if (playerHpText) playerHpText.text = $"{player.combatantName} HP: {player.currentHp}/{player.maxHp}";
        if (enemyHpText) enemyHpText.text = $"{enemy.combatantName} HP: {enemy.currentHp}/{enemy.maxHp}";
        if (playerApText) playerApText.text = $"AP: {player.ap}";
        SetControlActive(playerTurn);
    }

    public void SetMessage(string msg)
    {
        if (messageText) messageText.text = msg;
    }

    public void SetControlActive(bool active)
    {
        if (attackButton) attackButton.interactable = active;
        if (defendButton) defendButton.interactable = active;
        if (specialButton) specialButton.interactable = active;
        if (endTurnButton) endTurnButton.interactable = active;
    }

    // UI button hooks
    public void OnAttackPressed() => manager?.OnPlayerAttack();
    public void OnDefendPressed() => manager?.OnPlayerDefend();
    public void OnSpecialPressed() => manager?.OnPlayerSpecial();
    public void OnEndTurnPressed() => manager?.OnEndTurn();
}