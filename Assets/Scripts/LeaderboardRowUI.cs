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
        if (lineText == null)
        {
            // Fallback: grab the first TMP child that is not the rank text
            var tmps = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var tmp in tmps)
            {
                if (tmp != null && tmp != rankText)
                {
                    lineText = tmp;
                    break;
                }
            }
        }
        if (lineText != null) lineText.text = line;
    }
}
