using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSystemTester : MonoBehaviour
{
    [Header("测试关卡配置")]
    public int testLevel = 1;

    private void OnGUI()
    {
        // 在游戏画面的左上角绘制控制面板
        GUILayout.BeginArea(new Rect(20, 20, 350, 600));
        
        GUILayout.Label("<size=18><b>=== 刷怪系统测试面板 ===</b></size>");
        GUILayout.Space(10);

        // 模拟玩家选择关卡，GameManager 下达初始化指令
        if (GUILayout.Button("1. 初始化关卡数据 (Init)", GUILayout.Height(40)))
        {
            if (MonsterManager.Instance != null)
            {
                MonsterManager.Instance.Init(testLevel);
                Debug.Log("【测试】GameManager 成功下达关卡初始化指令");
            }
        }

        // 模拟商店关闭，开启下一波
        if (GUILayout.Button("2. 开启下一大波 (NextWave)", GUILayout.Height(40)))
        {
            if (MonsterManager.Instance != null)
            {
                MonsterManager.Instance.NextWave();
                Debug.Log("【测试】GameManager 成功下达开启新波次指令");
            }
        }

        GUILayout.Space(20);
        GUILayout.Label("<size=14><b>=== 场景活跃怪物状态监视 ===</b></size>");

        // 兼容低版本 Unity，使用 FindObjectsOfType 查找在场的所有 Monster 组件
        Monster[] activeMonsters = FindObjectsOfType<Monster>();
        GUILayout.Label($"当前在场怪物数量: {activeMonsters.Length} / (最大上限 5)");

        GUILayout.BeginVertical("box");
        foreach (var monster in activeMonsters)
        {
            if (!monster.gameObject.activeInHierarchy) continue;

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{monster.MonsterName} ({monster.gameObject.name})");
            
            // 点击该按钮，手动模拟该怪物被打死
            if (GUILayout.Button("手动击杀", GUILayout.Width(80)))
            {
                monster.Die();
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.EndArea();
    }
}
