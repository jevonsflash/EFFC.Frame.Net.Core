using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using EFFC.Frame.Net.Base.Parameter;
using EFFC.Frame.Net.Base.Common;
using Microsoft.SqlServer.Server;
using Frame.Net.Base.Interfaces.DataConvert;
using System.Collections;
using System.Data.Common;
using EFFC.Frame.Net.Base.Data.Base;
using System.Linq;
using System.Data.SqlTypes;

namespace EFFC.Frame.Net.Base.Data
{
    public class DataTableStd : ICloneable,IDisposable
    {

        protected List<DataColumn> pkContent = new List<DataColumn>();
        protected List<FrameDLRObject> data = new List<FrameDLRObject>();
        protected List<DataColumn> schema = new List<DataColumn>();

        public DataTableStd()
        {
            pkContent.Clear();
        }
        /// <summary>
        /// Clone
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var rtn = new DataTableStd();
            foreach (var s in this.pkContent)
            {
                rtn.pkContent.Add(s);
            }
            foreach (var item in this.data)
            {
                rtn.data.Add((FrameDLRObject)item.Clone());
            }

            return rtn;
        }
        /// <summary>
        /// 獲取或者設置值
        /// </summary>
        /// <param name="x">行</param>
        /// <param name="y">列</param>
        /// <returns></returns>
        public object this[int x, string y]
        {
            get
            {
                object rtn = GetValue(y, x);
                return rtn;
            }
            set
            {
                SetValue(value, y, x);
            }
        }
        /// <summary>
        /// 獲取或者設置值
        /// </summary>
        /// <param name="x">行</param>
        /// <param name="y">列</param>
        /// <returns></returns>
        public object this[int x, int y]
        {
            get
            {
                object rtn = GetValue(y, x);
                return rtn;
            }
            set
            {
                SetValue(value, y, x);
            }
        }
        /// <summary>
        /// 隐式转成DataTableStd
        /// </summary>
        /// <param name="dt"></param>
        public static implicit operator DataTableStd(DataTable dt)
        {
            return ParseStd(dt);
        }
        /// <summary>
        /// 转成DataTable类型
        /// </summary>
        /// <param name="dt"></param>
        public static implicit operator DataTable(DataTableStd dt)
        {
            var rtn = new DataTable();

            if (dt.schema.Count <= 0)
            {
                if (dt.data.Count > 0)
                {
                    var item = dt.data[0];
                    foreach (var s in item.Keys)
                    {
                        dt.Columns.Add(new DataColumn()
                        {
                            ColumnName = s,
                            DataType = item.GetValue(s).GetType()
                        });
                    }

                }
            }
            else
            {
                dt.Columns.AddRange(dt.schema.ToArray());
            }
            

            return rtn ;
        }
        /// <summary>
        /// 数据行
        /// </summary>
        public List<FrameDLRObject> Rows
        {
            get
            {
                return data;
            }
        }
        /// <summary>
        /// 数据列资料
        /// </summary>
        public List<DataColumn> Columns
        {
            get
            {
                return this.schema;
            }
        }
        public void ClearData()
        {
            this.data.Clear();
        }

        /// <summary>
        /// 本表的行數
        /// </summary>
        public int RowLength
        {
            get
            {
                return this.data.Count;
            }
        }
        /// <summary>
        /// 返回指定欄位的內容的MaxLength
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public int ColumnMaxLength(string columnName)
        {
            return this.schema.Where(w => w.ColumnName == columnName).FirstOrDefault().MaxLength;

        }
        /// <summary>
        /// 返回指定欄位的内容的MaxLength
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public int ColumnMaxLength(int columnIndex)
        {
            return this.schema.Count > columnIndex ? this.schema[columnIndex].MaxLength : 0;
        }
        /// <summary>
        /// 返回指定欄位的数据类型
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public Type ColumnDateType(string columnName)
        {
            return this.schema.Where(w => w.ColumnName == columnName).FirstOrDefault().DataType;
        }
        /// <summary>
        /// 返回指定欄位的数据类型
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public Type ColumnDateType(int columnIndex)
        {
            return this.schema.Count > columnIndex ? this.schema[columnIndex].DataType : null;
        }
        /// <summary>
        /// 添加列，V3.5.2
        /// </summary>
        /// <param name="p"></param>
        public void AddColumn(ColumnP p)
        {
            if(schema.Where(w=>w.ColumnName.ToLower() == p.ColumnName.ToLower()).Count() > 0)
            {
                return;
            }
            DataColumn dc = new DataColumn();
            dc.ColumnName = p.ColumnName;
            if (!string.IsNullOrEmpty(p.DataType))
                dc.DataType = Type.GetType(p.DataType);
            if (dc.DataType == typeof(string))
            {
                dc.MaxLength = p.Length;
            }
            dc.AllowDBNull = p.IsAllowNull;
            dc.AutoIncrement = p.IsAutoIncrement;
            dc.AutoIncrementSeed = p.AutoIncrementSeed;
            dc.AutoIncrementStep = p.AutoIncrementStep;
            bool isPK = p.IsPK;

            this.schema.Add(dc);
            if (isPK)
            {
                if (pkContent.Where(w => w.ColumnName != dc.ColumnName).Count() <= 0)
                    pkContent.Add(dc);
            }
        }
        /// <summary>
        /// 添加一个column,并标识是否为pk
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="ispk"></param>
        public void AddColumn(DataColumn dc,bool ispk=false)
        {
            if (schema.Where(w => w.ColumnName.ToLower() == dc.ColumnName.ToLower()).Count() > 0)
            {
                return;
            }
            var mydc = new DataColumn()
            {
                AllowDBNull = dc.AllowDBNull,
                AutoIncrement = dc.AutoIncrement,
                AutoIncrementSeed = dc.AutoIncrementSeed,
                AutoIncrementStep = dc.AutoIncrementStep,
                Caption = dc.Caption,
                ColumnMapping = dc.ColumnMapping,
                ColumnName = dc.ColumnName,
                DataType = dc.DataType,
                DateTimeMode = dc.DateTimeMode,
                DefaultValue = dc.DefaultValue,
                Expression = dc.Expression,
                MaxLength = dc.MaxLength,
                Namespace = dc.Namespace,
                Prefix = dc.Prefix,
                ReadOnly = dc.ReadOnly,
                Site = dc.Site,
                Unique = dc.Unique

            };
            schema.Add(mydc);
            if (ispk)
            {
                pkContent.Add(mydc);
            }

        }
        /// <summary>
        /// 判断欄位是否为自增长
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public bool isAutoIncrement(int columnIndex)
        {
            return this.schema.Count > columnIndex ? this.schema[columnIndex].AutoIncrement : false;
        }
        /// <summary>
        /// 判断欄位是否为自增长
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public bool isAutoIncrement(string columnName)
        {
            return this.schema.Where(w => w.ColumnName == columnName).FirstOrDefault().AutoIncrement;
        }

        /// <summary>
        /// 在指定的位置設置值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="columnName"></param>
        /// <param name="rowIndex"></param>
        public void SetValue(object value, string columnName, int rowIndex)
        {
            if (rowIndex >= this.data.Count)
            {
                return;
            }
            var dc = this.schema.Where(w => w.ColumnName.ToLower() == columnName.ToLower());
            if (dc.Count() <= 0)
            {
                return;
            }
            if (dc.First().DataType == null) dc.First().DataType = value.GetType();
            this.data[rowIndex].SetValue(columnName, value);
        }
        /// <summary>
        /// 在指定的位置設置值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="columnIndex"></param>
        /// <param name="rowIndex"></param>
        public void SetValue(object value, int columnIndex, int rowIndex)
        {
            if (rowIndex >= this.data.Count)
            {
                return;
            }
            else if (columnIndex >= this.schema.Count)
            {
                return;
            }
            else
            {
                var columnname = this.schema[columnIndex].ColumnName;
                if (this.schema[columnIndex].DataType == null) this.schema[columnIndex].DataType = value.GetType();
                this.data[rowIndex].SetValue(columnname,value);

            }
        }
        /// <summary>
        /// 将from表中的数据写入到本表中，按照欄位名稱对应，Append方式
        /// </summary>
        /// <param name="from"></param>
        public void SetValueApppend_From(DataTableStd from)
        {
            for (int i = 0; i < from.RowLength; i++)
            {
                foreach (var dc in from.schema)
                {
                    var mycolumn = this.schema.Where(w => w.ColumnName.ToLower() == dc.ColumnName.ToLower()).First();
                    if (this.schema.Where(w=>w.ColumnName.ToLower() == dc.ColumnName.ToLower()).Count() > 0
                        && mycolumn.DataType == dc.DataType)
                    {
                        this.SetNewRowValue(from[i, dc.ColumnName], dc.ColumnName);
                    }
                }
                this.AddNewRow();
            }

        }
        /// <summary>
        /// 给指定的列赋值-所有行
        /// </summary>
        /// <param name="value"></param>
        /// <param name="columnName"></param>
        public void SetValueByColumn(object value, string columnName)
        {
            for (int i = 0; i < this.RowLength; i++)
            {
                this[i, columnName] = value;
            }
        }
        /// <summary>
        /// 给指定的列赋值-所有行
        /// </summary>
        /// <param name="value"></param>
        /// <param name="columnIndex"></param>
        public void SetValueByColumn(object value, int columnIndex)
        {
            for (int i = 0; i < this.RowLength; i++)
            {
                this[i, columnIndex] = value;
            }
        }

        /// <summary>
        /// 根據columnName和rowIndex獲取值
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public object GetValue(string columnName, int rowIndex)
        {
            if (this == null)
            {
                return null;
            }
            if (rowIndex >= this.data.Count)
            {
                return null;
            }
            if (this.schema.Where(w => w.ColumnName.ToLower() == columnName.ToLower()).Count() <= 0)
            {
                return null;
            }
            var dc = this.schema.Where(w => w.ColumnName.ToLower() == columnName.ToLower()).First();
            if (this.data[rowIndex].GetValue(columnName) != null && this.data[rowIndex].GetValue(columnName) != DBNull.Value)
            {
                if (dc.DataType == typeof(string))
                {
                    return ComFunc.nvl(this.data[rowIndex].GetValue(columnName));
                }
                else
                {
                    return this.data[rowIndex].GetValue(columnName);
                }
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 根據columnName和rowIndex獲取值
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public object GetValue(int columnIndex, int rowIndex)
        {
            if (this == null)
            {
                return null;
            }
            if (rowIndex >= this.data.Count)
            {
                return null;
            }
            if (columnIndex >= this.schema.Count)
            {
                return null;
            }
            var dc = this.schema[columnIndex];
            if (this.data[rowIndex].GetValue(dc.ColumnName) != null && this.data[rowIndex].GetValue(dc.ColumnName) != DBNull.Value)
            {
                if (dc.DataType == typeof(string))
                {
                    return ComFunc.nvl(this.data[rowIndex].GetValue(dc.ColumnName));
                }
                else
                {
                    return this.data[rowIndex].GetValue(dc.ColumnName);
                }
            }
            else
            {
                return null;
            }

        }

       
        FrameDLRObject _new_row = null;
        /// <summary>
        /// 新增一個臨時行
        /// </summary>
        public void NewRow()
        {
            this._new_row = FrameDLRObject.CreateInstance(Constants.FrameDLRFlags.SensitiveCase);
        }
        /// <summary>
        /// 給新增行寫值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="columnName"></param>
        public void SetNewRowValue(object value, string columnName)
        {
            if (this._new_row == null) NewRow();
            if (columnName == null)
            {
                return;
            }
            var dc = this.schema.Where(w => w.ColumnName.ToLower() == columnName.ToLower());
            if (dc.Count() <= 0)
            {
                return;
            }
            if (dc.First().DataType == null) dc.First().DataType = value.GetType();
            _new_row.SetValue(columnName, value);
        }
        /// <summary>
        /// 給新增行寫值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="columnIndex"></param>
        public void SetNewRowValue(object value, int columnIndex)
        {
            if (this._new_row == null) NewRow();

            if (columnIndex >= this.schema.Count)
            {
                return;
            }
            var dc = this.schema[columnIndex];
            if (dc.DataType == null) dc.DataType = value.GetType();
            _new_row.SetValue(dc.ColumnName, value);
        }
        /// <summary>
        /// 獲取临时新增行的数据
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public object GetNewRowValue(string columnName)
        {
            if (this._new_row == null)
            {
                return null;
            }
            var dc = this.schema.Where(w => w.ColumnName.ToLower() == columnName.ToLower());
            if (dc.Count() <= 0)
            {
                return null;
            }

            if (this._new_row.GetValue(dc.First().ColumnName) != null && this._new_row.GetValue(dc.First().ColumnName) != DBNull.Value)
            {
                if (dc.First().DataType == typeof(string))
                {
                    return ComFunc.nvl(this._new_row.GetValue(dc.First().ColumnName));
                }
                else
                {
                    return this._new_row.GetValue(dc.First().ColumnName);
                }
            }
            else
            {
                return null;
            }

        }
        /// <summary>
        /// 獲取临时新增行的数据
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public object GetNewRowValue(int columnIndex)
        {
            if (this._new_row == null)
            {
                return null;
            }
             if (columnIndex >= this.schema.Count)
            {
                return null;
            }
            var dc = this.schema[columnIndex];
            if (this._new_row.GetValue(dc.ColumnName) != null && this._new_row.GetValue(dc.ColumnName) != DBNull.Value)
            {
                if (dc.DataType == typeof(string))
                {
                    return ComFunc.nvl(this._new_row.GetValue(dc.ColumnName));
                }
                else
                {
                    return this._new_row.GetValue(dc.ColumnName);
                }
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 將新增的臨時行Add到table中
        /// </summary>
        public void AddNewRow()
        {
            this.data.Add(_new_row);
            _new_row = null;
        }

        /// <summary>
        /// 獲得该表的PK，dt必须带schema
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static DataColumn[] GetPK(DataTable dt)
        {
            if (dt == null) return null;

            return dt.PrimaryKey;
        }

        public DataColumn[] PK
        {
            get {
                return pkContent.ToArray();
            }
        }

        /// <summary>
        /// 獲得该表的PK的名稱列表，dt必须带schema
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string[] GetPKName(DataTable dt)
        {
            DataColumn[] dc = GetPK(dt);
            string[] rtn = new string[dc.Length];
            for (int i = 0; i < dc.Length; i++)
            {
                rtn[i] = dc[i].ColumnName;
            }

            return rtn;
        }

        public string[] PKNames
        {
            get { return pkContent.Select(p => p.ColumnName).ToArray(); }
        }
        /// <summary>
        /// 獲得该表的Columns的名稱列表
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string[] GetColumnName(DataTable dt)
        {
            string[] rtn = new string[dt.Columns.Count];
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                rtn[i] = dt.Columns[i].ColumnName;
            }

            return rtn;
        }

        public string[] ColumnNames
        {
            get { return this.schema.Select(p=>p.ColumnName).ToArray(); }
        }

        /// <summary>
        /// 找出Dt中RowCount
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static int RowNumber(DataTable dt)
        {
            int rtn = 0;

            if (dt != null)
            {
                rtn = dt.Rows.Count;
            }

            return rtn;
        }
        /// <summary>
        /// 复制本表的结构
        /// </summary>
        /// <returns></returns>
        public DataTableStd CloneStd()
        {
            DataTableStd rtn = new DataTableStd();
            foreach(var item in pkContent)
            {
                rtn.pkContent.Add(new DataColumn()
                {
                    AllowDBNull = item.AllowDBNull,
                    AutoIncrement = item.AutoIncrement,
                    AutoIncrementSeed = item.AutoIncrementSeed,
                    AutoIncrementStep = item.AutoIncrementStep,
                    Caption = item.Caption,
                    ColumnMapping = item.ColumnMapping,
                    ColumnName = item.ColumnName,
                    DataType = item.DataType,
                    DateTimeMode = item.DateTimeMode,
                    DefaultValue = item.DefaultValue,
                    Expression = item.Expression,
                    MaxLength = item.MaxLength,
                    Namespace = item.Namespace,
                    Prefix = item.Prefix,
                    ReadOnly = item.ReadOnly,
                    Site = item.Site,
                    Unique = item.Unique

                });
            }
            foreach(var item in schema)
            {
                rtn.schema.Add(new DataColumn()
                {
                    AllowDBNull = item.AllowDBNull,
                    AutoIncrement = item.AutoIncrement,
                    AutoIncrementSeed = item.AutoIncrementSeed,
                    AutoIncrementStep = item.AutoIncrementStep,
                    Caption = item.Caption,
                    ColumnMapping = item.ColumnMapping,
                    ColumnName = item.ColumnName,
                    DataType = item.DataType,
                    DateTimeMode = item.DateTimeMode,
                    DefaultValue = item.DefaultValue,
                    Expression = item.Expression,
                    MaxLength = item.MaxLength,
                    Namespace = item.Namespace,
                    Prefix = item.Prefix,
                    ReadOnly = item.ReadOnly,
                    Site = item.Site,
                    Unique = item.Unique

                });
            }
            foreach(var item in data)
            {
                rtn.data.Add((FrameDLRObject)item.Clone());
            }
            return rtn;
        }

        /// <summary>
        /// 根據columns搜索与values中相同的数据，判断是否存在
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public bool IsExist(Dictionary<string, object> values)
        {
            bool rtn = true;

            if (this.data.Count <= 0)
            {
                rtn = false;
            }
            else
            {
                for (int i = 0; i < this.data.Count; i++)
                {
                    bool tmp = true;
                    foreach (KeyValuePair<string, object> kvp in values)
                    {
                        tmp = tmp & (ComFunc.nvl(GetValue(kvp.Key, i)) == ComFunc.nvl(kvp.Value));
                    }
                    //如果為true，則表示有完全與values相同的資料存在，否則當前這行資料與values不相同，則瀏覽下一行資料
                    if (tmp)
                    {
                        rtn = true;
                        break;
                    }
                    else
                    {
                        rtn = rtn & tmp;
                    }
                }
            }

            return rtn;
        }

        /// <summary>
        /// clone一個DataRow
        /// </summary>
        /// <param name="rowindex"></param>
        /// <param name="todt"></param>
        public void CloneDataRow(int rowindex, ref DataTable todt)
        {
            DataRow rtn = todt.NewRow();
            foreach (DataColumn dc in this.schema)
            {
                if (todt.Columns.Contains(dc.ColumnName))
                {
                    object v = GetValue(dc.ColumnName, rowindex);
                    rtn[dc.ColumnName] = v == null ? DBNull.Value : v;
                }
            }
            todt.Rows.Add(rtn);
        }
        /// <summary>
        /// clone一個DataRow
        /// </summary>
        /// <param name="rowindex"></param>
        /// <param name="todt"></param>
        public void CloneDataRow(int rowindex, ref DataTableStd todt)
        {
            if (todt.data.Count > rowindex)
                todt.data.Add((FrameDLRObject)this.data[rowindex].Clone());
        }
        /// <summary>
        /// 转化成标准类型
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static DataTableStd ParseStd(object o)
        {
            if (o == null)
            {
                return null;
            }
            else if (o is DbDataReader)
            {
                var ddr = (DbDataReader)o;
                var rtn = new DataTableStd();

                if (!ddr.CanGetColumnSchema())
                {
                    for (var i = 0; i < ddr.FieldCount; i++)
                    {
                        rtn.AddColumn(new ColumnP()
                        {
                            ColumnName = ddr.GetName(i)
                        });
                    }

                    while (ddr.Read())
                    {
                        rtn.NewRow();
                        foreach (var col in rtn.schema)
                        {
                            var val = ddr[col.ColumnName];
                            rtn.SetNewRowValue(ConvertDBValue2Object(val), col.ColumnName);
                        }
                        rtn.AddNewRow();
                    }
                }
                else
                {
                    var cschema = ddr.GetColumnSchema();
                    for (int i = 0; i < ddr.FieldCount; i++)
                    {
                        rtn.AddColumn(new ColumnP()
                        {
                            ColumnName = cschema[i].ColumnName,
                            DataType = cschema[i].DataType.FullName,
                            IsAllowNull = cschema[i].AllowDBNull == null ? false : cschema[i].AllowDBNull.Value,
                            IsAutoIncrement = cschema[i].IsAutoIncrement == null ? false : cschema[i].IsAutoIncrement.Value,
                            IsPK = cschema[i].IsKey == null ? false : cschema[i].IsKey.Value,
                            Length = cschema[i].ColumnSize == null ? -1 : cschema[i].ColumnSize.Value
                        });
                    }

                    while (ddr.Read())
                    {
                        rtn.NewRow();
                        foreach (var col in rtn.ColumnNames)
                        {
                            rtn.SetNewRowValue(ConvertDBValue2Object(ddr[col]), col);
                        }

                        rtn.AddNewRow();
                    }
                }


                return rtn;
            }
            else if (o.GetType() != Type.GetType("System.Data.DataTable"))
            {
                return null;
            }
            else if (o is DataTableStd)
            {
                return (DataTableStd)o;
            }
            else if (o is IEnumerable<object>)
            {
                var rtn = new DataTableStd();
                var list = ((IEnumerable<object>)o).Select(d=>(FrameDLRObject)FrameDLRObject.CreateInstance(d,Constants.FrameDLRFlags.SensitiveCase));
                if(list.Count() > 0)
                {
                    var first = list.First();
                    foreach(var item in first.Items)
                    {
                        rtn.AddColumn(new ColumnP() { ColumnName = item.Key });
                    }
                    foreach(var item in list)
                    {
                        rtn.NewRow();
                        foreach(var key in item.Keys)
                        {
                            rtn.SetNewRowValue(item.GetValue(key), key);
                        }
                        rtn.AddNewRow();
                    }
                }
                return rtn;
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 转化成标准类型
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static DataTableStd ParseStd(DataTable o)
        {
            if (o == null)
            {
                return null;
            }
            else
            {
                DataTableStd dts = new DataTableStd();
                foreach(DataColumn c in o.Columns)
                {
                    dts.AddColumn(c, o.PrimaryKey.Contains(c));
                }
                return dts;
            }
        }

        /// <summary>
        /// 根據指定column个数转化成标准类型
        /// </summary>
        /// <param name="columnCount"></param>
        /// <returns></returns>
        public static DataTableStd ParseStd(int columnCount)
        {
            DataTableStd dt = new DataTableStd();

            for (int i = 0; i < columnCount; i++)
            {
                dt.schema.Add(new DataColumn("F" + i));
            }

            return dt;
        }

        /// <summary>
        /// 根據指定columns转化成标准类型
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        public static DataTableStd ParseStd(string[] columns)
        {
            DataTableStd dt = new DataTableStd();

            for (int i = 0; i < columns.Length; i++)
            {
                dt.schema.Add(new DataColumn(columns[i]));
            }

            return dt;
        }

        /// <summary>
        /// 将table中的指定一行的数据转化成字符串
        /// </summary>
        /// <param name="splitComm"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public string ToString(string splitComm, int rowIndex)
        {
            string str = "";
            foreach (DataColumn dc in this.schema)
            {
                str = str.Length > 0 ? str + splitComm + GetValue(dc.ColumnName, rowIndex) : str + GetValue(dc.ColumnName, rowIndex);
            }

            return str;
        }
        /// <summary>
        /// 将table中的Header转化成字符串
        /// </summary>
        /// <param name="splitComm"></param>
        /// <returns></returns>
        public string HeaderToString(string splitComm)
        {
            string str = "";
            foreach (DataColumn dc in this.schema)
            {
                str = str.Length > 0 ? str + splitComm + dc.ColumnName : str + dc.ColumnName;
            }

            return str;
        }

        /// <summary>
        /// 将table中的数据转化成字符串（数据量大的时候不建议使用）
        /// </summary>
        /// <param name="splitComm"></param>
        /// <param name="isIncHeader"></param>
        /// <returns></returns>
        public StringBuilder ToString(string splitComm, bool isIncHeader)
        {
            StringBuilder rtn = new StringBuilder();
            if (isIncHeader)
            {
                rtn.AppendLine(HeaderToString(splitComm));
            }

            for (int i = 0; i < this.RowLength; i++)
            {
                rtn.AppendLine(this.ToString(splitComm, i));
            }

            return rtn;
        }
        /// <summary>
        /// 将DB数据对象转成当前运行环境中的常用对象
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private static object ConvertDBValue2Object(object v)
        {
            if (v is DBNull)
            {
                return null;
            }
            else if (v is SqlDateTime)
            {
                return ((SqlDateTime)v).Value;
            }
            else if (v.GetType().GetInterfaces().Where(p=>p.FullName == "MySql.Data.Types.IMySqlValue").Count() > 0)
            {
                var dobj = FrameExposedObject.From(v);
                if (v.GetType().Name == "MySqlDateTime")
                {
                    //0000-00-00 00:00:00这种格式会无法识别成datetime
                    if (dobj.IsValidDateTime)
                    {
                        return dobj.Value;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return dobj.Value;
                }
               
            }
            else
            {
                return v;
            }
        }

        public void Dispose()
        {
            if(this.data != null)
            {
                data.Clear();
                data = null;
            }
            if (this.schema != null)
            {
                schema.Clear();
                schema = null;
            }
            if (this.pkContent != null)
            {
                pkContent.Clear();
                pkContent = null;
            }
        }
    }
}
