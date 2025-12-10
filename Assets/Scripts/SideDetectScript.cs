using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
using System;

public class SideDetectScript : MonoBehaviour
{
    DiceRollScript diceRollScript;
    DiceController diceController;

    void Awake()
    {
        diceRollScript = FindObjectOfType<DiceRollScript>();
        diceController = FindFirstObjectByType<DiceController>();
    }

    private void OnTriggerStay(Collider sideCollider)
    {
        if(diceRollScript != null)
        {
            if (diceRollScript.GetComponent<Rigidbody>().linearVelocity == Vector3.zero)
            {
                diceRollScript.isLanded = true;
                diceRollScript.diceFaceNum = sideCollider.name;
                int face;
                if (int.TryParse(sideCollider.name, out face) && diceController != null)
                {
                    diceController.SetPhysicalResult(face);
                }
            }
            else
                diceRollScript.isLanded = false;
        }
    }
}
