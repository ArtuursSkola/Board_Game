using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
using System;
using Rigidbody = UnityEngine.Rigidbody;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using Mathf = UnityEngine.Mathf;


public class DiceRollScript : MonoBehaviour
{
    Rigidbody rBody;
    Vector3 position, startPosition;
    [Header("Force Settings")]
    [SerializeField] private float minUpImpulse = 6f;
    [SerializeField] private float maxUpImpulse = 10f;
    [SerializeField] private float maxTorque = 6f;
    [SerializeField] private float maxRandForcVal, startRollingForce; // legacy fields (ignored for up force)
    float forceX, forceY, forceZ;
    public int sides = 6;
    public string diceFaceNum;
    public bool isLanded = false;
    public bool firstThrow = false;

    public System.Action<int> OnRolled; // notified when a face is determined
    private bool canRoll = true;
    private bool reportedThisRoll = false;
    private bool rolling = false;
    private float rollTimer = 0f;
    [SerializeField] private float maxRollDuration = 3f; // fallback if die never lands

    void Awake()
    {
        startPosition = transform.position;
        Initialize();
    }

    private void Initialize()
    {
        rBody = GetComponent<Rigidbody>();
        rBody.isKinematic = true;
        position = transform.position;    
        transform.rotation = new Quaternion(
            Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360), 0);  
    }

    private void RollDice()
    {
        // Enable physics then reset velocities so each roll is predictable
        rBody.isKinematic = false;
        rBody.linearVelocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;
        forceX = Random.Range(0, maxRandForcVal);
        forceY = Random.Range(0, maxRandForcVal);
        forceZ = Random.Range(0, maxRandForcVal);
        float upImpulse = Random.Range(minUpImpulse, maxUpImpulse);
        rBody.AddForce(Vector3.up * upImpulse, ForceMode.Impulse);
        rBody.AddTorque(
            Random.Range(-maxTorque, maxTorque),
            Random.Range(-maxTorque, maxTorque),
            Random.Range(-maxTorque, maxTorque),
            ForceMode.Impulse);
        reportedThisRoll = false;
        rolling = true;
        rollTimer = 0f;
    }

    public void ResetDice()
    {
        transform.position = startPosition;
        firstThrow = false;
        isLanded = false;
        reportedThisRoll = false;
        rolling = false;
        rollTimer = 0f;
        Initialize();

    }

    public void SetRollPermission(bool allowed)
    {
        canRoll = allowed;
    }

    public void TriggerRoll()
    {
        if (!canRoll) return;
        if (!firstThrow) firstThrow = true;
        RollDice();
    }

    public void ReportResult(int face)
    {
        if (reportedThisRoll) return;
        diceFaceNum = face.ToString();
        reportedThisRoll = true;
        rolling = false;
        OnRolled?.Invoke(face);
    }

    void Update()
    {
        if(rBody != null)
        {
            if (rolling && !isLanded)
            {
                rollTimer += Time.deltaTime;
                if (rollTimer > maxRollDuration)
                {
                    // fallback random result if die never lands or falls off the board
                    int fallback = UnityEngine.Random.Range(1, sides + 1);
                    ReportResult(fallback);
                }
            }

            if(canRoll && (Input.GetMouseButtonDown(0) && isLanded ||
             Input.GetMouseButtonDown(0) && !firstThrow))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if(Physics.Raycast(ray, out hit))
                {
                    if(hit.collider != null && hit.collider.gameObject == this.gameObject)
                    {
                        if(!firstThrow)
                        {
                            firstThrow = true;
                        }
                        RollDice();
                    }
            }
        }
    }
    }
}
