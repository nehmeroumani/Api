using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Core.DB
{
    public class CustomSqlConnection : IDisposable
    {
        private readonly IDbConnection _connection;
        public CustomSqlConnection(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
        }

        public void Open()
        {
            ((SqlConnection)_connection).Open();
        }

        public SqlMapper.GridReader QueryMultiple(string query, object param = null)
        {
            Debug.WriteLine(query);

            var results = _connection.QueryMultiple(query, param);
            return results;

        }

        public IEnumerable<dynamic> Query(string query, object param = null, IDbTransaction transaction = null)
        {
            Debug.WriteLine(query);

            var results = _connection.Query(query, param, transaction);
            return results;

        }

        public IEnumerable<T> Query<T>(string query, object param = null, IDbTransaction transaction = null)
        {
            Debug.WriteLine(query);
            var results = _connection.Query<T>(query, param, transaction);
            return results;

        }

        public IEnumerable<TThird> Query<T, TFirst, TSecond, TThird>(string query, Func<T, TFirst, TSecond, TThird> map,
            object param = null, IDbTransaction transaction = null)
        {
            var results = _connection.Query<T, TFirst, TSecond, TThird>(query, map, param, transaction);
            return results;

        }

        public IEnumerable<TSecond> Query<T, TFirst, TSecond>(string query, Func<T, TFirst, TSecond> map,
            object param = null, IDbTransaction transaction = null)
        {
            Debug.WriteLine(query);

            var results = _connection.Query<T, TFirst, TSecond>(query, map, param, transaction);
            return results;

        }
        public int ExecuteScaler(string query, object param = null, IDbTransaction transaction = null)
        {
            Debug.WriteLine(query);

            return _connection.ExecuteScalar<int>(query, param, transaction);
        }
        public int Execute(string query, object param = null, IDbTransaction transaction = null)
        {
            Debug.WriteLine(query);

            var result = _connection.Execute(query, param, transaction);
            return result;

        }


        public IDbTransaction BeginTransaction()
        {
            return _connection.BeginTransaction();
        }

        public void Dispose()
        {
            _connection.Dispose();
        }

        public void Close()
        {
            _connection.Close();
        }

        public ConnectionState State()
        {
            return _connection.State;
        }

        public bool IsAvailable()
        {
            return _connection.State != ConnectionState.Connecting && _connection.State != ConnectionState.Executing &&
                   _connection.State != ConnectionState.Fetching;
        }

        public bool IsClosed()
        {
            return _connection.State == ConnectionState.Closed;
        }
    }
}
