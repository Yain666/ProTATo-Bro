using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttributeTester : MonoBehaviour
{
    [Header("测试关卡配置")]
    public int testLevel = 1;

    // 当前正在检视属性的目标怪物
    private Monster _selectedMonster;

    private void OnGUI()
    {
        // 在屏幕左上角绘制一个稍宽的测试面板 (宽450，高700)
        GUILayout.BeginArea(new Rect(20, 20, 450, 700));
        
        GUILayout.Label("<size=18><b>=== 怪物属性注入测试面板 ===</b></size>");
        GUILayout.Space(10);

        // 1. 初始化关卡
        if (GUILayout.Button("1. 初始化关卡数据 (Init)", GUILayout.Height(35)))
        {
            if (MonsterManager.Instance != null)
            {
                MonsterManager.Instance.Init(testLevel);
                Debug.Log("【测试】GameManager 下达关卡初始化指令");
            }
        }

        // 2. 开启波次
        if (GUILayout.Button("2. 开启下一大波 (NextWave)", GUILayout.Height(35)))
        {
            if (MonsterManager.Instance != null)
            {
                MonsterManager.Instance.NextWave();
                Debug.Log("【测试】GameManager 下达开启新波次指令");
            }
        }

        GUILayout.Space(15);
        GUILayout.Label("<size=14><b>场景活跃怪物列表 (点击选择监视)</b></size>");

        // 查找在场的所有怪物
        Monster[] activeMonsters = FindObjectsOfType<Monster>();
        
        GUILayout.BeginVertical("box", GUILayout.MaxHeight(150));
        if (activeMonsters.Length == 0)
        {
            GUILayout.Label("当前场景中无活跃怪物。");
        }
        foreach (var monster in activeMonsters)
        {
            if (!monster.gameObject.activeInHierarchy) continue;

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{monster.MonsterName} (ID: {monster.GetInstanceID()})");
            
            // 点击选择，下方会实时显示该怪物的属性
            if (GUILayout.Button("选择监视", GUILayout.Width(80)))
            {
                _selectedMonster = monster;
            }

            // 点击手动击杀，回收到对象池
            if (GUILayout.Button("击杀", GUILayout.Width(50)))
            {
                monster.Die();
                if (_selectedMonster == monster) _selectedMonster = null;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.Space(15);

        // 3. 核心：属性检视面板
        if (_selectedMonster != null && _selectedMonster.gameObject.activeInHierarchy)
        {
            GUILayout.Label($"<size=14><b>属性实时检视: <color=yellow>{_selectedMonster.MonsterName}</color></b></size>");
            
            // 获取怪物身上的属性系统
            CharacterStatus status = _selectedMonster.GetComponent<CharacterStatus>();
            if (status != null)
            {
                GUILayout.BeginVertical("box");
                
                // 打印我们在 MonsterData.json 中重点配置的 8 项属性
                DrawPropertyRow(status, PropertyType.MaxHp, "最大生命值 (MaxHp) [ID: 1]");
                DrawPropertyRow(status, PropertyType.CurrentHp, "当前生命值 (CurrentHp) [ID: 2]");
                DrawPropertyRow(status, PropertyType.Speed, "速度 (Speed) [ID: 15]");
                DrawPropertyRow(status, PropertyType.Armor, "护甲 (Armor) [ID: 13]");
                DrawPropertyRow(status, PropertyType.AttackSpeed, "攻击速度 (AttackSpeed) [ID: 9]");
                DrawPropertyRow(status, PropertyType.DamagePercent, "伤害加成% (DamagePercent) [ID: 5]");
                DrawPropertyRow(status, PropertyType.MeleeDamage, "近战伤害 (MeleeDamage) [ID: 6]");
                DrawPropertyRow(status, PropertyType.Range, "范围 (Range) [ID: 12]");
                
                GUILayout.Space(10);
                
                // 【联动测试】：调用您 CharacterStatus.cs 里的受伤计算公式
                if (GUILayout.Button("模拟怪物受到 5 点原始伤害 (测试公式与扣血)", GUILayout.Height(30)))
                {
                    // 调用您自己写的伤害扣除公式，会触发护甲减伤判定和 Death 检测 [1]
                    status.TakeDamage(5,"Player"); 
                }
                
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.Label("<color=red>警告：该怪物的预制体上未挂载 CharacterStatus (MonsterStatus) 组件！</color>");
            }
        }
        else
        {
            GUILayout.Label("<i>请在上方怪物列表中选择一个生成的怪物以进行属性监视。</i>");
        }

        GUILayout.EndArea();
    }

    /// <summary>
    /// 绘制单行属性的辅助方法
    /// </summary>
    private void DrawPropertyRow(CharacterStatus status, PropertyType type, string labelName)
    {
        float val = status.GetPropertyValue(type);
        GUILayout.BeginHorizontal();
        GUILayout.Label(labelName);
        GUILayout.FlexibleSpace();
        // 渲染属性数值
        GUILayout.Label($"<b><color=cyan>{val}</color></b>");
        GUILayout.EndHorizontal();
    }
}
