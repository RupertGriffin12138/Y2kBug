using System.Collections.Generic;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using UnityEngine;

public class NPOIExcelReader
{
    /// <summary>
    /// 解析Excel
    /// </summary>
    public static List<Dictionary<string, string>> ParseDic(string path, int beginParseRow)
    {
        List<Dictionary<string, string>> result = new List<Dictionary<string, string>>();

        if (!File.Exists(path))
        {
            Debug.LogError("Excel文件不存在：" + path);
            return result;
        }

        using (FileStream fs = File.OpenRead(path))
        {
            XSSFWorkbook wk = new XSSFWorkbook(fs);
            ISheet firstSheet = wk.GetSheetAt(0);

            //title  row 0
            List<string> titleList = new List<string>();
            IRow titleRow = firstSheet.GetRow(1);

            for (int i = 0; i < titleRow.Cells.Count; i++)
            {
                titleList.Add(titleRow.GetCell(i).ToString());
            }
            for (int j = beginParseRow; j <= firstSheet.LastRowNum; j++)
            {
                Dictionary<string, string> titleToColValue = new Dictionary<string, string>();
                IRow row = firstSheet.GetRow(j);
                //Debug.Log(j);
                if (row.GetCell(0) == null || string.IsNullOrEmpty(row.GetCell(0).ToString()))
                {
                    break;
                }

                for (int i = 0; i < titleList.Count; i++)
                {
                    string value = row.GetCell(i) == null ? "" : row.GetCell(i).ToString();

                    titleToColValue.Add(titleList[i], value);
                }

                result.Add(titleToColValue);
            }

            fs.Close();
            fs.Dispose();
        }

        return result;
    }

    /// <summary>
    /// 解析Excel
    /// </summary>
    public static List<List<string>> Parse(string path, int beginParseRow, int colCount)
    {
        List<List<string>> result = new List<List<string>>();

        if (!File.Exists(path))
        {
            Debug.LogError("Excel文件不存在：" + path);
            return result;
        }

        using (FileStream fs = File.OpenRead(path))
        {
            XSSFWorkbook wk = new XSSFWorkbook(fs);
            ISheet firstSheet = wk.GetSheetAt(0);
            for (int j = beginParseRow; j <= firstSheet.LastRowNum; j++)
            {
                List<string> rowStrings = new List<string>();
                IRow row = firstSheet.GetRow(j);

                if (string.IsNullOrEmpty(row.GetCell(0).ToString()))
                {
                    break;
                }

                for (int i = 0; i < colCount; i++)
                {
                    rowStrings.Add(row.GetCell(i).ToString());
                }

                result.Add(rowStrings);
            }

            fs.Close();
            fs.Dispose();
        }

        return result;
    }
}