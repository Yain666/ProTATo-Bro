using UnityEngine;
using UnityEditor;
using System.IO;
using System.Data;
using ExcelDataReader;
using Newtonsoft.Json;
using System.Collections.Generic;

public class ExcelToJsonConverter : EditorWindow
{
    [MenuItem("Tools/一键批量转换 Excel 为 Json")]
    public static void BatchConvertExcelToJson()
    {
        // 1. 设置你指定的绝对路径 (加了 @ 符号可以防止 \ 被转义)
        string excelFolder = @"D:\UnityProject\2DModulePlay\Assets\Data\Excel";
        string jsonFolder  = @"D:\UnityProject\2DModulePlay\Assets\Resources\Config\DataJson";

        // 如果Excel文件夹不存在，直接报错提示
        if (!Directory.Exists(excelFolder))
        {
            Debug.LogError($"找不到Excel文件夹，请检查路径: {excelFolder}");
            return;
        }

        // 如果Json目标文件夹不存在，自动帮你创建出来
        if (!Directory.Exists(jsonFolder))
        {
            Directory.CreateDirectory(jsonFolder);
        }

        // 注册编码提供程序（解决编码报错，基于你刚刚放入的 System.Text.Encoding.CodePages.dll）
        //System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        // 2. 获取文件夹下所有的 .xlsx 文件
        string[] excelFiles = Directory.GetFiles(excelFolder, "*.xlsx");
        int successCount = 0;

        // 遍历所有找到的 Excel 文件
        for (int f = 0; f < excelFiles.Length; f++)
        {
            string excelPath = excelFiles[f];
            string fileName = Path.GetFileName(excelPath);

            // 【重点防坑】过滤掉 Excel 处于打开状态时产生的临时文件 (以 ~$ 开头)
            if (fileName.StartsWith("~$")) continue;

            // 显示 Unity 加载进度条，体验极佳
            EditorUtility.DisplayProgressBar("批量导出 Json", $"正在处理: {fileName} ({f + 1}/{excelFiles.Length})", (float)f / excelFiles.Length);

            try
            {
                using (FileStream stream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        DataSet result = reader.AsDataSet();
                        
                        // 默认只读第一页 (Sheet1)
                        if (result.Tables.Count == 0) continue;
                        DataTable table = result.Tables[0]; 

                        List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();

                        // 3. 遍历数据行 (假设前三行是：字段名、类型、注释，数据从第4行/索引3开始)
                        for (int i = 3; i < table.Rows.Count; i++)
                        {
                            var rowData = new Dictionary<string, object>();
                            bool isRowEmpty = true; // 用于检测是否是全空的尾行

                            for (int j = 0; j < table.Columns.Count; j++)
                            {
                                string fieldName = table.Rows[0][j].ToString().Trim();
                                string fieldType = table.Rows[1][j].ToString().Trim().ToLower();
                                string cellValue = table.Rows[i][j].ToString().Trim();

                                // 忽略没有配置字段名的列
                                if (string.IsNullOrEmpty(fieldName)) continue;

                                // 如果这一行有任何一个格子有数据，就说明不是空行
                                if (!string.IsNullOrEmpty(cellValue)) isRowEmpty = false;

                                // 4. 解析各种数据类型 (包含基础类型和数组)
                                try
                                {
                                    if (fieldType == "int")
                                        rowData[fieldName] = string.IsNullOrEmpty(cellValue) ? 0 : int.Parse(cellValue);
                                    else if (fieldType == "float")
                                        rowData[fieldName] = string.IsNullOrEmpty(cellValue) ? 0f : float.Parse(cellValue);
                                    else if (fieldType == "bool")
                                        rowData[fieldName] = (cellValue == "1" || cellValue.ToLower() == "true");
                                        
                                    // 数组解析 (支持中文逗号、英文逗号、竖线)
                                    else if (fieldType == "int[]" || fieldType == "list_int")
                                    {
                                        if (string.IsNullOrEmpty(cellValue)) rowData[fieldName] = new int[0];
                                        else
                                        {
                                            string[] parts = cellValue.Split(new char[] { ',', '，', '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                                            int[] arr = new int[parts.Length];
                                            for (int k = 0; k < parts.Length; k++) arr[k] = int.Parse(parts[k].Trim());
                                            rowData[fieldName] = arr;
                                        }
                                    }
                                    else if (fieldType == "float[]" || fieldType == "list_float")
                                    {
                                        if (string.IsNullOrEmpty(cellValue)) rowData[fieldName] = new float[0];
                                        else
                                        {
                                            string[] parts = cellValue.Split(new char[] { ',', '，', '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                                            float[] arr = new float[parts.Length];
                                            for (int k = 0; k < parts.Length; k++) arr[k] = float.Parse(parts[k].Trim());
                                            rowData[fieldName] = arr;
                                        }
                                    }
                                    else if (fieldType == "string[]" || fieldType == "list_string")
                                    {
                                        if (string.IsNullOrEmpty(cellValue)) rowData[fieldName] = new string[0];
                                        else
                                        {
                                            string[] parts = cellValue.Split(new char[] { ',', '，', '|' }, System.StringSplitOptions.RemoveEmptyEntries);
                                            for (int k = 0; k < parts.Length; k++) parts[k] = parts[k].Trim();
                                            rowData[fieldName] = parts;
                                        }
                                    }
                                    else
                                    {
                                        // 默认当字符串
                                        rowData[fieldName] = cellValue;
                                    }
                                }
                                catch (System.Exception e)
                                {
                                    Debug.LogError($"表[{fileName}]解析报错！单元格[{i + 1}行, {j + 1}列], 内容:{cellValue}, 预期:{fieldType}. 错误:{e.Message}");
                                }
                            }

                            // 只有当这一行不是全空的时候，才加入列表（防止Excel下方被误触生成的无数空行）
                            if (!isRowEmpty)
                            {
                                dataList.Add(rowData);
                            }
                        }

                        // 5. 将解析好的列表转换为 Json 字符串
                        string jsonString = JsonConvert.SerializeObject(dataList, Formatting.Indented);

                        // 6. 动态生成 Json 文件名并写入 (去除了 .xlsx 后缀，换成 .json)
                        string jsonFileName = Path.GetFileNameWithoutExtension(excelPath) + ".json";
                        string finalJsonPath = Path.Combine(jsonFolder, jsonFileName);
                        
                        File.WriteAllText(finalJsonPath, jsonString, System.Text.Encoding.UTF8);
                        successCount++;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"文件 {fileName} 转换失败: {ex.Message}");
            }
        }

        // 清除进度条
        EditorUtility.ClearProgressBar();
        
        // 刷新 Unity 的 AssetDatabase，让它立刻识别生成出来的 Json 文件
        AssetDatabase.Refresh();

        Debug.Log($"<color=#00FF00><b>批量转换完成！成功导出了 {successCount} 个配置表！</b></color>\nJson路径: {jsonFolder}");
    }
}
