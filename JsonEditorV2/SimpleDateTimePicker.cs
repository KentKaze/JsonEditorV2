﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JsonEditorV2.Resources;

namespace JsonEditorV2
{
    public enum DateTimePickerStyle
    {
        DateTime = 0,
        Date,
        Time
    }

    public partial class SimpleDateTimePicker : UserControl
    {
        private static readonly byte[] years100 = {
                    99, 98, 97, 96, 95, 94, 93, 92, 91, 90,
                    89, 88, 87, 86, 85, 84, 83, 82, 81, 80,
                    79, 78, 77, 76, 75, 74, 73, 72, 71, 70,
                    69, 68, 67, 66, 65, 64, 63, 62, 61, 60,
                    59, 58, 57, 56, 55, 54, 53, 52, 51, 50,
                    49, 48, 47, 46, 45, 44, 43, 42, 41, 40,
                    39, 38, 37, 36, 35, 34, 33, 32, 31, 30,
                    29, 28, 27, 26, 25, 24, 23, 22, 21, 20,
                    19, 18, 17, 16, 15, 14, 13, 12, 11, 10,
                    9, 8, 7, 6, 5, 4, 3, 2, 1, 0
                };

        private static readonly byte[] years99 = {
                    99, 98, 97, 96, 95, 94, 93, 92, 91, 90,
                    89, 88, 87, 86, 85, 84, 83, 82, 81, 80,
                    79, 78, 77, 76, 75, 74, 73, 72, 71, 70,
                    69, 68, 67, 66, 65, 64, 63, 62, 61, 60,
                    59, 58, 57, 56, 55, 54, 53, 52, 51, 50,
                    49, 48, 47, 46, 45, 44, 43, 42, 41, 40,
                    39, 38, 37, 36, 35, 34, 33, 32, 31, 30,
                    29, 28, 27, 26, 25, 24, 23, 22, 21, 20,
                    19, 18, 17, 16, 15, 14, 13, 12, 11, 10,
                    9, 8, 7, 6, 5, 4, 3, 2, 1
                };

        private static readonly byte[] months = {
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12
                };

        private static readonly byte[] days28 = {
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                    11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                    21, 22, 23, 24, 25, 26, 27, 28
                };

        private static readonly byte[] days29 = {
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                    11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                    21, 22, 23, 24, 25, 26, 27, 28, 29
                };

        private static readonly byte[] days30 = {
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                    11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                    21, 22, 23, 24, 25, 26, 27, 28, 29, 30
                };

        private static readonly byte[] days31 = {
                    1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                    11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                    21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31
                };

        private static readonly byte[] hours = {
                    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                    11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                    21, 22, 23
                };

        private static readonly byte[] minutes = {
                    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                    11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                    21, 22, 23, 24, 25, 26, 27, 28, 29, 30,
                    31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
                    41, 42, 43, 44, 45, 46, 47, 48, 49, 50,
                    51, 52, 53, 54, 55, 56, 57, 58, 59
                };

        private static readonly byte[] seconds = {
                    0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                    11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
                    21, 22, 23, 24, 25, 26, 27, 28, 29, 30,
                    31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
                    41, 42, 43, 44, 45, 46, 47, 48, 49, 50,
                    51, 52, 53, 54, 55, 56, 57, 58, 59
                };

        //[Description("綁定的文字方塊")]
        //public TextBox BindingTextBox {
        //    get; private set; }

        [Description("顯示型態")]
        public DateTimePickerStyle Style { get; set; }

        [Description("值")]
        public DateTime Value {
            get
            {   
                if (!int.TryParse(txtMillisecond.Text, out int milsec))
                    milsec = 0;

                return new DateTime((byte)dud100Year.SelectedItem * 100 + (byte)cobYear.SelectedItem,
                   (byte)cobMonth.SelectedItem, (byte)cobDay.SelectedItem,
                   (byte)cobHour.SelectedItem, (byte)cobMinute.SelectedItem,
                   (byte)cobSecond.SelectedItem, milsec);
            }
            set
            {                
                dud100Year.SelectedItem = Convert.ToByte(value.Year / 100);
                cobYear.SelectedItem = Convert.ToByte(value.Year % 100);
                cobMonth.SelectedItem = Convert.ToByte(value.Month);
                SetDays(value.Year, value.Month);
                cobDay.SelectedItem = Convert.ToByte(value.Day);
                cobHour.SelectedItem = Convert.ToByte(value.Hour);
                cobMinute.SelectedItem = Convert.ToByte(value.Minute);
                cobSecond.SelectedItem = Convert.ToByte(value.Second);
                txtMillisecond.Text = value.Millisecond.ToString().PadRight(6, '0');
            }
        }

        public void PatchTextFromResource()
        {
            lblYear.Text = Res.JE_DATETIME_YEAR;
            lblMonth.Text = Res.JE_DATETIME_MONTH;
            lblDay.Text = Res.JE_DATETIME_DAY;
            lblHour.Text = Res.JE_DATETIME_HOUR;
            lblMinute.Text = Res.JE_DATETIME_MINUTE;
            lblSecond.Text = Res.JE_DATETIME_SECOND;
        }

        public SimpleDateTimePicker()
        {
            InitializeComponent();
            PatchTextFromResource();

            dud100Year.Items.AddRange(years100);
            cobYear.DataSource = years99;
            cobMonth.DataSource = months;
            cobDay.DataSource = days31;
            cobHour.DataSource = hours;
            cobMinute.DataSource = minutes;
            cobSecond.DataSource = seconds;
        }
      
        //public void SetBindingTextBox(TextBox textbox)
        //{
        //    BindingTextBox = textbox;
        //    //DateTimeBoxTypes dtbt = JColumn.Type == JType.Date ? DateTimeBoxTypes.Date :
        //    //    JColumn.Type == JType.Time ? DateTimeBoxTypes.Time : DateTimeBoxTypes.DateTime;

        //    if (!DateTime.TryParse(ValueControl.Text, out DateTime r1))
        //        r1 = DateTime.Now;

        //    Value = r1;
        //}

        public void Clear()
        {
            dud100Year.SelectedIndex = cobYear.SelectedIndex = cobMonth.SelectedIndex =
            cobDay.SelectedIndex = cobHour.SelectedIndex = cobMinute.SelectedIndex =
            cobSecond.SelectedIndex = 0;
            dud100Year.Enabled = cobYear.Enabled = cobMonth.Enabled = cobDay.Enabled =
            cobHour.Enabled = cobMinute.Enabled = cobSecond.Enabled = txtMillisecond.Enabled =
            false;
        }

        public void SetType(DateTimePickerStyle type)
        {
            Style = type;
            Clear();
            if (type == DateTimePickerStyle.Date || type == DateTimePickerStyle.DateTime)
                dud100Year.Enabled = cobYear.Enabled = cobMonth.Enabled = cobDay.Enabled = true;
            if (type == DateTimePickerStyle.Time || type == DateTimePickerStyle.DateTime)
                cobHour.Enabled = cobMinute.Enabled = cobSecond.Enabled = txtMillisecond.Enabled = true;
        }

        public void SetDays(int year, int month)
        {
            switch (DateTime.DaysInMonth(year, month))
            {
                case 31:
                    cobDay.DataSource = days31;
                    break;
                case 30:
                    cobDay.DataSource = days30;
                    break;
                case 28:
                    cobDay.DataSource = days28;
                    break;
                case 29:
                    cobDay.DataSource = days29;
                    break;
            }
        }

        private void SimpleDateTimePicker_Load(object sender, EventArgs e)
        {

        }

        private void dud100Year_SelectedItemChanged(object sender, EventArgs e)
        {
            if ((byte)dud100Year.SelectedItem == 0)
                cobYear.DataSource = years99;
            else
                cobYear.DataSource = years100;
        }

        private void cobYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cobYear.DataSource != null && cobMonth.DataSource != null)
                SetDays(Convert.ToInt16(dud100Year.SelectedItem) * 100 + Convert.ToInt16(cobYear.SelectedItem), (byte)cobMonth.SelectedItem);
        }

        private void cobMonth_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cobYear.DataSource != null && cobMonth.DataSource != null)
                SetDays(Convert.ToInt16(dud100Year.SelectedItem) * 100 + Convert.ToInt16(cobYear.SelectedItem), (byte)cobMonth.SelectedItem);
        }
       
    }
}
