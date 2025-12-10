using UnityEngine;
using System.Collections;

public class Combatant : MonoBehaviour
{
    public string combatantName = "Fighter";
    public int maxHp = 20;
    public int attack = 2;
    public int defense = 0;
    [HideInInspector] public int currentHp;
    [HideInInspector] public int ap;
    [HideInInspector] public bool defended;

    void Awake()
    {
        currentHp = maxHp;
        ap = 0;
        defended = false;
    }

    public void StartTurn(int gainedAp)
    {
        ap = gainedAp;
        defended = false;
    }

    public void EndTurn()
    {
        // optionally reset temporary states
    }

    public void ApplyDefend()
    {
        defended = true;
        ap = Mathf.Max(0, ap - 1);
    }

    public int PerformAttack(int diceValue, bool isSpecial = false)
    {
        int baseDamage = (isSpecial ? (attack + diceValue + 2) : (attack + diceValue));
        ap = Mathf.Max(0, ap - (isSpecial ? 2 : 1));
        return baseDamage;
    }

    public void TakeDamage(int amount)
    {
        int final = defended ? Mathf.CeilToInt(amount / 2f) : amount;
        currentHp -= final;
        if (currentHp < 0) currentHp = 0;
    }

    public bool IsDead() => currentHp <= 0;
}
