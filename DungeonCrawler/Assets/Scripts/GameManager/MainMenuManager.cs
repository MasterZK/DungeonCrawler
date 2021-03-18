using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

enum SceneIndexes
{
    MainMenu = 0,
    GameScene = 1,
}

public class MainMenuManager : MonoBehaviour
{
    [Header("Dungeon Editor UI")]
    [SerializeField] private GameObject useSeedText;
    [SerializeField] private TextMeshProUGUI seedText;
    [SerializeField] private TMP_InputField seedInput;
    [SerializeField] private TextMeshProUGUI floorDensityText;
    [SerializeField] private TextMeshProUGUI floorSizeText;
    [SerializeField] private DungeonValues dungeonValues;
    [SerializeField] private Texture2D customMouseTexture = null;

    [Header("Loading Screen")] 
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider progressSlider;
    private AsyncOperation sceneLoading;

    private void Awake()
    {
        Cursor.visible = false;
        DontDestroyOnLoad(dungeonValues);

        seedInput.characterLimit = 10;
        UpdateEditorUI();
    }

    public void LoadGame()
    {
        StartCoroutine(LoadingScene());
    }

    IEnumerator LoadingScene()
    {
        loadingScreen.SetActive(true);
        sceneLoading = SceneManager.LoadSceneAsync((int)SceneIndexes.GameScene);
        sceneLoading.allowSceneActivation = false;

        while (sceneLoading.isDone == false)
        {
            progressSlider.value = sceneLoading.progress;
            if (sceneLoading.progress == 0.9f)
            {
                progressSlider.value = 1f;
                sceneLoading.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    public void SetCursor(bool state)
    {
        Cursor.visible = state;
    }

    public void SetCursorTexture(bool state)
    {
        if (state)
            Cursor.SetCursor(customMouseTexture, new Vector2(customMouseTexture.height / 2, customMouseTexture.width / 2), CursorMode.ForceSoftware);
        else
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
        
    }

    public void UpdateEditorUI()
    {
        useSeedText.SetActive(dungeonValues.useSeed);
        seedText.text = dungeonValues.seed;
        floorDensityText.text = dungeonValues.floorDensity.ToString();
        floorSizeText.text = dungeonValues.maxFloorSize.ToString();
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
