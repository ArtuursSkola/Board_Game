using UnityEngine;
using TMPro;

public class ChooseCharacterScript : MonoBehaviour
{
    public GameObject[] characters;
    int characterIndex;

    [Header("Inputs")]
    public GameObject inputField; // Name field (existing)
    public TMP_InputField playerCountField; // New: number of players 2-6
    string characterName;
    public int playerCount = 2;
    public SceneChanger sceneChanger;

    private void Awake()
    {
        characterIndex = 0;
        if (characters != null)
        {
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i] != null) characters[i].SetActive(false);
            }
            if (characters.Length > 0 && characters[characterIndex] != null)
                characters[characterIndex].SetActive(true);
        }
    }
    public void NextCharacter()
    {
        characters[characterIndex].SetActive(false);
        characterIndex++;
        if(characterIndex == characters.Length)
        {
          characterIndex = 0;
        }
        characters[characterIndex].SetActive(true);
    }

        public void PreviousCharacter()
    {
        characters[characterIndex].SetActive(false);
        characterIndex--;
        if(characterIndex == -1)
        {
           characterIndex = characters.Length - 1; 
        }
        characters[characterIndex].SetActive(true);
        
    }

    public void Play()
    {
        var nameInput = inputField.GetComponent<TMP_InputField>();
        characterName = nameInput != null ? nameInput.text : string.Empty;

        // Validate player count from input field (default to current playerCount)
        int desiredCount = playerCount;
        if (playerCountField != null)
        {
            if (!int.TryParse(playerCountField.text, out desiredCount) || desiredCount < 2 || desiredCount > 6)
            {
                // Reject invalid input; reselect the field and early-return
                playerCountField.text = "";
                playerCountField.ActivateInputField();
                return;
            }
        }

        if (characterName.Length >= 3)
        {
            PlayerPrefs.SetInt("SelectedCharacter", characterIndex);
            PlayerPrefs.SetString("PlayerName", characterName);
            PlayerPrefs.SetInt("PlayerCount", desiredCount);
            StartCoroutine(sceneChanger.Delay("play", characterIndex, characterName));
        }
        else
        {
            if (nameInput != null) nameInput.Select();
        }
    }
}
