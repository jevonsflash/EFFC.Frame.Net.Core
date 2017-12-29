using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Frame.Net.Base.Interfaces.DataConvert;

namespace EFFC.Frame.Net.Base.Data
{
    public class DataSetStd: ICloneable
    {
        List<DataTableStd> _tables = new List<DataTableStd>();

        public DataTableStd this[int index]
        {
            get
            {
                return DataTableStd.ParseStd(this._tables[index]);
            }
        }

        public object Clone()
        {
            var rtn = new DataSetStd();
            foreach(var item in this._tables)
            {
                rtn._tables.Add(item.CloneStd());
            }
            return rtn;
        }

        public void AddTable(DataTableStd dts)
        {
            _tables.Add(dts);
        }

        /// <summary>
        /// 根据columnName和rowIndex获取指定table中的值
        /// </summary>
        /// <param name="tableIndex"></param>
        /// <param name="columnName"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public object GetValue(int tableIndex, string columnName, int rowIndex)
        {
            if (this == null)
            {
                return "";
            }
            else if (tableIndex >= this._tables.Count)
            {
                return "";
            }
            else
            {
                return this._tables[tableIndex][rowIndex,columnName];
            }
        }
        /// <summary>
        /// 根据columnIndex和rowIndex获取指定table中的值
        /// </summary>
        /// <param name="tableIndex"></param>
        /// <param name="columnIndex"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public object GetValue(int tableIndex, int columnIndex, int rowIndex)
        {
            if (this == null)
            {
                return "";
            }
            else if (tableIndex >= this._tables.Count)
            {
                return "";
            }
            else
            {
                return this._tables[tableIndex][rowIndex, columnIndex];
            }
        }
        /// <summary>
        /// 根据columnName和rowIndex获取第一个table中的值
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public object GetValue(string columnName, int rowIndex)
        {

            return GetValue(0, columnName, rowIndex);

        }
        /// <summary>
        /// 根据columnIndex和rowIndex获取第一个table中的值
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public object GetValue(int columnIndex, int rowIndex)
        {
            return GetValue(0, columnIndex, rowIndex);
        }
        /// <summary>
        /// 从游标中填充数据
        /// </summary>
        /// <param name="ddr"></param>
        /// <returns></returns>
        public static DataSetStd FillData(DbDataReader ddr)
        {
            var rtn = new DataSetStd();
            if (ddr != null)
            {
                var index = 0;
                do
                {
                    var dt = DataTableStd.ParseStd(ddr);
                    rtn._tables.Add(dt);
                    index++;
                } while (ddr.NextResult());
            }
            return rtn;
        }
        /// <summary>
        /// table的数量
        /// </summary>
        public int TableCount
        {
            get
            {
                return _tables.Count;
            }
        }
    }
}
