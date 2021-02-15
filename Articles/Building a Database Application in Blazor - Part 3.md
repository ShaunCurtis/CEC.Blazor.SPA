# Part 3 - View Components - CRUD Edit and View Operations in the UI

## Introduction

This is the third in a series of articles looking at how to build and structure a real Database Application in Blazor. The articles so far are:

1. [Project Structure and Framework](https://www.codeproject.com/Articles/5279560/Building-a-Database-Application-in-Blazor-Part-1-P)
2. [Services - Building the CRUD Data Layers](https://www.codeproject.com/Articles/5279596/Building-a-Database-Application-in-Blazor-Part-2-S)
3. [View Components - CRUD Edit and View Operations in the UI](https://www.codeproject.com/Articles/5279963/Building-a-Database-Application-in-Blazor-Part-3-C)
4. [UI Components - Building HTML/CSS Controls](https://www.codeproject.com/Articles/5280090/Building-a-Database-Application-in-Blazor-Part-4-U)
5. [View Components - CRUD List Operations in the UI](https://www.codeproject.com/Articles/5280391/Building-a-Database-Application-in-Blazor-Part-5-V)
6. [A walk through detailing how to add weather stations and weather station data to the application](https://www.codeproject.com/Articles/5281000/Building-a-Database-Application-in-Blazor-Part-6-A)

This article looks in detail at building reusable CRUD presentation layer components, specifically Edit and View functionality - and using them in Server and WASM projects.  There are significant changes since the first release.

## Sample Project and Code

The repository for the articles has moved to [CEC.Blazor.SPA Repository](https://github.com/ShaunCurtis/CEC.Blazor.SPA).  [CEC.Blazor GitHub Repository](https://github.com/ShaunCurtis/CEC.Blazor) is obselete and will be removed.

There's a SQL script in /SQL in the repository for building the database.

[You can see the Server and WASM versions of the project running here on the same site](https://cec-blazor-server.azurewebsites.net/).

Serveral classes described here are part of the separate *CEC.Blazor.Core* library.  The Github is [here](https://github.com/ShaunCurtis/CEC.Blazor.Core), and is available as a Nuget Package.

## The Base Forms

All CRUD UI components inherit from `Component` - see the following article about [Component](https://www.codeproject.com/Articles/5277618/A-Dive-into-Blazor-Components).  Not all the code is shown in the article: some class are simply too big to show everything.  All source files can be viewed on the Github site, and I include references or links to specific code files at appropriate places in the article.  Much of the information detail is in the comments in the code sections.

### FormBase

`Formbase` is part of the *CEC.Blazor.Core* libary.

All Forms inherit from `FormBase`.  `FormBase` provides the following functionality:

1. Replicates the code from `OwningComponentBase` to implement scoped service management.
2. Gets the User if Authentication is enabled.
3. Manages Form closure in Modal or Non-Modal state.
4. Implements the `IForm` and `IDisposable` interfaces. 

The scope management code looks like this.  You can search the Internet for articles on how to use `OwningComponentBase`.
   
```c#
// CEC.Blazor/Components/Base/BaseForm.cs
private IServiceScope _scope;

/// Scope Factory to manage Scoped Services
[Inject] protected IServiceScopeFactory ScopeFactory { get; set; } = default!;

/// Gets the scoped IServiceProvider that is associated with this component.
protected IServiceProvider ScopedServices
{
    get
    {
        if (ScopeFactory == null) throw new InvalidOperationException("Services cannot be accessed before the component is initialized.");
        if (IsDisposed) throw new ObjectDisposedException(GetType().Name);
        _scope ??= ScopeFactory.CreateScope();
        return _scope.ServiceProvider;
    }
}
```
The `IDisposable` interface implementation is tied in with scoped service management.  We'll use it later for removing event handlers.

```c#
protected bool IsDisposed { get; private set; }

/// IDisposable Interface
async void IDisposable.Dispose()
{
    if (!IsDisposed)
    {
        _scope?.Dispose();
        _scope = null;
        Dispose(disposing: true);
        await this.DisposeAsync(true);
        IsDisposed = true;
    }
}

/// Dispose Method
protected virtual void Dispose(bool disposing) { }

/// Async Dispose event to clean up event handlers
public virtual Task DisposeAsync(bool disposing) => Task.CompletedTask;

```
The rest of the properties are:

```c#
[CascadingParameter] protected IModal ModalParent { get; set; }

/// Boolean Property to check if this component is in Modal Mode
public bool IsModal => this.ModalParent != null;

/// Cascaded Authentication State Task from CascadingAuthenticationState in App
[CascadingParameter] public Task<AuthenticationState> AuthenticationStateTask { get; set; }

/// Cascaded ViewManager 
[CascadingParameter] public ViewManager ViewManager { get; set; }

/// Check if ViewManager exists
public bool IsViewManager => this.ViewManager != null;

/// Property holding the current user name
public string CurrentUser { get; protected set; }

/// Guid string for user
public string CurrentUserID { get; set; }

/// UserName without the domain name
public string CurrentUserName => (!string.IsNullOrEmpty(this.CurrentUser)) && this.CurrentUser.Contains("@") ? this.CurrentUser.Substring(0, this.CurrentUser.IndexOf("@")) : string.Empty;

```
The main event methods:

```c#
/// OnRenderAsync Method from Component
protected async override Task OnRenderAsync(bool firstRender)
{
    if (firstRender) await GetUserAsync();
    await base.OnRenderAsync(firstRender);
}

/// Method to get the current user from the Authentication State
protected async Task GetUserAsync()
{
    if (this.AuthenticationStateTask != null)
    {
        var state = await AuthenticationStateTask;
        // Get the current user
        this.CurrentUser = state.User.Identity.Name;
        var x = state.User.Claims.ToList().FirstOrDefault(c => c.Type.Contains("nameidentifier"));
        this.CurrentUserID = x?.Value ?? string.Empty;
    }
}
```
Finally, the exit button methods.
   
```c#
public void Exit(ModalResult result)
{
    if (IsModal) this.ModalParent.Close(result);
    else this.ViewManager.LoadViewAsync(this.ViewManager.LastViewData);
}

public void Exit()
{
    if (IsModal) this.ModalParent.Close(ModalResult.Exit());
    else this.ViewManager.LoadViewAsync(this.ViewManager.LastViewData);
}

public void Cancel()
{
    if (IsModal) this.ModalParent.Close(ModalResult.Cancel());
    else this.ViewManager.LoadViewAsync(this.ViewManager.LastViewData);
}

public void OK()
{
    if (IsModal) this.ModalParent.Close(ModalResult.OK());
    else this.ViewManager.LoadViewAsync(this.ViewManager.LastViewData);
}
```

### ControllerServiceFormBase

At this point in the Form heirachy we add some complexity with generics.  We inject the Controller Service through the `IFactoryControllerService` interface, and we need to provide it with the RecordType we're loading `TRecord` and the DbContext to use `TContext`.  The class declaration apples the same contraints on the generics as `IFactoryControllerService`. The rest of the Properties are descibes in the code block.
   
```c#
// CEC.Blazor/Components/BaseForms/ControllerServiceFormBase.cs
    public class ControllerServiceFormBase<TRecord, TContext> : 
        FormBase 
        where TRecord : class, IDbRecord<TRecord>, new()
        where TContext : DbContext
    {
        /// Controller Service for Type TRecord
        /// Normally set as the first line in the OnRender event.
        public IFactoryControllerService<TRecord, TContext> Service { get; set; }

        /// Property Collection to control various UI Settings
        /// Cascaded by control
        [Parameter] public PropertyCollection Properties { get; set; } = new PropertyCollection();

        /// The default alert used by all inherited components
        /// Used for Editor Alerts, error messages, ....
        [Parameter] public Alert AlertMessage { get; set; } = new Alert();

        /// Property with generic error message for the Page Manager 
        protected virtual string RecordErrorMessage { get; set; } = "The Application is loading the record.";

        /// Boolean check if the Service exists
        protected bool IsService { get => this.Service != null; }
    }
```
### RecordFormBase

This form is used directly by all the record display forms.  It introduces record management.  `RecordFormBase` holds the ID and makes the calls to the Record Service to load and reset the record.
   
```c#
// CEC.Blazor/Components/Base/RecordFormBase.cs
public class RecordFormBase<TRecord, TContext> :
    ControllerServiceFormBase<TRecord, TContext>
    where TRecord : class, IDbRecord<TRecord>, new()
    where TContext : DbContext
{
    /// This Page/Component Title
    public virtual string PageTitle => (this.Service?.Record?.DisplayName ?? string.Empty).Trim();

    /// Boolean Property that checks if a record exists
    protected virtual bool IsRecord => this.Service?.IsRecord ?? false;

    /// Used to determine if the page can display data
    protected virtual bool IsError { get => !this.IsRecord; }

    /// Used to determine if the page has display data i.e. it's not loading or in error
    protected virtual bool IsLoaded => !(this.Loading) && !(this.IsError);

    /// Property for the Record ID
    [Parameter]
    public int? ID
    {
        get => this._ID;
        set => this._ID = (value is null) ? -1 : (int)value;
    }

    /// No Null Version of the ID
    public int _ID { get; private set; }

    protected async override Task OnRenderAsync(bool firstRender)
    {
        // On first load reset the Service record to make sure we're clean
        if (firstRender && this.IsService)
        {
            await this.Service.ResetRecordAsync();
            // Set up the Record Changed event to trigger a render event on the component
            this.Service.RecordHasChanged += this.OnRecordChangedAsync;
        }
        // Load the Record
        await this.LoadRecordAsync(firstRender);
        //  Call down the hierachy
        await base.OnRenderAsync(firstRender);
    }

    // Event handler that triggers a page render
    protected void Render_Async(object sender, EventArgs e) => this.RenderAsync();

    /// Reloads the record if the ID has changed
    protected virtual async Task LoadRecordAsync(bool firstload = false)
    {
        if (this.IsService)
        {
            // Set the Loading flag 
            if (setLoading) this.Loading = true;
            //  call Render only if we are responding to an event.  In the component loading cycle it will be called for us shortly
            if (!firstload) await RenderAsync();
            if (this.IsModal && this.ViewManager.ModalDialog.Options.Parameters.TryGetValue("ID", out object modalid)) this.ID = (int)modalid > -1 ? (int)modalid : this.ID;

            // Get the current record - this will check if the id is different from the current record and only update if it's changed
            await this.Service.GetRecordAsync(this._ID, firstload);
            //if (this._ID == 0 && firstload) await this.Service.SetToNewRecordAsync();
            //else await this.Service.GetRecordAsync(this._ID, firstload);

            // Set the error message - it will only be displayed if we have an error
            this.RecordErrorMessage = $"The Application can't load the Record with ID: {this._ID}";

            // Set the Loading flag
            if (setLoading) this.Loading = false;
            //  call Render only if we are responding to an event.  In the component loading cycle it will be called for us shortly
            if (!firstload) await RenderAsync();
        }
    }

    // OnRecordChanged Event Handler
    protected virtual async void OnRecordChangedAsync(object sender, EventArgs e) => await this.RenderAsync();

    // Disposer - unhooks wired events
    protected override void Dispose(bool disposing)
    {
        if (this.IsService) this.Service.RecordHasChanged -= this.OnRecordChangedAsync;
        base.Dispose(disposing);
    }
}
```

### EditRecordFormBase

This is the base for editor forms.  It:
1. Manages the `EditContext`.  We don't use the standard `EditForm`.  the local `EditContext` is built from the `IRecordEditContext` property.
2. Has a set of Boolean Properties to keep the UI clean.  I code any UI test statements into properties.
3. Manages the Form state based on the record state.  It locks the page in the application when the state is dirty and blocks browser navigation through the browser navigation challenge.
4. Saves the record.   

```c#
// CEC.Blazor/Components/Base/EditRecordFormBase.cs
    public abstract class EditRecordFormBase<TRecord, TDbContext> :
        RecordFormBase<TRecord, TDbContext>
        where TRecord : class, IDbRecord<TRecord>, new()
       where TDbContext : DbContext
    {
        #region Public Properties

        public EditContext EditContext
        {
            get => _EditContext;
        }

        public override string PageTitle
        {
            get
            {
                if (this.IsNewRecord) return $"New {this.Service?.RecordInfo?.RecordDescription ?? "Record"}";
                else return $"{this.Service?.RecordInfo?.RecordDescription ?? "Record"} Editor";
            }
        }

        // a set of state properties to keep the UI clean
        public bool IsClean => (this.RecordEditorContext?.IsClean ?? true);
        public bool IsDirty => this.RecordEditorContext?.IsDirty ?? false;
        protected bool IsValidated => this.RecordEditorContext?.IsValid ?? true;
        protected override bool IsError => !(this.IsRecord);
        public bool IsNewRecord => this.Service?.RecordID == 0 ? true : false;

        protected IRecordEditContext RecordEditorContext { get; set; }

        private EditContext _EditContext = null;

        // overridden LoadRecord Method
        protected async override Task LoadRecordAsync(bool firstLoad = false, bool setLoading = true)
        {
            await Task.Yield();
            this.Loading = true;
            await base.LoadRecordAsync(firstLoad, false);
            if (firstLoad)
            {
                this._EditContext = new EditContext(RecordEditorContext);
                await this.RecordEditorContext.NotifyEditContextChangedAsync(this.EditContext);
                this.EditContext.OnFieldChanged += onFieldChanged;
            }
            this.Loading = false;
        }

        /// Event handler for EditContext OnFieldChanged
        protected void onFieldChanged(object sender, EventArgs e)
        {
            this.SetViewManagerLock();
            this.RenderAsync();
        }

        /// Save Method called from the Save Button
        protected virtual async Task<bool> Save()
        {
            var ok = false;
            // Validate the EditContext
            if (this.RecordEditorContext.EditContext.Validate())
            {
                // Save the Record
                ok = await this.Service.SaveRecordAsync();
                if (ok)
                {
                    // Set the EditContext State
                    this.RecordEditorContext.EditContext.MarkAsUnmodified();
                    // Set the View Lock i.e. unlock it
                    this.SetViewManagerLock();
                }
                // Set the alert message to the return result
                this.AlertMessage.SetAlert(this.Service.TaskResult);
                // Trigger a component State update - buttons and alert need to be sorted
                await RenderAsync();
            }
            else this.AlertMessage.SetAlert("A validation error occurred.  Check individual fields for the relevant error.", MessageType.Danger);
            return ok;
        }

        protected virtual async void SaveAndExit()
        {
            if (await this.Save()) this.ConfirmExit();
        }

        protected virtual void TryExit()
        {
            // Check if we are free to exit ot need confirmation
            if (this.IsClean) ConfirmExit();
        }

        protected virtual void ConfirmExit()
        {
            // To escape a dirty component reset the Record Collection and unlock the View
            this.Service.RecordValueCollection.ResetValues();
            this.SetViewManagerLock();
            this.Exit();
        }

        private void OnValidationStateChanged(object sender, ValidationStateChangedEventArgs e) 
            => this.RenderAsync();

        private void SetViewManagerLock()
        {
            if (this.Service.RecordValueCollection.IsDirty)
            {
                this.ViewManager.LockView();
                this.AlertMessage.SetAlert("The Record isn't Saved", MessageType.Warning);
            }
            else
            {
                this.ViewManager.UnLockView();
                this.AlertMessage.ClearAlert();
            }
        }
    }
```

## Implementing Edit Components

Before we look at the edit componments we need to take a sep back and look at the `Model` we'll use.  In Article 2 we looked at our data class `DbWeatherForecast` and saw the code for creating a `RecordCollection` from the data class and building a new `DbWeatherForecast` record from a `RecordCollection`.  Remember the properties in a `DbWeatherForecast` as immutable - they are declared `{get; init;}`.  The `RecordCollection` for a record is the editable copy of that record.  We don't create a simple edit version of `DbWeatherForecast`, or a simple `Dictionary` object because we want a build in a lot more functionality.

#### RecordFieldValue

a `RecordFieldValue` represents an editable version of a database record field.

Note:
1. There's `Value` and `EditedValue` properties to keep track of the edit state of the field.
2. `IsDirty`  is a readonly property use to check the edit state.
3. `Reset` sets `EditedValue` back to `Value`.
4. `Guid` provides uniqueness.

```c#
public class RecordFieldValue
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

A `RecordCollection` is a collection of `RecordFieldValue` objects used to store record fields.  It:
1. Implements `ICollection` - a lot of the functionality is the interface implementation.  
2. Provides controlled access to the underlying list and methods to get and set `RecordFieldValue` objects.
3. As `RecordFieldValue.Value` is defined as an object, generics are used to get values from a `RecordFieldValue` object.
4. `IsDirty` tests all the `RecordFieldValue` objects in the list to check if the collection `IsDirty`.

```c#
public class RecordCollection : ICollection
{
    private List<RecordFieldValue> _items = new List<RecordFieldValue>() ;
    public int Count => _items.Count;
    public bool IsSynchronized => false;
    public object SyncRoot => this;
    public Action<bool> FieldValueChanged;
    public bool IsDirty => _items.Any(item => item.IsDirty);

    public void ResetValues()
        => _items.ForEach(item => item.Reset());

    public void Clear() => _items.Clear();

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
        else _items.Add(new RecordFieldValue(FieldName, value));
        return true;
    }
        
    // a whole host of methods for getting and setting, adding and deleting Record Values 
    public T GetEditValue<T>(string FieldName)
    {
        var x = _items.FirstOrDefault(item => item.Field.Equals(FieldName, StringComparison.CurrentCultureIgnoreCase));
        if (x != null && x.EditedValue is T t) return t;
        return default;
    }
}

...... 
// Enumerator code
```

### The RecordEditContext

`RecordEditContext` is the final bit of the data record jigsaw.  The `RecordEditContext` is the `Model` for the edit form and `EditContext` and what the `InputBase` fields are wired into.  `RecordEditContext` is the glue for the editing process, it:
1. Manages the edit state and exposes that state in `IsClean` and `IsDirty`.
2. Mananges the load state and exposes that state through `IsLoaded`.
3. Manages the validation process and state and exposes that state through `IsValid`.
4. Exposes the edit fields through properties.
 
### IRecordEditContext

First we have an interface `IRecordEditContext`.  The properties and methods are self-evident.

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
The class is initialised by passing it a RecordCollection.  `null` is not allowed.  The initialisation method also calls `LoadValidationActions` which populates the `ValidationActions` property.  The method prototype does nothing, but in a record implementation `ValidationActions` is a `Func` delegate that holds the validation functions for the record.  We'll see some of these when we look at `WaetherForecastEditContext`.

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

`NotifyEditContextChangedAsync` is called by the editor form when the `EditContext` is changed.  This will normally only be called during the iinitial rendering process.  It:
1. Will error on being passed a `null`.
2. Disconnects wiring to the old `EditContext` if one existed.
3. Sets it's `EditContext` to the new one.
4. Creates a new `ValidationMessageStore` object from the new `EditContext`.
5. Wires up to the `EditContext.OnValidationRequested` event to `ValidationRequested`.
6. Calls `Validate` on the current object to set the initial validation state. 

`RecordEditContext` is now wired into the form edit controls through the `EditContext`.  We don't wire into the  `OnFieldChanged` event because we keep our own track of edit state.

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

The Validation process is either triggered manually by calling `Validate` or by the `EditContext` triggering `OnValidationRequested`.  The validation process:
1. Checks the `ValidationMessageStore` and the `Validating` flag.  We don't want to fire one validation while another is already running.
2. Clears the `ValidationMessageStore`.  As the `ValidationMessageStore` is wired into the `EditContext` it actually clears the messages from `EditContext`.
3. Sets the tripwire `Trip`.
4. Loops through and invokes each validator, tripping the tripwire if the validator doesn't return true.
5. Calls `NotifyValidationStateChanged` on the `EditContext` to cascade the new validation.
6. Resets `Validating`.

The important bit here is the interconnect between `RecordEditContext` and `EditContext`.  The input fields and validation messages on the edit form are all wired up to the `EditContext` and will update when the validation changes.

> If UI fields don't "light up" when they're invalid, you almost certainly have a naming issue in the `FieldName` used in `FieldIdentifiers`.

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

Moving on to a real `RecordEditContext` implementation -  `WeatherForecastEditContext`.  The code below is a shortened version of `WeatherForecastEditContext`.  Most of the properties and validators have been removed.

1. There's a public Property for all the fields used in the editor form.  The `getter` gets the field from the underlying `RecordCollection` and the setter calls `SetField` in the `RecordCollection`.  
3. The setter calls `Validate` on fields that require validation.
4. ReadOnly fields only have a getter.
5. `LoadValidationActions` loads the validators on initialisation.
6. Validators are declared as private, they are specific to this implementation.  There's one Validator for each field that requires validating.

```c#
public class WeatherForecastEditContext : RecordEditContext, IRecordEditContext
{
    // Example public Edit Property with Validation
    public DateTime WeatherForecastDate
    {
        get => this.RecordValues.GetEditValue<DateTime>(DataDictionary.__WeatherForecastDate);
        set
        {
            this.RecordValues.SetField(DataDictionary.__WeatherForecastDate, value);
            this.Validate();
        }
    }

    // Example public Edit Property without Validation
    public int WeatherForecastOutlookValue
    {
        get => this.RecordValues.GetEditValue<int>(DataDictionary.__WeatherForecastOutlookValue);
        set
        {
            this.RecordValues.SetField(DataDictionary.__WeatherForecastOutlookValue, value);
        }
    }

    // Example public read only Property used for diplay purposes only
    public int WeatherForecastID
        => this.RecordValues.GetEditValue<int>(DataDictionary.__WeatherForecastID);

    public WeatherForecastEditContext(RecordCollection collection) : base(collection) { }

    // loader loading all the validators - LoadValidationActions is run by the base new
    protected override void LoadValidationActions()
    {
        this.ValidationActions.Add(ValidateDescription);
        this.ValidationActions.Add(ValidateTemperatureC);
        this.ValidationActions.Add(ValidateDate);
        this.ValidationActions.Add(ValidatePostCode);
    }

    // Example Validator - this one uses a Regex
    private bool ValidatePostCode()
    {
        return this.WeatherForecastPostCode.Validation(DataDictionary.__WeatherForecastPostCode.FieldName, this, ValidationMessageStore)
            .Matches(@"^([A-PR-UWYZ0-9][A-HK-Y0-9][AEHMNPRTVXY0-9]?[ABEHMNPRVWXY0-9]? {1,2}[0-9][ABD-HJLN-UW-Z]{2}|GIR 0AA)$")
            .Validate("You must enter a Post Code (e.g. GL52 8BX)");
    }

    // Example Validator - this one checks the date is withon bounds.  Note the specific messages for each fault condition
    private bool ValidateDate()
    {
        return this.WeatherForecastDate.Validation(DataDictionary.__WeatherForecastDate.FieldName, this, ValidationMessageStore)
            .NotDefault("You must select a date")
            .LessThan(DateTime.Now.AddMonths(1), true, "Date can only be up to 1 month ahead")
            .Validate();
    }
}
```

Let's look at the Date Validator in more detail.

#### Validator

`Validator` is the base class for all validators and provides the common boilerplate functionality.

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

        // New called by the extension method for the specific validator
        public Validator(T value, string fieldName, object model, ValidationMessageStore validationMessageStore, string message)
        {
            this.FieldName = fieldName;
            this.Value = value;
            this.Model = model;
            this.ValidationMessageStore = validationMessageStore;
            this.DefaultMessage = string.IsNullOrWhiteSpace(message) ? this.DefaultMessage : message;
        }

        // final chain method called that returns a bool and updates the ValidationMessageStore
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

1. The initialiser gets passed the `ValidationMessageStore` so it can log the validation result directly to the store.  You can see that happening in `Validate` on the base class.  It needs the `Model` and the `FieldName` to associate the entry with the correct field.  Get the field name wrong and the field input control won't turn red, and the message won't appear!
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

To Implement a validator on a `DateTime` field, call the extension `Validation` with the correct arguments returns the validator.  The validation methods can thern be chained - each has a different message which gets logged to the internal message store if tripped.  The final call to `Validate` completes the process, adding any messages to the `ValidationMessageStore`, and returns true of false.

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

Let's now move on to the UI components.

## the UI Components

All the Forms and Views are implemented in the `CEC.Weather` library.

### The View

The View is a very simple. It:
1. Sets the inheritance to `ViewBase` which implments `IView`.
2. Declares an `ID` Parameter which is passed to `WeatherForecastEditorForm`
3. Adds a `WeatherForecastEditorForm` component to the view.

```c#
// CEC.Weather/Components/Views/WeatherForecastEditorView.razor
@inherits Component
@implements IView

@namespace CEC.Weather.Components.Views

<WeatherForecastEditorForm ID="this.ID"></WeatherForecastEditorForm>

@code {

    [CascadingParameter] public ViewManager ViewManager { get; set; }

    [Parameter] public int ID { get; set; } = 0;

}
```
#### ModalEditForm

The final component to look at before we look at `WeatherForecastEditorForm` is `ModalEditForm`.  This replaces `Editform`.  It's a lot simpler, managing the render process to only show the edit components when the data, edit context and RecordEditContext are loaded.  It also cascades the `EditContext` to the input controls.

1. There are four `RenderFragments` which are self-evident.
2. `Isloaded` and `IsError` control which `RenderFragments` get rendered.


```c#
public class ModalEditForm : Component
{

    [Parameter] public RenderFragment EditorContent { get; set; }
    [Parameter] public RenderFragment ButtonContent { get; set; }
    [Parameter] public RenderFragment LoadingContent { get; set; }
    [Parameter] public RenderFragment ErrorContent { get; set; }

    [Parameter] public bool IsLoaded { get; set; }
    [Parameter] public bool IsError { get; set; }

    [Parameter] public EditContext EditContext { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        //Debug.Assert(EditContext != null);
        if (this.IsLoaded)
        {
            // If EditContext changes, tear down and recreate all descendants.
            // This is so we can safely use the IsFixed optimization on CascadingValue,
            // optimizing for the common case where EditContext never changes.
            builder.OpenRegion(EditContext.GetHashCode());
            builder.OpenComponent<CascadingValue<EditContext>>(1);
            builder.AddAttribute(2, "IsFixed", true);
            builder.AddAttribute(3, "Value", this.EditContext);
            builder.AddAttribute(4, "ChildContent", this.EditorContent);
            builder.CloseComponent();
            builder.CloseRegion();
        }
        else if (this.IsError)
            builder.AddContent(10, this.ErrorContent);
        else
            builder.AddContent(10, this.LoadingContent);
        builder.AddContent(20, this.ButtonContent);
    }
}
```

### The Form

The code file is relatively simple, with most of the detail in the razor markup. It:
1. Declares the class and inheritance defining `TRecord` and `TDbContext`.
1. Injects the correct Controller Service and assigns the controller service to `Service`.
2. Creates a WeatherForecastEditorContext instance and assigns it to `RecordEditorContext`.

```C#
// CEC.Weather/Components/Forms/WeatherForecastEditorForm.razor
public partial class WeatherForecastEditorForm : EditRecordFormBase<DbWeatherForecast, WeatherForecastDbContext>
{
    [Inject]
    public WeatherForecastControllerService ControllerService { get; set; }

    private WeatherForecastEditContext WeatherForecastEditorContext { get; set; }

    protected override Task OnRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            this.Service = this.ControllerService;
            this.WeatherForecastEditorContext = new WeatherForecastEditContext(this.ControllerService.RecordValueCollection);
            this.RecordEditorContext = this.WeatherForecastEditorContext;
        }
        return base.OnRenderAsync(firstRender);
    }
}
```

The Razor Markup below is an abbreviated version of the full file.  This makes extensive use of UIControls which will be covered in detail in the next article.  See the comments for detail.  The import concept to note here is the Razor Markup is all Controls - there's no HTML in sight. 

```C#
// CEC.Weather/Components/Forms/WeatherForecastEditorForm.razor.cs
// UI Card is a Bootstrap Card
<UICard IsCollapsible="false">
    <Header>
        @this.PageTitle
    </Header>
    <Body>
        // Handles errors and controls when the enclosed code is rendered
        <ModalEditForm EditContext="this.EditContext" IsError="this.IsError" IsLoaded="this.IsLoaded">
            <ErrorContent>
                <UIContainer>
                    <UIRow>
                        <UIColumn>
                            @this.RecordErrorMessage
                        </UIColumn>
                    </UIRow>
                </UIContainer>
            </ErrorContent>

            <LoadingContent>
                <UILoading/>
            </LoadingContent>

            <EditorContent>
                <UIContainer>
                    // Example display only row
                    <UIFormRow>
                        <UILabelColumn Columns="4">
                            Record ID:
                        </UILabelColumn>
                        <UIColumn Columns="2">
                            <InputReadOnlyText Value="@this.WeatherForecastEditorContext.WeatherForecastID.ToString()"></InputReadOnlyText>
                        </UIColumn>
                    </UIFormRow>

                    // Example data value row with label and edit control
                    <UIFormRow>
                        <UILabelColumn Columns="4">
                            Record Date:
                        </UILabelColumn>
                        <UIColumn Columns="4">
                            <InputDate class="form-control" @bind-Value="this.WeatherForecastEditorContext.WeatherForecastDate"></InputDate>
                        </UIColumn>
                    </UIFormRow>
                    ..... // more form rows here
                </UIContainer>
            </EditorContent>

            // always displayed section of the ErrorHandler.  Exit button to exit the form no matter what happens
            <ButtonContent>
                <UIContainer>
                    <UIRow>
                        <UIColumn Columns="7">
                            <UIAlert Alert="this.AlertMessage" SizeCode="Bootstrap.SizeCode.sm"></UIAlert>
                        </UIColumn>
                        <UIButtonColumn Columns="5">
                            <UIButton Disabled="!this.DisplaySave" ClickEvent="this.SaveAndExit" ColourCode="Bootstrap.ColourCode.save">@this.SaveButtonText &amp; Exit</UIButton>
                            <UIButton Disabled="!this.DisplaySave" ClickEvent="this.Save" ColourCode="Bootstrap.ColourCode.save">@this.SaveButtonText</UIButton>
                            <UIButton Show="this.DisplayCheckExit" ClickEvent="this.ConfirmExit" ColourCode="Bootstrap.ColourCode.danger_exit">Exit Without Saving</UIButton>
                            <UIButton Show="this.DisplayExit" ClickEvent="this.TryExit" ColourCode="Bootstrap.ColourCode.nav">Exit</UIButton>
                        </UIButtonColumn>
                    </UIRow>
                </UIContainer>
            </ButtonContent>
        </ModalEditForm>
    </Body>
</UICard>
```
### Form Event Code

Let's take a look at how all this code wires up.

#### Component Event Code

Start with `OnRenderAsync`.

##### OnInitializedAsync

`OnRenderAsync` is implemented from top down (local code is run before calling the base method).  It:

1. Assigns the right data service to `Service`.
4. Creates the `WeatherForecastEditorContext` instance and assigns it to the Interface property.
2. Resets the Record.
3. Wires up `OnRecordChangedAsync` to the service event.
5. Loads the record through `LoadRecordAsync`.
6. Gets The user information.

```c#
// CEC.Weather/Components/Forms/WeatherEditorForm.razor.cs
protected override Task OnRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        this.Service = this.ControllerService;
        this.WeatherForecastEditorContext = new WeatherForecastEditContext(this.ControllerService.RecordValueCollection);
        this.RecordEditorContext = this.WeatherForecastEditorContext;
    }
    return base.OnRenderAsync(firstRender);
}

// CEC.Blazor/Components/BaseForms/RecordFormBase.cs
protected async override Task OnRenderAsync(bool firstRender)
{
    if (firstRender && this.IsService)
    {
        await this.Service.ResetRecordAsync();
        this.Service.RecordHasChanged += this.OnRecordChangedAsync;
    }
    await this.LoadRecordAsync(firstRender);
    await base.OnRenderAsync(firstRender);
}


// CEC.Blazor/Components/BaseForms/ApplicationComponentBase.cs
protected async override Task OnRenderAsync(bool firstRender)
{
    if (firstRender) {
        await GetUserAsync();
    }
    await base.OnRenderAsync(firstRender);
}
```
##### LoadRecordAsync

Record loading code is broken out so it can be used outside the component event driven methods.  It's implemented from bottom up (base method is called before any local code).

The primary record load functionaility is in `RecordFormBase` which gets and loads the record based on ID.  `EditFormBase` adds the extra editing functionality - it creates the edit context for the record. 

```C#
// CEC.Blazor/Components/BaseForms/RecordComponentBase.cs
protected virtual async Task LoadRecordAsync(bool firstload = false, bool setLoading = true )
{
    if (this.IsService)
    {
        // Set the Loading flag 
        if (setLoading) this.Loading = true;
        //  call Render only if we are not responding to first load
        if (!firstload) await RenderAsync();
        if (this.IsModal && this.ViewManager.ModalDialog.Options.Parameters.TryGetValue("ID", out object modalid)) this.ID = (int)modalid > -1 ? (int)modalid : this.ID;

        // Get the current record - this will check if the id is different from the current record and only update if it's changed
        await this.Service.GetRecordAsync(this._ID, false);

        // Set the error message - it will only be displayed if we have an error
        this.RecordErrorMessage = $"The Application can't load the Record with ID: {this._ID}";

        // Set the Loading flag
        if (setLoading) this.Loading = false;
        //  call Render only if we are not responding to first load
        if (!firstload) await RenderAsync();
    }
}

// CEC.Blazor/Components/BaseForms/EditComponentBase.cs
protected async override Task LoadRecordAsync(bool firstLoad = false, bool setLoading = true)
{
    // yield to allow fired event can run before we load the record
    await Task.Yield();
    this.Loading = true;
    await base.LoadRecordAsync(firstLoad, false);
    if (firstLoad)
    {
        // Set up the edit context
        this._EditContext = new EditContext(RecordEditorContext);
        // pass it back to the `RecordEditorContext`
        await this.RecordEditorContext.NotifyEditContextChangedAsync(this.EditContext);
        // wire up the onfieldchanged event.
        this.EditContext.OnFieldChanged += onFieldChanged;
    }
    this.Loading = false;
}
```

Some points to note:
1. We connect up `WeatherForecastEditorContext` and then reset the record before loading it.  This isn't a problem because `WeatherForecastEditorContext` is connected to the service `RecordCollection` which `ResetRecordAsync` only clears.  `LoadRecordAsync` gets the record and repopulates the `RecordCollection` before the editor section of the form is displayed. That's controlled by the `UIErrorHandler` component which only renders the editor section of the form when loading has completed.
2.     

#### Event Handlers

There's one event handler wired up in the form.  Whenever a field changes `SetViewManagerLock` checks the edit state of the `RecordEditorContext` and unlocks or locks the form and sorts out the form message.  View locking prevents the user exiting the form without accpeting the data loss.

```c#
// CEC.Blazor/Components/BaseForms/EditRecordFormBase.cs
protected void onFieldChanged(object sender, EventArgs e)
{
    this.SetViewManagerLock();
    this.RenderAsync();
}

private void SetViewManagerLock()
{
    if (this.RecordEditorContext.IsDirty)
    {
        this.ViewManager.LockView();
        this.AlertMessage.SetAlert("The Record isn't Saved", MessageType.Warning);
    }
    else
    {
        this.ViewManager.UnLockView();
        this.AlertMessage.ClearAlert();
    }
}
```

#### Action Button Events

There are various actions wired up to buttons.  The important one is save.  The others are all self-evident.

```c#
// CEC.Blazor/Components/BaseForms/EditRecordComponentBase.cs
/// Save Method called from the Button
protected virtual async Task<bool> Save()
{
    var ok = false;
    // Validate the EditContext
    if (this.RecordEditorContext.EditContext.Validate())
    {
        // Save the Record
        ok = await this.Service.SaveRecordAsync();
        if (ok)
        {
            // Set the EditContext State
            this.RecordEditorContext.EditContext.MarkAsUnmodified();
            // Set the View Lock i.e. unlock it
            this.SetViewManagerLock();
        }
        // Set the alert message to the return result
        this.AlertMessage.SetAlert(this.Service.TaskResult);
        // Trigger a component State update - buttons and alert need to be sorted
        await RenderAsync();
    }
    else this.AlertMessage.SetAlert("A validation error occurred.  Check individual fields for the relevant error.", MessageType.Danger);
    return ok;
}
```

## Implementing Viewer Pages

### The View

The view is a very simple.  It contains the form to load.

```c#
@using CEC.Weather.Components

@namespace CEC.Weather.Components.Views
@implements IView

@inherits ViewBase

<WeatherForecastViewerForm ID="this.ID"></WeatherForecastViewerForm>

@code {
    [Parameter] public int ID { get; set; } = 0;
}
```

### The Form

The code file is also relatively simple. `RecordFormBase` boilerplate code loads the record. Most of the detail in the razor markup.

```C#
// CEC.Weather/Components/Forms/WeatherViewerForm.razor
public partial class WeatherForecastViewerForm : RecordFormBase<DbWeatherForecast, WeatherForecastDbContext>
{
    [Inject]
    private WeatherForecastControllerService ControllerService { get; set; }

    public override string PageTitle => $"Weather Forecast Viewer {this.Service?.Record?.Date.AsShortDate() ?? string.Empty}".Trim();

    protected override Task OnRenderAsync(bool firstRender)
    {
        if (firstRender) this.Service = this.ControllerService;
        return base.OnRenderAsync(firstRender);
    }
}
```

This gets and assigns the specific ControllerService through DI to the `IContollerService Service` Property.

The Razor Markup below is an abbreviated version of the full file.  This makes extensive use of UIControls which will be covered in detail in a later article.  See the comments for detail. 
```C#
// CEC.Weather/Components/Forms/WeatherViewerForm.razor.cs
// UI Card is a Bootstrap Card
<UICard IsCollapsible="false">
    <Header>
        @this.PageTitle
    </Header>
    <Body>
        // Error handler - only renders it's content when the record exists and is loaded
        <UIErrorHandler IsError="@this.IsError" IsLoading="this.Loading" ErrorMessage="@this.RecordErrorMessage">
            <UIContainer>
                    .....
                    // Example data value row with label and edit control
                    <UIRow>
                        <UILabelColumn Columns="2">
                            Date
                        </UILabelColumn>

                        <UIColumn Columns="2">
                            <FormControlPlainText Value="@this.Service.Record.Date.AsShortDate()"></FormControlPlainText>
                        </UIColumn>

                        <UILabelColumn Columns="2">
                            ID
                        </UILabelColumn>

                        <UIColumn Columns="2">
                            <FormControlPlainText Value="@this.Service.Record.ID.ToString()"></FormControlPlainText>
                        </UIColumn>

                        <UILabelColumn Columns="2">
                            Frost
                        </UILabelColumn>

                        <UIColumn Columns="2">
                            <FormControlPlainText Value="@this.Service.Record.Frost.AsYesNo()"></FormControlPlainText>
                        </UIColumn>
                    </UIRow>
                    ..... // more form rows here
            </UIContainer>
        </UIErrorHandler>
        // Container for the buttons - not record dependant so outside the error handler to allow navigation if UIErrorHandler is in error.
        <UIContainer>
            <UIRow>
                <UIColumn Columns="6">
                    <UIButton Show="this.IsLoaded" ColourCode="Bootstrap.ColourCode.dark" ClickEvent="(e => this.NextRecord(-1))">
                        Previous
                    </UIButton>
                    <UIButton Show="this.IsLoaded" ColourCode="Bootstrap.ColourCode.dark" ClickEvent="(e => this.NextRecord(1))">
                        Next
                    </UIButton>
                </UIColumn>
                <UIButtonColumn Columns="6">
                    <UIButton Show="true" ColourCode="Bootstrap.ColourCode.nav" ClickEvent="(e => this.Exit())">
                        Exit
                    </UIButton>
                </UIButtonColumn>
            </UIRow>
        </UIContainer>
    </Body>
</UICard>
```

### Wrap Up
That wraps up this article.  We've looked at the Editor code in detail to see how it works, and then taken a quick look at the Viewer code.  We'll look in more detail at the List components in a separate article.   
Some key points to note:
1. The Blazor Server and Blazor WASM code is the same - it's in the common library.
2. Almost all the functionality is implemented in the library components.  Most of the application code is Razor markup for the individual record fields.
3. The Razor files contains controls, not HTML.
4. Async functionality in used through.


## History

* 19-Sep-2020: Initial version.
* 17-Nov-2020: Major Blazor.CEC library changes.  Change to ViewManager from Router and new Component base implementation.
* 7-Feb-2021: Major updates to Services, project structure and data editing.
