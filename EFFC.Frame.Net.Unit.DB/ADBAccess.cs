using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using EFFC.Frame.Net.Base.Data;
using EFFC.Frame.Net.Base.Interfaces.Core;
using EFFC.Frame.Net.Base.Constants;
using EFFC.Frame.Net.Unit.DB.Parameters;
using EFFC.Frame.Net.Unit.DB.Datas;
using EFFC.Extends.LinqDLR2SQL;
using System.Data.SqlClient;

namespace EFFC.Frame.Net.Unit.DB
{


    public abstract class ADBAccess : ITransaction, IDBAccessInfo
    {
        protected DBStatus _s = DBStatus.Empty;
        int _Count_of_OnePage = 0;
        int _current_page = 0;
        int _to_Page = 0;
        int _total_page = 0;
        int _row_count = 0;
        string _source_sql_page = "";
        string _rownumber_order_by = "";
        bool _is_page = false;
        /// <summary>
        /// 指令执行的timeout执行，默认为
        /// </summary>
        protected virtual int CommandTimeOut
        {
            get
            {
                return 60000;
            }
        }

        public DBStatus CurrentStatus
        {
            get
            {
                return _s;
            }
        }
        

        /// <summary>
        /// 查询接口
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dbp"></param>
        /// <returns></returns>
        public abstract DataSetStd Query(string sql, DBOParameterCollection dbp);
        /// <summary>
        /// 开启连接
        /// </summary>
        /// <param name="connString"></param>
        /// <param name="p"></param>
        public void Open(string connString, params object[] p)
        {
            Open(connString);
        }
        /// <summary>
        /// 开启连接
        /// </summary>
        /// <param name="connString"></param>
        public abstract void Open(string connString);
        /// <summary>
        /// 关闭连接
        /// </summary>
        public abstract void Close();
        /// <summary>
        /// 开启事务
        /// </summary>
        public void BeginTransaction()
        {
            BeginTransaction(FrameIsolationLevel.Default);
        }
        /// <summary>
        /// 开启事务
        /// </summary>
        /// <param name="level"></param>
        public void BeginTransaction(FrameIsolationLevel level)
        {
            switch (level)
            {
                case FrameIsolationLevel.Chaos:
                    BeginTransaction(IsolationLevel.Chaos);
                    break;
                case FrameIsolationLevel.ReadCommitted:
                    BeginTransaction(IsolationLevel.ReadCommitted);
                    break;
                case FrameIsolationLevel.ReadUncommitted:
                    BeginTransaction(IsolationLevel.ReadUncommitted);
                    break;
                case FrameIsolationLevel.RepeatableRead:
                    BeginTransaction(IsolationLevel.RepeatableRead);
                    break;
                case FrameIsolationLevel.Serializable:
                    BeginTransaction(IsolationLevel.Serializable);
                    break;
                case FrameIsolationLevel.Snapshot:
                    BeginTransaction(IsolationLevel.Snapshot);
                    break;
                case FrameIsolationLevel.Unspecified:
                    BeginTransaction(IsolationLevel.Unspecified);
                    break;
                default:
                    BeginTransaction(IsolationLevel.ReadCommitted);
                    break;
            }
        }
        /// <summary>
        /// 开启事务
        /// </summary>
        /// <param name="level"></param>
        public abstract void BeginTransaction(IsolationLevel level);
        /// <summary>
        /// 提交事务
        /// </summary>
        public abstract void CommitTransaction();
        /// <summary>
        /// 回滚事务
        /// </summary>
        public abstract void RollbackTransaction();
        /// <summary>
        /// 执行非查询操作
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="dbp"></param>
        public abstract void ExecuteNoQuery(string sql, DBOParameterCollection dbp);
        /// <summary>
        /// 更新操作
        /// </summary>
        /// <param name="data"></param>
        /// <param name="selectsql"></param>
        public abstract void Update(object data, string selectsql);
        /// <summary>
        /// 新增操作
        /// </summary>
        /// <param name="data"></param>
        /// <param name="toTable"></param>
        public abstract void Insert(object data, string toTable);
        /// <summary>
        /// 删除操作
        /// </summary>
        /// <param name="data"></param>
        /// <param name="toTable"></param>
        public abstract void Delete(object data, string toTable);
        /// <summary>
        /// 執行存儲過程
        /// </summary>
        /// <param name="sp_name"></param>
        /// <param name="isReturnDataSet"></param>
        /// <param name="dbp"></param>
        /// <returns></returns>
        public abstract DBDataCollection ExcuteProcedure(string sp_name, bool isReturnDataSet, ref DBOParameterCollection dbp);
        /// <summary>
        /// 執行翻頁查詢，V1.0.0.0新增
        /// </summary>
        /// <param name="startRow"></param>
        /// <param name="endRow"></param>
        /// <param name="toPage"></param>
        /// <param name="count_of_page"></param>
        /// <param name="sql"></param>
        /// <param name="orderby"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        protected abstract DataTableStd QueryByPage(int startRow, int endRow, int toPage, int count_of_page, string sql, string orderby, DBOParameterCollection p);

        DBOParameterCollection _source_sql_parameters_page;
        /// <summary>
        /// 設定翻頁條件，啟動翻頁機制
        /// </summary>
        /// <param name="p"></param>
        public void StartPageByCondition(DBAPageP p)
        {
            this._Count_of_OnePage = 0;
            this._current_page = 0;
            this._source_sql_page = "";
            this._source_sql_parameters_page = null;
            this._to_Page = 0;
            this._total_page = 0;
            this._is_page = false;
            this._rownumber_order_by = "";
            this._row_count = 0;

            this._Count_of_OnePage = p.Count_of_OnePage;
            this._source_sql_page = p.SQL;
            this._source_sql_parameters_page = p.SQL_Parameters;
            if (!string.IsNullOrEmpty(p.OrderBy))
                this._rownumber_order_by = p.OrderBy;
            this._is_page = true;
            if (p.CurrentPage >= 0)
                this._current_page = p.CurrentPage;

            if (this._source_sql_page.Trim().EndsWith(";"))
            {
                this._source_sql_page = this._source_sql_page.Trim().Substring(0, this._source_sql_page.Trim().Length - 1);
            }
            string sql = "select count(1) from (" + this._source_sql_page + ") t";
            DBOParameterCollection SQL_Parameters = this._source_sql_parameters_page;
            DataSetStd dss = Query(sql, SQL_Parameters);
            IntStd totalcount = IntStd.ParseStd(dss.GetValue(0, 0));
            if (totalcount != null && totalcount != 0)
            {
                _row_count = totalcount;

                if (totalcount.Value % this._Count_of_OnePage == 0)
                    this._total_page = (int)(totalcount.Value / this._Count_of_OnePage);
                else
                    this._total_page = (int)(totalcount.Value / this._Count_of_OnePage) + 1;
            }
        }
        /// <summary>
        /// 總比數
        /// </summary>
        public int TotalRow
        {
            get
            {
                return _row_count;
            }
        }
        /// <summary>
        /// 獲取翻頁下的總資料行數
        /// </summary>
        /// <returns></returns>
        public int RowCountByPage
        {
            get
            {
                return this._row_count;
            }
        }
        /// <summary>
        /// 獲取翻頁下的總頁數
        /// </summary>
        public int TotalPage
        {
            get { return this._total_page; }
        }

        /// <summary>
        /// 獲取翻頁下的當前頁數
        /// </summary>
        public int CurrentPage
        {
            get { return this._current_page; }
        }
        /// <summary>
        /// 下一頁的資料
        /// </summary>
        /// <returns></returns>
        public virtual DataTableStd NextPage()
        {
            this._to_Page = this._current_page >= this._total_page ? this._current_page : this._current_page + 1;
            int start = (this._to_Page - 1) * this._Count_of_OnePage + 1;
            int end = start + this._Count_of_OnePage - 1;
            this._current_page = this._to_Page;
            return QueryByPage(start, end, this._to_Page, this._Count_of_OnePage, this._source_sql_page, this._rownumber_order_by, this._source_sql_parameters_page);
        }
        /// <summary>
        /// 上一頁的資料
        /// </summary>
        /// <returns></returns>
        public virtual DataTableStd PrePage()
        {
            this._to_Page = this._current_page <= 1 ? 1 : this._current_page - 1;
            int start = (this._total_page - 1) * this._Count_of_OnePage + 1;
            int end = start + this._Count_of_OnePage - 1;
            this._current_page = this._to_Page;
            return QueryByPage(start, end, this._to_Page, this._Count_of_OnePage, this._source_sql_page, this._rownumber_order_by, this._source_sql_parameters_page);
        }
        /// <summary>
        /// 最後一頁的資料
        /// </summary>
        /// <returns></returns>
        public virtual DataTableStd LastPage()
        {
            this._to_Page = this._total_page;
            int start = (this._to_Page - 1) * this._Count_of_OnePage + 1;
            int end = start + this._Count_of_OnePage - 1;
            this._current_page = this._to_Page;
            return QueryByPage(start, end, this._to_Page, this._Count_of_OnePage, this._source_sql_page, this._rownumber_order_by, this._source_sql_parameters_page);
        }
        /// <summary>
        /// 第一頁的資料
        /// </summary>
        /// <returns></returns>
        public virtual DataTableStd FirstPage()
        {
            this._to_Page = 1;
            int start = (this._to_Page - 1) * this._Count_of_OnePage + 1;
            int end = start + this._Count_of_OnePage - 1;
            this._current_page = this._to_Page;
            return QueryByPage(start, end, this._to_Page, this._Count_of_OnePage, this._source_sql_page, this._rownumber_order_by, this._source_sql_parameters_page);
        }
        /// <summary>
        /// 跳到toPage页
        /// </summary>
        /// <returns></returns>
        public virtual DataTableStd GoToPage(int toPage)
        {
            this._to_Page = toPage >= this._total_page && this._total_page > 0 ? this._total_page : toPage;
            int start = (this._to_Page - 1) * this._Count_of_OnePage + 1;
            int end = start + this._Count_of_OnePage - 1;
            this._current_page = this._to_Page;
            return QueryByPage(start, end, this._to_Page, this._Count_of_OnePage, this._source_sql_page, this._rownumber_order_by, this._source_sql_parameters_page);
        }
        /// <summary>
        /// 判定是否已經尾頁
        /// </summary>
        /// <returns></returns>
        public bool EndOfPage()
        {
            if (this._current_page == this._total_page)
                return true;
            else
                return false;
        }
        /// <summary>
        /// prepare参数标识符，如sqlserver的参数标识符为@，oracle为:
        /// </summary>
        public abstract string ParameterFlagChar
        {
            get;
        }
        /// <summary>
        /// 获取本DAO对应的Express
        /// </summary>
        public abstract DBExpress MyDBExpress
        {
            get;
        }
        /// <summary>
        /// 获取本Dao对应的LinqDLR2SQL表达式实例
        /// </summary>
        /// <returns></returns>
        public abstract LinqDLRTable NewLinqTable(string table, string alianname = "");
        /// <summary>
        /// DB数据类型
        /// </summary>
        public abstract DBType MyType { get; }
    }
}
