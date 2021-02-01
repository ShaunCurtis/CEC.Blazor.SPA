# Chapter X Working with Data Records

I'm a firm believer in the notion that databse data records should be immutable.  A record should always represent the underlying source data.  If you want to change that data, make a copy of the record, update it and then submit the edited version back to the data source to update the original.  This framework implements that principle.

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
## The RecordEditContext

`RecordEditContext` is the final bit of the data record jigsaw.  The `RecordEditContext` is what the editor form interfaces with and the `InputBase` fields connect to.
 
### IRecordEditContext

`IRecordEditContext` defines the common interface

```c#
    public interface IRecordEditContext
    {
        public EditContext EditContext { get; }
        public bool IsValid { get; }
        public bool IsDirty { get; }
        public bool IsClean { get; }
        public bool IsLoaded { get; }
        public Task NotifyEditContextChangedAsync(EditContext context);
        public bool Validate();
    }
```

### RecordEditContext

`RecordEditContext` is the base implementation for any record.  It contains the common boilerplate code.

The properties and fields are shown below:

```c#
    public abstract class RecordEditContext : IRecordEditContext
    {
        public EditContext EditContext { get; private set; }
        public bool IsValid => !Trip;
        public bool IsDirty => this.RecordValues?.IsDirty ?? false;
        public bool IsClean => !this.IsDirty;
        public bool IsLoaded => this.EditContext != null && this.RecordValues != null;

        protected RecordCollection RecordValues { get; private set; } = new RecordCollection();
        protected bool Trip = false;
        protected List<Func<bool>> ValidationActions { get; } = new List<Func<bool>>();
        protected ValidationMessageStore ValidationMessageStore;

        private bool Validating;

```
The class is initialised by passing it a RecordCollection.  `null` is not allowed.  The initialisation method also calls `LoadValidationActions` which populates the `ValidationActions` property.  The method prototype does nothing, but in a record implementation `ValidationActions` is a `Func` delegate that holds the validation functions for the record.  We'll see some of these when we look at a concrete implementation.

```c#
        public RecordEditContext(RecordCollection collection)
        {
            Debug.Assert(collection != null);

            if (collection is null)
                throw new InvalidOperationException($"{nameof(RecordEditContext)} requires a valid {nameof(RecordCollection)} object");
            else
            {
                this.RecordValues = collection;
                this.LoadValidationActions();
            }
        }

        protected virtual void LoadValidationActions() { }

```

`NotifyEditContextChangedAsync` is called by the editor when the form `EditContext` is changed.  It's normally only called during the Form rendering process.  It:
1. Will error on being passed a `null`.
2. Disconnects wiring to the old `EditContext` if one existed.
3. Sets the objects `EditContext` to the new one.
4. Creates a new `ValidationMessageStore` object from the EditContext.
5. Wires up to the new `EditContext.OnValidationRequested` to `ValidationRequested`.
6. Calls `Validate` on the current object to set the validation state correctly. 

We're wiring the `RecordEditContext` into the form controls through the `EditContext` when the editor form pulls the `NotifyEditContextChangedAsync` trigger.

```c#
        public Task NotifyEditContextChangedAsync(EditContext context)
        {
            var oldcontext = this.EditContext;
            if (context is null)
                throw new InvalidOperationException($"{nameof(RecordEditContext)} - NotifyEditContextChangedAsync requires a valid {nameof(EditContext)} object");
            // if we already have an edit context, we will have registered with OnValidationRequested, so we need to drop it before losing our reference to the editcontext object.
            if (this.EditContext != null)
            {
                EditContext.OnValidationRequested -= ValidationRequested;
            }
            // assign the Edit Context internally
            this.EditContext = context;
            if (this.IsLoaded)
            {
                // Get the Validation Message Store from the EditContext
                ValidationMessageStore = new ValidationMessageStore(EditContext);
                // Wire up to the Editcontext to service Validation Requests
                EditContext.OnValidationRequested += ValidationRequested;
            }
            // Call a validation on the current data set
            Validate();
            return Task.CompletedTask;
        }
```

The Validation process is either triggered manually by calling `Validate` or by the `EditContext` through the wired up event handler.  The validation process:
1. Checks the `ValidationMessageStore` and the `Validating` flag.  We don't want to fire one validation is another is already running.
2. Clears the `ValidationMessageStore`.  As the `ValidationMessageStore` is wired into the `EditContext` it actually clears the messages out of the `EditContext`.
3. Sets the tripwire.
4. Loops through and invokes each validator, tripping the tripwire if we don't get a true back.
5. Calls `NotifyValidationStateChanged` on the `EditContext` to cascade the new validation down.
6. Resets `Validating`.

The important bit here is the interconnect between `RecordEditContext` and `EditContext`.  The input fields and validation messages on the edit form are all wired up to the `EditContext` and will update when the validation changes.

```c#
        public bool Validate()
        {
            ValidationRequested(this, ValidationRequestedEventArgs.Empty);
            return IsValid;
        }

        private void ValidationRequested(object sender, ValidationRequestedEventArgs args)
        {
            // using Validating to stop being called multiple times
            if (ValidationMessageStore != null && !this.Validating)
            {
                this.Validating = true;
                // clear the message store and trip wire and check we have Validators to run
                this.ValidationMessageStore.Clear();
                this.Trip = false;
                    foreach (var validator in this.ValidationActions)
                    {
                            if (!validator.Invoke()) this.Trip = true;
                    }
                EditContext.NotifyValidationStateChanged();
                this.Validating = false;
            }
        }
    }
```

Lets now look at a `RecordEditContext` implementation for a record.  Below is a shortened version of `WeatherForecastEditContext`.  I've removed most of the properties and validators.

1. There's an exposed Property for all the fields we want to use in the editor form.  the `getter` gets the field from the underlying `RecordCollection` and the setter calls `SetField` in the `RecordCollection`.  
2. The setter calls `Validate` on fields that require validation.
3. ReadOnly fields only have a getter.
4. `LoadValidationActions` loads the validators on initialisation.
5. Validators are declared as private, they are specific to this implementation.  There's one Validator for each field that requires validating.

```c#
public class WeatherForecastEditContext : RecordEditContext, IRecordEditContext
{
    public DateTime WeatherForecastDate
    {
        get => this.RecordValues.GetEditValue<DateTime>(DataDictionary.__WeatherForecastDate);
        set
        {
            this.RecordValues.SetField(DataDictionary.__WeatherForecastDate, value);
            this.Validate();
        }
    }

    public int WeatherForecastOutlookValue
    {
        get => this.RecordValues.GetEditValue<int>(DataDictionary.__WeatherForecastOutlookValue);
        set
        {
            this.RecordValues.SetField(DataDictionary.__WeatherForecastOutlookValue, value);
        }
    }

    public int WeatherForecastID
        => this.RecordValues.GetEditValue<int>(DataDictionary.__WeatherForecastID);

    public WeatherForecastEditContext(RecordCollection collection) : base(collection) { }

    #endregion

    #region Protected

    protected override void LoadValidationActions()
    {
        this.ValidationActions.Add(ValidateDescription);
        this.ValidationActions.Add(ValidateTemperatureC);
        this.ValidationActions.Add(ValidateDate);
        this.ValidationActions.Add(ValidatePostCode);
    }

    #endregion

    #region Private

    private bool ValidatePostCode()
    {
        return this.WeatherForecastPostCode.Validation(DataDictionary.__WeatherForecastPostCode.FieldName, this, ValidationMessageStore)
            .Matches(@"^([A-PR-UWYZ0-9][A-HK-Y0-9][AEHMNPRTVXY0-9]?[ABEHMNPRVWXY0-9]? {1,2}[0-9][ABD-HJLN-UW-Z]{2}|GIR 0AA)$")
            .Validate("You must enter a Post Code (e.g. GL52 8BX)");
    }

    private bool ValidateDate()
    {
        return this.WeatherForecastDate.Validation(DataDictionary.__WeatherForecastDate.FieldName, this, ValidationMessageStore)
            .NotDefault("You must select a date")
            .LessThan(DateTime.Now.AddMonths(1), true, "Date can only be up to 1 month ahead")
            .Validate();
    }
}
```

Lets look at the Date Validator in more detail.

#### Validator

`Validator` is the base class for all validators and provides the common functionality

```c#
    public abstract class Validator<T>
    {
        public bool IsValid => !Trip;
        public bool Trip {get; set;};
        public List<string> Messages {get;} = new List<string>();
        protected string FieldName { get; set; }
        protected T Value { get; set; }
        protected string DefaultMessage { get; set; } = "The value failed validation";
        protected ValidationMessageStore ValidationMessageStore { get; set; }
        protected object Model { get; set; }

        public Validator(T value, string fieldName, object model, ValidationMessageStore validationMessageStore, string message)
        {
            this.FieldName = fieldName;
            this.Value = value;
            this.Model = model;
            this.ValidationMessageStore = validationMessageStore;
            this.DefaultMessage = string.IsNullOrWhiteSpace(message) ? this.DefaultMessage : message;
        }

        public virtual bool Validate(string message = null)
        {
            if (!this.IsValid)
            {
                message ??= this.DefaultMessage;
                // Check if we've logged specific messages.  If not add the default message
                if (this.Messages.Count == 0) Messages.Add(message);
                //set up a FieldIdentifier and add the message to the Edit Context ValidationMessageStore
                var fi = new FieldIdentifier(this.Model, this.FieldName);
                this.ValidationMessageStore.Add(fi, this.Messages);
            }
            return this.IsValid;
        }

        protected void LogMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message)) Messages.Add(message);
        }
    }
```

`DateTimeValidator` is the validator for `DateTime` fields.

1. The initialiser gets passed the `ValidationMessageStore` so it can log the validation result directly to the store.  You can see that happening in `Validate` on the base class.  It needs the `Model` and the `FieldName` to associate the entry with the correct field.  Get the field name wrong and the field input control own't turn red, and the message won't appear!
2. The Value is the actual field value is passed in as `value`.
3. The validation error message can be set at various points.
4. Each validator method returns the validator object, so you can chain validation methods together.
5. Calling `Validate` completes the validation, logging any errors to the `ValidationMessageStore`.

```c#
public class DateTimeValidator : Validator<DateTime>
{
    public DateTimeValidator(DateTime value, string fieldName, object model, ValidationMessageStore validationMessageStore, string message) : base(value, fieldName, model, validationMessageStore, message) { }


    public DateTimeValidator LessThan(DateTime test, bool dateOnly = false, string message = null)
    {
        if (dateOnly)
        {
            if (!(Value.Date < test.Date))
            {
                Trip = true;
                LogMessage(message);
            }
        }
        else
        {
            if (!(this.Value < test))
            {
                Trip = true;
                LogMessage(message);
            }
        }
        return this;
    }

    public DateTimeValidator GreaterThan(DateTime test, bool dateOnly = false, string message = null)
    {
        if (dateOnly)
        {
            if (!(Value.Date > test.Date))
            {
                Trip = true;
                LogMessage(message);
            }
        }
        else
        {
            if (!(this.Value > test))
            {
                Trip = true;
                LogMessage(message);
            }
        }
        return this;
    }

    public DateTimeValidator NotDefault(string message = null)
    {
        if (this.Value == default(DateTime))
        {
            Trip = true;
            LogMessage(message);
        }
        return this;
    }
}
```

`DateTimeValidatorExtensions` defines `Validation` as a extension to the `DateTime` type.  It's arguments are the same as the initialiser for the validator and it returns a `DateTimeValidator` object.

```c#
public static class DateTimeValidatorExtensions
{
    public static DateTimeValidator Validation(this DateTime value, string fieldName, object model, ValidationMessageStore validationMessageStore, string message = null)
    {
        var validation = new DateTimeValidator(value, fieldName, model, validationMessageStore, message);
        return validation;
    }
}
```

To Implement a validator on a `DateTime` field, call the extension `Validation` with the correct arguments gets the validator.  The validation methods called are chained together - each has a different message which gets logged to the internal message store if tripped.  The final call to `Validate` completes the process, adding any messages to the `ValidationMessageStore`.

```c#
private bool ValidateDate()
{
    return this.WeatherForecastDate.Validation(DataDictionary.__WeatherForecastDate.FieldName, this, ValidationMessageStore)
        .NotDefault("You must select a date")
        .LessThan(DateTime.Now.AddMonths(1), true, "Date can only be up to 1 month ahead")
        .Validate();
}
```

Creating custom validation methods is a simple case of creating an extension method on the correct validator and following the pattern:
```c#
public xxxValidator MethodName( this xxxValidator validator, your arguments, string message = null) 
{
    // if fails
    validator.Trip = true;
    if (!string.IsNullOrWhiteSpace(message)) validator.Messages.Add(message);
    return validator;
}
```

## Section Wrap Up


