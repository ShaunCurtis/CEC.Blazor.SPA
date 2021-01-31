/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using CEC.Blazor.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using CEC.Blazor.Extensions;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace CEC.Blazor.Services
{
    public abstract class BaseServerDataService<TRecord, TContext> :
        BaseDataService<TRecord, TContext>,
        IDataService<TRecord, TContext>
        where TRecord : class, IDbRecord<TRecord>, new()
        where TContext : DbContext
    {

        public BaseServerDataService(IConfiguration configuration, IDbContextFactory<TContext> dbContext) : base(configuration)
        {
            this.DBContext = dbContext;
            this.RecordInfo = new TRecord().RecordInfo;
            Debug.WriteLine($"==> New Instance {this.ToString()} ID:{this.ServiceID.ToString()} ");
        }

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<TRecord>> GetRecordListAsync() => await this.DBContext.CreateDbContext().GetRecordListAsync<TRecord>();

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public virtual async Task<List<TRecord>> GetFilteredRecordListAsync(IFilterList filterList) => await this.DBContext.CreateDbContext().GetRecordFilteredListAsync<TRecord>(filterList);

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public virtual async Task<TRecord> GetRecordAsync(int id) => await this.DBContext.CreateDbContext().GetRecordAsync<TRecord>(id);

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public virtual async Task<TRecord> GetRecordAsync(Guid guid) => await this.DBContext.CreateDbContext().GetRecordAsync<TRecord>(guid);

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <returns></returns>
        public virtual async Task<int> GetRecordListCountAsync() => await this.DBContext.CreateDbContext().GetRecordListCountAsync<TRecord>();

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public virtual async Task<DbTaskResult> UpdateRecordAsync(TRecord record) => await this.RunStoredProcedure(record, SPType.Update);

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public async virtual Task<DbTaskResult> CreateRecordAsync(TRecord record) => await this.RunStoredProcedure(record, SPType.Create);

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<DbTaskResult> DeleteRecordAsync(TRecord record) => await this.RunStoredProcedure(record, SPType.Delete);

        /// <summary>
        /// Inherited IDataService Method
        /// </summary>
        /// <typeparam name="TLookup"></typeparam>
        /// <returns></returns>
        public virtual async Task<List<string>> GetDistinctListAsync(DbDistinctRequest req) => await this.DBContext.CreateDbContext().GetDistinctListAsync(req);

        /// <summary>
        /// Method to execute a stored procedure against the dataservice
        /// </summary>
        /// <param name="record"></param>
        /// <param name="spType"></param>
        /// <returns></returns>
        protected async Task<DbTaskResult> RunStoredProcedure(TRecord record, SPType spType)
        {
            Debug.WriteLine($"==> RunStoredProcedure on {this.ToString()} ID:{this.ServiceID.ToString()} ");
            var ret = new DbTaskResult()
            {
                Message = $"Error saving {this.RecordInfo.RecordDescription}",
                IsOK = false,
                Type = MessageType.Danger
            };

            var spname = spType switch
            {
                SPType.Create => this.RecordInfo.CreateSP,
                SPType.Update => this.RecordInfo.UpdateSP,
                SPType.Delete => this.RecordInfo.DeleteSP,
                _ => string.Empty
            };
            var parms = this.GetSQLParameters(record, spType);
            if (await this.DBContext.CreateDbContext().ExecStoredProcAsync(spname, parms))
            {
                var idparam = parms.FirstOrDefault(item => item.Direction == ParameterDirection.Output && item.SqlDbType == SqlDbType.Int && item.ParameterName.Contains("ID"));
                ret = new DbTaskResult()
                {
                    Message = $"{this.RecordInfo.RecordDescription} saved",
                    IsOK = true,
                    Type = MessageType.Success
                };
                if (idparam != null) ret.NewID = Convert.ToInt32(idparam.Value);
            }
            return ret;
        }

        /// <summary>
        /// Method that sets up the SQL Stored Procedure Parameters
        /// </summary>
        /// <param name="item"></param>
        /// <param name="withid"></param>
        /// <returns></returns>
        protected virtual List<SqlParameter> GetSQLParameters(TRecord record, SPType spType)
        {
            var parameters = new List<SqlParameter>();
            foreach (var prop in RecordInfo.SPProperties)
            {
                var attr = prop.GetCustomAttribute<SPParameterAttribute>();
                attr.CheckName(prop);
                // If its a delete we only need the ID and then break out of the for
                if (attr.IsID && spType == SPType.Delete)
                {
                    parameters.Add(new SqlParameter(attr.ParameterName, attr.DataType) { Direction = ParameterDirection.Input, Value = prop.GetValue(record) });
                    break;
                }
                // skip if its a delete
                if (spType != SPType.Delete)
                {
                    // if its a create add the ID as an output foe capturing the new ID
                    if (attr.IsID && spType == SPType.Create) parameters.Add(new SqlParameter(attr.ParameterName, attr.DataType) { Direction = ParameterDirection.Output });
                    // Deal with dates
                    else if (attr.DataType == SqlDbType.SmallDateTime) parameters.Add(new SqlParameter(attr.ParameterName, attr.DataType) { Direction = ParameterDirection.Input, Value = ((DateTime)prop.GetValue(record)).ToString("dd-MMM-yyyy") });
                    // Deal with Strings in default
                    else if (attr.DataType == SqlDbType.NVarChar || attr.DataType == SqlDbType.NVarChar) parameters.Add(new SqlParameter(attr.ParameterName, attr.DataType) { Direction = ParameterDirection.Input, Value = string.IsNullOrEmpty(prop.GetValueAsString(record)) ? "" : prop.GetValueAsString(record) });
                    else parameters.Add(new SqlParameter(attr.ParameterName, attr.DataType) { Direction = ParameterDirection.Input, Value = prop.GetValue(record) });
                }
            }
            return parameters;
        }
    }
}
