using UnityEngine;
using System;
using System.Collections;

public class DiceController : MonoBehaviour
{
    public int sides = 6;
    public float rollTime = 1.0f;

    // If you have a physical dice object, call StartPhysicalRoll() and set result via SetPhysicalResult(int)
    private int physicalResult = -1;

    public IEnumerator RollCoroutine(Action<int> callback)
    {
        // If a physical dice result was supplied, honor it after a small wait
        yield return new WaitForSeconds(rollTime * 0.5f);
        int value;
        if (physicalResult > 0)
        {
            value = physicalResult;
            physicalResult = -1;
        }
        else
        {
            value = UnityEngine.Random.Range(1, sides + 1);
        }
        yield return new WaitForSeconds(rollTime * 0.5f);
        callback?.Invoke(value);
    }

    // Call this if your dice physics determines the face:
    public void SetPhysicalResult(int face)
    {
        physicalResult = Mathf.Clamp(face, 1, sides);
    }

    // convenience synchronous roll
    public int RollInstant() => UnityEngine.Random.Range(1, sides + 1);
}