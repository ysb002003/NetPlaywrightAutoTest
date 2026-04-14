namespace AutoTest
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtUrl = new TextBox();
            this.txtUsername = new TextBox();
            this.txtPassword = new TextBox();
            this.lblUrl = new Label();
            this.lblUsername = new Label();
            this.lblPassword = new Label();
            this.lblCaseFile = new Label();
            this.txtCaseFile = new TextBox();
            this.btnBrowseCaseFile = new Button();
            this.btnImportCases = new Button();
            this.btnRunSelectedCases = new Button();
            this.btnExportReport = new Button();
            this.btnRecordCase = new Button();
            this.btnSaveRecordedCase = new Button();
            this.btnRunRecordedCsCases = new Button();
            this.chkEnableVideo = new CheckBox();
            this.chkShowBrowser = new CheckBox();
            this.lstTestCases = new ListView();
            this.colCaseName = new ColumnHeader();
            this.colCaseStatus = new ColumnHeader();
            this.colCaseExecutor = new ColumnHeader();
            this.colCaseExpected = new ColumnHeader();
            this.lstResults = new ListView();
            this.colTime = new ColumnHeader();
            this.colCase = new ColumnHeader();
            this.colStatus = new ColumnHeader();
            this.colMessage = new ColumnHeader();
            this.colScreenshot = new ColumnHeader();
            this.colVideo = new ColumnHeader();
            this.lblRunLog = new Label();
            this.btnClearLog = new Button();
            this.btnOpenReportsFolder = new Button();
            this.ctxResults = new ContextMenuStrip();
            this.menuResultsCopyCell = new ToolStripMenuItem();
            this.menuResultsCopyRow = new ToolStripMenuItem();
            this.ctxResults.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtUrl
            // 
            this.txtUrl.Location = new System.Drawing.Point(138, 22);
            this.txtUrl.Name = "txtUrl";
            this.txtUrl.Size = new System.Drawing.Size(700, 23);
            this.txtUrl.TabIndex = 0;
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(138, 60);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(300, 23);
            this.txtUsername.TabIndex = 1;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(538, 60);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(300, 23);
            this.txtPassword.TabIndex = 2;
            this.txtPassword.UseSystemPasswordChar = true;
            // lblUrl
            // 
            this.lblUrl.AutoSize = true;
            this.lblUrl.Location = new System.Drawing.Point(26, 25);
            this.lblUrl.Name = "lblUrl";
            this.lblUrl.Size = new System.Drawing.Size(63, 15);
            this.lblUrl.TabIndex = 4;
            this.lblUrl.Text = "Test URL";
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new System.Drawing.Point(26, 63);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(65, 15);
            this.lblUsername.TabIndex = 5;
            this.lblUsername.Text = "Username";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(454, 63);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(57, 15);
            this.lblPassword.TabIndex = 6;
            this.lblPassword.Text = "Password";
            // 
            // lblCaseFile
            // 
            this.lblCaseFile.AutoSize = true;
            this.lblCaseFile.Location = new System.Drawing.Point(26, 92);
            this.lblCaseFile.Name = "lblCaseFile";
            this.lblCaseFile.Size = new System.Drawing.Size(57, 15);
            this.lblCaseFile.TabIndex = 7;
            this.lblCaseFile.Text = "Case File";
            // 
            // txtCaseFile
            // 
            this.txtCaseFile.Location = new System.Drawing.Point(138, 89);
            this.txtCaseFile.Name = "txtCaseFile";
            this.txtCaseFile.Size = new System.Drawing.Size(551, 23);
            this.txtCaseFile.TabIndex = 8;
            // 
            // btnBrowseCaseFile
            // 
            this.btnBrowseCaseFile.Location = new System.Drawing.Point(706, 88);
            this.btnBrowseCaseFile.Name = "btnBrowseCaseFile";
            this.btnBrowseCaseFile.Size = new System.Drawing.Size(90, 24);
            this.btnBrowseCaseFile.TabIndex = 9;
            this.btnBrowseCaseFile.Text = "Browse";
            this.btnBrowseCaseFile.UseVisualStyleBackColor = true;
            this.btnBrowseCaseFile.Click += new System.EventHandler(this.BtnBrowseCaseFile_Click);
            // 
            // btnImportCases
            // 
            this.btnImportCases.Location = new System.Drawing.Point(808, 88);
            this.btnImportCases.Name = "btnImportCases";
            this.btnImportCases.Size = new System.Drawing.Size(90, 24);
            this.btnImportCases.TabIndex = 10;
            this.btnImportCases.Text = "Import";
            this.btnImportCases.UseVisualStyleBackColor = true;
            this.btnImportCases.Click += new System.EventHandler(this.BtnImportCases_Click);
            // 
            // btnRunSelectedCases
            // 
            this.btnRunSelectedCases.Location = new System.Drawing.Point(858, 22);
            this.btnRunSelectedCases.Name = "btnRunSelectedCases";
            this.btnRunSelectedCases.Size = new System.Drawing.Size(100, 61);
            this.btnRunSelectedCases.TabIndex = 3;
            this.btnRunSelectedCases.Text = "Run Cases";
            this.btnRunSelectedCases.UseVisualStyleBackColor = true;
            this.btnRunSelectedCases.Click += new System.EventHandler(this.BtnRunSelectedCases_Click);
            // 
            // btnExportReport
            // 
            this.btnExportReport.Location = new System.Drawing.Point(330, 114);
            this.btnExportReport.Name = "btnExportReport";
            this.btnExportReport.Size = new System.Drawing.Size(110, 24);
            this.btnExportReport.TabIndex = 15;
            this.btnExportReport.Text = "Export Report";
            this.btnExportReport.UseVisualStyleBackColor = true;
            this.btnExportReport.Click += new System.EventHandler(this.BtnExportReport_Click);
            // 
            // btnRecordCase
            // 
            this.btnRecordCase.Location = new System.Drawing.Point(570, 114);
            this.btnRecordCase.Name = "btnRecordCase";
            this.btnRecordCase.Size = new System.Drawing.Size(110, 24);
            this.btnRecordCase.TabIndex = 20;
            this.btnRecordCase.Text = "Record Case";
            this.btnRecordCase.UseVisualStyleBackColor = true;
            this.btnRecordCase.Click += new System.EventHandler(this.BtnRecordCase_Click);
            // 
            // btnSaveRecordedCase
            // 
            this.btnSaveRecordedCase.Location = new System.Drawing.Point(688, 114);
            this.btnSaveRecordedCase.Name = "btnSaveRecordedCase";
            this.btnSaveRecordedCase.Size = new System.Drawing.Size(110, 24);
            this.btnSaveRecordedCase.TabIndex = 21;
            this.btnSaveRecordedCase.Text = "Save .cs Case";
            this.btnSaveRecordedCase.UseVisualStyleBackColor = true;
            this.btnSaveRecordedCase.Click += new System.EventHandler(this.BtnSaveRecordedCase_Click);
            // 
            // btnRunRecordedCsCases
            // 
            this.btnRunRecordedCsCases.Location = new System.Drawing.Point(806, 114);
            this.btnRunRecordedCsCases.Name = "btnRunRecordedCsCases";
            this.btnRunRecordedCsCases.Size = new System.Drawing.Size(152, 24);
            this.btnRunRecordedCsCases.TabIndex = 22;
            this.btnRunRecordedCsCases.Text = "Run Recorded .cs";
            this.btnRunRecordedCsCases.UseVisualStyleBackColor = true;
            this.btnRunRecordedCsCases.Click += new System.EventHandler(this.BtnRunRecordedCsCases_Click);
            // 
            // chkEnableVideo
            // 
            this.chkEnableVideo.AutoSize = true;
            this.chkEnableVideo.Checked = true;
            this.chkEnableVideo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableVideo.Location = new System.Drawing.Point(26, 118);
            this.chkEnableVideo.Name = "chkEnableVideo";
            this.chkEnableVideo.Size = new System.Drawing.Size(178, 19);
            this.chkEnableVideo.TabIndex = 11;
            this.chkEnableVideo.Text = "Enable video on failure";
            this.chkEnableVideo.UseVisualStyleBackColor = true;
            // 
            // chkShowBrowser
            // 
            this.chkShowBrowser.AutoSize = true;
            this.chkShowBrowser.Checked = true;
            this.chkShowBrowser.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowBrowser.Location = new System.Drawing.Point(455, 118);
            this.chkShowBrowser.Name = "chkShowBrowser";
            this.chkShowBrowser.Size = new System.Drawing.Size(102, 19);
            this.chkShowBrowser.TabIndex = 16;
            this.chkShowBrowser.Text = "Show browser";
            this.chkShowBrowser.UseVisualStyleBackColor = true;
            // 
            // lstTestCases
            // 
            this.lstTestCases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colCaseName,
            this.colCaseStatus,
            this.colCaseExecutor,
            this.colCaseExpected});
            this.lstTestCases.FullRowSelect = true;
            this.lstTestCases.GridLines = true;
            this.lstTestCases.CheckBoxes = true;
            this.lstTestCases.Location = new System.Drawing.Point(26, 143);
            this.lstTestCases.Name = "lstTestCases";
            this.lstTestCases.Size = new System.Drawing.Size(932, 154);
            this.lstTestCases.TabIndex = 12;
            this.lstTestCases.UseCompatibleStateImageBehavior = false;
            this.lstTestCases.View = System.Windows.Forms.View.Details;
            // 
            // colCaseName
            // 
            this.colCaseName.Text = "Case Name";
            this.colCaseName.Width = 190;
            // 
            // colCaseStatus
            // 
            this.colCaseStatus.Text = "Status";
            this.colCaseStatus.Width = 120;
            // 
            // colCaseExecutor
            // 
            this.colCaseExecutor.Text = "Executor";
            this.colCaseExecutor.Width = 130;
            // 
            // colCaseExpected
            // 
            this.colCaseExpected.Text = "Expected";
            this.colCaseExpected.Width = 470;
            // 
            // lstResults
            // 
            this.lstResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colTime,
            this.colCase,
            this.colStatus,
            this.colMessage,
            this.colScreenshot,
            this.colVideo});
            this.lstResults.FullRowSelect = true;
            this.lstResults.GridLines = true;
            this.lstResults.ContextMenuStrip = this.ctxResults;
            this.lstResults.Location = new System.Drawing.Point(26, 333);
            this.lstResults.Name = "lstResults";
            this.lstResults.Size = new System.Drawing.Size(932, 216);
            this.lstResults.TabIndex = 13;
            this.lstResults.UseCompatibleStateImageBehavior = false;
            this.lstResults.View = System.Windows.Forms.View.Details;
            this.lstResults.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.LstResults_ColumnClick);
            this.lstResults.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LstResults_MouseDown);
            this.lstResults.DoubleClick += new System.EventHandler(this.LstResults_DoubleClick);
            // 
            // colTime
            // 
            this.colTime.Text = "Time";
            this.colTime.Width = 140;
            // 
            // colCase
            // 
            this.colCase.Text = "Case";
            this.colCase.Width = 120;
            // 
            // colStatus
            // 
            this.colStatus.Text = "Status";
            this.colStatus.Width = 70;
            // 
            // colMessage
            // 
            this.colMessage.Text = "Message";
            this.colMessage.Width = 260;
            // 
            // colScreenshot
            // 
            this.colScreenshot.Text = "Screenshot Path";
            this.colScreenshot.Width = 200;
            // 
            // colVideo
            // 
            this.colVideo.Text = "Video Path";
            this.colVideo.Width = 210;
            // 
            // lblRunLog
            // 
            this.lblRunLog.AutoSize = true;
            this.lblRunLog.Location = new System.Drawing.Point(26, 308);
            this.lblRunLog.Name = "lblRunLog";
            this.lblRunLog.Size = new System.Drawing.Size(52, 15);
            this.lblRunLog.TabIndex = 17;
            this.lblRunLog.Text = "Run log";
            // 
            // btnClearLog
            // 
            this.btnClearLog.Location = new System.Drawing.Point(100, 303);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(85, 24);
            this.btnClearLog.TabIndex = 18;
            this.btnClearLog.Text = "Clear log";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.BtnClearLog_Click);
            // 
            // btnOpenReportsFolder
            // 
            this.btnOpenReportsFolder.Location = new System.Drawing.Point(195, 303);
            this.btnOpenReportsFolder.Name = "btnOpenReportsFolder";
            this.btnOpenReportsFolder.Size = new System.Drawing.Size(130, 24);
            this.btnOpenReportsFolder.TabIndex = 19;
            this.btnOpenReportsFolder.Text = "Reports folder";
            this.btnOpenReportsFolder.UseVisualStyleBackColor = true;
            this.btnOpenReportsFolder.Click += new System.EventHandler(this.BtnOpenReportsFolder_Click);
            // 
            // ctxResults
            // 
            this.ctxResults.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuResultsCopyCell,
            this.menuResultsCopyRow});
            this.ctxResults.Name = "ctxResults";
            this.ctxResults.Size = new System.Drawing.Size(181, 70);
            // 
            // menuResultsCopyCell
            // 
            this.menuResultsCopyCell.Name = "menuResultsCopyCell";
            this.menuResultsCopyCell.Size = new System.Drawing.Size(180, 22);
            this.menuResultsCopyCell.Text = "Copy cell";
            this.menuResultsCopyCell.Click += new System.EventHandler(this.MenuResultsCopyCell_Click);
            // 
            // menuResultsCopyRow
            // 
            this.menuResultsCopyRow.Name = "menuResultsCopyRow";
            this.menuResultsCopyRow.Size = new System.Drawing.Size(180, 22);
            this.menuResultsCopyRow.Text = "Copy row";
            this.menuResultsCopyRow.Click += new System.EventHandler(this.MenuResultsCopyRow_Click);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(980, 600);
            this.Controls.Add(this.btnOpenReportsFolder);
            this.Controls.Add(this.btnClearLog);
            this.Controls.Add(this.lblRunLog);
            this.Controls.Add(this.lstResults);
            this.Controls.Add(this.lstTestCases);
            this.Controls.Add(this.chkShowBrowser);
            this.Controls.Add(this.btnRunRecordedCsCases);
            this.Controls.Add(this.btnSaveRecordedCase);
            this.Controls.Add(this.btnRecordCase);
            this.Controls.Add(this.btnExportReport);
            this.Controls.Add(this.btnRunSelectedCases);
            this.Controls.Add(this.chkEnableVideo);
            this.Controls.Add(this.btnImportCases);
            this.Controls.Add(this.btnBrowseCaseFile);
            this.Controls.Add(this.txtCaseFile);
            this.Controls.Add(this.lblCaseFile);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.lblUrl);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.txtUrl);
            this.Name = "Form1";
            this.Text = "AutoTest - Iteration A";
            this.ctxResults.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private TextBox txtUrl;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Label lblUrl;
        private Label lblUsername;
        private Label lblPassword;
        private Label lblCaseFile;
        private TextBox txtCaseFile;
        private Button btnBrowseCaseFile;
        private Button btnImportCases;
        private Button btnRunSelectedCases;
        private Button btnExportReport;
        private Button btnRecordCase;
        private Button btnSaveRecordedCase;
        private Button btnRunRecordedCsCases;
        private CheckBox chkEnableVideo;
        private CheckBox chkShowBrowser;
        private ListView lstTestCases;
        private ColumnHeader colCaseName;
        private ColumnHeader colCaseStatus;
        private ColumnHeader colCaseExecutor;
        private ColumnHeader colCaseExpected;
        private ListView lstResults;
        private ColumnHeader colTime;
        private ColumnHeader colCase;
        private ColumnHeader colStatus;
        private ColumnHeader colMessage;
        private ColumnHeader colScreenshot;
        private ColumnHeader colVideo;
        private Label lblRunLog;
        private Button btnClearLog;
        private Button btnOpenReportsFolder;
        private ContextMenuStrip ctxResults;
        private ToolStripMenuItem menuResultsCopyCell;
        private ToolStripMenuItem menuResultsCopyRow;
    }
}
