using TMPro;
using UnityEngine;

public enum FloorSize
{
    small = 1,
    medium = 2,
    large = 3,
    extraLarge = 4,
}

public enum FloorDensity
{
    loose = 1,
    average = 2,
    tight = 3,
}

public class DungeonValues : MonoBehaviour
{
    [SerializeField] public string seed;
    [SerializeField] public bool useSeed;
    [SerializeField] public FloorDensity floorDensity;
    [SerializeField] public FloorSize maxFloorSize;

    private MainMenuManager manager;
    public bool edited { get; internal set; }
 
    private void Awake()
    {
        edited = false;
        floorDensity = FloorDensity.average;
        maxFloorSize = FloorSize.medium;
        seed = "Default";
        useSeed = true;

        manager = FindObjectOfType<MainMenuManager>();
        manager.UpdateEditorUI();
    }

    public void ChangeUseSeed()
    {
        edited = true;

        useSeed = !useSeed;
        manager.UpdateEditorUI();
    }

    public void SetSeed(TMP_InputField input)
    {
        edited = true;

        seed = input.text;
        manager.UpdateEditorUI();
    }

    public void ChangeFloorDensity(int change)
    {
        edited = true;

        if ((floorDensity == FloorDensity.tight && change == 1) || (floorDensity == FloorDensity.loose && change == -1))
            return;

        floorDensity += change;
        manager.UpdateEditorUI();
    }

    public void ChangeFloorSize(int change)
    {
        edited = true;

        if ((maxFloorSize == FloorSize.extraLarge && change == 1) || (maxFloorSize == FloorSize.small && change == -1))
            return;

        maxFloorSize += change;
        manager.UpdateEditorUI();
    }
}
