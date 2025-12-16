using UnityEngine;
using System.Collections;
using System.Collections.Generic; 
using TMPro;
using UnityEngine.UI;
using System.IO;  


public class PlayerScript : MonoBehaviour
{
    public GameObject[] playerPrefabs;
    int characterIndex;
    public GameObject spawnPoint;
    int index;
    private const string textFileName = "PlayerName";


    void Start()
    {
        characterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);
        List<GameObject> spawnedPlayers = new List<GameObject>();

        GameObject mainCharacter = Instantiate(
            playerPrefabs[characterIndex], spawnPoint.transform.position, Quaternion.identity);
        string playerName = PlayerPrefs.GetString("PlayerName", "Player");
        string displayName = playerName + " (You)";
        mainCharacter.name = displayName; // prevents Unity's default (Clone) name showing
        mainCharacter.GetComponent<NameScript>().SetName(displayName);
        spawnedPlayers.Add(mainCharacter);

        int botCount = Mathf.Max(0, PlayerPrefs.GetInt("PlayerCount") - 1);
        List<string> availableNames = new List<string>(ReadLinesFromFile(textFileName));
        ShuffleNames(availableNames);

        for(int i = 0; i < botCount; i++)
        {
            spawnPoint.transform.position += new Vector3(0.2f, 0, 0.08f);
            index = Random.Range(0, playerPrefabs.Length);
            GameObject otherPlayer = Instantiate(
                playerPrefabs[index], spawnPoint.transform.position, Quaternion.identity);

            string botName;
            if (availableNames.Count > 0)
            {
                botName = availableNames[0];
                availableNames.RemoveAt(0);
            }
            else
            {
                botName = "Bot " + (i + 1);
            }

            otherPlayer.name = botName;
            otherPlayer.GetComponent<NameScript>().SetName(botName);

            spawnedPlayers.Add(otherPlayer);
        }

        // Wire into the board game manager
        BoardGameManager boardManager = FindFirstObjectByType<BoardGameManager>();
        if (boardManager != null)
        {
            boardManager.SetupPlayers(spawnedPlayers);
        }
    }
    string[]ReadLinesFromFile(string fileName){

        TextAsset textAsset = Resources.Load<TextAsset>(textFileName);

        if(textAsset !=null){
            return textAsset.text.Split(new[] { '\r', '\n' },
             System.StringSplitOptions.RemoveEmptyEntries);
        }else{
            Debug.LogWarning("File not found: " + fileName);
            return new string[0];
        }
    }

    void ShuffleNames(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int swapIndex = Random.Range(i, list.Count);
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }
    }
}
