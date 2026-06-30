using UnityEngine;

[System.Serializable]
public class RunStartContextData
{
    public int selectedCharacterId;
    public int selectedWeaponId;
    public int selectedLevel;
    public bool isEndless;
    public bool isBanSystemEnabled;
}

public class RunStartContext : MonoBehaviour
{
    private static RunStartContext instance;

    public static RunStartContext Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<RunStartContext>();
                if (instance == null)
                {
                    GameObject go = new GameObject("RunStartContext");
                    instance = go.AddComponent<RunStartContext>();
                }
            }

            return instance;
        }
    }

    [SerializeField] private RunStartContextData data = new RunStartContextData();

    public RunStartContextData Data => data;
    public int SelectedCharacterId => data.selectedCharacterId;
    public int SelectedWeaponId => data.selectedWeaponId;
    public int SelectedLevel => data.selectedLevel;
    public bool IsEndless => data.isEndless;
    public bool IsBanSystemEnabled => data.isBanSystemEnabled;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetCharacter(int characterId)
    {
        data.selectedCharacterId = characterId;
    }

    public void ClearCharacter()
    {
        data.selectedCharacterId = 0;
    }

    public void SetWeapon(int weaponId)
    {
        data.selectedWeaponId = weaponId;
    }

    public void SetLevel(int level)
    {
        data.selectedLevel = Mathf.Max(0, level);
    }

    public void SetModes(bool endless, bool banSystem)
    {
        data.isEndless = endless;
        data.isBanSystemEnabled = banSystem;
    }

    public void ResetWeaponSelection()
    {
        data.selectedWeaponId = 0;
    }

    public void ResetDifficultySelection()
    {
        data.selectedLevel = 0;
        data.isEndless = false;
        data.isBanSystemEnabled = false;
    }
}
