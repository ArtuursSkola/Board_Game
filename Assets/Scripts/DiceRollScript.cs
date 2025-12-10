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
    [SerializeField] private float maxRandForcVal, startRollingForce;
    float forceX, forceY, forceZ;
    public string diceFaceNum;
    public bool isLanded = false;
    public bool firstThrow = false;

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
        rBody.isKinematic = false;
        forceX = Random.Range(0, maxRandForcVal);
        forceY = Random.Range(0, maxRandForcVal);
        forceZ = Random.Range(0, maxRandForcVal);
        rBody.AddForce(Vector3.up*Random.Range(800, startRollingForce));
        rBody.AddTorque(forceX, forceY, forceZ);
    }

    public void ResetDice()
    {
        transform.position = startPosition;
        firstThrow = false;
        isLanded = false;
        Initialize();
    }

    void Update()
    {
        if(rBody != null)
        {
            if(Input.GetMouseButtonDown(0) && isLanded ||
             Input.GetMouseButtonDown(0) && !firstThrow)
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
