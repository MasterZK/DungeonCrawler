using System;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    private DungeonValues dungeonValues;

    private void Awake()
    {
        var gameObject = new GameObject("DungeonValues");
        dungeonValues = gameObject.AddComponent<DungeonValues>();

        DontDestroyOnLoad(gameObject);
    }

    public void CloseGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
 