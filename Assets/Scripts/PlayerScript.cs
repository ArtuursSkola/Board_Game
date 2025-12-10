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
    int[] otherPlayers;
    int index;
    private const string textFileName = "PlayerName";


    void Start()
    {
        characterIndex = PlayerPrefs.GetInt("SelectedCharacter", 0);
        GameObject mainCharacter = Instantiate(
            playerPrefabs[characterIndex], spawnPoint.transform.position, Quaternion.identity);
        mainCharacter.GetComponent<NameScript>().SetName(
            PlayerPrefs.GetString("PlayerName", "Moe Lester"));
        Combatant playerCombatant = mainCharacter.GetComponent<Combatant>();
        if (playerCombatant == null) playerCombatant = mainCharacter.AddComponent<Combatant>();
        
        otherPlayers = new int[PlayerPrefs.GetInt("PlayerCount")];
        string[] nameArray = ReadLinesFromFile(textFileName);
        Combatant enemyCombatant = null;

        for(int i=0; i<otherPlayers.Length-1; i++)
        {
            spawnPoint.transform.position += new Vector3(0.2f, 0, 0.08f);
            index = Random.Range(0, playerPrefabs.Length);
            GameObject otherPlayer = Instantiate(
                playerPrefabs[index], spawnPoint.transform.position, Quaternion.identity);
            otherPlayer.GetComponent<NameScript>().SetName(
                nameArray[Random.Range(0, nameArray.Length)]);

            if (enemyCombatant == null)
            {
                enemyCombatant = otherPlayer.GetComponent<Combatant>();
                if (enemyCombatant == null) enemyCombatant = otherPlayer.AddComponent<Combatant>();
            }
        }

        // Find the TurnBasedGameManager in scene and wire it up with the spawned combatants
        TurnBasedGameManager manager = FindFirstObjectByType<TurnBasedGameManager>();
        CombatUI combatUi = FindFirstObjectByType<CombatUI>();
        DiceController dice = FindFirstObjectByType<DiceController>();
        if (manager != null && playerCombatant != null && enemyCombatant != null)
        {
            manager.Setup(playerCombatant, enemyCombatant, dice, combatUi);
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
}
