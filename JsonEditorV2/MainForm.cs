﻿using JsonEditor;
using JsonEditorV2.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace JsonEditorV2
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Var.RM = new ResourceManager("JsonEditorV2.Resources.Main", Type.GetType("JsonEditorV2.Resources.Main").Assembly);
            Var.CI = new CultureInfo("zh-TW");
            PatchTextFromResource(Var.CI);
            cobColumnType.DataSource = Enum.GetValues(typeof(JType));
            cobColumnType.SelectedIndex = -1;
            tbpStart.BackColor = this.BackColor;
        }

        #region RESOURCES_TEXT_PATCH
        private void PatchTextFromResource(CultureInfo ci)
        {
            Main.Culture = ci;
            Text = Main.JSON_FILE_EDITOR_TITLE;
            lblColumnName.Text = Main.JE_COLUMN_NAME;
            lblColumnType.Text = Main.JE_COLUMN_TYPE;
            lblColumnIsKey.Text = Main.JE_COLUMN_IS_KEY;
            lblColumnDisplay.Text = Main.JE_COLUMN_DISPLAY;
            lblColumnFK.Text = Main.JE_COLUMN_FK;
            lblColumnNumberOfRows.Text = Main.JE_COLUMN_NUM_OF_ROWS;
            btnClearMain.Text = Main.JE_BTN_CLEAR_MAIN;
            btnUpdateMain.Text = Main.JE_BTN_UPDATE_MAIN;
            btnUpdateColumn.Text = Main.JE_BTN_UPDATE_COLUMN;
            btnClearColumn.Text = Main.JE_BTN_CLEAR_COLUMN;
            tmiFile.Text = Main.JE_TMI_FILE;
            tmiAbout.Text = Main.JE_TMI_ABOUT;
            tmiNewJsonFiles.Text = Main.JE_TMI_NEW_JSON_FILES;
            tmiLoadJsonFiles.Text = Main.JE_TMI_LOAD_JSON_FILES;
            tmiScanJsonFiles.Text = Main.JE_TMI_SCAN_JSON_FILES;
            tmiSaveJsonFiles.Text = Main.JE_TMI_SAVE_JSON_FILES;
            tmiCloseAllFiles.Text = Main.JE_TMI_CLOSE_ALL_FILES;
            tmiExit.Text = Main.JE_TMI_EXIT;
            tmiOpenJsonFile.Text = Main.JE_TMI_OPEN_JSON_FILE;
            tmiDeleteJsonFile.Text = Main.JE_TMI_DELETE_JSON_FILE;
            tmiCloseJsonFile.Text = Main.JE_TMI_CLOSE_JSON_FILE;
            tmiRenameJsonFile.Text = Main.JE_TMI_RENAME_JSON_FILE;
            tmiAddColumn.Text = Main.JE_TMI_ADD_COLUMN;
            tmiNewJsonFile.Text = Main.JE_TMI_NEW_JSON_FILE;
        }
        #endregion

        private void libLines_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnUpdateColumn_Click(object sender, EventArgs e)
        {
            //確認資料正確
            if (!Regex.IsMatch(txtColumnName.Text, Const.ColumnNameRegex))
            {
                MessageBox.Show(Main.JE_RUN_UPDATE_COLUMN_M_1, Main.JE_RUN_UPDATE_COLUMN_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else if (!Regex.IsMatch(txtColumnNumberOfRows.Text, Const.NumberOfRowsRegex))
            {
                MessageBox.Show(Main.JE_RUN_UPDATE_COLUMN_M_2, Main.JE_RUN_UPDATE_COLUMN_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //如果有資料秀出訊息視窗
            if (Var.SelectedColumnParentTable.Count != 0)
            {
                DialogResult dr = MessageBox.Show(string.Format(Main.JE_RUN_UPDATE_COLUMN_M_3, Var.SelectedColumnParentTable.Count), Main.JE_RUN_UPDATE_COLUMN_TITLE, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dr == DialogResult.Cancel)
                    return;
            }

            Var.SelectedColumn.Name = txtColumnName.Text;
            Var.SelectedColumn.Display = chbColumnDisplay.Checked;
            Var.SelectedColumn.NumberOfRows = Convert.ToInt32(txtColumnNumberOfRows.Text);
            if(cobColumnFK.SelectedValue != null)
                Var.SelectedColumn.ForeignKey = cobColumnFK.SelectedValue.ToString();

            //改型態檢查            
            JType newType = (JType)cobColumnType.SelectedValue;            
            if (Var.SelectedColumn.Type != newType)
            {
                Var.SelectedColumn.Type = newType;
                int index = Var.SelectedColumnIndex;
                foreach (JLine jl in Var.SelectedColumnParentTable)
                    jl[index].Value = jl[index].ParseJType(newType);
            }

            //Key檢查，取消Key時同時取消所有相關FK            
            if (Var.SelectedColumn.IsKey && !chbColumnIsKey.Checked)
            {
                Var.SelectedColumn.IsKey = false;
                //沒有其他Key
                if (!Var.SelectedColumnParentTable.HasKey)
                {
                    foreach (JTable jt in Var.Tables)
                        foreach (JColumn jc in jt.Columns)
                            if (jc.ForeignKey == Var.SelectedColumn.Name)
                                jc.ForeignKey = null;
                }
            }
            else
                Var.SelectedColumn.IsKey = chbColumnIsKey.Checked;

            sslMain.Text = string.Format(Main.JE_RUN_UPDATE_COLUMN_M_4, Var.SelectedColumn.Name);
            RefreshTrvJsonFiles();

            //RefreshLibLinesUI();
            //RefreshPnlMainUI();
            //RefreshPnlMainValue();
        }

        private void tmiAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"{Main.JE_ABOUT_MESSAGE}\n\n{Main.JE_ABOUT_MESSAGE_2}", Main.JE_ABOUT_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void tmiNewJsonFiles_Click(object sender, EventArgs e)
        {
#if DEBUG
            fbdMain.SelectedPath = @"C:\Programs\WinForm\JsonEditorV2\JsonEditorV2\TestArea\";
#endif      
            DialogResult dr = fbdMain.ShowDialog(this);
            if (dr == DialogResult.OK)
            {
                Var.JFI = new JFilesInfo(fbdMain.SelectedPath);
                string[] jsonfiles = Directory.GetFiles(fbdMain.SelectedPath, "*.json");
                if (jsonfiles.Length > 0)
                {
                    if (jsonfiles.Length == 1 && jsonfiles[0] == Var.JFI.FileInfoPath)
                        File.Delete(jsonfiles[0]);
                    else
                    {
                        if (File.Exists(Var.JFI.FileInfoPath))
                            dr = MessageBox.Show(string.Format(Main.JE_RUN_NEW_JSON_FILES_Q_1, jsonfiles.Length - 1), Main.JE_RUN_NEW_JSON_FILES_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        else
                            dr = MessageBox.Show(string.Format(Main.JE_RUN_NEW_JSON_FILES_Q_1, jsonfiles.Length), Main.JE_RUN_NEW_JSON_FILES_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (dr == DialogResult.Yes)
                        {
                            foreach (string s in jsonfiles)
                                File.Delete(s);
                        }
                        else
                            return;
                    }
                }

                tmiCloseAllFiles_Click(this, e);
                Var.Tables = new List<JTable>();
                Var.JFI.DirectoryPath = fbdMain.SelectedPath;
                tmiCloseAllFiles.Enabled = true;
                tmiScanJsonFiles.Enabled = true;
                tmiNewJsonFile.Enabled = true;
                RefreshTrvJsonFiles();
                sslMain.Text = string.Format(Main.JE_RUN_NEW_JSON_FILES_M_1, Var.JFI.DirectoryPath);
            }
        }

        private void tmiExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void tmiScanJsonFiles_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show(Main.JE_RUN_SCAN_JSON_FILES_M_1, Main.JE_RUN_SCAN_JSON_FILES_TITLE, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dr != DialogResult.OK)
                return;

            tmiCloseAllFiles_Click(this, e);
            Var.Tables = new List<JTable>();
            Var.JFI = new JFilesInfo(Var.JFI.DirectoryPath);
            string[] jsonfiles = Directory.GetFiles(Var.JFI.DirectoryPath, "*.json");
            foreach (string file in jsonfiles)
            {
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    StreamReader sr = new StreamReader(fs);
                    if (file == Var.JFI.FileInfoPath)
                    {
                        Var.JFI = JsonConvert.DeserializeObject<JFilesInfo>(sr.ReadToEnd());
                        Var.JFI.DirectoryPath = fbdMain.SelectedPath;
                    }
                    else
                    {
                        JTable jt = new JTable(Path.GetFileNameWithoutExtension(file), JsonConvert.DeserializeObject(sr.ReadToEnd()));
                        Var.Tables.Add(jt);
                    }
                }
            }
            RefreshTrvJsonFiles();
            sslMain.Text = string.Format(Main.JE_RUN_SCAN_JSON_FILES_M_2, Var.Tables.Count);
        }

        private void tmiLoadJsonFiles_Click(object sender, EventArgs e)
        {
#if DEBUG
            fbdMain.SelectedPath = @"C:\Programs\WinForm\JsonEditorV2\JsonEditorV2\TestArea\Test1";
#endif
            DialogResult dr = fbdMain.ShowDialog(this);
            if (dr != DialogResult.OK)
                return;

            tmiCloseAllFiles_Click(this, e);
            Var.JFI = new JFilesInfo(fbdMain.SelectedPath);
            string[] jsonfiles = Directory.GetFiles(Var.JFI.DirectoryPath, "*.json");
            Var.Tables = new List<JTable>();

            foreach (string file in jsonfiles)
            {
                using (FileStream fs = new FileStream(file, FileMode.Open))
                {
                    StreamReader sr = new StreamReader(fs);
                    if (file == Var.JFI.FileInfoPath)
                    {
                        Var.JFI = JsonConvert.DeserializeObject<JFilesInfo>(sr.ReadToEnd());
                        Var.JFI.DirectoryPath = fbdMain.SelectedPath;
                    }
                    else
                    {
                        //如果很小全讀
                        if (fs.Length < 10)
                            Var.Tables.Add(new JTable(Path.GetFileNameWithoutExtension(file), JsonConvert.DeserializeObject(sr.ReadToEnd())));
                        else
                        {
                            //讀5行之後結束
                            int pflag = 0;
                            object value = "";
                            JTable jt = new JTable(Path.GetFileNameWithoutExtension(file));

                            JsonTextReader reader = new JsonTextReader(sr);
                            reader.Skip();//StartArray

                            for (int i = 0; i < 5; i++)
                            {
                                JLine jl = new JLine();
                                jt.Lines.Add(jl);
                                reader.Skip();//StartObject

                                while (reader.TokenType != JsonToken.EndObject)
                                {
                                    reader.Read();
                                    if (reader.TokenType == JsonToken.PropertyName)
                                    {
                                        pflag = 1;
                                        value = reader.Value;
                                    }
                                    else if (pflag == 1)
                                    {
                                        if (reader.TokenType == JsonToken.StartObject ||
                                            reader.TokenType == JsonToken.StartArray)
                                        {
                                            //直接跳出
                                            int skipFlag = 1;
                                            while (skipFlag == 0)
                                            {
                                                reader.Read();
                                                if (reader.TokenType == JsonToken.StartObject)
                                                    skipFlag++;
                                                else if (reader.TokenType == JsonToken.EndObject)
                                                    skipFlag--;
                                            }
                                        }
                                        else
                                        {
                                            if (i == 0)
                                                jt.Columns.Add(new JColumn(value.ToString(), Extentions.ToJType(reader.TokenType)));
                                            jl.Add(JValue.FromObject(reader.Value));
                                        }
                                        pflag = 0;
                                    }
                                }
                            }
                            Var.Tables.Add(jt);
                        }
                    }
                    sr.Close();
                }
            }

            //有JFileInfo的話相連
            try
            {
                if (Var.JFI.TablesInfo.Count != 0)
                    foreach (JTable jt in Var.Tables)
                        jt.LoadFileInfo(Var.JFI.TablesInfo.Find(m => m.Name == jt.Name));
            }
            catch (Exception ex)
            {
                HandleException(ex, Main.JE_RUN_LOAD_JSON_FILES_M_2, Main.JE_RUN_LOAD_JSON_FILES_TITLE);
            }
            RefreshTrvJsonFiles();
            sslMain.Text = string.Format(Main.JE_RUN_LOAD_JSON_FILES_M_1, Var.Tables.Count);

        }

        private string GetColumnNodeString(JColumn jc)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(jc.Name);
            if(jc.IsKey)
                sb.Append("[Key]");
            if (!string.IsNullOrEmpty(jc.ForeignKey))
                sb.AppendFormat("[FK:{0}]", jc.ForeignKey);
            sb.Append(":");
            sb.Append(jc.Type);
            return sb.ToString();
        }  

        private void RefreshTrvJsonFiles()
        {
            trvJsonFiles.Nodes.Clear();
            tmiCloseAllFiles.Enabled = false;
            tmiScanJsonFiles.Enabled = false;
            tmiNewJsonFile.Enabled = false;
            if (Var.Tables == null)
                return;

            Var.RootNode = new TreeNode(Var.JFI.Name, 0, 0);
            Var.RootNode.ToolTipText = Var.JFI.Name;
            trvJsonFiles.Nodes.Add(Var.RootNode);
            TreeNode fileNode, tr;

            Dictionary<string, string> fks = new Dictionary<string, string>();
            foreach (JTable jt in Var.Tables)
            {
                fileNode = new TreeNode { Text = jt.Name, Tag = jt.Name, ImageIndex = 1, SelectedImageIndex = 1 };
                fileNode.ToolTipText = jt.Name;
                
                Var.RootNode.Nodes.Add(fileNode);
                if (Var.SelectedColumnParentTable == jt && Var.SelectedColumn == null)
                    trvJsonFiles.SelectedNode = fileNode;
                foreach (JColumn jc in jt.Columns)
                {
                    tr = new TreeNode { Text = GetColumnNodeString(jc), Tag = jc.Name, ImageIndex = 2, SelectedImageIndex = 2 };
                    tr.ToolTipText = Text;
                    fileNode.Nodes.Add(tr);
                    if (Var.SelectedColumn == jc)
                        trvJsonFiles.SelectedNode = tr;
                    if (!string.IsNullOrEmpty(jc.ForeignKey))
                        fks.Add(jc.Name, jc.ForeignKey);
                }

                //foreach (KeyValuePair<string, string> kvp in fks)
                //    fileNode.Nodes.Add(new TreeNode { Name = $"FK:{kvp.Key} -> {kvp.Value}", ImageIndex = 3, SelectedImageIndex = 3, ));

            }

            tmiCloseAllFiles.Enabled = true;
            tmiScanJsonFiles.Enabled = true;
            tmiNewJsonFile.Enabled = true;
            RefreshPnlFileInfo();
        }

        private void tmiCloseAllFiles_Click(object sender, EventArgs e)
        {
            Var.Tables = null;
            Var.OpenedTable.Clear();
            if (Var.JFI != null)
                Var.JFI.Dispose();
            Var.RootNode = null;
            Var.SelectedColumn = null;
            Var.SelectedColumnParentTable = null;
            Var.PageIndex = -1;
            Var.Lines.Clear();
            RefreshTrvJsonFiles();
            //RefreshLibLinesUI();
            //RefreshPnlMainUI();
            sslMain.Text = "";
        }

        private void tmiSaveJsonFiles_Click(object sender, EventArgs e)
        {
            Var.JFI.TablesInfo.Clear();
            foreach (JTable jt in Var.Tables)
                Var.JFI.TablesInfo.Add(jt.GetJTableInfo());

            //存JSONFilesInfo檔
            using (FileStream fs = new FileStream(Var.JFI.FileInfoPath, FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(JsonConvert.SerializeObject(Var.JFI, Formatting.Indented));
                sw.Close();
            }

            //存JSONFiles
            foreach (JTable jt in Var.OpenedTable)
            {
                using (FileStream fs = new FileStream(Path.Combine(Var.JFI.DirectoryPath, $"{jt.Name}.json"), FileMode.Create))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write(JsonConvert.SerializeObject(jt.GetJsonObject(), Formatting.Indented));
                    sw.Close();
                }
            }
            sslMain.Text = string.Format(Main.JE_RUN_SAVE_JSON_FILES_M_1, Var.JFI.DirectoryPath);
        }

        private void trvJsonFiles_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {

        }

        private void trvJsonFiles_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (Var.DblClick)
                e.Cancel = true;
        }

        private void trvJsonFiles_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (Var.DblClick)
                e.Cancel = true;
        }

        private void trvJsonFiles_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {

        }

        private void trvJsonFiles_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                trvJsonFiles.ContextMenuStrip = cmsJsonFiles;
            Var.DblClick = e.Button == MouseButtons.Left && e.Clicks >= 2;
        }

        private void trvJsonFiles_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == Var.RootNode)
            { }
            else if (e.Node.Parent == Var.RootNode)
            {
                Var.SelectedColumn = null;
                Var.SelectedColumnParentTable = Var.Tables.Find(m => m.Name == e.Node.Tag.ToString()); ;
                RefreshPnlFileInfo();

                if (e.Button == MouseButtons.Right)
                {
                    trvJsonFiles.SelectedNode = e.Node;
                    if (Var.OpenedTable.Exists(m => m.Name == e.Node.Tag.ToString()))
                    {
                        tmiOpenJsonFile.Enabled = false;
                        tmiCloseJsonFile.Enabled = true;
                    }
                    else
                    {
                        tmiOpenJsonFile.Enabled = true;
                        tmiCloseJsonFile.Enabled = false;
                    }
                    trvJsonFiles.ContextMenuStrip = cmsJsonFilesSelected;
                }
            }            
            else
            {
                Var.SelectedColumnParentTable = Var.Tables.Find(m => m.Name == e.Node.Parent.Tag.ToString());
                Var.SelectedColumn = Var.SelectedColumnParentTable.Columns.Find(t => t.Name == e.Node.Tag.ToString());
                RefreshPnlFileInfo();
                if (e.Button == MouseButtons.Right)
                    trvJsonFiles.ContextMenuStrip = null;
            }
        }

        private void RefreshPnlFileInfo()
        {
            var ds = (from t in Var.Tables
                      where t.HasKey
                      select new { t.Name }).ToList();
            ds.Insert(0, new { Name = (string)null });
            cobColumnFK.DataSource = ds;
            if (Var.SelectedColumn != null)
            {
                cobColumnType.SelectedIndex = cobColumnType.Items.IndexOf(Var.SelectedColumn.Type);
                txtColumnName.Text = Var.SelectedColumn.Name;
                chbColumnDisplay.Checked = Var.SelectedColumn.Display;
                chbColumnIsKey.Checked = Var.SelectedColumn.IsKey;
                if (!string.IsNullOrEmpty(Var.SelectedColumn.ForeignKey))
                    cobColumnFK.SelectedValue = Var.SelectedColumn.ForeignKey;
                txtColumnNumberOfRows.Text = Var.SelectedColumn.NumberOfRows.ToString();
                btnUpdateColumn.Enabled = true;
            }
            else
            {
                btnClearColumn_Click(this, new EventArgs());
                btnUpdateColumn.Enabled = false;
            }
        }

        private void RefreshCloseFileState()
        {

        }

        private void trvJsonFiles_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == Var.RootNode)
            { }
            else if (e.Node.Parent == Var.RootNode)
            {
                trvJsonFiles.SelectedNode = e.Node;
                //tmiOpenJsonFile_Click(this, e);
            }
        }

        private void tmiNewJsonFile_Click(object sender, EventArgs e)
        {
            frmInputBox fib = new frmInputBox("New File");
            fib.StartPosition = FormStartPosition.CenterParent;
            DialogResult dr = fib.ShowDialog(this);
            if (dr == DialogResult.Cancel)
                return;
            try
            {
                string newFile = Path.Combine(Var.JFI.DirectoryPath, $"{fib.InputValue}.json");
                using (FileStream fs = new FileStream(newFile, FileMode.Create))
                { }

                JTable jt = new JTable(fib.InputValue);
                Var.Tables.Add(jt);
                Var.RootNode.Nodes.Add(new TreeNode { Text = jt.Name, ImageIndex = 1, SelectedImageIndex = 1, Tag = jt.Name });
            }
            catch (Exception ex)
            {
                HandleException(ex, Main.JE_RUN_NEW_JSON_FILE_M_2, Main.JE_RUN_NEW_JSON_FILE_TITLE);
            }
        }

        private void HandleException(Exception ex, string content, string title)
        {
            MessageBox.Show(string.Format(content, ex.Message), title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void tmiAddColumn_Click(object sender, EventArgs e)
        {
            frmInputBox fib = new frmInputBox("Add Column");
            fib.StartPosition = FormStartPosition.CenterParent;
            DialogResult dr = fib.ShowDialog(this);
            if (dr == DialogResult.Cancel)
                return;

            JColumn jc = new JColumn(fib.InputValue);
            Var.SelectedColumnParentTable.Columns.Add(jc);
            Var.SelectedColumn = jc;
            RefreshTrvJsonFiles();
        }

        private void backupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string BackupPath = @"E:\Backup\JsonEditorV2";
            string ProjectPath = @"C:\Programs\WinForm\JsonEditorV2";
            if (Directory.Exists(BackupPath))
                DirectoryCopy(ProjectPath, BackupPath);
            else
            {
                MessageBox.Show("Target backup drive or directory do not exist.");
                return;
            }
            MessageBox.Show("OK");
        }
        #region DirectoryCopy
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
        #endregion

        private void btnClearColumn_Click(object sender, EventArgs e)
        {
            txtColumnName.Text = "";
            cobColumnType.SelectedIndex = -1;
            chbColumnDisplay.Checked = false;
            chbColumnIsKey.Checked = false;
            cobColumnFK.SelectedIndex = -1;
            txtColumnNumberOfRows.Text = "0";            
        }
    }
}
