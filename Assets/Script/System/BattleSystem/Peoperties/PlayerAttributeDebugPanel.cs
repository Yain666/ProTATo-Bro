using Script.Player.PlayerComponent;
using UnityEngine;

public class PlayerAttributeDebugPanel : MonoBehaviour
{
    public PlayerStatus playerStatus;
    public float rangeStep = 5f;
    public float speedStep = 5f;
    public float dodgeStep = 5f;
    public float attackSpeedStep = 5f;
    public float luckStep = 5f;
    public float pickupRangeStep = 10f;

    private void OnGUI()
    {
        if (playerStatus == null)
        {
            playerStatus = FindObjectOfType<PlayerStatus>();
        }

        GUILayout.BeginArea(new Rect(20, 20, 380, 320), GUI.skin.box);
        GUILayout.Label("<b>玩家属性调试面板</b>");

        if (playerStatus == null)
        {
            GUILayout.Label("未找到 PlayerStatus");
            GUILayout.EndArea();
            return;
        }

        DrawStatRow("Range", PropertyType.Range, rangeStep);
        DrawStatRow("Speed", PropertyType.Speed, speedStep);
        DrawStatRow("Dodge", PropertyType.Dodge, dodgeStep);
        DrawStatRow("AttackSpeed", PropertyType.AttackSpeed, attackSpeedStep);
        DrawStatRow("Luck", PropertyType.Luck, luckStep);
        DrawStatRow("PickupRange", PropertyType.PickupRange, pickupRangeStep);

        GUILayout.Space(8f);
        GUILayout.Label($"MaxHp: {playerStatus.GetPropertyValue(PropertyType.MaxHp):F1}");
        GUILayout.Label($"CurrentHp: {playerStatus.GetPropertyValue(PropertyType.CurrentHp):F1}");
        GUILayout.Label($"MeleeDamage: {playerStatus.GetPropertyValue(PropertyType.MeleeDamage):F1}");
        GUILayout.Label($"RangedDamage: {playerStatus.GetPropertyValue(PropertyType.RangedDamage):F1}");

        GUILayout.EndArea();
    }

    private void DrawStatRow(string label, PropertyType type, float step)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{label}: {playerStatus.GetPropertyValue(type):F1}", GUILayout.Width(170));
        if (GUILayout.Button($"+{step}", GUILayout.Height(26)))
        {
            playerStatus.ModifyAttribute(type, step);
        }

        if (GUILayout.Button($"-{step}", GUILayout.Height(26)))
        {
            playerStatus.ModifyAttribute(type, -step);
        }
        GUILayout.EndHorizontal();
    }
}
