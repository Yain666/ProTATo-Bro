using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

/// <summary>
/// UI管理器
/// 1. 管理所有显示的面板
/// 2. 提供给外部 显示和隐藏等等的接口
/// </summary>
public class UIManager : MonoBehaviour
{
    #region --- Component ---
    //public  Dictionary<string,BasePanel> panelDic = new Dictionary<string, BasePanel>();
    public  Dictionary<string,GameObject> panelDictionary = new Dictionary<string, GameObject>();
    private Dictionary<int,Dictionary<int,List<ChestData>>> chestDataDic = new Dictionary<int, Dictionary<int,List<ChestData>>>();// <Level,<Round,Data>>
    
    public  RectTransform _canvas;
    private Transform bot;
    private Transform mid;
    private Transform top;
    private Transform system;
    
    public static UIManager Instance;

    #endregion --- Component ---
    
    public void Awake()
    {
        if (Instance == null) Instance = this;
        GameObject.DontDestroyOnLoad(this.gameObject);
        _canvas = GameObject.Find("Canvas").transform as RectTransform;
        //找到各层
        bot = _canvas.transform.Find("Bot");
        mid = _canvas.transform.Find("Mid");
        top = _canvas.transform.Find("Top");
        system = _canvas.transform.Find("System");
        InitializeChest();
    }

    // 显示面板, 如果你需要一些识别操作之类的, 可以丢到callBack再来做初始化\移位等操作, 比如你可以在Panel子脚本中留下改变位置的方法,然后在callback中调用,并传递位置。初始化亦是如此。
    public void ShowPanel<T>(string panelName,UILayer layer, UnityAction<T> callBack = null)// where T : BasePanel
    {
        Addressables.InstantiateAsync(panelName).Completed += (obj) =>
        {
            var resObj = obj.Result;
            if (obj.IsDone)
            {
                Transform father = GetParentLayer(layer);
                resObj.transform.SetParent(father,false);
                //resObj.transform.localPosition = Vector3.zero;
                resObj.transform.localScale = Vector3.one;
                
                T panel = resObj.gameObject.GetComponent<T>();
                if(callBack != null)
                    callBack(panel);
                panelDictionary.Add(panelName, resObj);
            }
        };
    }

    private Transform GetParentLayer(UILayer layer)
    {
        Transform father = bot;
        switch (layer)
        {
            case UILayer.Mid:
                father = mid;
                break;
            case UILayer.Top:
                father = top;
                break;
            case UILayer.System:
                father = system;
                break;
        }
        return father;
    }
    
    public void HidePanel(string panelName) // 隐藏面板
    {
        if (panelDictionary.ContainsKey(panelName))
        {
            Destroy(panelDictionary[panelName]);
            panelDictionary.Remove(panelName);
        }
    }
    
    public T GetPanel<T>(String panelName)// 获取面板对象
    {
        if(panelDictionary.ContainsKey(panelName))
            return panelDictionary[panelName].GetComponent<T>();
        return default(T);
    }
    
    private void InitializeChest() // 获取宝箱Chest配置数据
    {
        var chests = new List<ChestData>();
        chests = DataReader.Instance.ReadData<ChestData>("treasure_chest");

        foreach (var chestData in chests)
        {
            if (chestDataDic.ContainsKey(chestData.level))
            {
                if (chestDataDic[chestData.level].ContainsKey(chestData.round))
                {
                    chestDataDic[chestData.level][chestData.round].Add(chestData);
                }
                else
                {
                    chestDataDic[chestData.level].Add(chestData.round,new List<ChestData>(){chestData});
                }
            }
            else
            {
                chestDataDic.Add(chestData.level,new Dictionary<int, List<ChestData>>());
                chestDataDic[chestData.level].Add(chestData.round,new List<ChestData>(){chestData});
            }
        }
    }
    
    public List<ChestData> GetChestData(int level,int round) // 获取当前 关卡level 波次round 的所有的宝箱
    {
        if(!chestDataDic.ContainsKey(level)||!chestDataDic[level].ContainsKey(round))
            return null;
        
        return chestDataDic[level][round];
    }
    
    public int GetRoundCountByLevel(int level) // 获取当前 关卡level 的总波次round 数目
    {
        int roundCount = chestDataDic[level].Count;
        return roundCount;
    }
}


