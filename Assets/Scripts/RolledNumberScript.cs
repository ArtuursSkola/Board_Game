using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RolledNumberScript : MonoBehaviour
{
    DiceRollScript diceRollScript;
    [SerializeField]
    Text rolledNumberText;


    void Awake()
    {
        diceRollScript = FindFirstObjectByType<DiceRollScript>();
    }

    void Update()
    {
        if(diceRollScript != null)
        {
            if(diceRollScript)
            rolledNumberText.text = diceRollScript.diceFaceNum;
        
        else
        rolledNumberText.text = "?";
    }else 
    Debug.LogWarning("DiceRollScript not found in the scene.");
}
}

