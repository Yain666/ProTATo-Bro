using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattleGroundTilemapController))]
public class BattleGroundTilemapControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(8f);
        BattleGroundTilemapController controller = (BattleGroundTilemapController)target;

        if (GUILayout.Button("刷新地板"))
        {
            controller.RefreshGround();
            EditorUtility.SetDirty(controller);
        }
    }
}
