using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertiesComponent : MonoBehaviour
{
    #region --- Properties ---

    public List<BasicProperties> BasicProperties;
    public List<IncreaseProperties> IncreaseProperties;

    #endregion --- Properties ---

    public void Awake()
    {
        
    }

    // 



    // 初步设计是直接减去 Properties 的属性,后面等属性词条做完以后改成利用词条来增删改查, 这个样子是很危险和冗余的
    public void ChangeValue(PropertiesData changeData)
    {
        
    }

    public void CheckValue()
    {
        
    }
}
