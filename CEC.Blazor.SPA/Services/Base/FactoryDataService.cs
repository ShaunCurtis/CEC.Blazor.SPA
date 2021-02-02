/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

using Microsoft.Extensions.Configuration;
using CEC.Blazor.Data;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using System;

namespace CEC.Blazor.Services
{
    public abstract class FactoryDataService<TContext>: IFactoryDataService<TContext>
        where TContext : DbContext
    {
        /// <summary>
        /// Guid for Service
        /// </summary>
        public Guid ServiceID { get; } = Guid.NewGuid();

        /// <summary>
        /// Access to the HttpClient
        /// </summary>
        public HttpClient HttpClient { get; set; } = null;

        public virtual IDbContextFactory<TContext> DBContext { get; set; } = null;

        /// <summary>
        /// Access to the Application Configuration data
        /// </summary>
        public IConfiguration AppConfiguration { get; set; }

        public FactoryDataService(IConfiguration configuration) => this.AppConfiguration = configuration;

        /// <summary>
        /// Gets the Record Name from TRecord
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <returns></returns>
        protected string GetRecordName<TRecord>() where TRecord : class, IDbRecord<TRecord>, new()
        {
            var rec = new TRecord();
            return rec.RecordInfo.RecordName ?? string.Empty;
        }

        /// <summary>
        /// Gets the Record Name from TRecord
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        protected bool TryGetRecordName<TRecord>(out string name) where TRecord : class, IDbRecord<TRecord>, new()
        {
            var rec = new TRecord();
            name = rec.RecordInfo.RecordName ?? string.Empty;
            return !string.IsNullOrWhiteSpace(name);

        }

    }
}
