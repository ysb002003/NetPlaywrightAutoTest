using AutoTest.Models;
using ClosedXML.Excel;

namespace AutoTest.Services;

public sealed class ExcelTestCaseService
{
    private static readonly string[] RequiredHeaders =
    [
        "CaseName",
        "Description",
        "Steps",
        "ExpectedResult",
        "ActualResult",
        "ExecutionStatus",
        "ExecutionTime",
        "Executor",
        "ExecutionResult"
    ];

    public IReadOnlyList<TestCaseItem> LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Excel file not found.", filePath);
        }

        using var workbook = new XLWorkbook(filePath);
        var sheet = workbook.Worksheets.FirstOrDefault();
        if (sheet is null)
        {
            throw new InvalidOperationException("No worksheet found.");
        }

        var headerRow = sheet.Row(1);
        var indexMap = BuildHeaderIndexMap(headerRow);
        ValidateHeaders(indexMap);

        var cases = new List<TestCaseItem>();
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        for (var row = 2; row <= lastRow; row++)
        {
            if (string.IsNullOrWhiteSpace(sheet.Cell(row, indexMap["CaseName"]).GetString()))
            {
                continue;
            }

            cases.Add(new TestCaseItem(
                GetCell(sheet, row, indexMap["CaseName"]),
                GetCell(sheet, row, indexMap["Description"]),
                GetCell(sheet, row, indexMap["Steps"]),
                GetCell(sheet, row, indexMap["ExpectedResult"]),
                GetCell(sheet, row, indexMap["ActualResult"]),
                GetCell(sheet, row, indexMap["ExecutionStatus"]),
                GetCell(sheet, row, indexMap["ExecutionTime"]),
                GetCell(sheet, row, indexMap["Executor"]),
                GetCell(sheet, row, indexMap["ExecutionResult"])));
        }

        return cases;
    }

    private static Dictionary<string, int> BuildHeaderIndexMap(IXLRow headerRow)
    {
        var map = new Dictionary<string, int>();
        var lastCell = headerRow.LastCellUsed()?.Address.ColumnNumber ?? 0;
        for (var col = 1; col <= lastCell; col++)
        {
            var text = headerRow.Cell(col).GetString().Trim();
            if (!string.IsNullOrWhiteSpace(text))
            {
                map[text] = col;
            }
        }

        return map;
    }

    private static void ValidateHeaders(IReadOnlyDictionary<string, int> indexMap)
    {
        var missing = RequiredHeaders.Where(x => !indexMap.ContainsKey(x)).ToList();
        if (missing.Count > 0)
        {
            throw new InvalidOperationException($"Missing required columns: {string.Join(", ", missing)}");
        }
    }

    private static string GetCell(IXLWorksheet sheet, int row, int col)
    {
        return sheet.Cell(row, col).GetString().Trim();
    }
}
