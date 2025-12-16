using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
using System;

public class SideDetectScript : MonoBehaviour
{
    DiceRollScript diceRollScript;
    DiceController diceController;
    [Header("Stability")]
    [Tooltip("Seconds the die must stay still on the same face before we report it.")]
    public float settleTime = 0.25f;
    [Tooltip("Linear speed below which the die is considered still.")]
    public float settleVelocityThreshold = 0.05f;
    [Tooltip("Angular speed below which the die is considered still.")]
    public float settleAngularThreshold = 1f;
    [Tooltip("Seconds the die position must remain unchanged to treat it as fully landed.")]
    public float settleStillTime = 2f;
    [Tooltip("Squared distance threshold to consider the die unmoved between frames.")]
    public float positionEpsilonSqr = 0.0001f;

    private string lastFace = null;
    private float stableTimer = 0f;
    private bool hasReported = false;
    private Vector3 lastPos;
    private float stillTimer = 0f;

    void Awake()
    {
		diceRollScript = FindFirstObjectByType<DiceRollScript>();
		diceController = FindFirstObjectByType<DiceController>();
        lastPos = transform.position;
    }

    private void OnTriggerStay(Collider sideCollider)
    {
        if(diceRollScript != null)
        {
            var body = diceRollScript.GetComponent<Rigidbody>();
            if (body == null) return;

            float speed = body.linearVelocity.magnitude;
            float angSpeed = body.angularVelocity.magnitude;

            // Track positional stillness
            float movedSqr = (transform.position - lastPos).sqrMagnitude;
            lastPos = transform.position;

            if (movedSqr <= positionEpsilonSqr)
            {
                stillTimer += Time.deltaTime;
            }
            else
            {
                stillTimer = 0f;
            }

            // If still moving, reset stability tracking
            if (speed > settleVelocityThreshold || angSpeed > settleAngularThreshold)
            {
                stableTimer = 0f;
                lastFace = null;
                hasReported = false;
                stillTimer = 0f;
                diceRollScript.isLanded = false;
                return;
            }

            // Stable enough: accumulate time on the same face
            diceRollScript.isLanded = true;
            if (sideCollider.name != lastFace)
            {
                lastFace = sideCollider.name;
                stableTimer = 0f;
            }
            stableTimer += Time.deltaTime;

            if (hasReported) return;

            // Require both face stability and positional stillness time
            if (stableTimer >= settleTime && stillTimer >= settleStillTime)
            {
                int parsedFace;
                if (int.TryParse(sideCollider.name, out parsedFace))
                {
                    diceRollScript.diceFaceNum = parsedFace.ToString();
                    if (diceController != null)
                    {
                        diceController.SetPhysicalResult(parsedFace);
                    }
                    diceRollScript.ReportResult(parsedFace);
                    hasReported = true;
                }
            }
        }
    }
}
