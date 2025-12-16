using TMPro;
using UnityEngine;

public class LeaderboardRowUI : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI lineText;

    // Displays a single combined line like "Imants (bot), 7 Moves, 03:58 sec, Score: 7143"
    public void SetData(int rank, string line)
    {
        if (rankText != null) rankText.text = "#" + rank;
        if (lineText != null) lineText.text = line;
    }
}
