using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(this);
        DataInit();

        CharacterData data = CharacterDataController.Instance.GetCharacterById(1001);
        if (data != null)
        {
            Debug.Log($"瞬间拿到数据！角色名字是：{data.characterName}，职业：{data.job}");
        }
    }
    
    public void DataInit()
    {
        CharacterDataController.Instance.Init();
    }
}
