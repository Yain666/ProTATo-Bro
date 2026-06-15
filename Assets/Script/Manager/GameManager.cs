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
    
    // TODO: 这里后面的流程就是将这个放到开启游戏后面，就不在Start里面调用了
    public void DataInit()
    {
        CharacterDataController.Instance.Init();
        BasicPropertiesDataController.Instance.Init();
        
    }
    
    private void MonsterInit()
    {
        
    }

    // 商店关闭、关卡开启
    private void NextWave()
    {
        MonsterNextWave();
    }

    private void MonsterNextWave()
    {
        
    }
}
