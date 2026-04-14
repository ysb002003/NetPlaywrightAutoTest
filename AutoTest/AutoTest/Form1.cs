using AutoTest.Services;
using AutoTest.Models;
using System.Diagnostics;

namespace AutoTest
{
    public partial class Form1 : Form
    {
        private readonly TestCaseBatchRunner _batchRunner;
        private readonly ResultStore _resultStore;
        private readonly ExcelTestCaseService _excelTestCaseService;
        private readonly RecordedCsCaseService _recordedCsCaseService;
        private List<TestCaseItem> _importedCases = [];
        private IReadOnlyList<TestCaseExecutionResult> _lastCaseRunResults = [];
        private int _resultsSortColumn = -1;
        private bool _resultsSortAscending = true;
        private int _resultsCopyColumnIndex;

        public Form1()
        {
            InitializeComponent();
            _batchRunner = new TestCaseBatchRunner();
            _resultStore = new ResultStore();
            _excelTestCaseService = new ExcelTestCaseService();
            _recordedCsCaseService = new RecordedCsCaseService();
            txtUrl.Text = "https://www.baidu.com";
            txtCaseFile.Text = Path.Combine(AppContext.BaseDirectory, "TestCases.xlsx");
            LoadHistory();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.C) && lstResults.Focused && lstResults.SelectedItems.Count > 0)
            {
                CopySelectedResultsCell();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void BtnClearLog_Click(object sender, EventArgs e)
        {
            _resultStore.ClearAll();
            lstResults.Items.Clear();
            _lastCaseRunResults = [];
            _resultsSortColumn = -1;
            lstResults.ListViewItemSorter = null;
        }

        private void BtnOpenReportsFolder_Click(object sender, EventArgs e)
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "artifacts", "reports");
            Directory.CreateDirectory(dir);
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = dir,
                UseShellExecute = true
            });
        }

        private void LstResults_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (_resultsSortColumn == e.Column)
            {
                _resultsSortAscending = !_resultsSortAscending;
            }
            else
            {
                _resultsSortColumn = e.Column;
                _resultsSortAscending = true;
            }

            ApplyResultsSort();
        }

        private void LstResults_MouseDown(object sender, MouseEventArgs e)
        {
            var hit = lstResults.HitTest(e.Location);
            if (hit.Item is null)
            {
                return;
            }

            if (hit.SubItem is not null)
            {
                _resultsCopyColumnIndex = hit.Item.SubItems.IndexOf(hit.SubItem);
            }
            else
            {
                _resultsCopyColumnIndex = 0;
            }

            if (_resultsCopyColumnIndex < 0)
            {
                _resultsCopyColumnIndex = 0;
            }
        }

        private void MenuResultsCopyCell_Click(object sender, EventArgs e)
        {
            CopySelectedResultsCell();
        }

        private void MenuResultsCopyRow_Click(object sender, EventArgs e)
        {
            CopySelectedResultsRow();
        }

        private void CopySelectedResultsCell()
        {
            if (lstResults.SelectedItems.Count == 0)
            {
                return;
            }

            var item = lstResults.SelectedItems[0];
            if (item.SubItems.Count == 0)
            {
                return;
            }

            var col = Math.Clamp(_resultsCopyColumnIndex, 0, item.SubItems.Count - 1);
            var text = item.SubItems[col].Text;
            try
            {
                Clipboard.SetText(text ?? string.Empty);
            }
            catch
            {
                // Clipboard can throw if locked by another app.
            }
        }

        private void CopySelectedResultsRow()
        {
            if (lstResults.SelectedItems.Count == 0)
            {
                return;
            }

            var item = lstResults.SelectedItems[0];
            var parts = new List<string> { item.Text };
            for (var i = 1; i < item.SubItems.Count; i++)
            {
                parts.Add(item.SubItems[i].Text);
            }

            try
            {
                Clipboard.SetText(string.Join("\t", parts));
            }
            catch
            {
                // ignore clipboard errors
            }
        }

        private void ApplyResultsSort()
        {
            if (_resultsSortColumn < 0)
            {
                return;
            }

            lstResults.ListViewItemSorter = new ListViewItemComparer(_resultsSortColumn, _resultsSortAscending);
            lstResults.Sort();
        }

        private async void BtnRunSelectedCases_Click(object sender, EventArgs e)
        {
            var selected = new List<TestCaseItem>();
            foreach (ListViewItem item in lstTestCases.Items)
            {
                if (!item.Checked || item.Tag is not int idx || idx < 0 || idx >= _importedCases.Count)
                {
                    continue;
                }

                selected.Add(_importedCases[idx]);
            }

            if (selected.Count == 0)
            {
                MessageBox.Show("Check one or more test cases to run.", "AutoTest", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                await RunCasesInternalAsync(selected, "Excel");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Run Cases Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnExportReport_Click(object sender, EventArgs e)
        {
            if (_lastCaseRunResults.Count == 0)
            {
                MessageBox.Show("No case run results yet. Run selected cases first.", "AutoTest", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Filter = "HTML Report (*.html)|*.html",
                FileName = $"report_{DateTime.Now:yyyyMMdd_HHmmss}.html"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                ReportExporter.ExportHtml(_lastCaseRunResults, dialog.FileName);
                MessageBox.Show($"Report saved:\n{dialog.FileName}", "AutoTest", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRecordCase_Click(object sender, EventArgs e)
        {
            var targetUrl = string.IsNullOrWhiteSpace(txtUrl.Text) ? "https://pci.moa.com.au" : txtUrl.Text.Trim();
            var scriptPath = Path.Combine(AppContext.BaseDirectory, "playwright.ps1");
            if (!File.Exists(scriptPath))
            {
                MessageBox.Show(
                    $"playwright.ps1 not found:\n{scriptPath}\n\nBuild the project first, then retry.",
                    "AutoTest",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                StartCodegen("pwsh", scriptPath, targetUrl);
            }
            catch
            {
                try
                {
                    StartCodegen("powershell", scriptPath, targetUrl);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to start Playwright codegen:\n{ex.Message}", "AutoTest", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnSaveRecordedCase_Click(object sender, EventArgs e)
        {
            var text = Clipboard.ContainsText() ? Clipboard.GetText() : string.Empty;
            if (string.IsNullOrWhiteSpace(text))
            {
                MessageBox.Show("Clipboard is empty. Copy codegen output first.", "AutoTest", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var baseDir = _recordedCsCaseService.GetCasesDirectory();
            using var dialog = new SaveFileDialog
            {
                Title = "Save recorded case as .cs",
                InitialDirectory = baseDir,
                Filter = "C# files (*.cs)|*.cs",
                FileName = $"TC_{DateTime.Now:yyyyMMdd_HHmmss}.cs"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                _recordedCsCaseService.SaveFromCodegenText(text, dialog.FileName);
                MessageBox.Show($"Saved:\n{dialog.FileName}", "AutoTest", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save .cs Case Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnRunRecordedCsCases_Click(object sender, EventArgs e)
        {
            try
            {
                var cases = _recordedCsCaseService.LoadCases();
                if (cases.Count == 0)
                {
                    MessageBox.Show("No recorded .cs cases found in TestCasesCs.", "AutoTest", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                await RunCasesInternalAsync(cases, "Recorded .cs");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Run Recorded .cs Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void StartCodegen(string shell, string scriptPath, string targetUrl)
        {
            var info = new ProcessStartInfo
            {
                FileName = shell,
                UseShellExecute = true,
                Arguments = $"-NoExit -ExecutionPolicy Bypass -Command \"& '{scriptPath}' codegen --target csharp '{targetUrl}'\""
            };
            Process.Start(info);
        }

        private void AddResultRow(SmokeTestResult result)
        {
            var item = new ListViewItem(result.ExecutedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            item.SubItems.Add(result.CaseName ?? string.Empty);
            item.SubItems.Add(result.Status);
            item.SubItems.Add(result.Message);
            item.SubItems.Add(result.ScreenshotPath ?? string.Empty);
            item.SubItems.Add(result.VideoPath ?? string.Empty);
            lstResults.Items.Insert(0, item);
            ApplyResultsSort();
        }

        private void LoadHistory()
        {
            foreach (var result in _resultStore.ReadAll().OrderByDescending(x => x.ExecutedAt))
            {
                AddResultRow(result);
            }
        }

        private void LstResults_DoubleClick(object sender, EventArgs e)
        {
            if (lstResults.SelectedItems.Count == 0)
            {
                return;
            }

            var selected = lstResults.SelectedItems[0];
            var screenshotPath = selected.SubItems.Count > 4 ? selected.SubItems[4].Text : string.Empty;
            var videoPath = selected.SubItems.Count > 5 ? selected.SubItems[5].Text : string.Empty;

            if (TryOpenFile(videoPath))
            {
                return;
            }

            if (!TryOpenFile(screenshotPath))
            {
                MessageBox.Show("No evidence file found for this record.", "AutoTest", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static bool TryOpenFile(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return false;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
            return true;
        }

        private void BtnBrowseCaseFile_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Select TestCases.xlsx"
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                txtCaseFile.Text = dialog.FileName;
            }
        }

        private void BtnImportCases_Click(object sender, EventArgs e)
        {
            try
            {
                var filePath = txtCaseFile.Text.Trim();
                var cases = _excelTestCaseService.LoadFromFile(filePath);

                _importedCases = cases.ToList();
                lstTestCases.Items.Clear();
                for (var i = 0; i < cases.Count; i++)
                {
                    var testCase = cases[i];
                    var item = new ListViewItem(testCase.Name);
                    item.SubItems.Add(testCase.ExecutionStatus);
                    item.SubItems.Add(testCase.Executor);
                    item.SubItems.Add(testCase.ExpectedResult);
                    item.Tag = i;
                    lstTestCases.Items.Add(item);
                }

                MessageBox.Show($"Imported {cases.Count} test cases.", "AutoTest", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RunCasesInternalAsync(IReadOnlyList<TestCaseItem> selected, string sourceName)
        {
            if (selected.Count == 0)
            {
                MessageBox.Show("No test cases selected to run.", "AutoTest", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtUrl.Text.Trim()))
            {
                MessageBox.Show("Test URL is required.", "AutoTest", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnRunSelectedCases.Enabled = false;
            btnRunRecordedCsCases.Enabled = false;
            btnRunSelectedCases.Text = "Running...";

            try
            {
                var options = new TestCaseRunOptions(
                    txtUrl.Text.Trim(),
                    txtUsername.Text.Trim(),
                    txtPassword.Text.Trim(),
                    chkEnableVideo.Checked,
                    chkShowBrowser.Checked);

                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(10));
                IReadOnlyList<TestCaseExecutionResult> batch;
                try
                {
                    batch = await _batchRunner.RunAsync(selected, options, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    MessageBox.Show($"{sourceName} run timeout (10 min).", "AutoTest", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _lastCaseRunResults = batch;
                foreach (var r in batch)
                {
                    var row = new SmokeTestResult(
                        r.ExecutedAt,
                        r.Status,
                        r.Message,
                        r.ScreenshotPath,
                        r.VideoPath,
                        r.CaseName,
                        r.ExpectedResult,
                        r.ActualResult,
                        r.DurationMs);
                    _resultStore.Append(row);
                    AddResultRow(row);
                }

                var defaultReport = Path.Combine(
                    AppContext.BaseDirectory,
                    "artifacts",
                    "reports",
                    $"report_{DateTime.Now:yyyyMMdd_HHmmss}.html");
                Directory.CreateDirectory(Path.GetDirectoryName(defaultReport)!);
                ReportExporter.ExportHtml(batch, defaultReport);
                MessageBox.Show($"Finished {batch.Count} case(s) from {sourceName}.\nReport: {defaultReport}", "AutoTest", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                btnRunSelectedCases.Enabled = true;
                btnRunRecordedCsCases.Enabled = true;
                btnRunSelectedCases.Text = "Run Cases";
            }
        }
    }
}
