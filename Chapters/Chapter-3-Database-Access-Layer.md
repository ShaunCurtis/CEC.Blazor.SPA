# Data Access Layers

Data access is all about CRUD and List operations.  Create, read, update, delete and list data from the database, along with some additions activities such as getting lookup lists for foreign key selection and unique value lists for selectors in the UI.

The framework breaks down the data access into three layers:
1. The raw database interface between the application and the underlying database.  This is implemented in Entity Framework with a set of extensions to provide generics and running Stored Procedures
2. A Data Layer to provide the bridge between the Logical Layer and the database interface.  This is defined in a DataService. 
3. A Logical Layer to provide the bridge between the Data Layer and the UI.  The Logical Layer is defined in  DataControllerServices that are specific to each record type.

Before we dive into the detail, let's look at the main methods defined on `IDataService`. Most of the methods are self explantory and give you a overview of what the data layers are all about, and then look at some of the support classes.

The important points to understand at this point are:

1. All the methods are `async` based, returning `Tasks`.
2. All the methods use generics to define the record they are operating on.
3. CUD operations return a `DbTaskResult` object that contains detail about the result.
4. There's a filtered version of get list using a `IFilterList` object defining the filters to apply to the returned dataset.
5. `GetLookupListAsync` provides a Id/Value `SortedDictionary` to use in *Select* controls in the UI.
6. `GetDistinctListAsync` builds a unique list of the field defined in `DbDinstinctRequest`.  These are used in filter list controls in the UI.  


```c#
public Task<TRecord> GetRecordAsync(int id) => Task.FromResult(new TRecord());

public Task<TRecord> GetRecordAsync(Guid guid) => Task.FromResult(new TRecord());

public Task<int> GetRecordListCountAsync() => Task.FromResult(0);

public Task<DbTaskResult> UpdateRecordAsync(TRecord record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

public Task<DbTaskResult> CreateRecordAsync(TRecord record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

public Task<DbTaskResult> DeleteRecordAsync(TRecord record) => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

public Task<List<TRecord>> GetRecordListAsync() => Task.FromResult(new List<TRecord>());

public Task<List<TRecord>> GetFilteredRecordListAsync(IFilterList filterList) => Task.FromResult(new List<TRecord>());

public Task<SortedDictionary<int, string>> GetLookupListAsync<TLookup>() where TLookup : class, IDbRecord<TLookup>, new() => Task.FromResult(new SortedDictionary<int,string>());

public Task<List<string>> GetDistinctListAsync(DbDistinctRequest req) => Task.FromResult(new List<string>());
```


Before we dive into the implementation of the layers we need to look at some support classes we need in place.

#### SPParameterAttribute

`SPParameterAttribute` is a custom attribute class used to label Properties in a Record class.
```c#
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SPParameterAttribute : Attribute
    {
        public string ParameterName = string.Empty;
        public SqlDbType DataType = SqlDbType.VarChar;
        public bool IsID = false;
        public bool IsLookUpDecription = false;

        public SPParameterAttribute() { }

        public SPParameterAttribute(string pName, SqlDbType dataType, bool isID = false)
        {
            this.ParameterName = pName;
            this.DataType = dataType;
            this.IsID = isID;
        }

        public void CheckName( PropertyInfo prop) => this.ParameterName = string.IsNullOrEmpty(this.ParameterName) ? $"@{prop.Name}" : this.ParameterName;
    }
```
  
Any property used by the CUD stored procedures needs to have a `SPParameter` attrbute set.  Lets look at two examples - shown below.

The first is for the ID field for the database record. `ISID` is set to true and the data type set to `SqlDbType.Int`.  As `ParameterName` isn't set, the the Property name is used as the Stored Procedure parameter name - in this case `@ID`.  In the second example we've set both the `DataType` and `ParameterName`.

```c#
[SPParameter(IsID = true, DataType = SqlDbType.Int)] public int ID { get; init; } = -1;

[SPParameter(DataType = SqlDbType.SmallDateTime, ParameterName ="Date")] public DateTime Date { get; init; } = DateTime.Now.Date;
```

There's important points to understand here:
1. The Create and Update Stored Procedures need to be defined with exactly the same parameter sets, even if they aren't used in both.
2. The ID field is unique - only specify one per Record.
3. The ID field is used for different purposes in the Create, Update and Delete stored procedures.  In Create it's used as an `OUT` parameter to pass the new ID back to the caller.  In Update its used to identify the record to update.  In Delete it's the only field supplied.

We'll look at how `SPParameter` is used in the section on the `DbContext` extensions.

####  DbRecordInfo

Each Record class declares a `DbRecordInfo` object.  If defines information needed by the data layer boilerplate functions to operate on a specific Record Type.  Most of the Properties are self-evident.  The SP names are used by the database layer to know which Stored Procedures to run for the Record. The Record/list Names are also used in the database layer to identify the correct `DBSet` for the record.  The Descriptions are used by the UI in titles. `SPProperties`

```c#
    public class DbRecordInfo
    {
        public string CreateSP { get; init; }
        public string UpdateSP { get; init; }
        public string DeleteSP { get; init; }

        public string RecordName { get; init; } = "Record";
        public string RecordListName { get; init; } = "Records";
        public string RecordDescription { get; init; } = "Record";
        public string RecordListDescription { get; init; } = "Records";

        public List<PropertyInfo> SPProperties { get; set; } = new List<PropertyInfo>();

    }

```  

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

#### DbDistinctRequest

When using filters on lists, the filter controls need to get distinct lists on the properties they are filtering.  `DbDistinctRequest` defines the data needed for the database layer to get that data.

`DbDistinctRequest` looks like this.

```c#
public class DbDistinctRequest
{
    public string FieldName { get; set; } // Field name in the database
    public string DistinctSetName { get; set; } // DbDistinct DbSet name in the DbContext - should be DistinctList
    public string QuerySetName { get; set; } // Record/model name e.g. WeatherReport
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

The other read operations use the same principles to retrieve one or a list of records from the `DbSet`. 



Let's start by looking at a Record.

```c#
public class DbWeatherForecast : IDbRecord<DbWeatherForecast>
{
[NotMapped] public Guid GUID => Guid.NewGuid();

[NotMapped] public int WeatherForecastID { get => this.ID; }

[SPParameter(IsID = true, DataType = SqlDbType.Int)] public int ID { get; init; } = -1;

[SPParameter(DataType = SqlDbType.SmallDateTime)] public DateTime Date { get; init; } = DateTime.Now.Date;

[SPParameter(DataType = SqlDbType.Decimal)] public decimal TemperatureC { get; init; } = 20;

[SPParameter(DataType = SqlDbType.VarChar)] public string PostCode { get; init; } = string.Empty;

[SPParameter(DataType = SqlDbType.Bit)] public bool Frost { get; init; }

[SPParameter(DataType = SqlDbType.Int)] public int SummaryValue { get; init; }

[SPParameter(DataType = SqlDbType.Int)] public int OutlookValue { get; init; }

[SPParameter(DataType = SqlDbType.VarChar)] public string Description { get; init; } = string.Empty;

[SPParameter(DataType = SqlDbType.VarChar)] public string Detail { get; init; } = string.Empty;

public string DisplayName { get; init; }

[NotMapped] public decimal TemperatureF => decimal.Round(32 + (TemperatureC / 0.5556M), 2);

[NotMapped] public WeatherSummary Summary => (WeatherSummary)this.SummaryValue;

[NotMapped] public WeatherOutlook Outlook => (WeatherOutlook)this.OutlookValue;

[NotMapped] public DbRecordInfo RecordInfo => DbWeatherForecast.RecInfo;

public DbWeatherForecast GetFromProperties(RecordCollection recordvalues) => DbWeatherForecast.FromProperties(recordvalues);

public static DbRecordInfo RecInfo => new DbRecordInfo()
{
    CreateSP = "sp_Create_WeatherForecast",
    UpdateSP = "sp_Update_WeatherForecast",
    DeleteSP = "sp_Delete_WeatherForecast",
    RecordDescription = "Weather Forecast",
    RecordName = "WeatherForecast",
    RecordListDescription = "Weather Forecasts",
    RecordListName = "WeatherForecasts"
};

[NotMapped] public RecordCollection AsProperties() =>
    new RecordCollection()
    {
        { DataDictionary.__WeatherForecastID, this.ID },
        { DataDictionary.__WeatherForecastDate, this.Date },
        { DataDictionary.__WeatherForecastTemperatureC, this.TemperatureC },
        { DataDictionary.__WeatherForecastTemperatureF, this.TemperatureF },
        { DataDictionary.__WeatherForecastPostCode, this.PostCode },
        { DataDictionary.__WeatherForecastFrost, this.Frost },
        { DataDictionary.__WeatherForecastSummary, this.Summary },
        { DataDictionary.__WeatherForecastSummaryValue, this.SummaryValue },
        { DataDictionary.__WeatherForecastOutlook, this.Outlook },
        { DataDictionary.__WeatherForecastOutlookValue, this.OutlookValue },
        { DataDictionary.__WeatherForecastDescription, this.Description },
        { DataDictionary.__WeatherForecastDetail, this.Detail },
        { DataDictionary.__WeatherForecastDisplayName, this.DisplayName },
};


public static DbWeatherForecast FromProperties(RecordCollection recordvalues) =>
    new DbWeatherForecast()
    {
        ID = recordvalues.GetEditValue<int>(DataDictionary.__WeatherForecastID),
        Date = recordvalues.GetEditValue<DateTime>(DataDictionary.__WeatherForecastDate),
        TemperatureC = recordvalues.GetEditValue<decimal>(DataDictionary.__WeatherForecastTemperatureC),
        PostCode = recordvalues.GetEditValue<string>(DataDictionary.__WeatherForecastPostCode),
        Frost = recordvalues.GetEditValue<bool>(DataDictionary.__WeatherForecastFrost),
        Summary = recordvalues.GetEditValue<WeatherSummary>(DataDictionary.__WeatherForecastSummary),
        SummaryValue = recordvalues.GetEditValue<int>(DataDictionary.__WeatherForecastSummaryValue),
        Outlook = recordvalues.GetEditValue<WeatherOutlook>(DataDictionary.__WeatherForecastOutlook),
        OutlookValue = recordvalues.GetEditValue<int>(DataDictionary.__WeatherForecastOutlookValue),
        Description = recordvalues.GetEditValue<string>(DataDictionary.__WeatherForecastDescription),
        Detail = recordvalues.GetEditValue<string>(DataDictionary.__WeatherForecastDetail),
        DisplayName = recordvalues.GetEditValue<string>(DataDictionary.__WeatherForecastDisplayName),

    };
```
