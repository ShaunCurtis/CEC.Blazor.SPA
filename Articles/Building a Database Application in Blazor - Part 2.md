# Part 2 - Services - Building the CRUD Data Layers

This article is the second in a series on Building Blazor Projects: it describes techniques and methodologies for abstracting the data and business logic layers into boilerplate code in a library.  It has significant changes from it's intial release.

1. [Project Structure and Framework](https://www.codeproject.com/Articles/5279560/Building-a-Database-Application-in-Blazor-Part-1-P)
2. [Services - Building the CRUD Data Layers](https://www.codeproject.com/Articles/5279596/Building-a-Database-Application-in-Blazor-Part-2-S)
3. [View Components - CRUD Edit and View Operations in the UI](https://www.codeproject.com/Articles/5279963/Building-a-Database-Application-in-Blazor-Part-3-C)
4. [UI Components - Building HTML/CSS Controls](https://www.codeproject.com/Articles/5280090/Building-a-Database-Application-in-Blazor-Part-4-U)
5. [View Components - CRUD List Operations in the UI](https://www.codeproject.com/Articles/5280391/Building-a-Database-Application-in-Blazor-Part-5-V)
6. [A walk through detailing how to add weather stations and weather station data to the application](https://www.codeproject.com/Articles/5281000/Building-a-Database-Application-in-Blazor-Part-6-A)

## Repository and Database

The repository for the articles has moved to [CEC.Blazor.SPA Repository](https://github.com/ShaunCurtis/CEC.Blazor.SPA).  [CEC.Blazor GitHub Repository](https://github.com/ShaunCurtis/CEC.Blazor) is obselete and will be removed.

There's a SQL script in /SQL in the repository for building the database.

[You can see the Server and WASM versions of the project running here on the same site](https://cec-blazor-server.azurewebsites.net/).

## Services

Blazor is built on DI [Dependency Injection] and IOC [Inversion of Control].  If your not familiar with these concepts, do a little [backgound reading](https://www.codeproject.com/Articles/5274732/Dependency-Injection-and-IoC-Containers-in-Csharp) before diving into Blazor.  You'll save yourself time in the long run!

Blazor Singleton and Transient services are relatively straight forward.  You can read more about them in the [Microsoft Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/fundamentals/dependency-injection).  Scoped are a little more complicated.

1. A scoped service object exists for the lifetime of a client application session - note client and not server.  Any application resets, such as F5 or navigation away from the application, resets all scoped services.  A duplicated tab in a browser creates a new application, and a new set of scoped services.
2. A scoped service can be scoped to an object in code.  This is most common in a UI conponent.  The `OwningComponentBase` component class has functionality to restrict the life of a scoped service to the lifetime of the component. This is covered in more detail in another article. 

Services is the Blazor IOC [Inversion of Control] container.

In Server mode services are configured in `startup.cs`:

```c#
// CEC.Blazor.Server/startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddRazorPages();
    services.AddServerSideBlazor();
    // the Services for the CEC.Blazor .
    services.AddCECBlazorSPA();
    // the local application Services defined in ServiceCollectionExtensions.cs
    services.AddApplicationServices(Configurtion);
}
```

```c#
// CEC.Blazor.Server/Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
{
    // Singleton service for the Server Side version of WeatherForecast Data Service 
    // Dummy service produces a new recordset each time the application runs 
    services.AddSingleton<IFactoryDataService<WeatherForecastDbContext>, WeatherDummyDataService>();
    // services.AddSingleton<IFactoryDataService<WeatherForecastDbContext>, FactoryServerDataService<WeatherForecastDbContext>>();

    // Scoped service for the WeatherForecast Controller Service
    services.AddScoped<WeatherForecastControllerService>();
    services.AddScoped<WeatherStationControllerService>();
    services.AddScoped<WeatherReportControllerService>();
    // Factory for building the DBContext 
    var dbContext = configuration.GetValue<string>("Configuration:DBContext");
    services.AddDbContextFactory<WeatherForecastDbContext>(options => options.UseSqlServer(dbContext), ServiceLifetime.Singleton);
    return services;
}
```

 and `program.cs` in WASM mode:

```c#
// CEC.Blazor.WASM.Client/program.cs
public static async Task Main(string[] args)
{
    .....
    // Added here as we don't have access to buildler in AddApplicationServices
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
    // the Services for the CEC.Blazor Library
    builder.Services.AddCECBlazorSPA();
    // the local application Services defined in ServiceCollectionExtensions.cs
    builder.Services.AddApplicationServices();
    .....
}
```

```c#
// CEC.Blazor.WASM.Client/Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
    // Scoped service for the WASM Client version of WASM Factory Data Service 
    services.AddScoped<IFactoryDataService<WeatherForecastDbContext>, FactoryWASMDataService<WeatherForecastDbContext>>();
    // Scoped service for the WeatherForecast Controller Service
    services.AddScoped<WeatherForecastControllerService>();
    services.AddScoped<WeatherStationControllerService>();
    services.AddScoped<WeatherReportControllerService>();
    return services;
}
```
Points:
1. There's an `IServiceCollection` extension method for each project/library to encapsulate the specific services needed for the project.
2. Only the data layer service is different.  The Server version, used by both the Blazor Server and the WASM API Server, interfaces with the database and Entitiy Framework.  It's scoped as a Singleton - as we are running async, DbContexts are created and closed per query.  The Client version uses `HttpClient` (which is a scoped service) to make calls to the API and is therefore itself scoped.  There's also a dummy data service to emulate the database.
3. A code factory is used to build the specific DBContext, and provide the necessary level of abstraction for boilerplating the core data service code in the base library.
4. A code "factory" `FactoryDataService` implmenting `IFactoryDataService` is used to process all the data requests with the generic `TRecord` defining which dataset is retrieved and returned.   These services, along with a set of `DbContext` extensions, boilerplate all the core data service code into the base library.


### Generics

The boilerplate library code relies heavily on Generics.  The two generic entities used are:
1. `TRecord` - this represents a model record class.  It must implement `IDbRecord`, a vanilla `new()` and be a class.  `TRecord` is defined on a Method by Method basis, not at the class level.
2. `TContext` - this is the database context and must inherit from the `DbContext` class.

Class declarations look like this:

```c#
// CEC.Blazor.SPA/Services/FactoryDataService.cs
public abstract class FactoryDataService<TContext>: IFactoryDataService<TContext>
    where TContext : DbContext
......
    // example method template  
    public virtual Task<TRecord> GetRecordAsync<TRecord>(int id) where TRecord : class, IDbRecord<TRecord>, new()
        => Task.FromResult(new TRecord());

```
### Data Access

Before we dive into the detail, let's look at the main methods we need to implement:

1. *GetRecordList* - get a List of all the records in the dataset.
2. *GetFilteredList* - get a list of all the records in the dataset that comply with the filters.
3. *GetRecord* - get a single record by ID or GUID
4. *CreateRecord* - Create a new record
5. *UpdateRecord* - Update the record based on ID
6. *DeleteRecord* - Delete the record based on ID
7. *GetLookupList* - get a dictionary of ID and Description for the dataset. To be used in Select controls.
8. *GetDistinctList* - get a list of unique values for a field in a dataset. To be used in filter Select controls.

Keep this in mind as we work through the data layers.

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
### Data Classes

I'm a firm believer in the notion that database data records should be immutable.  A record should always represent the underlying source data.  If you want to change that data, make a copy, update it and then submit the edited version back to the data source to update the original.  This framework implements that principle.

Having worked with Sharepoint there are some features that I've come to appreciate.  The field uniqueness is one.  This framework implements a `DataDictionary` object that holds information about each record field we use.

Within the Dictionary each record field is defined as a `RecordFieldInfo` object.  The class looks like this:

```c#
public class RecordFieldInfo
{
    public string FieldName { get; init; }
    public Guid FieldGUID { get; init; }
    public string DisplayName { get; set; }

    // Various setter methods
    public RecordFieldInfo(string name)
    {
        FieldName = name;
        DisplayName = name.AsSeparatedString();
        this.FieldGUID = Guid.NewGuid(); ;
    }
}
```

The `DataDictionary` is implemented in project library as a static class with static members.  Each unique field is declared as a `RecordFieldInfo object.  An abbreviated version of the Weather library data dictionary is shown below.

```c#
    public static class DataDictionary
    {
        // Weather Forecast Fields
        public static readonly RecordFieldInfo __WeatherForecastID = new RecordFieldInfo("WeatherForecastID");
        public static readonly RecordFieldInfo __WeatherForecastDate = new RecordFieldInfo("WeatherForecastDate");
        public static readonly RecordFieldInfo __WeatherForecastTemperatureC = new RecordFieldInfo("WeatherForecastTemperatureC");
        ......

        // Weather Station Fields
        public static readonly RecordFieldInfo __WeatherStationID = new RecordFieldInfo("WeatherStationID");
        public static readonly RecordFieldInfo __WeatherStationName = new RecordFieldInfo("WeatherStationName");
        public static readonly RecordFieldInfo __WeatherStationLatitude = new RecordFieldInfo("WeatherStationLatitude");
        public static readonly RecordFieldInfo __WeatherStationLongitude = new RecordFieldInfo("WeatherStationLongitude");
        public static readonly RecordFieldInfo __WeatherStationElevation = new RecordFieldInfo("WeatherStationElevation");
    }
```
We'll see them in use later.


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

You will see how `SPParameter` is used in the Data Service section.

####  DbRecordInfo

Each Record class declares a `DbRecordInfo` object.  If defines information needed by the data layer boilerplate functions to operate on a specific Record Type.  Most of the Properties are self-evident.  The SP names are used by the database layer to know which Stored Procedures to run for the Record. The Record/list Names are also used in the database layer to identify the correct `DBSet` for the record.  The Description properties are used by the UI in titles.

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

#### IDbRecord

`IDbRecord` defines the common public interface for all database derived records.  

`GetSPParameters` gets a `List<PropertyInfo>` for all the Properties in a Record that implement `SPParameterAttribute`.  It's used by the `FactoryDataService` to build the Stored Procedure parameters list for the specific record.

We'll look in more detail at most of the Properties and Methods when we look at a typical record.    

```c#
public interface IDbRecord<TRecord> 
    where TRecord : class, IDbRecord<TRecord>, new()
{
    public int ID { get; }

    public Guid GUID { get; }

    public string DisplayName { get; }

    public DbRecordInfo RecordInfo { get; }

    public RecordCollection AsProperties();

    public static TRecord FromProperties(RecordCollection props) => default;

    public TRecord GetFromProperties(RecordCollection props);

    public List<PropertyInfo> GetSPParameters()
    {
        var attrprops = new List<PropertyInfo>();
        foreach (var prop in typeof(TRecord).GetProperties())
        {
            if (HasSPParameterAttribute(prop.Name)) attrprops.Add(prop);
        }
        return attrprops;
    }

    private static bool HasSPParameterAttribute(string propertyName)
    {
        var prop = typeof(TRecord).GetProperty(propertyName);
        var attrs = prop.GetCustomAttributes(true);
        var attribute = (SPParameterAttribute)attrs.FirstOrDefault(item => item.GetType() == typeof(SPParameterAttribute));
        return attribute != null;
    }
}
```
#### RecordValue

a `RecordValue` represents an editable version of a database record field.  We'll look at how its populated and used shortly.

Note:
1. There's `Value` and `EditedValue` properties to keep track of the edit state of the field.
2. There's a `IsDirty` property to check the edit state of the field.

```c#
public class RecordValue
{
    public string Field { get; }
    public Guid GUID { get; }
    public object Value { get; }
    public object EditedValue { get; set; }

    public bool IsDirty
    {
        get
        {
            if (Value != null && EditedValue != null) return !Value.Equals(EditedValue);
            if (Value is null && EditedValue is null) return false;
            return true;
        }
    }
    
    // setter methods

    public void Reset()
        => this.EditedValue = this.Value;
}
```

#### RecordCollection

A `RecordCollection` is a collection of `RecordValue` objects used to store record fields.  It implements `ICollection`.  It provides controlled access to the underlying list and methods to get and set `RecordValue` objects.  As `RecordValue.Value` is an object, generics are used to get values from a `RecordValue` object.

```c#
public class RecordCollection : ICollection
{
    private List<RecordValue> _items = new List<RecordValue>() ;
    public int Count => _items.Count;
    public bool IsSynchronized => false;
    public object SyncRoot => this;
    public Action<bool> FieldValueChanged;
    public bool IsDirty => _items.Any(item => item.IsDirty);

    public void ResetValues()
        => _items.ForEach(item => item.Reset());

    public void Clear() => _items.Clear();
    // ICollection impelemtations

    public IEnumerator GetEnumerator()
        => new RecordCollectionEnumerator(_items);

    public bool SetField(RecordFieldInfo field, object value) => this.SetField(field.FieldName, value);

    public bool SetField(string FieldName, object value )
    {
        var x = _items.FirstOrDefault(item => item.Field.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));
        if (x != null && x != default)
        {
            x.EditedValue = value;
            this.FieldValueChanged?.Invoke(this.IsDirty);
        }
        else _items.Add(new RecordValue(FieldName, value));
        return true;
    }
        
    // a whole host of methods for getting and setting, adding deleting Record Values 
    public T GetEditValue<T>(string FieldName)
    {
        var x = _items.FirstOrDefault(item => item.Field.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));
        if (x != null && x.EditedValue is T t) return t;
        return default;
    }
}
```

The `IEnumerator` for the collection.

```c#
public class RecordCollectionEnumerator : IEnumerator
{
    private List<RecordValue> _items = new List<RecordValue>();
    private int _cursor;

    object IEnumerator.Current
    {
        get
        {
            if ((_cursor < 0) || (_cursor == _items.Count))
                throw new InvalidOperationException();
            return _items[_cursor];
        }
    }
    public RecordCollectionEnumerator(List<RecordValue> items)
    {
        this._items = items;
        _cursor = -1;
    }
    void IEnumerator.Reset()
        => _cursor = -1;

    bool IEnumerator.MoveNext()
    {
        if (_cursor < _items.Count)
            _cursor++;

        return (!(_cursor == _items.Count));
    }
}
```

Let's start by looking at a Record.

#### DbWeatherForecast

First the data properties.  Note:
1. `SPParameter` attributes for all the properties used in stored procedure CUD operations.
2. `[NotMapped]` attributes for all the properties not returned in `DbSet` operations.
3. `init` setter used for all `DBSet` properties to make them immutable.
4. `GUID` set to a new Guid as the `DbSet` doesn't define one.
5. a static `DbRecordInfo` defining the information about the record.  It's common to all `DbWeatherForecast` objects so declared `static`.
6. `RecordInfo` linking to the undelying static definition of `RecInfo`.

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

```
`IDbRecord` defines two methods that build and read a RecordCollection for the record.
1. You can now see the `DataDictionary` in action
2. Generic methods used to retrieve values from the `RecordColection`.
3. `FromProperties` is declared static because it doesn't operate on the current record but creates a new record from the edited values in the `RecordCollection`
4. `GetFromProperties` only exists because we're using generics and you can't access `static` methods directly like this `TRecord.FromProperties().  You need to create an instance of `TRecord` so `new TRecord().GetFromProperties()` works.

```c#
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


public DbWeatherForecast GetFromProperties(RecordCollection recordvalues) => DbWeatherForecast.FromProperties(recordvalues);

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

At this point you wil probably be wondering why are we creating the `RecordCollection`.  There's an awful lot of code here which at present appears to do nothing.  There is a purpose, but you will need to wait until we address record editing in Part 3 to see how it's used.

### The Entity Framework Tier

The solution uses a combination of Entity Framework [EF] and normal database access. Being old school (the application gets nowhere near the data tables). I implement CUD [CRUD without the Read] through stored procedures, and R [Read access] and List through views.  The data tier has two layers - the EF Database Context and a Data Service.

In a production application the database account used by Entity Framework should be configured with only select on Views and execute on Stored Procedures.

The demo application can be run with or without a full database connection - there's a "Dummy database" server Data Service.

All EF code is implemented in `CEC.Weather`, the shared project specific library.

#### WeatherForecastDBContext

The `DbContext` has a `DbSet` per record type.  Each `DbSet` is linked to a view in `OnModelCreating()`.  The WeatherForecast application has one record type.

The class looks like this:
```c#
public class WeatherForecastDbContext : DbContext
{
    public WeatherForecastDbContext(DbContextOptions<WeatherForecastDbContext> options)
        : base(options) {}

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
It looks pretty bare because most of the real database activity takes place in a set of extensions.  Out-of-the-box `DbContext` doesn't provide the Stored Procedure and generics support we require, so we add that functionality through a set of extensions on `DbContext`.  These are defined in the static class `DbContextExtensions`.  We'll look at the methods individually.

`GetDbSet` is a utility function to get the correct `DbSet` for the record type defined in `TRecord`.  `TRecord` must implement `IDbRecord` (we'll look at it shortly).  `IDbRecord` has a `DbRecordInfo` property `RecordInfo` which contains the information we need to get the correct `DbSet`.  The `DbSet` name can be provided directly provided as `dbSetName` when we call the method, or (normally the case), the method creates an instance of `TRecord` and gets `RecordInfo` to get the `DbSet` name.  The method uses reflection to get the correct reference to the `DbSet`, casts it and returns the casted reference.  To be clear, we return a reference to the correct `DbSet` in `WeatherForecastDbContext`.  If `TRecord` is `DbWeatherForecast`, the `RecordName` defined in `RecordInfo` is *WeatherForecast*, `GetDbSet` returns a reference to `DbWeatherForecast.WeatherForecast`.  With the correct `DbSet` we can run Linq queries against that `DbSet`.  `DbContext` will translate those Linq queries into SQL queries against the database View.

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

`GetRecordFilteredListAsync` gets a `<List<TRecord>>` based on the filters set in `filterList`.  It cycles through the filters applying them sequentially.  The method runs the filter against the `DbSet` until it gets a result, then runs the rest of the filters against `list`.

```c#
public async static Task<List<TRecord>> GetRecordFilteredListAsync<TRecord>(this DbContext context, FilterListCollection filterList, string dbSetName = null) where TRecord : class, IDbRecord<TRecord>, new()
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


`GetLookupListAsync` gets a `SortedDictionary` for the UI to use in Selects.  It gets the `DbSet` for `TRecord` and builds a `SortedDictionary` from the `ID` and `DisplayName` fields.  `ID` and `DisplayName` are defined in the `IDbRecord` interface.

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

`ExecStoredProcAsync` is a classic Stored Procedure implementation run through a `DbCommand` obtained from  Entity Framework's underlying database connection.

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

### The Data Service Tier

#### IFactoryDataService

Core Data Service functionality is defined in the `IFactoryDataService` interface.
Important points to understand at this point are:

1. All methods are `async`, returning `Tasks`.
2. All methods use generics. `TRecord` defines the record type.
3. CUD operations return a `DbTaskResult` object that contains detail about the result.
4. There's a filtered version of get list using a `IFilterList` object defining the filters to apply to the returned dataset.
5. `GetLookupListAsync` provides a Id/Value `SortedDictionary` to use in *Select* controls in the UI.
6. `GetDistinctListAsync` builds a unique list of the field defined in `DbDinstinctRequest`.  These are used in filter list controls in the UI.  


```c#
// CEC.Blazor.SPA/Services/Interfaces
public interface IFactoryDataService<TContext> 
    where TContext : DbContext
{
    public HttpClient HttpClient { get; set; }

    public IDbContextFactory<TContext> DBContext { get; set; }

    public IConfiguration AppConfiguration { get; set; }

    public Task<List<TRecord>> GetRecordListAsync<TRecord>() where TRecord : class, IDbRecord<TRecord>, new();

    public Task<List<TRecord>> GetFilteredRecordListAsync<TRecord>(IFilterList filterList) where TRecord : class, IDbRecord<TRecord>, new(); 

    public Task<TRecord> GetRecordAsync<TRecord>(int id) where TRecord : class, IDbRecord<TRecord>, new() ;

    public Task<TRecord> GetRecordAsync<TRecord>(Guid guid) where TRecord : class, IDbRecord<TRecord>, new(); 

    public Task<int> GetRecordListCountAsync<TRecord>() where TRecord : class, IDbRecord<TRecord>, new() ;

    public Task<DbTaskResult> UpdateRecordAsync<TRecord>(TRecord record) where TRecord : class, IDbRecord<TRecord>, new() ;

    public Task<DbTaskResult> CreateRecordAsync<TRecord>(TRecord record) where TRecord : class, IDbRecord<TRecord>, new() ;

    public Task<DbTaskResult> DeleteRecordAsync<TRecord>(TRecord record) where TRecord : class, IDbRecord<TRecord>, new() ;

    public Task<SortedDictionary<int, string>> GetLookupListAsync<TLookup>() where TLookup : class, IDbRecord<TLookup>, new() ;

    public Task<List<string>> GetDistinctListAsync<TRecord>(string fieldName) where TRecord : class, IDbRecord<TRecord>, new() ;
        
    public Task<List<DbBaseRecord>> GetBaseRecordListAsync<TLookup>() where TLookup : class, IDbRecord<TLookup>, new() ;

    public List<SqlParameter> GetSQLParameters<TRecord>(TRecord item, bool withid = false) where TRecord : class, new() ;
}
```
#### FactoryDataService

`FactoryDataService` is a base abstract implementation of the Interface

```c#
// CEC.Blazor.SPA/Services/Base
    public abstract class FactoryDataService<TContext>: IFactoryDataService<TContext>
        where TContext : DbContext
    {
        public Guid ServiceID { get; } = Guid.NewGuid();

        public HttpClient HttpClient { get; set; } = null;

        public virtual IDbContextFactory<TContext> DBContext { get; set; } = null;

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

        protected bool TryGetRecordName<TRecord>(out string name) where TRecord : class, IDbRecord<TRecord>, new()
        {
            var rec = new TRecord();
            name = rec.RecordInfo.RecordName ?? string.Empty;
            return !string.IsNullOrWhiteSpace(name);
        }

        public virtual Task<List<TRecord>> GetRecordListAsync<TRecord>() where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(new List<TRecord>());

        public virtual Task<List<TRecord>> GetFilteredRecordListAsync<TRecord>(FilterListCollection filterList) where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(new List<TRecord>());

        public virtual Task<TRecord> GetRecordAsync<TRecord>(int id) where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(new TRecord());

        public virtual Task<TRecord> GetRecordAsync<TRecord>(Guid guid) where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(new TRecord());

        public virtual Task<int> GetRecordListCountAsync<TRecord>() where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(0);

        public virtual Task<DbTaskResult> UpdateRecordAsync<TRecord>(TRecord record) where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        public virtual Task<DbTaskResult> CreateRecordAsync<TRecord>(TRecord record) where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        public virtual Task<DbTaskResult> DeleteRecordAsync<TRecord>(TRecord record) where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(new DbTaskResult() { IsOK = false, Type = MessageType.NotImplemented, Message = "Method not implemented" });

        public virtual Task<SortedDictionary<int, string>> GetLookupListAsync<TLookup>() where TLookup : class, IDbRecord<TLookup>, new()
            => Task.FromResult(new SortedDictionary<int, string>());

        public virtual Task<SortedDictionary<int, string>> GetLookupListAsync(string recordName)
            => Task.FromResult(new SortedDictionary<int, string>());

        public virtual Task<List<string>> GetDistinctListAsync<TRecord>(string fieldName) where TRecord : class, IDbRecord<TRecord>, new()
            => Task.FromResult(new List<string>());

        public virtual List<SqlParameter> GetSQLParameters<TRecord>(TRecord item, bool withid = false) where TRecord : class, new()
            => new List<SqlParameter>();
    }
```
#### FactoryServerDataService

`FactoryServerDataService` is a concrete implementation of `IFactoryDataService`.  Methods call back into the `DBContext` extension methods.  

`RunStoredProcedure`:

1. Uses the `RecordInfo` property of an instance of `TRecord` to get the correct stored procedure
2. Builds the SQLParameters through `GetSQLParameters` which reads the `SPParameters` attribute labelled properties of `TRecord`.

```c#
// CEC.Blazor.SPA/Services/Base
    public class FactoryServerDataService<TContext> :
        FactoryDataService<TContext>,
        IFactoryDataService<TContext>
        where TContext : DbContext
    {

        public FactoryServerDataService(IConfiguration configuration, IDbContextFactory<TContext> dbContext) : base(configuration)
            => this.DBContext = dbContext;

        public override async Task<List<TRecord>> GetRecordListAsync<TRecord>()
            => await this.DBContext.CreateDbContext().GetRecordListAsync<TRecord>();

        public override async Task<List<TRecord>> GetFilteredRecordListAsync<TRecord>(FilterListCollection filterList)
            => await this.DBContext.CreateDbContext().GetRecordFilteredListAsync<TRecord>(filterList);

        public override async Task<TRecord> GetRecordAsync<TRecord>(int id)
            => await this.DBContext.CreateDbContext().GetRecordAsync<TRecord>(id);

        public override async Task<TRecord> GetRecordAsync<TRecord>(Guid guid)
            => await this.DBContext.CreateDbContext().GetRecordAsync<TRecord>(guid);

        public override async Task<int> GetRecordListCountAsync<TRecord>()
            => await this.DBContext.CreateDbContext().GetRecordListCountAsync<TRecord>();

        public override async Task<DbTaskResult> UpdateRecordAsync<TRecord>(TRecord record)
            => await this.RunStoredProcedure<TRecord>(record, SPType.Update);

        public override async Task<DbTaskResult> CreateRecordAsync<TRecord>(TRecord record)
            => await this.RunStoredProcedure<TRecord>(record, SPType.Create);

        public override async Task<DbTaskResult> DeleteRecordAsync<TRecord>(TRecord record)
            => await this.RunStoredProcedure<TRecord>(record, SPType.Delete);

        public override async Task<List<string>> GetDistinctListAsync<TRecord>(string fieldName)
            => await this.DBContext.CreateDbContext().GetDistinctListAsync<TRecord>(fieldName);

        public override async Task<SortedDictionary<int, string>> GetLookupListAsync<TLookup>()
            => await this.DBContext.CreateDbContext().GetLookupListAsync<TLookup>() ?? new SortedDictionary<int, string>();

        protected async Task<DbTaskResult> RunStoredProcedure<TRecord>(TRecord record, SPType spType) where TRecord : class, IDbRecord<TRecord>, new()
        {
            var recordInfo = new TRecord().RecordInfo;
            var ret = new DbTaskResult()
            {
                Message = $"Error saving {recordInfo.RecordDescription}",
                IsOK = false,
                Type = MessageType.Danger
            };

            var spname = spType switch
            {
                SPType.Create => recordInfo.CreateSP,
                SPType.Update => recordInfo.UpdateSP,
                SPType.Delete => recordInfo.DeleteSP,
                _ => string.Empty
            };
            var parms = this.GetSQLParameters(record, spType);
            if (await this.DBContext.CreateDbContext().ExecStoredProcAsync(spname, parms))
            {
                var idparam = parms.FirstOrDefault(item => item.Direction == ParameterDirection.Output && item.SqlDbType == SqlDbType.Int && item.ParameterName.Contains("ID"));
                ret = new DbTaskResult()
                {
                    Message = $"{recordInfo.RecordDescription} saved",
                    IsOK = true,
                    Type = MessageType.Success
                };
                if (idparam != null) ret.NewID = Convert.ToInt32(idparam.Value);
            }
            return ret;
        }

        protected virtual List<SqlParameter> GetSQLParameters<TRecord>(TRecord record, SPType spType) where TRecord : class, IDbRecord<TRecord>, new()
        {
            var parameters = new List<SqlParameter>();
            foreach (var prop in (record as IDbRecord<TRecord>).GetSPParameters())
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
                    // Deal with Strings in default or null
                    else if (attr.DataType == SqlDbType.NVarChar || attr.DataType == SqlDbType.VarChar) parameters.Add(new SqlParameter(attr.ParameterName, attr.DataType) { Direction = ParameterDirection.Input, Value = string.IsNullOrEmpty(prop.GetValueAsString(record)) ? "" : prop.GetValueAsString(record) });
                    else parameters.Add(new SqlParameter(attr.ParameterName, attr.DataType) { Direction = ParameterDirection.Input, Value = prop.GetValue(record) });
                }
            }
            return parameters;
        }
    }
```

#### FactoryWASMDataService

`FactoryWASMDataService` is the WASM client implementation of `IFactoryDataService`.  It makes API calls into the server controller services.

```c#
// CEC.Blazor/Services/Base
    public class FactoryWASMDataService<TContext> :
        FactoryDataService<TContext>
        where TContext : DbContext
    {

        public FactoryWASMDataService(IConfiguration configuration, HttpClient httpClient) : base(configuration) 
            => this.HttpClient = httpClient;

        public override async Task<TRecord> GetRecordAsync<TRecord>(int id)
        {
            var result = new TRecord();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync($"{recName}/read?gid={Guid.NewGuid().ToString("D")}", id);
                result = await response.Content.ReadFromJsonAsync<TRecord>();
            }
            return result ?? default;
        }

        public override async Task<SortedDictionary<int, string>> GetLookupListAsync<TRecord>()
        {
            var result = new SortedDictionary<int, string>();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                result = await this.HttpClient.GetFromJsonAsync<SortedDictionary<int, string>>($"{recName}/lookuplist?gid={Guid.NewGuid().ToString("D")}");
            }
            return result ?? default;
        }

        public override async Task<List<string>> GetDistinctListAsync<TRecord>(string fieldName)
        {
            var result = new List<string>();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync($"{recName}/distinctlist?gid={Guid.NewGuid().ToString("D")}", fieldName);
                result = await response.Content.ReadFromJsonAsync<List<string>>();
            }
            return result ?? default;
        }

        public override async Task<List<TRecord>> GetFilteredRecordListAsync<TRecord>(FilterListCollection filterList)
        {
            var result = new List<TRecord>();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync<FilterListCollection>($"{recName}/filteredlist?gid={Guid.NewGuid().ToString("D")}", filterList);
                result = await response.Content.ReadFromJsonAsync<List<TRecord>>();
            }
            return result ?? default;
        }

        public override async Task<List<TRecord>> GetRecordListAsync<TRecord>()
        {
            var result = new List<TRecord>();
            if (TryGetRecordName<TRecord>(out string recName))
                result = await this.HttpClient.GetFromJsonAsync<List<TRecord>>($"{recName}/list?gid={Guid.NewGuid().ToString("D")}");
            return result;
        }

        public override async Task<int> GetRecordListCountAsync<TRecord>()
        {
            var result = 0;
            if (TryGetRecordName<TRecord>(out string recName))
                result = await this.HttpClient.GetFromJsonAsync<int>($"{recName}/count?gid={Guid.NewGuid().ToString("D")}"); 
            return result;
        }

        public override async Task<DbTaskResult> UpdateRecordAsync<TRecord>(TRecord record)
        {
            var result = new DbTaskResult();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync<TRecord>($"{recName}/update?gid={Guid.NewGuid().ToString("D")}", record);
                result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
            }
            return result ?? default;
        }

        public override async Task<DbTaskResult> CreateRecordAsync<TRecord>(TRecord record)
        {
            var result = new DbTaskResult();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync<TRecord>($"{recName}/create?gid={Guid.NewGuid().ToString("D")}", record);
                result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
            }
            return result ?? default;
        }

        public override async Task<DbTaskResult> DeleteRecordAsync<TRecord>(TRecord record)
        {
            var result = new DbTaskResult();
            if (TryGetRecordName<TRecord>(out string recName))
            {
                var response = await this.HttpClient.PostAsJsonAsync<TRecord>($"{recName}/delete?gid={Guid.NewGuid().ToString("D")}", record);
                result = await response.Content.ReadFromJsonAsync<DbTaskResult>();
            }
            return result ?? default;
        }
    }
```

We'll look at the Server Side Controller shortly.

### The Business Logic/Controller Service Tier

Controllers are normally configured as Scoped Services.

The controller tier interface and base class are generic and reside in the CEC.Blazor.SPA library.  Two interfaces `IFactoryControllerService` and `IControllerPagingService` define the required functionality.  Both are implemented in the `FactoryControllerService` class.  

The code for these services is far too long to reproduce here.  `FactoryControllerService` is split into three partial classes.  We'll cover most of the functionality when we look at how the UI layer interfaces with the controller layer.

The main functionality implemented is:

1. Properties to hold the current record and recordset and their status.
2. Properties and methods - defined in `IControllerPagingService` - for UI paging operations on large datasets.
4. Properties and methods to sort the the dataset.
3. Properties and methods to track the edit status of the record (Dirty/Clean).
4. Methods to implement CRUD operations through the IDataService Interface.
5. Events triggered on record and record set changes.  Used by the UI to control page refreshes.
7. Methods to reset the Controller to a vanilla state.

All code needed to provide the above functionality is boilerplated in the base class.  Implementing specific record based controllers is a simple task with minimal coding.

#### WeatherForecastControllerService

Let's look at the `WeatherForecastControllerService` as an example.

The class:
 
1. Inherits from `FactoryControllerService` and sets the generic `TDbContext` and `TRecord` to specific classes.
2. Implements the class constructor that gets the required DI services and sets up the base class.
3. Gets the Dictionary object for the Outlook Enum Select box in the UI.

Note that the data service used is the `IFactoryDataService` configured in Services.  For WASM this is `FasctoryWASMDataService` and for Server or the API EASM Server this is `FactoryServerDataService`.
```c#
// CEC.Weather/Controllers/ControllerServices/WeatherForecastControllerService.cs
public class WeatherForecastControllerService : 
    FactoryControllerService<DbWeatherForecast, WeatherForecastDbContext> 
{
    /// List of Outlooks for Select Controls
    public SortedDictionary<int, string> OutlookOptionList => Utils.GetEnumList<WeatherOutlook>();

    public WeatherForecastControllerService(NavigationManager navmanager, IConfiguration appconfiguration, IFactoryDataService<WeatherForecastDbContext> dataService) : base(appconfiguration, navmanager,dataService)
    {
    }
}
```

#### API Controllers

While they are not a service they are the final bit of the data layers to cover.  All the controllers use  the `IFactoryDataService` service to access the `FactoryServerDataService` data layer.  There's a controller per record.  We'll look at `WeatherForecastController` to review the code.

Note:
1. It gets the `IFactoryDataService` when the controller is initialized.
2. It maps the API calls directly into the `IFactoryDataService` methods.
 
```c#
// CEC.Blazor.WASM.Server/Controllers/WeatherForecastController.cs
[ApiController]
public class WeatherForecastController : ControllerBase
{
    protected IFactoryDataService<WeatherForecastDbContext> DataService { get; set; }

    private readonly ILogger<WeatherForecastController> logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IFactoryDataService<WeatherForecastDbContext> dataService)
    {
        this.DataService = dataService;
        this.logger = logger;
    }

    [MVC.Route("weatherforecast/list")]
    [HttpGet]
    public async Task<List<DbWeatherForecast>> GetList() => await DataService.GetRecordListAsync<DbWeatherForecast>();

    [MVC.Route("weatherforecast/filteredlist")]
    [HttpPost]
    public async Task<List<DbWeatherForecast>> GetFilteredRecordListAsync([FromBody] FilterListCollection filterList) => await DataService.GetFilteredRecordListAsync<DbWeatherForecast>(filterList);

    [MVC.Route("weatherforecast/lookuplist")]
    [HttpGet]
    public async Task<SortedDictionary<int, string>> GetLookupListAsync() => await DataService.GetLookupListAsync<DbWeatherForecast>();

    [MVC.Route("weatherforecast/distinctlist")]
    [HttpPost]
    public async Task<List<string>> GetDistinctListAsync([FromBody] string fieldName) => await DataService.GetDistinctListAsync<DbWeatherForecast>(fieldName);

    [MVC.Route("weatherforecast/count")]
    [HttpGet]
    public async Task<int> Count() => await DataService.GetRecordListCountAsync<DbWeatherForecast>();

    [MVC.Route("weatherforecast/get")]
    [HttpGet]
    public async Task<DbWeatherForecast> GetRec(int id) => await DataService.GetRecordAsync<DbWeatherForecast>(id);

    [MVC.Route("weatherforecast/read")]
    [HttpPost]
    public async Task<DbWeatherForecast> Read([FromBody]int id) => await DataService.GetRecordAsync<DbWeatherForecast>(id);

    [MVC.Route("weatherforecast/update")]
    [HttpPost]
    public async Task<DbTaskResult> Update([FromBody]DbWeatherForecast record) => await DataService.UpdateRecordAsync<DbWeatherForecast>(record);

    [MVC.Route("weatherforecast/create")]
    [HttpPost]
    public async Task<DbTaskResult> Create([FromBody]DbWeatherForecast record) => await DataService.CreateRecordAsync<DbWeatherForecast>(record);

    [MVC.Route("weatherforecast/delete")]
    [HttpPost]
    public async Task<DbTaskResult> Delete([FromBody] DbWeatherForecast record) => await DataService.DeleteRecordAsync<DbWeatherForecast>(record);
}
```

### Wrap Up
This article demonstrates how to abstract the data and controller tier code into a library and build boilerplate code using generics.

Some key points to note:
1. Aysnc code is used wherever possible.  The data access functions are all async.
2. Generics make much of the boilerplating possible.  They create complexity, but are worth the effort.
3. Interfaces are crucial for Dependancy Injection and UI boilerplating.

The next section looks at the [Presentation Layer / UI Framework](https://www.codeproject.com/Articles/5279963/Building-a-Database-Application-in-Blazor-Part-3-C).

## History

* 15-Sep-2020: Initial version.
* 2-Oct-2020: Minor formatting updates and typo fixes.
* 17-Nov-2020: Major Blazor.CEC library changes.  Change to ViewManager from Router and new Component base implementation.
* 7-Feb-2021: Major updates to Services, project structure and data editing.
