using System;
using System.Collections.Generic;
using System.Linq;
using Core.DB;
using Core.Extentions;
using Dapper;
using Newtonsoft.Json;

namespace Core.Repositories
{
    public class BaseRepository<T> where T : BaseModel
    {
        public static string ConnectionString => Pool.I.ConnectionString;

        protected string BaseColumns = "Id,CreationDate,PublicId,DisplayOrder,LastModified,IsActive,IsDeleted";
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
            SqlInsert = Columns.GenerateInsertQuery(TableName);
            SqlUpdate = Columns.GenerateUpdateQuery(TableName, "Id", "Id,PublicId,CreationDate" + excludeupdate);
            SqlDelete = $"UPDATE [{TableName}] SET IsDeleted=1 WHERE Id=@Id; ";
        }

        #region common functions: Getall, getbyid, save update insert, delete

        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }
        public T GetView(Guid id)
        {
            return Query<T>($"Select * From {ViewName}  WHERE Id='{id}'").SingleOrDefault();
        }
        public T Get(string id)
        {
            if (string.IsNullOrEmpty(SqlSelect))
                return default(T);
            return Query<T>(SqlSelect + $" WHERE Id='{id}'").FirstOrDefault();
        }

        public T GetOld(int id)
        {
            return Query<T>(SqlSelect + $" WHERE OldId='{id}'").SingleOrDefault();
        }
        public T GetSingle(string key, string value)
        {
            if (string.IsNullOrEmpty(SqlSelect))
                return default(T);
            return Query<T>(SqlSelect + " WHERE [" + key + "]=@Key and IsDeleted!=1  ", new { Key = value }).SingleOrDefault();
        }

        public IEnumerable<T> Get(string key, string value, string orderBy = "CreationDate Desc")
        {
            if (string.IsNullOrEmpty(SqlSelect))
                return default(IEnumerable<T>);
            return Query<T>(SqlSelect + " WHERE [" + key + "]=@Key and IsDeleted!=1  order by " + orderBy,
                new { Key = value });
        }

        public T Get(int id)
        {
            if (string.IsNullOrEmpty(SqlSelect))
                return default(T);

            return Query<T>(SqlSelect + " WHERE PublicId=@PublicId", new { PublicId = id }).FirstOrDefault();
        }
        public T GetSingleView(string id )
        {
           
            return Query<T>("Select * from [" + ViewName + "] WHERE Id = @Id   " ,new { Id = id }).SingleOrDefault();
        }
        public T GetSingleWhere(string where = "")
        {
            if (!string.IsNullOrEmpty(where))
                where = "AND " + where;
            return Query<T>("Select * from [" + TableName + "] WHERE IsDeleted!=1   " + where).SingleOrDefault();
        }

        public List<T> GetWhere(string where = "")
        {
            if (!string.IsNullOrEmpty(where))
                where = "AND " + where;
            return Query<T>("Select * from [" + TableName + "] WHERE IsDeleted!=1  " + where).ToList();
        }

        public T GetFirstWhere(string where = "")
        {
            if (!string.IsNullOrEmpty(where))
                where = "AND " + where;
            return Query<T>("Select * from [" + TableName + "] WHERE IsDeleted!=1   " + where).FirstOrDefault();
        }

        public int GetCountView(string where = "", string view = "")
        {
            string viewt = view.IsNotNullOrEmpty() ? view : ViewName;
            if (!string.IsNullOrEmpty(where))
                where = "AND " + where;
            return ExecuteScaler("Select count(*) from [" + viewt + "] WHERE IsDeleted!=1  " + where);
        }

        //public int GetCount(string where = "")
        //{
        //    if (!string.IsNullOrEmpty(where))
        //        where = "AND " + where;
        //    return ExecuteScaler("Select count(*) from [" + TableName + "] WHERE IsDeleted!=1  " + where);
        //}
        public int GetCountWithDeleted()
        {
            return ExecuteScaler("Select count(*) from [" + TableName + "]  ");
        }
        public IEnumerable<T> GetAll(string orderBy = "CreationDate Desc")
        {
            if (string.IsNullOrEmpty(SqlSelect))
                return default(IEnumerable<T>);
            return Query<T>(SqlSelect + " WHERE IsDeleted!=1  order by " + orderBy);
        }
        public IEnumerable<T> GetView(string where = "")
        {
            return GetView("", where);
        }

        public IEnumerable<T> GetView(string view, string where, string orderBy = "CreationDate Desc")
        {
            string viewt = view.IsNotNullOrEmpty() ? view : ViewName;
            if (!string.IsNullOrEmpty(where))
                where = "AND " + where;
            return Query<T>("Select * From " + viewt + " WHERE IsDeleted!=1  " + where + " order By " + orderBy);
        }
        public IEnumerable<T> GetView(int size, int page = 1, string where = "",
            string orderBy = "CreationDate Desc", string view = "")
        {
            string viewt = view.IsNotNullOrEmpty() ? view : ViewName;
            if (!string.IsNullOrEmpty(where))
                where = "AND " + where;
            if (string.IsNullOrEmpty(SqlSelect))
                return default(IEnumerable<T>);

            return
                Query<T>("Select * From " + viewt + " WHERE IsDeleted!=1  " + where +
                         string.Format(" ORDER BY {2} OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", (page - 1) * size, size,
                             orderBy));
        }

        public IEnumerable<T> GetAll(string query, int size, int page = 1, string orderBy = "CreationDate Desc", dynamic param = null)
        {
            return Query<T>(query +
                         string.Format(" ORDER BY {2} OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", (page - 1) * size, size, orderBy), param);
        }
        public IEnumerable<T> GetAllView(RequestData rd, out int total,bool alsoIdsView=false)
        {
            return GetAll(rd, out total, ViewName, alsoIdsView);
        }
        public IEnumerable<T> GetByIds(string[] ids, string idcolumn = "Id",string view="")
        {
            string table = view.IsNotNullOrEmpty() ? view : TableName;
            return
                Query<T>($"SELECT * from [{table}]  WHERE {idcolumn} IN  ('{string.Join("','", ids)}')");
        }
        public int GetCount(RequestData rd)
        {
            string where = GetWhere(rd);
            if (!string.IsNullOrEmpty(where))
                where = "AND " + where;
            return ExecuteScaler("Select count(*) from [" + TableName + "] tb WHERE IsDeleted=0 " + where);
        }
        public int GetCount(string where = "", string view = "")
        {
            string table = view.IsNotNullOrEmpty() ? view : TableName;
            if (!string.IsNullOrEmpty(where))
                where = "AND " + where;
            return ExecuteScaler("Select count(*) from [" + table + "] tb WHERE IsDeleted=0 " + where);
        }
        public IEnumerable<T> GetAll(RequestData rd, out int total, string view = "", bool alsoIdsView = false)
        {

            if (rd.Ids != null && rd.Ids.Any())
            {
                total = rd.Ids.Count;
                return
                    GetByIds(rd.Ids.Select(z=>z.ToString()).ToArray(), "Id", alsoIdsView? view:"");
            }

            string where = GetWhere(rd);
            
            var list = GetAll(size: rd.Size, page: rd.Page, where: where, orderBy: rd.OrderBy, view: view);
            total = GetCount(where: where, view: view);
            return list;
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
        public IEnumerable<T> GetAll(int size, int page = 1, string where = "", string orderBy = "CreationDate Desc", string view = "")
        {
            if (!string.IsNullOrEmpty(where))
                where = "AND " + where;
            if (string.IsNullOrEmpty(SqlSelect))
                return default(IEnumerable<T>);


            return
                Query<T>($"Select * from [{(string.IsNullOrEmpty(view) ? TableName : view)}] AS tb  WHERE IsDeleted=0 " + where +
                         string.Format(" ORDER BY {2} OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", (page - 1) * size, size, orderBy));
        }


        //public IEnumerable<T> GetAll(RequestData rd, out int total)
        //{
        //    if (rd.Ids != null && rd.Ids.Any())
        //    {
        //        total = rd.Ids.Count;
        //        return
        //            Query<T>(SqlSelect + $" WHERE Id IN  ('{string.Join("','", rd.Ids)}')");
        //    }

        //    var list = GetAll(size: rd.Size, page: rd.Page, orderBy: rd.OrderBy);
        //    total = GetCount();
        //    return list;
        //}
        //        public IEnumerable<T> GetAll(int size, int page = 1, string where = "", string orderBy = "CreationDate Desc")
        //        {
        //            if (!string.IsNullOrEmpty(where))
        //                where = "AND " + where;
        //            if (string.IsNullOrEmpty(SqlSelect))
        //                return default(IEnumerable<T>);

        //#if MySql
        //    return Query<T>(SqlSelect + " Where  IsDeleted = 0  " + where + string.Format(" ORDER BY {2}  LIMIT  {1} OFFSET {0}", (page - 1) * size, size, orderBy));
        //#else

        //            return
        //                Query<T>(SqlSelect + " WHERE IsDeleted!=1  " + where +
        //                         string.Format(" ORDER BY {2} OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", (page - 1) * size, size, orderBy));
        //#endif


        //        }

        public string Save(T model)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                model.Id = Guid.NewGuid().ToString();
                model.CreationDate = DateTime.Now;
                model.LastModified = DateTime.Now;
                model.IsDeleted = false;
                Insert(model);
            }
            else
            {
                model.LastModified = DateTime.Now;
                Update(model);
            }

            return model.Id;
        }

        //public Guid Save(T model)
        //{
        //    model.LastModified = DateTime.Now;

        //    if (model.Id == Guid.Empty)
        //    {
        //        model.Id = Guid.NewGuid();
        //        model.CreationDate = DateTime.Now;
        //        if (model.Status == 0)
        //            model.Status = (int)StatusEnum.Active;
        //        Insert(model);
        //    }
        //    else
        //        Update(model);

        //    return model.Id;
        //}

        public void Insert(IEnumerable<T> models)
        {
            if (SqlInsert.IsNotNullOrEmpty())
                Execute(SqlInsert, models);
        }

        public void Insert(T model)
        {
            if (SqlInsert.IsNotNullOrEmpty())
                Execute(SqlInsert, model);
        }

        public void Update(T model)
        {
            if (SqlUpdate.IsNotNullOrEmpty())
                Execute(SqlUpdate, model);
        }

        public void Delete(string id)
        {
            if (SqlDelete.IsNotNullOrEmpty())
                Execute(SqlDelete, new { Id = id });
        }

        public void UpdateColumnQuery(string column, string value, Guid id)
        {
            Execute("UPDATE [" + TableName + "] SET " + column + "=" + value + " WHERE Id='" + id + "'");
        }

        public void UpdateColumn(string column, string value, string id)
        {
            Execute("UPDATE [" + TableName + "] SET " + column + "=@value WHERE Id=@id", new { value, id });
        }

        public void UpdateColumn(string column, DateTime value, Guid id)
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

        protected SqlMapper.GridReader QueryMultiple(string sql, object param = null)
        {
            return ConnectionManager.GetConnection(ConnectionString).QueryMultiple(sql, param);
        }

        #endregion
    }

    public class BaseModel
    {
        public string Id { get; set; }

        [JsonIgnore]
        public DateTime CreationDate { get; set; }
        public DateTime LastModified { get; set; }

        [JsonIgnore]
        public int PublicId { get; set; }

        public int DisplayOrder { get; set; }
        //public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

    }

    public class SearchCriteria
    {
        public int Page { get; set; }
        public int Size { get; set; }
        public string Query { get; set; }
        public string OrderBy { get; set; }
    }
    public class SubFilter
    {
        public string SubTable { get; set; }
        public string MainTableField { get; set; }
        public string SubTableField { get; set; }
    }
}