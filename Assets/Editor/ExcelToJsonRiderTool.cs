using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;
using LicenseContext = System.ComponentModel.LicenseContext;

/// <summary>
/// Excel 批量转 JSON 工具（Rider + Unity 适配版）
/// 功能：批量解析.xlsx文件，生成格式化JSON，输出到Resources目录方便游戏读取
/// 适配 Rider 开发环境，兼容 Unity 2020+. 需要安装NewtonsoftJson + EPPlus
/// </summary>
public class ExcelToJsonRiderTool : EditorWindow
{
    // 路径配置（默认指向项目内的默认目录，你可以直接修改）
    private string _excelFolder;
    private string _jsonOutputFolder;

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(_excelFolder))
        {
            _excelFolder = Path.Combine(Application.dataPath, "Data\\Excel");    
        }

        if (string.IsNullOrEmpty(_jsonOutputFolder))
        {
            _jsonOutputFolder = Path.Combine(Application.dataPath, "Resources\\Config\\DataJson");
        }
    }
    
    // 静态构造函数：提前设置EPPlus许可证（避免运行时报错）
    static ExcelToJsonRiderTool()
    {
        //ExcelPackage.LicenseContext = LicenseContext.NonCommercialUse;
    }

    /// <summary>
    /// 打开工具窗口（Unity菜单入口，Rider中可直接通过菜单调用）
    /// </summary>
    [MenuItem("Tools/Excel工具/批量转JSON", false, 100)]
    private static void OpenToolWindow()
    {
        var window = GetWindow<ExcelToJsonRiderTool>("Excel → JSON");
        window.minSize = new Vector2(480, 240); // 适配Rider窗口布局
        window.Show();
    }

    /// <summary>
    /// 绘制工具窗口UI（就是Unity里面的最上方工具栏）GUI
    /// </summary>
    private void OnGUI()
    {
        // 标题区域
        GUILayout.Space(10);
        GUILayout.Label("📊 Excel 批量转 JSON 配置", EditorStyles.boldLabel);
        GUILayout.Space(5);
        EditorGUILayout.LabelField("📌 规则：仅支持.xlsx | 第一行是表头 | 第二行开始是数据", EditorStyles.miniLabel);
        GUILayout.Space(10);

        // 路径配置区域（带边框）
        using (new EditorGUILayout.VerticalScope("Box"))
        {
            // Excel源文件夹
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Excel 源文件夹", GUILayout.Width(110));
            _excelFolder = EditorGUILayout.TextField(_excelFolder);
            if (GUILayout.Button("📁", GUILayout.Width(32)))
            {
                var selectedFolder = EditorUtility.OpenFolderPanel("选择Excel文件夹", _excelFolder, "");
                if (!string.IsNullOrEmpty(selectedFolder))
                {
                    _excelFolder = selectedFolder;
                }
            }
            EditorGUILayout.EndHorizontal();

            // JSON输出文件夹（默认指向Resources，方便游戏读取）
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("JSON 输出文件夹", GUILayout.Width(110));
            _jsonOutputFolder = EditorGUILayout.TextField(_jsonOutputFolder);
            if (GUILayout.Button("📁", GUILayout.Width(32)))
            {
                var selectedFolder = EditorUtility.OpenFolderPanel("选择JSON输出文件夹", _jsonOutputFolder, "");
                if (!string.IsNullOrEmpty(selectedFolder))
                {
                    _jsonOutputFolder = selectedFolder;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(15);

        // 转换按钮（禁用状态：Excel文件夹不存在时）
        using (new EditorGUI.DisabledScope(!Directory.Exists(_excelFolder)))
        {
            if (GUILayout.Button("🚀 开始批量转换", GUILayout.Height(48)))
            {
                BatchConvertExcelToJson();
            }
        }

        // 警告提示：文件夹不存在
        if (!Directory.Exists(_excelFolder))
        {
            GUILayout.Space(8);
            EditorGUILayout.HelpBox(
                $"⚠️ Excel文件夹不存在：{_excelFolder}\n请先创建文件夹并放入.xlsx文件", 
                MessageType.Warning
            );
        }
    }

    /// <summary>
    /// 批量转换核心逻辑（带进度条，Rider中可断点调试）
    /// </summary>
    private void BatchConvertExcelToJson()
    {
        // 确保输出文件夹存在
        if (!Directory.Exists(_jsonOutputFolder))
        {
            Directory.CreateDirectory(_jsonOutputFolder);
            AssetDatabase.Refresh(); // 刷新Unity/Rider资源视图
        }

        // 获取所有.xlsx文件（仅遍历顶层目录，避免嵌套文件夹干扰）
        var excelFiles = Directory.GetFiles(_excelFolder, "*.xlsx", SearchOption.TopDirectoryOnly);
        if (excelFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "未找到任何.xlsx格式的Excel文件！", "确定");
            return;
        }

        // 初始化统计数据
        int successCount = 0;
        int failCount = 0;
        EditorUtility.DisplayProgressBar("Excel转JSON", "正在处理...", 0f);

        // 遍历转换每个Excel文件
        for (int i = 0; i < excelFiles.Length; i++)
        {
            var filePath = excelFiles[i];
            var fileName = Path.GetFileName(filePath);
            
            // 更新进度条（Rider中可直观看到转换进度）
            EditorUtility.DisplayProgressBar(
                "Excel转JSON", 
                $"正在转换：{fileName} ({i+1}/{excelFiles.Length})", 
                (float)i / excelFiles.Length
            );

            try
            {
                ConvertSingleExcel(filePath);
                successCount++;
                Debug.Log($"✅ 转换成功：{fileName}");
            }
            catch (Exception ex)
            {
                failCount++;
                // Rider中输出详细错误堆栈，方便调试
                Debug.LogError($"❌ 转换失败 [{fileName}]：{ex.Message}\n{ex.StackTrace}");
            }
        }

        // 完成清理
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh(); // 刷新资源，Rider立即可见新JSON

        // 转换结果提示
        var resultMsg = $"转换完成！\n✅ 成功：{successCount} 个\n❌ 失败：{failCount} 个\n📂 JSON路径：{_jsonOutputFolder}";
        EditorUtility.DisplayDialog("🎉 转换完成", resultMsg, "确定");
    }

    /// <summary>
    /// 单个Excel文件转换逻辑（核心解析）
    /// </summary>
    /// <param name="excelFilePath">Excel文件完整路径</param>
    private void ConvertSingleExcel(string excelFilePath)
    {
        // 使用using自动释放资源，避免Rider报内存泄漏警告
        using (var excelPackage = new ExcelPackage(new FileInfo(excelFilePath)))
        {
            // 读取第一个工作表（策划配置默认用第一个Sheet）
            var worksheet = excelPackage.Workbook.Worksheets[0];
            if (worksheet == null)
            {
                throw new Exception("工作表为空，无数据可转换");
            }

            // 获取Excel行列维度（Rider中可直接查看dimension值）
            var dimension = worksheet.Dimension;
            if (dimension == null || dimension.Rows < 2)
            {
                throw new Exception("无效数据：至少需要1行表头 + 1行数据");
            }

            int rowCount = dimension.Rows;
            int colCount = dimension.Columns;

            // 第一步：读取表头（第一行）
            var headers = new List<string>();
            for (int col = 1; col <= colCount; col++)
            {
                var header = worksheet.Cells[1, col].Text.Trim();
                if (string.IsNullOrEmpty(header))
                {
                    throw new Exception($"第{col}列表头为空，无法转换");
                }
                headers.Add(header);
            }

            // 第二步：读取数据行（第二行开始）
            var dataList = new List<Dictionary<string, object>>();
            for (int row = 2; row <= rowCount; row++)
            {
                var rowData = new Dictionary<string, object>();
                for (int col = 1; col <= colCount; col++)
                {
                    var cellValue = worksheet.Cells[row, col].Text.Trim();
                    // 自动转换数据类型（适配游戏配置常见类型：int/float/bool/string）
                    rowData.Add(headers[col - 1], ConvertCellValue(cellValue));
                }
                dataList.Add(rowData);
            }

            // 第三步：序列化JSON（格式化输出，Rider中易读）
            var jsonContent = JsonConvert.SerializeObject(
                dataList, 
                Formatting.Indented, // 格式化缩进
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore } // 忽略空值
            );

            // 生成JSON文件路径（UTF8无BOM，避免Unity读取乱码）
            var jsonFileName = Path.GetFileNameWithoutExtension(excelFilePath) + ".json";
            var jsonFilePath = Path.Combine(_jsonOutputFolder, jsonFileName);
            File.WriteAllText(jsonFilePath, jsonContent, new System.Text.UTF8Encoding(false));
        }
    }

    /// <summary>
    /// 自动转换单元格值类型（适配游戏配置场景）
    /// </summary>
    /// <param name="cellValue">Excel单元格文本值</param>
    /// <returns>强类型值（int/float/bool/string）</returns>
    private object ConvertCellValue(string cellValue)
    {
        // 空值返回空字符串
        if (string.IsNullOrEmpty(cellValue))
        {
            return string.Empty;
        }

        // 整数（ID/数量等）
        if (int.TryParse(cellValue, out int intValue))
        {
            return intValue;
        }

        // 浮点数（血量/速度/概率等）
        if (float.TryParse(cellValue, out float floatValue))
        {
            return floatValue;
        }

        // 布尔值（是否开启/是否有效等）
        if (bool.TryParse(cellValue, out bool boolValue))
        {
            return boolValue;
        }

        // 长整数（大数值ID/时间戳等）
        if (long.TryParse(cellValue, out long longValue))
        {
            return longValue;
        }

        // 其他情况返回字符串（名称/描述等）
        return cellValue;
    }
}
