/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using CEC.Blazor.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CEC.Blazor.Extensions
{
    /// <summary>
    /// Class defining extension methods for <see cref="DbContext"/>
    ///  1. Methods use the context to access the database for running stored Procedures
    ///  2. Generic Methods to access to the DbSets
    ///  Many of the Methods rely on:
    ///  1. Correct naming conventions
    ///  2. A <see cref="DbRecordInfo"/> object defined for the Record/Model
    ///  3. Correct <see cref="SPParameterAttribute"/> Attrubute labelling of Record/Model Properties 
    /// </summary>
    public static class DbContextExtensions
    {

        /// <summary>
        /// Extension Method to run a parameterized Store Procedure using an Entity Framework Context DBConnection
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <param name="storedProcName">Stored Procedure Name to run</param>
        /// <param name="parameters">Parameter set for the Stored Procedure</param>
        /// <returns></returns>
        public static async Task<bool> ExecStoredProcAsync(this DbContext context, string storedProcName, List<SqlParameter> parameters)
        {
            var result = false;

            var cmd = context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = storedProcName;
            cmd.CommandType = CommandType.StoredProcedure;
            parameters.ForEach(item => cmd.Parameters.Add(item));
            using (cmd)
            {
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                try
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                catch(Exception e) {
                    Debug.WriteLine(e.Message);
                }
                finally
                {
                    cmd.Connection.Close();
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Generic Method to get a record List from a DbSet
        /// </summary>
        /// <typeparam name="TRecord">Record/Model Type</typeparam>
        /// <param name="context">DBContext</param>
        /// <param name="dbSetName">DbSet Name</param>
        /// <returns></returns>
        public async static Task<List<TRecord>> GetRecordListAsync<TRecord>(this DbContext context, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>, new()
        {
            var dbset = GetDbSet<TRecord>(context, dbSetName);
            return await dbset.ToListAsync();
        }

        /// <summary>
        /// Generic Method to get a filtered record List from a DbSet
        /// Make sure your first filter takes the biggest cut at the dataset 
        /// as it is the only filter run directly against the database.
        /// Subsequent filters are run against the dataset.
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="context">DbContext</param>
        /// <param name="filterList">List of Filter Parameters</param>
        /// <param name="dbSetName">DBSet Name to run the filters against</param>
        /// <returns></returns>
        public async static Task<List<TRecord>> GetRecordFilteredListAsync<TRecord>(this DbContext context, IFilterList filterList, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>, new()
        {
            var firstrun = true;
            var rec = new TRecord();
            // Get the PropertInfo object for the record DbSet
            var propertyInfo = context.GetType().GetProperty(dbSetName ?? rec.RecordInfo.RecordName);
            // Get the actual value and cast it correctly
            var dbset = (DbSet<TRecord>)(propertyInfo.GetValue(context));
            // Get a empty list
            var list = new List<TRecord>();
            // if we have a filter go through each filter
            // note that only the first filter runs a SQL query against the database
            // the rest are run against the dataset.  So do the biggest slice with the first filter for maximum efficiency.
            if (filterList != null && filterList.Filters.Count > 0)
            {
                foreach (var filter in filterList.Filters)
                {
                    // Get the filter propertyinfo object
                    var x = typeof(TRecord).GetProperty(filter.Key);
                    // if we have a list already apply the filter to the list
                    if (list.Count > 0) list = list.Where(item => x.GetValue(item).Equals(filter.Value)).ToList();
                    // If this is the first run we query the database directly
                    else if (firstrun) list = await dbset.FromSqlRaw($"SELECT * FROM vw_{ propertyInfo.Name} WHERE {filter.Key} = {filter.Value}").ToListAsync();
                    firstrun = false;
                }
            }
            //  No list, just get the full recordset
            else list = await dbset.ToListAsync();
            return list;
        }

        /// <summary>
        /// Generic Method to get a Distinct List of a specific field from a Database View 
        /// You must have a DbSet in your DBContext called dbSetName of type <see cref="DbDistinct"/>
        /// </summary>
        /// <typeparam name="TRecord"> Record Type</typeparam>
        /// <param name="context">DBContext</param>
        /// <param name="dbSetName">DbSet Name</param>
        /// <returns></returns>
        public async static Task<List<string>> GetDistinctListAsync(this DbContext context, DbDistinctRequest req)
        {
            var list = new List<string>();
            // wrap in a try as there are many things that can go wrong
            try
            {
                //get the DbDistinct DB Set so we can load the query data into it
                var dbset = GetDbSet<DbDistinct>(context, req.DistinctSetName);
                // Get the data by building the SQL query to run against the view
                var dlist = await dbset.FromSqlRaw($"SELECT DISTINCT(CONVERT(varchar(max), {req.FieldName})) as Value FROM vw_{req.QuerySetName} ORDER BY Value").ToListAsync();
                // Load the results into a string list
                dlist.ForEach(item => list.Add(item.Value));
            }
            catch
            {
                throw new ArgumentException("The SQL Query did not complete.  The most likely cause is one of the DbDistinctRequest parameters is incorrect;");
            }
            return list;
        }

        /// <summary>
        /// Generic Method to get a record List count from a DbSet
        /// </summary>
        /// <typeparam name="TRecord">Record Type</typeparam>
        /// <param name="context">DbContext</param>
        /// <param name="dbSetName">DbSet Name</param>
        /// <returns></returns>
        public async static Task<int> GetRecordListCountAsync<TRecord>(this DbContext context, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>, new()
        {
            var dbset = GetDbSet<TRecord>(context, dbSetName);
            return await dbset.CountAsync();
        }

        /// <summary>
        /// Generic Method to get a record from a DbSet
        /// </summary>
        /// <typeparam name="TRecord">Record Type</typeparam>
        /// <param name="context">DbContext</param>
        /// <param name="id">record ID to fetch</param>
        /// <param name="dbSetName">DbSet Name</param>
        /// <returns></returns>
        public async static Task<TRecord> GetRecordAsync<TRecord>(this DbContext context, int id, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>, new()
        {
            var dbset = GetDbSet<TRecord>(context, dbSetName);
            return await dbset.FirstOrDefaultAsync(item => ((IDbRecord<TRecord>)item).ID == id);
        }

        /// <summary>
        /// Generic Method to get a record from a DbSet
        /// </summary>
        /// <typeparam name="TRecord">Record Type</typeparam>
        /// <param name="context">DbContext</param>
        /// <param name="guid">record GUID to fetch</param>
        /// <param name="dbSetName">DbSet Name</param>
        /// <returns></returns>
        public async static Task<TRecord> GetRecordAsync<TRecord>(this DbContext context, Guid guid, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>, new()
        {
            var dbset = GetDbSet<TRecord>(context, dbSetName);
            try
            {
                return await dbset.FirstOrDefaultAsync(item => ((IDbRecord<TRecord>)item).GUID == guid);
            }
            catch
            {
                throw new InvalidOperationException("The record does not have a database defined GUID Field that can be queried by SQL");
            }
        }

        /// <summary>
        /// Generic Method to get a lookuplist from a DbSet
        /// </summary>
        /// <typeparam name="TRecord">Record Type</typeparam>
        /// <param name="context">DbContext</param>
        /// <returns></returns>
        public async static Task<List<DbBaseRecord>> GetBaseRecordListAsync<TRecord>(this DbContext context) where TRecord : class, IDbRecord<TRecord>, new()
        {
            var list = new List<DbBaseRecord>();
            var dbset = GetDbSet<TRecord>(context, null);
            await dbset.ForEachAsync(item => list.Add(new DbBaseRecord() { ID = item.ID, DisplayName = item.DisplayName }));
            return list;
        }

        /// <summary>
        /// Method to get the DBSet from TRecord or a Name
        /// </summary>
        /// <typeparam name="TRecord">Record Type</typeparam>
        /// <param name="context">DbContext</param>
        /// <param name="dbSetName">DbSet Name</param>
        /// <returns></returns>
        private static DbSet<TRecord> GetDbSet<TRecord>(this DbContext context, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>, new()
        {
            // Get the property info object for the DbSet 
            var rec = new TRecord();
            var pinfo = context.GetType().GetProperty(dbSetName ?? rec.RecordInfo.RecordName);
            // Get the property DbSet
            return (DbSet<TRecord>)pinfo.GetValue(context);
        }
    }
}
