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

    // Update is called once per frame
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

