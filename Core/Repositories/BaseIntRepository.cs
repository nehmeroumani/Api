using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cache;
using Core.DB;
using Core.Extentions;
using Dapper;
using Newtonsoft.Json;

namespace Core.Repositories
{
    public class BaseIntRepository<T> where T : BaseIntModel
    {
        public static string ConnectionString => Pool.I.ConnectionString;

        protected string BaseColumns = "Id,CreationDate,IsDeleted,LastModified";
        protected string SqlSelect;
        protected string SqlInsert;
        protected string SqlUpdate;
        protected string SqlDelete;
        protected string TableName;
        protected string ViewName;
        protected string Columns;
        protected List<SubFilter> SubFilters;

        protected void Init(string table, string columns, string excludeupdate = "", List<SubFilter> subFilters = null)
        {
            if (string.IsNullOrEmpty(excludeupdate))
                excludeupdate = "," + excludeupdate;
            TableName = table;
            ViewName = "v" + table;
            this.SubFilters = subFilters;
            Columns = BaseColumns + (columns.IsNotNullOrEmpty() ? "," : "") + columns;
            SqlSelect = "SELECT " + Columns.AddBraces() + " FROM [" + TableName + "] ";
            SqlInsert = Columns.GenerateInsertQuery(TableName, "Id");
            SqlUpdate = Columns.GenerateUpdateQuery(TableName, "Id", "Id,CreationDate" + excludeupdate);
            SqlDelete = $"UPDATE [{TableName}] SET IsDeleted=1 WHERE Id=@Id; ";
        }

        #region common functions: Getall, getbyid, save update insert, delete


        //public T GetView(int id)
        //{
        //    return Query<T>($"Select * From {ViewName}  WHERE Id='{id}'").SingleOrDefault();
        //}
        public T Get(int id)
        {
            if (string.IsNullOrEmpty(SqlSelect))
                return default(T);
            return Query<T>(SqlSelect + $" WHERE Id='{id}'").FirstOrDefault();
        }


        public T GetSingle(string key, string value)
        {
            if (string.IsNullOrEmpty(SqlSelect))
                return default(T);
            return Query<T>(SqlSelect + " WHERE [" + key + "]=@Key and IsDeleted=0   ", new { Key = value }).SingleOrDefault();
        }

        public IEnumerable<T> Get(string key, string value, string orderBy = "CreationDate Desc")
        {
            if (string.IsNullOrEmpty(SqlSelect))
                return default(IEnumerable<T>);
            return Query<T>(SqlSelect + " WHERE [" + key + "]=@Key and IsDeleted=0 order by " + orderBy,
                new { Key = value });
        }


        public T GetSingleWhere(string where = "")
        {
            if (!string.IsNullOrEmpty(where))
                where = "AND " + where;
            return Query<T>("Select * from [" + TableName + "] WHERE IsDeleted=0  " + where).SingleOrDefault();
        }

        public List<T> GetWhere(string where = "")
        {
            if (!string.IsNullOrEmpty(where))
                where = "AND " + where;
            return Query<T>("Select * from [" + TableName + "] WHERE IsDeleted=0 " + where).ToList();
        }

        public T GetFirstWhere(string where = "")
        {
            if (!string.IsNullOrEmpty(where))
                where = "AND " + where;
            return Query<T>("Select * from [" + TableName + "] WHERE IsDeleted=0  " + where).FirstOrDefault();
        }

        public int GetCountView(string where = "", string view = "")
        {
            string viewt = view.IsNotNullOrEmpty() ? view : ViewName;
            if (!string.IsNullOrEmpty(where))
                where = "AND " + where;
            return ExecuteScaler("Select count(*) from [" + viewt + "] WHERE IsDeleted=0 " + where);
        }

        public int GetCount(string @where = "", string view = "", bool query=false)
        {

            string table = TableName;
            if (!string.IsNullOrEmpty(view))
                table = view;

            table = query ? $"({table}) as qv" : $"[{table}]";

            if (!string.IsNullOrEmpty(where))
                where = "AND " + where;
            return ExecuteScaler("Select count(*) from " + table + " WHERE IsDeleted=0 " + where);
        }
        public int GetCount(RequestData rd)
        {
            string where = GetWhere(rd);
            if (!string.IsNullOrEmpty(where))
                where = "AND " + where;
            return ExecuteScaler("Select count(*) from [" + TableName + "] tb WHERE IsDeleted=0 " + where);
        }

        public int GetCountWithDeleted()
        {
            return ExecuteScaler("Select count(*) from [" + TableName + "]  ");
        }

        public IEnumerable<T> GetAll(string orderBy = "CreationDate Desc")
        {
            if (string.IsNullOrEmpty(SqlSelect))
                return default(IEnumerable<T>);
            return Query<T>(SqlSelect + " WHERE IsDeleted=0 order by " + orderBy);
        }
        //public IEnumerable<T> GetView(string where = "")
        //{
        //    return GetView("", where);
        //}

        //public IEnumerable<T> GetView(string view, string where, string orderBy = "CreationDate Desc")
        //{
        //    string viewt = view.IsNotNullOrEmpty() ? view : ViewName;
        //    if (!string.IsNullOrEmpty(where))
        //        where = "AND " + where;
        //    return Query<T>("Select * From " + viewt + " WHERE IsDeleted=0 " + where + " order By " + orderBy);
        //}
        //public IEnumerable<T> GetView(int size, int page = 1, string where = "",
        //    string orderBy = "CreationDate Desc", string view = "")
        //{
        //    string viewt = view.IsNotNullOrEmpty() ? view : ViewName;
        //    if (!string.IsNullOrEmpty(where))
        //        where = "AND " + where;
        //    if (string.IsNullOrEmpty(SqlSelect))
        //        return default(IEnumerable<T>);

        //    return
        //        Query<T>("Select * From " + viewt + " WHERE IsDeleted=0 " + where +
        //                 string.Format(" ORDER BY {2} OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", (page - 1) * size, size,
        //                     orderBy));
        //}

        public IEnumerable<T> GetAll(string query, int size, int page = 1, string orderBy = "CreationDate Desc", dynamic param = null)
        {
            return Query<T>(query +
                         string.Format(" ORDER BY {2} OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", (page - 1) * size, size, orderBy), param);
        }

        //public IEnumerable<T> GetAllView(RequestData rd, out int total)
        //{
        //    return GetAll(rd, out total, ViewName);
        //}
        public IEnumerable<T> GetByIds(int[] ids, string idcolumn = "Id")
        {
            return
                Query<T>(SqlSelect + $" WHERE {idcolumn} IN  ('{string.Join("','", ids)}') and IsDeleted=0 ");
        }

        public string GetWhere(RequestData rd)
        {
            string where = "";
            int i = 0;

            foreach (var f in rd.Filter)
            {
                var subFilter = SubFilters?.SingleOrDefault(x => x.SubTableField.ToLower() == f.Key.ToLower());
                if (subFilter == null)
                {
                    if (f.Operator == "eq")
                        where += $"{f.Key}='{f.Value}'";
                    else if (f.Operator == "neq")
                        where += $"{f.Key}<>'{f.Value}'";
                    else if (f.Operator == "cs")
                        where += $"{f.Key} LIKE N'%{f.Value}%'";
                    else if (f.Operator == "gt")
                        where += $"{f.Key} > CONVERT(INT, {f.Value}) ";
                    else if (f.Operator == "lt")
                        where += $"{f.Key} < CONVERT(INT, {f.Value}) ";
                    else if (f.Operator == "gttime")
                        where += $"{f.Key} > '{f.Value}' ";
                    else if (f.Operator == "lttime")
                        where += $"{f.Key} < '{f.Value}' ";
                }
                else
                {
                    where += $"tb.Id In (SELECT DISTINCT {subFilter.MainTableField} FROM {subFilter.SubTable} Where {subFilter.MainTableField} = tb.Id and {subFilter.SubTableField} = '{f.Value}')";
                }
                if (++i < rd.Filter.Count)
                    where += " AND ";
            }
            return where;
        }
        public IEnumerable<T> GetAll(RequestData rd, out int total, string view = "", bool query = false)
        {

            if (rd.Ids != null && rd.Ids.Any())
            {
                total = rd.Ids.Count;
                return
                    GetByIds(rd.Ids.ToArray());
            }

            string where = GetWhere(rd);

            var list = GetAll(size: rd.Size, page: rd.Page, where: where, orderBy: rd.OrderBy, view: view, query: query);
            total = GetCount(where: where, view: view, query: query);
            return list;
        }
        public IEnumerable<T> GetAll(int size, int page = 1, string where = "", string orderBy = "CreationDate Desc", string view = "", bool query = false)
        {
            if (!string.IsNullOrEmpty(where))
                where = "AND " + where;
            if (string.IsNullOrEmpty(SqlSelect))
                return default(IEnumerable<T>);

            string table = TableName;
            if (!string.IsNullOrEmpty(view))
                table = view;

            table = query ? $"({table}) as qv" : $"[{table}]";

            return
                Query<T>($"Select * from {table}  WHERE IsDeleted=0 " + where +
                         string.Format(" ORDER BY {2} OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", (page - 1) * size, size, orderBy));
        }



        public int Save(T model)
        {
            model.LastModified = DateTime.Now;

            if (model.Id == 0)
            {
                //model.Id = Guid.NewGuid();
                model.CreationDate = DateTime.Now;
                Insert(model);
            }
            else
                Update(model);

            return model.Id;
        }

        public void Insert(IEnumerable<T> models)
        {
            if (SqlInsert.IsNotNullOrEmpty())
                Execute(SqlInsert, models);
        }

        protected void Insert(T model)
        {
            if (SqlInsert.IsNotNullOrEmpty())
                model.Id = ExecuteScaler(SqlInsert, model);
        }

        public void Update(T model)
        {
            if (SqlUpdate.IsNotNullOrEmpty())
                Execute(SqlUpdate, model);
        }

        public void Delete(int id)
        {
            if (SqlDelete.IsNotNullOrEmpty())
                Execute(SqlDelete, new { Id = id });
        }

        public void UpdateColumnQuery(string column, string value, int id)
        {
            Execute("UPDATE [" + TableName + "] SET " + column + "=" + value + " WHERE Id='" + id + "'");
        }

        public void UpdateColumn(string column, string value, int id)
        {
            Execute("UPDATE [" + TableName + "] SET " + column + "=@value WHERE Id=@id", new { value, id });
        }

        public void UpdateColumn(string column, DateTime value, int id)
        {
            Execute("UPDATE [" + TableName + "] SET " + column + "=@value WHERE Id=@id", new { value, id });
        }

        #endregion

        #region Dapper access

        public int Execute(string sql, dynamic param = null)
        {
            return ConnectionManager.GetConnection(ConnectionString).Execute(sql, param);
        }

        public int ExecuteScaler(string sql, dynamic param = null)
        {
            return ConnectionManager.GetConnection(ConnectionString).ExecuteScaler(sql, param);
        }


        protected IEnumerable<T> Query<T>(string sql, dynamic param = null)
        {
            var connection = ConnectionManager.GetConnection(ConnectionString);
            return connection.Query<T>(sql, param);
        }

        protected IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map,
            dynamic param = null)
        {
            return ConnectionManager.GetConnection(ConnectionString).Query<TFirst, TSecond, TReturn>(sql, map, param);
        }
        protected IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map,
            dynamic param = null)
        {
            return ConnectionManager.GetConnection(ConnectionString).Query<TFirst, TSecond, TThird, TReturn>(sql, map, param);
        }
        protected SqlMapper.GridReader QueryMultiple(string sql, object param = null)
        {
            return ConnectionManager.GetConnection(ConnectionString).QueryMultiple(sql, param);
        }

        #endregion
    }

    public class BaseIntModel
    {
        public int Id { get; set; }
        [JsonIgnore]
        public DateTime CreationDate { get; set; }

        public DateTime LastModified { get; set; }
        public bool IsDeleted { get; set; }


    }


}