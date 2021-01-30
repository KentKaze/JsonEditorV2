﻿using Aritiafel.Characters;
using Aritiafel.Organizations;
using JsonEditorV2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JsonEditorV2Tests
{
    public class JsonEditorTestSystem
    {
        public const string OutputFolder = @"C:\Programs\Reports\Json Editor V2";
        public const string OutputOverview = @"C:\Programs\Reports\Json Editor V2\Overview.txt";

        public Task TestThread { get; set; }
        public FileStream OutputFileStream { get; set; }
        public Exception Exception { get; set; }

        public MainForm MainForm { get; set; }
        public bool FormReady { get; set; }

        //In Form Variable
        public string InputText { get; set; }
        public string CurrentFileName { get; set; }
        public string CurrentColumnName { get; set; }
        public int SelectedLineIndex { get; set; }

        public JsonEditorTestSystem()
        {
            if (!AdventurerAssociation.Registered)
            {
                OutputFileStream = new FileStream(OutputOverview, FileMode.Create);
                AdventurerAssociation.RegisterMembers(OutputFileStream);
                AdventurerAssociation.Form_Start += AdventurerAssociation_Form_Start;
            }
            else
            {
                AdventurerAssociation.Archivist.WriteRecord("----------------------------------------------------");
                AdventurerAssociation.Archivist.WriteRecord("----------------------------------------------------");
            }
            MainForm = new MainForm();
            MainForm.StartPosition = FormStartPosition.Manual;
            MainForm.Left = 30000;
            MainForm.Top = 30000;
            MainForm.Shown += MainForm_Shown;
            TestThread = new Task(() => Application.Run(MainForm));
            TestThread.Start(); 
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            FormReady = true;
            MainForm.Visible = false;
        }
 
        private void DoEventsUntilFormReadyAndResetFormReady()
        {
            while (!FormReady)
                Application.DoEvents();
           
            FormReady = false;
        }

        private void EndInvokeAndThrowException(IAsyncResult ar)
        {
            while (!ar.IsCompleted)
                Application.DoEvents();

            MainForm.EndInvoke(ar);

            if (Exception != null)
                ExceptionDispatchInfo.Capture(Exception).Throw();

            AdventurerAssociation.RegisterMember(new Bard());
            AdventurerAssociation.RegisterMember(new Courier());

            FormReady = true;
        }

        private DialogResult AdventurerAssociation_Form_Start(Form newForm)
        {
            if (newForm is frmInputBox)
            {
                frmInputBox frmInputBox = newForm as frmInputBox;

                switch (frmInputBox.InputBoxType)
                {
                    case InputBoxTypes.NewFile:
                    case InputBoxTypes.RenameFile:
                    case InputBoxTypes.AddColumn:
                    case InputBoxTypes.RenameColumn:
                        //輸入值                        
                        (frmInputBox.Controls.Find("txtInput", false)[0] as TextBox).Text = InputText;
                        //按下OK                        
                        frmInputBox.btnConfirm_Click(frmInputBox, new EventArgs());
                        break;

                    default:
                        break;
                }
            }
            return newForm.DialogResult;
        }

        private delegate void MainForm_Events(object sender, EventArgs args);

        private void MainFormInvoke(MainForm_Events formEvent, EventArgs args = null, object sender = null)
        {
            DoEventsUntilFormReadyAndResetFormReady();
            EndInvokeAndThrowException(MainFormBeginInvoke(formEvent, args, sender));
        }

        private IAsyncResult MainFormBeginInvoke(MainForm_Events formEvent, EventArgs args = null, object sender = null)
        {
            return MainForm.BeginInvoke((MethodInvoker)delegate
            {
                try
                {
                    formEvent(sender ?? MainForm, args ?? new EventArgs());
                }
                catch (Exception ex)
                { Exception = ex; }
            });
        }

        public void OpenJsonFile(string fileName)
        {   
            ClickOnTreeView(MouseButtons.Right, fileName);

            MainFormInvoke(MainForm.tmiOpenJsonFile_Click);
        }

        public void UpdateMainValue()
        {
            MainFormInvoke(MainForm.btnUpdateMain_Click);
        }

        public Control SelectColumnPanelValueControl(ColumnAttributeNames attributeName)
        {
            Control valueControl = MainForm.Controls.Find("pnlFileInfo", false)[0].Controls.Find(TestConst.ColumnAttributesInfo[attributeName].ValueControlName, false)[0];
            
            DoEventsUntilFormReadyAndResetFormReady();
            IAsyncResult ar = MainForm.BeginInvoke((MethodInvoker)delegate
            {
                try
                { valueControl.Focus();
                  valueControl.Select(); }
                catch (Exception ex)
                { Exception = ex; }
            });
            EndInvokeAndThrowException(ar);
            return valueControl;
        }

        public void ChangeColumnPanelControlValue(ColumnAttributeNames attributeName, object value)
        {
            Control valueControl = SelectColumnPanelValueControl(attributeName);

            DoEventsUntilFormReadyAndResetFormReady();
            IAsyncResult ar = MainForm.BeginInvoke((MethodInvoker)delegate
            {
                try
                {
                    if (valueControl is TextBox)
                        valueControl.Text = value.ToString();
                    else if (valueControl is CheckBox)
                        ((CheckBox)valueControl).Checked = (bool)value;
                    else
                        ((ComboBox)valueControl).SelectedItem = value;
                }
                catch (Exception ex)
                { Exception = ex; }
            });            
            EndInvokeAndThrowException(ar);
        }

        //public void SetLineValue(string columnName, object value)
        //{
        //    if (CurrentFileName != fileName || CurrentColumnName != columnName)
        //        ClickOnTreeView(fileName, columnName);

        //    ChangeColumnPanelControlValue(attributeName, value);
        //}

        public void SetColumnAttribute(string fileName, string columnName, ColumnAttributeNames attributeName, object value)
        {
            if (CurrentFileName != fileName || CurrentColumnName != columnName)
                ClickOnTreeView(fileName, columnName);

            ChangeColumnPanelControlValue(attributeName, value);
        }

        public void ChangeMainPanelValueControlValue(string columnName, object value)
        {
            Control valueControl = SelectMainPanelValueControl(columnName);

            DoEventsUntilFormReadyAndResetFormReady();
            IAsyncResult ar = MainForm.BeginInvoke((MethodInvoker)delegate
            {
                try
                {
                    if (valueControl is TextBox)
                        valueControl.Text = value.ToString();
                    else if (valueControl is CheckBox)
                        ((CheckBox)valueControl).Checked = (bool)value;
                    else
                        ((ComboBox)valueControl).SelectedItem = value;
                }
                catch (Exception ex)
                { Exception = ex; }
            });
            EndInvokeAndThrowException(ar);
        }

        public Control SelectMainPanelValueControl(string columnName)
        {
            Control[] ctls;
            ctls = MainForm.Controls.Find("pnlMain", false)[0].Controls.Find($"txt{columnName}", false);
            if(ctls.Length == 0)
            {
                ctls = MainForm.Controls.Find("pnlMain", false)[0].Controls.Find($"ckb{columnName}", false);
                if (ctls.Length == 0)
                    ctls = MainForm.Controls.Find("pnlMain", false)[0].Controls.Find($"cob{columnName}", false);
            }
            if (ctls.Length == 0)
                throw new ArgumentException();

            DoEventsUntilFormReadyAndResetFormReady();
            Control valueControl = ctls[0];
            IAsyncResult ar = MainForm.BeginInvoke((MethodInvoker)delegate
            {
                try
                {
                    valueControl.Focus();
                    valueControl.Select();
                }
                catch (Exception ex)
                { Exception = ex; }
            });
            EndInvokeAndThrowException(ar);
            return valueControl;
        }

        public void SelectLine(int index)
        {
            if (SelectedLineIndex == index)
                return;

            ListBox lsb = MainForm.Controls.Find("lsbLines", false)[0] as ListBox;
            DoEventsUntilFormReadyAndResetFormReady();
            IAsyncResult ar = MainForm.BeginInvoke((MethodInvoker)delegate
            {
                try
                { lsb.SelectedIndex = index; }
                catch (Exception ex)
                { Exception = ex; }
            });
            EndInvokeAndThrowException(ar);
        }

        public void NewLine()
        {
            MainFormInvoke(MainForm.btnNewLine_Click);
        }

        public void CloseJsonFiles(ResponseOptions saveFile = ResponseOptions.Yes)
        {
            Courier courier = new Courier(saveFile, "JE_RUN_SAVE_FILES_CHECK");
            AdventurerAssociation.RegisterMember(courier);
            MainFormInvoke(MainForm.tmiCloseAllFiles_Click);
        }

        public void UpdateCurrentColumn()
        {
            MainFormInvoke(MainForm.btnUpdateColumn_Click);
        }

        public void ClickOnTreeView(string fileName = "", string columnName = "")
            => ClickOnTreeView(MouseButtons.Left, fileName, columnName);

        public void ClickOnTreeView(MouseButtons button ,string fileName = "", string columnName = "")
        {
            TreeNodeMouseClickEventArgs tea = null;
            TreeNode[] trs;

            if(string.IsNullOrEmpty(fileName))
                tea = new TreeNodeMouseClickEventArgs(Var.RootNode, button, 1, 0, 0);
            else if(string.IsNullOrEmpty(columnName))
            {
                trs = Var.RootNode.Nodes.Find(fileName, false);
                if (trs.Length != 0)
                    tea = new TreeNodeMouseClickEventArgs(trs[0], button, 1, 0, 0);
            }
            else
            {
                trs = Var.RootNode.Nodes.Find(fileName, false);
                if (trs.Length != 0)
                {
                    trs = trs[0].Nodes.Find(columnName, false);
                    if (trs.Length != 0)
                        tea = new TreeNodeMouseClickEventArgs(trs[0], button, 1, 0, 0);
                }   
            }

            if (tea == null)
                throw new ArgumentOutOfRangeException("TreeNode");

            DoEventsUntilFormReadyAndResetFormReady();
            IAsyncResult ar = MainForm.BeginInvoke((MethodInvoker)delegate
            {
                try
                { MainForm.trvJsonFiles_NodeMouseClick(MainForm, tea); }
                catch (Exception ex)
                { Exception = ex; }
            });
            EndInvokeAndThrowException(ar);

            CurrentFileName = fileName;
            CurrentColumnName = columnName;
        }

        public void AddColumn(string filename, string columnName)
        {            
            ClickOnTreeView(MouseButtons.Right, filename);

            InputText = columnName;
            MainFormInvoke(MainForm.tmiAddColumn_Click);
        }

        public void NewJsonFile(string fileName)
        {
            ClickOnTreeView(MouseButtons.Right);

            InputText = fileName;
            MainFormInvoke(MainForm.tmiNewJsonFile_Click);          
        }

        public void SaveJsonFiles()
        {
            MainFormInvoke(MainForm.tmiSaveJsonFiles_Click);
        }

        public void Exit(ResponseOptions saveFile = ResponseOptions.Yes)
        {
            Courier courier = new Courier(saveFile, "JE_RUN_SAVE_FILES_CHECK");
            AdventurerAssociation.RegisterMember(courier);
            MainFormInvoke(MainForm.tmiExit_Click);
        }

        public void ScanJsonFiles(string targetPath, ResponseOptions haveJFIQuestion = ResponseOptions.OK, ResponseOptions saveFile = ResponseOptions.Yes)
        {
            Courier courier = new Courier(saveFile, "JE_RUN_SAVE_FILES_CHECK");
            courier.AddResponse(haveJFIQuestion, "JE_RUN_SCAN_JSON_FILES_M_1");
            Bard bard = new Bard("SelectedPath", targetPath);
            bard.InputInformation.Add("DialogResult", ResponseOptions.OK);
            AdventurerAssociation.RegisterMember(bard);
            AdventurerAssociation.RegisterMember(courier);
            MainFormInvoke(MainForm.tmiScanJsonFiles_Click);
        }

        public void LoadJsonFiles(string targetPath, ResponseOptions saveFile = ResponseOptions.Yes)
        {   
            Courier courier = new Courier(saveFile, "JE_RUN_SAVE_FILES_CHECK");
            Bard bard = new Bard("SelectedPath", targetPath);
            bard.InputInformation.Add("DialogResult", ResponseOptions.OK);
            AdventurerAssociation.RegisterMember(bard);
            AdventurerAssociation.RegisterMember(courier);
            MainFormInvoke(MainForm.tmiLoadJsonFiles_Click);
        }

        public void NewJsonFiles(string targetPath, ResponseOptions deleteFile = ResponseOptions.Yes)
        {
            Courier courier = new Courier(deleteFile, "JE_RUN_NEW_JSON_FILES_Q_1");
            Bard bard = new Bard("SelectedPath", targetPath);
            bard.InputInformation.Add("DialogResult", ResponseOptions.OK);
            AdventurerAssociation.RegisterMember(bard);
            AdventurerAssociation.RegisterMember(courier);
            MainFormInvoke(MainForm.tmiNewJsonFiles_Click);
        }

        public void PrintMessage(TestContext testContext)
        {
            while (!FormReady)
                Application.DoEvents();
            AdventurerAssociation.PrintMessageFromArchivist(testContext);
        }

        ~JsonEditorTestSystem()
        {
            TestThread.Dispose();
        }
    }
}
