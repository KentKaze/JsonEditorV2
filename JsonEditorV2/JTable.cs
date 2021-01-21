﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace JsonEditor
{
    public class JTable : IList<JLine>
    {
        public string Name { get; set; }
        public List<JColumn> Columns { get; set; } = new List<JColumn>();
        public List<JLine> Lines { get; set; } = new List<JLine>();

        public bool HasKey { get => Columns.Exists(m => m.IsKey); }
        public bool Loaded { get; set; }
        public bool Changed { get; set; }
        public bool Valid { get; set; }

        public int Count => ((IList<JLine>)Lines).Count;

        public bool IsReadOnly => ((IList<JLine>)Lines).IsReadOnly;

        public JLine this[int index] { get => ((IList<JLine>)Lines)[index]; set => ((IList<JLine>)Lines)[index] = value; }

        public JTable()
            : this("")
        { }

        public JTable(string name, bool isNew = false)
        {
            Name = name;
            Loaded = isNew;
            Valid = true;
        }

        public JTable(string name, object jArray, bool isNew = false)
        {
            Name = name;
            Loaded = isNew;
            LoadJson(jArray, true);
        }

        /// <summary>
        /// 讀取JFileInfo檔案設定
        /// </summary>
        /// <param name="jfi"></param>
        public void LoadFileInfo(JTableInfo jfi)
        {
            //檢查一下是否正確
            if (jfi == null)
                throw new ArgumentException($"LoadFileInfo:{Name},");
            if (Name != jfi.Name)
                throw new MissingMemberException($"LoadFileInfo:{Name},{jfi.Name}");
            if (Columns.Count != 0)
            {
                if (Columns.Count != jfi.Columns.Count)
                    throw new IndexOutOfRangeException($"LoadFileInfo:{Columns.Count},{jfi.Columns.Count}");
                for (int i = 0; i < jfi.Columns.Count; i++)
                    if (Columns[i].Name != jfi.Columns[i].Name)
                        throw new MissingFieldException($"LoadFileInfo:{Columns[i].Name},{jfi.Columns[i].Name}");
            }
            Columns = jfi.Columns;
        }

        /// <summary>
        /// 擷取JFileInfo檔案內容
        /// </summary>
        /// <returns></returns>
        public JTableInfo GetJTableInfo()
        {
            JTableInfo jfi = new JTableInfo();
            List<JColumn> jcs = new List<JColumn>(Columns);
            jfi.Name = Name;
            jfi.Columns = jcs;
            return jfi;
        }

        /// <summary>
        /// 擷取存檔用的Data Object
        /// </summary>
        /// <returns></returns>
        public object GetJsonObject()
        {
            List<object> result = new List<object>();
            foreach (JLine jl in Lines)
            {
                var line = new ExpandoObject() as IDictionary<string, object>;
                for (int i = 0; i < Columns.Count; i++)
                {
                    switch(Columns[i].Type)
                    {
                        case JType.Date:
                            if (jl[i].Value != null)
                                line.Add(Columns[i].Name, ((DateTime)jl[i].Value).ToShortDateString());
                            else
                                line.Add(Columns[i].Name, null);
                            break;
                        case JType.Time:
                            if (jl[i].Value != null)
                                line.Add(Columns[i].Name, ((DateTime)jl[i].Value).ToShortTimeString());
                            else
                                line.Add(Columns[i].Name, null);
                            break;
                        default:
                            line.Add(Columns[i].Name, jl[i].Value);
                            break;
                    }                    
                }
                    
                result.Add(line);
            }
            return result;
        }

        //public void LoadJsonWithColumnsInfo(object jArray)
        //{
        //    if (jArray == null)
        //        return;

        //    JArray jr = jArray as JArray;
        //    if (jr == null)
        //        throw new ArgumentNullException();

        //    foreach (JToken jt in jr)
        //    {
        //        JLine items = new JLine();
        //        JObject jo = jt as JObject;

        //        foreach (KeyValuePair<string, JToken> kvp in jo)
        //        {

        //        }                
        //        Lines.Add(items);
        //    }
        //    Loaded = true;
        //    Valid = false;
        //    CheckAllValid();
        //}

        /// <summary>
        /// 讀取Json物件
        /// </summary>
        /// <param name="jArray">JArray</param>
        /// <param name="produceColumnInfo">是否更新欄位</param>
        public void LoadJson(object jArray, bool produceColumnInfo = false)
        {
            bool isFirst = true;
            bool isFirstFirst = true;

            if (jArray == null)
                return;

            JArray jr = jArray as JArray;
            if (jr == null)
                throw new ArgumentNullException();

            if (produceColumnInfo)
                Columns.Clear();

            foreach (JToken jt in jr)
            {
                JLine items = new JLine();
                JObject jo = jt as JObject;
                JColumn jc = null;

                int i = 0;
                foreach (KeyValuePair<string, JToken> kvp in jo)
                {
                    if (produceColumnInfo)
                    {
                        if (isFirstFirst)
                        {
                            jc = new JColumn(kvp.Key, kvp.Value.ToJType(), kvp.Key == "ID", true,
                                Math.Abs(kvp.Value.ToString().Length / 50) + 1);
                            Columns.Add(jc);
                            isFirstFirst = false;
                        }
                        else if (isFirst)
                        {
                            jc = new JColumn(kvp.Key, kvp.Value.ToJType(), kvp.Key == "ID", false,
                                Math.Abs(kvp.Value.ToString().Length / 50) + 1);
                            Columns.Add(jc);
                        }
                        else
                            jc = Columns[i];
                    }
                    else
                        jc = Columns[i];

                    if (kvp.Value.Type == JTokenType.Null)
                        items.Add(JValue.FromObject(null));
                    else
                        items.Add(JValue.FromObject(kvp.Value.ToString().ParseJType(jc.Type)));

                    i++;
                    //switch (kvp.Value.Type)
                    //{
                    //    case JTokenType.Integer:
                    //        items.Add(JValue.FromObject(kvp.Value.ParseJType(jc.Type) ));
                    //        break;
                    //    case JTokenType.Float:
                    //        items.Add(JValue.FromObject(Convert.ToDouble(kvp.Value)));
                    //        break;
                    //    case JTokenType.Guid:
                    //        items.Add(JValue.FromObject(Guid.Parse(kvp.Value.ToString())));
                    //        break;
                    //    case JTokenType.Null:
                    //        items.Add(JValue.FromObject(null));
                    //        break;
                    //    case JTokenType.Boolean:
                    //        items.Add(JValue.FromObject(Convert.ToBoolean(kvp.Value)));
                    //        break;
                    //    case JTokenType.Date:
                    //        items.Add(JValue.FromObject(DateTime.Parse(kvp.Value.ToString())));
                    //        break;
                    //    default:
                    //        items.Add(JValue.FromObject(kvp.Value.ToString()));
                    //        break;
                    //}
                }

                isFirst = false;
                Lines.Add(items);
            }
            Loaded = true;
            Valid = false;
            CheckAllValid();
        }

        /// <summary>
        /// 確認所有資料符合欄位定義
        /// </summary>
        public bool CheckAllValid()
        {
            Valid = false;

            //Key Check
            List<int> keyIndex = new List<int>();
            for (int i = 0; i < Columns.Count; i++)
                if (Columns[i].IsKey)
                    keyIndex.Add(i);

            HashSet<string> keyCheckSet = new HashSet<string>();
            string checkString;
            for (int i = 0; i < Lines.Count; i++)
            {
                if (keyIndex.Count != 0)
                {
                    checkString = "";
                    for (int j = 0; j < keyIndex.Count; j++)
                        if(Lines[i][j].Value != null)
                            checkString = string.Concat(checkString, Lines[i][j].Value.ToString());
                    if (!keyCheckSet.Add(checkString))
                        return Valid;
                }
                for (int j = 0; j < Columns.Count; j++)
                {
                    if (!Lines[i][j].Value.TryParseJType(Columns[j].Type, out object o))
                        return Valid;

                    if (Columns[j].Type == JType.String &&
                        !string.IsNullOrEmpty(Columns[j].Regex) &&
                        !Regex.IsMatch(Lines[i][j].Value.ToString(), Columns[j].Regex))
                        return Valid;
                }
            }
            Valid = true;
            return Valid;
        }

        public int IndexOf(JLine item)
        {
            return ((IList<JLine>)Lines).IndexOf(item);
        }

        public void Insert(int index, JLine item)
        {
            ((IList<JLine>)Lines).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<JLine>)Lines).RemoveAt(index);
        }

        public void Add(JLine item)
        {
            ((IList<JLine>)Lines).Add(item);
        }

        public void Clear()
        {
            ((IList<JLine>)Lines).Clear();
        }

        public bool Contains(JLine item)
        {
            return ((IList<JLine>)Lines).Contains(item);
        }

        public void CopyTo(JLine[] array, int arrayIndex)
        {
            ((IList<JLine>)Lines).CopyTo(array, arrayIndex);
        }

        public bool Remove(JLine item)
        {
            return ((IList<JLine>)Lines).Remove(item);
        }

        public IEnumerator<JLine> GetEnumerator()
        {
            return ((IList<JLine>)Lines).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<JLine>)Lines).GetEnumerator();
        }
    }
}
