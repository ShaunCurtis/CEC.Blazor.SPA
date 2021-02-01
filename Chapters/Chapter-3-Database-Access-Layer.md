# Data Access Layers

Data access is all about CRUD - create, read, update, delete and list operations on the database, along with some related activities such as getting lookup lists for foreign key selection and unique value lists for selectors in the UI.

The framework breaks data access into three layers:
1. The Database layer - the connection bewteen the application and it's underlying database.  This is implemented using Entity Framework complemented with a set of extensions for generics and Stored Procedures.
2. The Data Layer - the bridge between the Logical/Controller Layer and the database layer.  This is implementated as a FactoryDataService. 
3. A Logical Layer to bridge between the Data Layer and the UI.  it's implemented as a set of DataControllerServices specific to each record type.

Before we dive into the detail, let's look at the main methods we need to implement:

1. *GetRecordList* - get a List of all the records in the dataset.
2. *GetFilteredList* - get a list of all the records in the dataset that comply with the filters.
3. *GetRecord* - get a single record by ID or GUID
4. *CreateRecord* - Create a new record
5. *UpdateRecord* - Update the record based on ID
6. *DeleteRecord* - Delete the record based on ID
7. *GetLookupList* - get a dictionary of ID and Description for the dataset. To be used in Select controls.
8. *GetDistinctList* - get a list of unique values for a field in a dataset. To be used in filter Select controls.

Before we dive into the implementation of the layers we need to look at some support classes we need in place.

#### DbTaskResult

Data layer CUD operations return a `DbTaskResult` object.  Most of the properties are self-evident.  The UI can use the information returned to display messages based on the result.  `NewID` returns the new ID from a Create operation.

```c#
public class DbTaskResult
{
    public string Message { get; set; } = "New Object Message";
    public MessageType Type { get; set; } = MessageType.None;
    public bool IsOK { get; set; } = true;
    public int NewID { get; set; } = 0;
}
```

### The DbContext

`WeatherForecastDbContext` inherits from `DbContext`.  It has:

1. A `DbSet` for each record.
2. A `DbSet` called `DistinctList` to hold the distinct values for any distinct query.
3. Each `DbSet` connected to the relevant view in the database in `OnModelCreating`. 

```c#
public class WeatherForecastDbContext : DbContext
{
    private readonly Guid _id;

    public WeatherForecastDbContext(DbContextOptions<WeatherForecastDbContext> options)
        : base(options)
        => _id = Guid.NewGuid();

    public DbSet<DbDistinct> DistinctList { get; set; }

    public DbSet<DbWeatherForecast> WeatherForecast { get; set; }

    public DbSet<DbWeatherStation> WeatherStation { get; set; }

    public DbSet<DbWeatherReport> WeatherReport { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<DbWeatherForecast>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("vw_WeatherForecast");
            });
        modelBuilder
            .Entity<DbWeatherStation>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("vw_WeatherStation");
            });
        modelBuilder
            .Entity<DbWeatherReport>(eb =>
            {
                eb.HasNoKey();
                eb.ToView("vw_WeatherReport");
            });
    }
}
```

It looks pretty bare because most of the real database activity takes place in a set of extensions.  Out-of-the-box `DbContext` doesn't provide the Stored Procedure and generics support we require, so we add a set of extensions to do what we want.  These are defined in the static class `DbContextExtensions`.  We'll look at these individually.

`GetDbSet` is a utility function to get the correct `DbSet` for the record type defined in `TRecord`.  This is where the `DbRecordInfo` object defined in each record type definition class comes in.  The `DbSet` name can be provided directly provided as `dbSetName` when we call the method, or (normally the case), the method creates an instance of `TRecord` and then gets the `RecordInfo` object and from that the `DbSet` name.  The method uses reflection to get the correct reference to the `DbSet`, casts it and returns the casted reference.  To be clear, we return a reference to the correct `DbSet` in `WeatherForecastDbContext`.  If `TRecord` is `DbWeatherForecast`, the `RecordName` for `RecordInfo` for that record returns *WeatherForecast*, `GetDbSet` will return a reference to `DbWeatherForecast.WeatherForecast`.  We can then run Linq queries directly against that `DbSet`.  `DbContext` will translate those Linq queries into SQL queries against the database View.

```c#
private static DbSet<TRecord> GetDbSet<TRecord>(this DbContext context, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>, new()
{
    // Get the property info object for the DbSet 
    var rec = new TRecord();
    var pinfo = context.GetType().GetProperty(dbSetName ?? rec.RecordInfo.RecordName);
    // Get the property DbSet
    return (DbSet<TRecord>)pinfo.GetValue(context);
}
```

`GetRecordAsync` uses `GetDbSet` to get the `DbSet`, queries the `DbSet` for the specific record and returns it.

```c#
public async static Task<TRecord> GetRecordAsync<TRecord>(this DbContext context, int id, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>, new()
{
    var dbset = GetDbSet<TRecord>(context, dbSetName);
    return await dbset.FirstOrDefaultAsync(item => ((IDbRecord<TRecord>)item).ID == id);
}
```

`GetRecordListAsync` gets a `<List<TRecord>>` from the `DbSet` for `TRecord`.

```c#
public async static Task<List<TRecord>> GetRecordListAsync<TRecord>(this DbContext context, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>, new()
{
    var dbset = GetDbSet<TRecord>(context, dbSetName);
    return await dbset.ToListAsync() ?? new List<TRecord>();
}
```

`GetRecordFilteredListAsync` gets a `<List<TRecord>>` based on the filters set in `filterList`.  It cycles through the filters applying the filters sequentially.  The method runs the filter against the `DbSet` until it gets a result, then runs the rest of the filters against `list`.

```c#
public async static Task<List<TRecord>> GetRecordFilteredListAsync<TRecord>(this DbContext context, IFilterList filterList, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>, new()
{
    // Get the DbSet
    var dbset = GetDbSet<TRecord>(context, null);
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
            // We have records so need to filter on the list
            if (list.Count > 0)
                list = list.Where(item => x.GetValue(item).Equals(filter.Value)).ToList();
            // We don't have any records so can query the DbSet directly
            else
                list = await dbset.Where(item => x.GetValue(item).Equals(filter.Value)).ToListAsync();
        }
    }
    //  No list, just get the full recordset if allowed by filterlist
    else if (!filterList.OnlyLoadIfFilters)
        list = await dbset.ToListAsync();
    // otherwise return an empty list
    return list;
}
```


`GetLookupListAsync` gets a `SortedDictionary` for the UI to use in Selects.  It gets the `DbSet` for `TRecord` and builds the `SortedDictionary` from the `ID` and `DisplayName` fields.  `ID` and `DisplayName` are guaranteed because `TRecord` implements the `IDbRecord` interface.  We'll look at that shortly.

```c#
public async static Task<SortedDictionary<int, string>> GetLookupListAsync<TRecord>(this DbContext context) where TRecord : class, IDbRecord<TRecord>, new()
{
    var list = new SortedDictionary<int, string>();
    var dbset = GetDbSet<TRecord>(context, null);
    if (dbset != null) await dbset.ForEachAsync(item => list.Add(item.ID, item.DisplayName));
    return list;
}
```

`GetDistinctListAsync` gets a `List<string>` of `fieldName` from the `DbSet` for `TRecord`.  It uses reflection to the the `PropertyInfo` for `fieldName` and then runs a `Select` on `DbSet` to get the values, converting them to a string in the process.  It finally runs a `Distinct` operation on the list.  It's done this way as running `Distinct` on the `DbSet` doesn't work.

```c#
public async static Task<List<string>> GetDistinctListAsync<TRecord>(this DbContext context, string fieldName) where TRecord : class, IDbRecord<TRecord>, new()
{
    var dbset = GetDbSet<TRecord>(context, null);
    var list = new List<string>();
    var x = typeof(TRecord).GetProperty(fieldName);
    if (dbset != null && x != null)
    {
        // we get the full list and then run a distinct because we can't run a distinct directly on the dbSet
        var fulllist = await dbset.Select(item => x.GetValue(item).ToString()).ToListAsync();
        list = fulllist.Distinct().ToList();
    }
    return list ?? new List<string>();
}
```

`ExecStoredProcAsync` is a classic Stored Procedure implementation run through a `DbCommand` using Entity Framework's underlying database connection.

```c#
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
        catch (Exception e)
        {
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
```

## Data Service

The Data Service is implemented as a Factory, using generics and reflection to handle requests based on record type.

the public interface for the dataservice is defined in `IFactoryDataService` and the boilerplate code in the abstract class `FactoryDataService`.

### IFactoryDataService


Important points to understand at this point are:

1. All methods are `async`, returning `Tasks`.
2. All methods use generics. `TRecord` defines the record type.
3. CUD operations return a `DbTaskResult` object that contains detail about the result.
4. There's a filtered version of get list using a `IFilterList` object defining the filters to apply to the returned dataset.
5. `GetLookupListAsync` provides a Id/Value `SortedDictionary` to use in *Select* controls in the UI.
6. `GetDistinctListAsync` builds a unique list of the field defined in `DbDinstinctRequest`.  These are used in filter list controls in the UI.  


```c#
public interface IFactoryDataService<TContext> 
    where TContext : DbContext
{
    public HttpClient HttpClient { get; set; }

    public IDbContextFactory<TContext> DBContext { get; set; }

    public IConfiguration AppConfiguration { get; set; }

    public Task<List<TRecord>> GetRecordListAsync<TRecord>() where TRecord : class, IDbRecord<TRecord>, new() 
        => Task.FromResult(new List<TRecord>());

    public Task<List<TRecord>> GetFilteredRecordListAsync<TRecord>(IFilterList filterList) where TRecord : class, IDbRecord<TRecord>, new() 
        => Task.FromResult(new List<TRecord>());

    public Task<TRecord> GetRecordAsync<TRecord>(int id) where TRecord : class, IDbRecord<TRecord>, new() 
        => Task.FromResult(new TRecord());

    public Task<TRecord> GetRecordAsync<TRecord>(Guid guid) where TRecord : class, IDbRecord<TRecord>, new() 
        => Task.FromResult(new TRecord());

    public Task<int> GetRecordListCountAsync<TRecord>() where TRecord : class, IDbRecord<TRecord>, new() 
        => Task.FromResult(0);

    public Task<DbTaskResult> UpdateRecordAsync<TRecord>(TRecord record) where TRecord : class, IDbRecord<TRecord>, new() 
        => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

    public Task<DbTaskResult> CreateRecordAsync<TRecord>(TRecord record) where TRecord : class, IDbRecord<TRecord>, new() 
        => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

    public Task<DbTaskResult> DeleteRecordAsync<TRecord>(TRecord record) where TRecord : class, IDbRecord<TRecord>, new() 
        => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

    public Task<SortedDictionary<int, string>> GetLookupListAsync<TLookup>() where TLookup : class, IDbRecord<TLookup>, new() 
        => Task.FromResult(new SortedDictionary<int,string>());

    public Task<List<string>> GetDistinctListAsync<TRecord>(string fieldName) where TRecord : class, IDbRecord<TRecord>, new() 
        => Task.FromResult(new List<string>());
        
    public Task<List<DbBaseRecord>> GetBaseRecordListAsync<TLookup>() where TLookup : class, IDbRecord<TLookup>, new() 
        => Task.FromResult(new List<DbBaseRecord>());

    public List<SqlParameter> GetSQLParameters<TRecord>(TRecord item, bool withid = false) where TRecord : class, new() 
        => new List<SqlParameter>();
}
```

`IFilterList` is used to define filters to be applied to a dataset.  It's self-explanatory.

```c#
public interface IFilterList
{
    public enum FilterViewState
    {
        NotSet = 0,
        Show = 1,
        Hide = 2
    }

    public bool Show => this.ShowState == 0;
    public FilterViewState ShowState { get; set; }
    public Dictionary<string, object> Filters { get; set; }
    public bool OnlyLoadIfFilters { get; set; }
    public bool Load => this.Filters.Count > 0 || !this.OnlyLoadIfFilters;

    public void Reset()
        => this.ShowState = IFilterList.FilterViewState.NotSet;

    public bool TryGetFilter(string name, out object value)
    {
        value = null;
        if (Filters.ContainsKey(name)) value = this.Filters[name];
        return value != null;
    }

    public bool SetFilter(string name, object value)
    {
        if (Filters.ContainsKey(name)) this.Filters[name] = value;
        else Filters.Add(name, value);
        return Filters.ContainsKey(name);
    }

    public bool ClearFilter(string name)
    {
        if (Filters.ContainsKey(name)) this.Filters.Remove(name);
        return !Filters.ContainsKey(name);
    }
}
```

