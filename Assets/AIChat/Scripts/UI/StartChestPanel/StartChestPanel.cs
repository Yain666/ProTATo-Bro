using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class StartChestPanel : MonoBehaviour
{
    #region --- Properties ---
    
    private CommodityType _columnType;
    
    public int propColumnNum = 3;// 道具框数量
    public int weaponColumnNum = 3;// 武器框数量
    private LevelData LevelData;
    
    #endregion --- Properties ---

    #region --- Sturctures ---
    
    private Queue<ChestData> _chestQueue = new Queue<ChestData>();
    
    #endregion --- Sturctures ---
    
    #region --- Config ---

    private const string BTN_SELECTCOLOR = "#FFEE00";
    private const string BTN_NORMALCOLOR = "#FFFFFF";
    
    #endregion --- Config ---
    
    #region --- Component ---
    
    private BtnWidget continueGameBtn;
    private BtnWidget cancelGameBtn;
    private BtnWidget updateCommodityBtn;
    private Text textTitleRound;
    private ColumnWidget columnWidget;
    
    #endregion --- Component ---

    public void Awake()
    {
        InitComponents();
        WhenShowMe();
    }

    private void InitComponents()
    {
        continueGameBtn = transform.Find("Btn_ContinueGame").GetComponent<BtnWidget>();
        continueGameBtn.BindButton(ContinueGame);
        cancelGameBtn = transform.Find("Btn_CancelPanel").GetComponent<BtnWidget>();
        cancelGameBtn.BindButton(ContinueGame);
        updateCommodityBtn = transform.Find("Btn_UpdateCommodity").GetComponent<BtnWidget>();
        updateCommodityBtn.BindButton(UpdateCommodity);
        textTitleRound = transform.Find("Text_Title_Round").GetComponent<Text>();
        columnWidget = transform.Find("ColumnWidget").GetComponent<ColumnWidget>();
        columnWidget.InitComponents();
    }
    
    #region --- ButtonMethods ---

    public void ContinueGame() // 继续游戏按钮, 退出商店，开始游戏倒计时建造按钮。TODO: 开始游戏倒计时没做
    {
        // 游戏倒计时，关闭留到最后
        UIManager.Instance.HidePanel("StartChestPanel");
        UIManager.Instance.ShowPanel<GameMainPanel>("GameMainPanel", UILayer.Top,(panel) =>
        {
            panel.InitComponents();
        });
    }

    private void UpdateCommodity() // 暂时不知道怎么写，不懂怎么播放广告，TODO: 播放广告方法 , 刷新道具方法
    {
        // Debug.Log("广告 Timing 股！");
    }
    
    #endregion --- ButtonMethods ---
    
    #region --- HelpMethods ---
    
    public void WhenShowMe() // 在这里初始化数据信息，安排使用的道具、武器 具体该方法的调用位置在父类中有写 
    {
        LevelData = GameManager.Instance.waveController.GetCurLevel();
        List<ChestData> chestDatas = UIManager.Instance.GetChestData(LevelData.level, 0);
        foreach (var chestData in chestDatas)
        {
            _chestQueue.Enqueue(chestData);
        }
        InitInfo_Panel();
    }
    
    public void InitInfo_Panel()// 初始化面板数据信息，包含需要刷新的道具、当前道具栏状态、角色状态、第几波的Text改变等
    {
        textTitleRound.text = $"第0波（共{UIManager.Instance.GetRoundCountByLevel(LevelData.level)}）";
        switch (_chestQueue.Peek().contentType)
        {
            case 1:
                InitInfo_WeaponColumn();
                break;
            case 2:
                InitInfo_PropColumn();
                break;
            case 3:
                // Debug.Log("这个是到时候为了可以复制过去, 留给武器道具混合宝箱的位置");
                break;
            default:
                break;
        }
    }

    public void InitInfo_PropColumn()//  初始化道具栏数据信息,切换时、初始化时使用
    {
        columnWidget.InitInfo_PropColumn();
    }
    
    public void InitInfo_WeaponColumn()//  初始化武器栏数据信息,切换时、初始化时使用
    {
        columnWidget.InitInfo_WeaponColumn();
    }
    
    #endregion --- HelpMethods ---
}


