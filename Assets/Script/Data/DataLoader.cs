using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// 配置读取工具
/// </summary>
public class DataLoader : MonoBehaviour
{
    private void Start()
    {
        //LoadPlayerConfig();
    }
    
    /// <summary>
    /// 读取Resources下的玩家配置JSON
    /// </summary>
    
    // TODO: 后面要改成通用版的,然后在外部需要读取的时候将这个读取出来,好像我已经有ResourceManager了,后面再看看
    // private void LoadPlayerConfig()
    // {
    //     // 读取JSON（无需后缀，Rider中可快速跳转至Resources文件）
    //     var jsonAsset = Resources.Load<TextAsset>("PlayerConfig");
    //     if (jsonAsset == null)
    //     {
    //         Debug.LogError("❌ 未找到PlayerConfig.json配置文件");
    //         return;
    //     }
    //
    //     // 反序列化（Rider中可查看playerConfigs的具体数据）
    //     var playerConfigs = JsonConvert.DeserializeObject<List<PlayerConfig>>(jsonAsset.text);
    //     if (playerConfigs == null || playerConfigs.Count == 0)
    //     {
    //         Debug.LogWarning("⚠️ 玩家配置数据为空");
    //         return;
    //     }
    //
    //     // 输出配置信息（测试用）
    //     foreach (var config in playerConfigs)
    //     {
    //         Debug.Log($"📋 玩家ID：{config.ID} | 名称：{config.Name} | 血量：{config.HP}");
    //     }
    // }
}
