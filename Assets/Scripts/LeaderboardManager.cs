using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject leaderboardPanel;
    public Transform contentRoot;
    public GameObject entryPrefab; // should contain LeaderboardRowUI
    public TextMeshProUGUI emptyText;
    [Tooltip("Maximum entries to keep in the leaderboard.")]
    public int maxEntries = 20;

    [Header("Storage")]
    public string fileName = "leaderboard.json";

    [Serializable]
    public class Entry
    {
        public string name;
        public int score;
        public bool isBot;
        public string timestamp;
        public int moves;
        public float elapsedSeconds;
    }

    [Serializable]
    private class EntryList
    {
        public List<Entry> entries = new List<Entry>();
    }

    private List<Entry> entries = new List<Entry>();

    void Awake()
    {
        Load();
        RefreshUI();
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    public void ShowLeaderboard()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(true);
        RefreshUI();
    }

    public void HideLeaderboard()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }

    public void AddEntry(string name, int score, bool isBot, int moves, float elapsedSeconds)
    {
        Entry e = new Entry
        {
            name = string.IsNullOrEmpty(name) ? "Player" : name,
            score = Mathf.Max(0, score),
            isBot = isBot,
            timestamp = DateTime.UtcNow.ToString("o"),
            moves = Mathf.Max(0, moves),
            elapsedSeconds = Mathf.Max(0f, elapsedSeconds)
        };
        entries.Add(e);
        entries.Sort((a, b) => b.score.CompareTo(a.score));
        if (entries.Count > maxEntries) entries.RemoveRange(maxEntries, entries.Count - maxEntries);
        Save();
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (contentRoot == null || entryPrefab == null) return;

        // clear old
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }

        bool hasAny = entries.Count > 0;
        if (emptyText != null) emptyText.gameObject.SetActive(!hasAny);

        for (int i = 0; i < entries.Count; i++)
        {
            var go = Instantiate(entryPrefab, contentRoot);
            var row = go.GetComponent<LeaderboardRowUI>();
            if (row != null)
            {
                string displayName = entries[i].name + (entries[i].isBot ? " (bot)" : "");
                string timeStr = FormatElapsed(entries[i].elapsedSeconds);
                string line = string.Format("{0}, {1} Moves, {2}, Score: {3}", displayName, entries[i].moves, timeStr, entries[i].score);
                row.SetData(i + 1, line);
            }
            else
            {
                var text = go.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    string displayName = entries[i].name + (entries[i].isBot ? " (bot)" : "");
                    string timeStr = FormatElapsed(entries[i].elapsedSeconds);
                    text.text = string.Format("#{0} {1}, {2} Moves, {3}, Score: {4}", i + 1, displayName, entries[i].moves, timeStr, entries[i].score);
                }
            }
        }
    }

    private void Load()
    {
        string path = GetPath();
        if (!File.Exists(path))
        {
            entries = new List<Entry>();
            return;
        }
        try
        {
            string json = File.ReadAllText(path);
            var list = JsonUtility.FromJson<EntryList>(json);
            entries = list != null && list.entries != null ? list.entries : new List<Entry>();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Failed to load leaderboard: " + ex.Message);
            entries = new List<Entry>();
        }
    }

    private void Save()
    {
        string path = GetPath();
        var list = new EntryList { entries = entries };
        try
        {
            string json = JsonUtility.ToJson(list, true);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Failed to save leaderboard: " + ex.Message);
        }
    }

    private string GetPath()
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    private string FormatElapsed(float seconds)
    {
        float clamped = Mathf.Max(0f, seconds);
        int mins = Mathf.FloorToInt(clamped / 60f);
        int secs = Mathf.FloorToInt(clamped % 60f);
        return string.Format("{0:00}:{1:00} sec", mins, secs);
    }
}
