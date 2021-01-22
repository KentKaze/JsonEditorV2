﻿using JsonEditor;
using JsonEditorV2.Resources;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace JsonEditorV2
{
    public class InputControlSet
    {
        public JColumn JColumn { get; set; }

        public Control ValueControl { get; set; }
        public Button ButtonControl { get; set; }
        public ErrorProvider ValidControl { get; set; }
        public CheckBox NullCheckBox { get; set; }
        public Label NameLabel { get; set; }

        private Control errPositionControl;
        private Control ownerWindow;
        private object parsedValue;

        public InputControlSet(JColumn sourceColumn)
        {
            JColumn = sourceColumn;
        }

        public void DrawControl(Panel pnlMain, int lineIndex)
        {
            if (string.IsNullOrEmpty(JColumn.Name))
                throw new MissingMemberException();

            ownerWindow = pnlMain.Parent;

            NameLabel = new Label();
            NameLabel.Name = $"lbl{JColumn.Name}";
            NameLabel.Text = JColumn.Name;
            NameLabel.Left = 10;
            NameLabel.Top = 30 * lineIndex + 5;
            NameLabel.Width = 190;

            pnlMain.Controls.Add(NameLabel);

            ValueControl = GetValueControlFromJType(JColumn.Type, JColumn.Name);
            errPositionControl = ValueControl;

            if (ValueControl == null)
                return;

            ValueControl.Left = 200;
            ValueControl.Top = 30 * lineIndex + 5;

            if (ValueControl is TextBox)
            {
                ValueControl.Width = 200;
                if (JColumn.NumberOfRows > 1)
                    (ValueControl as TextBox).ScrollBars = ScrollBars.Vertical;
                ValueControl.Height = 30 * JColumn.NumberOfRows - 4;
                NameLabel.Height = 30 * JColumn.NumberOfRows;
                ((TextBox)ValueControl).TextChanged += ValueControl_TextChanged;
                if (!string.IsNullOrEmpty(JColumn.FKTable) && !string.IsNullOrEmpty(JColumn.FKColumn))
                ((TextBox)ValueControl).GotFocus += ValueControl_GotFocus;
            }

            pnlMain.Controls.Add(ValueControl);

            ButtonControl = GetButtonControlFromJType(JColumn.Type, JColumn.Name);

            if (ButtonControl != null && 
                (JColumn.FKTable == null || JColumn.FKColumn == null))
            {
                ValueControl.Width = 150;
                ButtonControl.Left = 350;
                ButtonControl.Width = 50;
                ButtonControl.Top = 30 * lineIndex + 5;
                pnlMain.Controls.Add(ButtonControl);
                errPositionControl = ButtonControl;
            }

            ValidControl = new ErrorProvider();

            NullCheckBox = new CheckBox { Name = $"ckbNull{JColumn.Name}" };
            NullCheckBox.Text = "Null";
            NullCheckBox.Left = 430;
            NullCheckBox.Top = 30 * lineIndex + 5;
            NullCheckBox.Width = 60;
            NullCheckBox.CheckedChanged += CkbCheckBox_CheckedChanged;

            if (!JColumn.IsNullable)
                NullCheckBox.Enabled = false;

            pnlMain.Controls.Add(NullCheckBox);
        }

        private void ValueControl_GotFocus(object sender, EventArgs e)
        {
            NameLabel.Focus();
            JTable fkTable = Var.Tables.Find(m => m.Name == JColumn.FKTable);

            object newValue = frmFKTable.Show(ownerWindow, JColumn.Name, fkTable, JColumn.FKColumn, ValueControl.Text);
            if (newValue != null)
                SetValueToString(newValue);
        }

        private void ValueControl_TextChanged(object sender, EventArgs e)
        {
            ValidControl.SetError(errPositionControl, "");
        }

        public bool CheckValid()
        {
            ValidControl.SetError(errPositionControl, "");

            //確認Null
            if (JColumn.IsNullable && NullCheckBox.Checked)
                return true;
            else if (!JColumn.IsNullable && NullCheckBox.Checked)
            {
                ValidControl.SetError(NullCheckBox, string.Format(Res.JE_VAL_NOT_NULLABLE));
                return false;
            }
   
            //確定型態符合
            if (ValueControl is TextBox)
                if (!ChangeTextToString(ValueControl.Text).TryParseJType(JColumn.Type, out parsedValue))
                {
                    ValidControl.SetError(errPositionControl, string.Format(Res.JE_VAL_INVALID_CAST, ValueControl.Text));
                    return false;
                }

            if (ValueControl is CheckBox)
                if (!(ValueControl as CheckBox).Checked.TryParseJType(JColumn.Type, out parsedValue))
                {
                    ValidControl.SetError(errPositionControl, string.Format(Res.JE_VAL_INVALID_CAST, ValueControl.Text));
                    return false;
                }

            //確認Regex正確
            if (JColumn.Type == JType.String)
                if (!string.IsNullOrEmpty(JColumn.Regex))
                    if (!Regex.IsMatch(ChangeTextToString(ValueControl.Text), JColumn.Regex))
                    {
                        ValidControl.SetError(errPositionControl, string.Format(Res.JE_VAL_REGEX_IS_NOT_MATCH, ValueControl.Text));
                        return false;
                    }

            //確認MinMax正確
            if (JColumn.Type.IsNumber() || JColumn.Type.IsDateTime())
            {
                if (!string.IsNullOrEmpty(JColumn.MinValue) && parsedValue.CompareTo(JColumn.MinValue.ParseJType(JColumn.Type), JColumn.Type) == -1)
                {
                    ValidControl.SetError(errPositionControl, string.Format(Res.JE_VAL_LESS_THEN_MIN_VALUE, ValueControl.Text, JColumn.MinValue));
                    return false;
                }
                if (!string.IsNullOrEmpty(JColumn.MaxValue) && parsedValue.CompareTo(JColumn.MaxValue.ParseJType(JColumn.Type), JColumn.Type) == 1)
                {
                    ValidControl.SetError(errPositionControl, string.Format(Res.JE_VAL_GREATER_THEN_MAX_VALUE, ValueControl.Text, JColumn.MaxValue));
                    return false;
                }
            }

            //外部驗證 - FK驗證
            if(JColumn.FKTable != null && JColumn.FKColumn != null)
            {   
                //有錯表示有欄位錯誤
                JTable jt = Var.Tables.Find(m => m.Name == JColumn.FKTable);
                int columnIndex = jt.Columns.FindIndex(m => m.Name == JColumn.FKColumn);
                //結束
                
                if(!jt.Lines.Exists(m => ChangeStringToText(m.Values[columnIndex].Value.ToString(jt.Columns[columnIndex].Type)) == ValueControl.Text))
                {
                    ValidControl.SetError(errPositionControl, string.Format(Res.JE_VAL_FK_IS_NOT_FOUND, ValueControl.Text));
                    return false;
                }
            }                
            return true;
        }

        public object GetValueValidated()
        {
            if (NullCheckBox.Checked)
                return null;
            return parsedValue;
        }

        public void SetValueToString(object value)
        {
            if (JColumn.IsNullable)
                NullCheckBox.Checked = value == null;
            else if (!JColumn.IsNullable)
                NullCheckBox.Checked = false;
            if (value == null)
                return;

            if (ValueControl is TextBox)
                (ValueControl as TextBox).Text = ChangeStringToText(value.ToString(JColumn.Type));
            else if (ValueControl is CheckBox)
                (ValueControl as CheckBox).Checked = (bool)value;
        }

        public string ChangeTextToString(string text)
            => text.Replace("\r\n", "\n");

        public string ChangeStringToText(string s)
            => s.Replace("\n", "\r\n");

        private void CkbCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ValueControl.Enabled = !NullCheckBox.Checked;
            ValidControl.SetError(errPositionControl, "");
        }

        private Button GetButtonControlFromJType(JType type, string name)
        {
            switch (type)
            {
                case JType.Guid:
                    Button btn = new Button { Name = $"btn{name}", Text = Res.JE_BTN_NEW_GUID }; ;
                    btn.Click += BtnNewGUID_Click;
                    return btn;
                default:
                    return null;
            }
        }

        private void BtnNewGUID_Click(object sender, EventArgs e)
        {
            ValueControl.Text = Guid.NewGuid().ToString();
        }

        private Control GetValueControlFromJType(JType type, string name)
        {
            switch (type)
            {
                case JType.Boolean:
                    return new CheckBox { Name = $"ckb{name}" };
                case JType.Byte:
                case JType.Double:
                case JType.Guid:
                case JType.Integer:
                case JType.Long:
                case JType.Decimal:
                //case JType.TimeSpan:
                case JType.Uri:
                case JType.String:

                case JType.Date:
                case JType.Time:
                case JType.DateTime:
                    return new TextBox { Name = $"txt{name}", Multiline = true };
                //case JType.Date:
                //    return new DateTimePicker { Name = $"dtp{name}", Format = DateTimePickerFormat.Short };
                //case JType.Time:
                //    return new DateTimePicker { Name = $"dtp{name}", Format = DateTimePickerFormat.Time, ShowUpDown = true };
                //case JType.DateTime:                    
                //    return new DateTimePicker { Name = $"dtp{name}", Format = DateTimePickerFormat.Long, ShowUpDown = true };
                case JType.None:
                case JType.JSONObject:
                default:
                    return new Label { Name = $"lvl{name}" };

            }
        }
    }
}
