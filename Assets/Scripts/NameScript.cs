using UnityEngine;
using TMPro;


public class NameScript : MonoBehaviour
{
    TextMeshPro tMP;

    void Awake()
    {
        tMP = transform.Find("Namefield").gameObject.GetComponent<TextMeshPro>();
    }

    public void SetName(string name)
    {
        tMP.text = name;
        tMP.color = new Color32(
        (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255);
    }
}
