---
title: Part 3 - Building Edit and View UI Components
oneliner: This article describes building the Edit and View UI components for a Blazor Database Application.
precis: This article describes building the Edit and View UI components for a Blazor Database Application.
date: 2020-10-03
published: 2020-10-03
---

# Part 3 - View Components - CRUD Edit and View Operations in the UI

## Introduction

This is the third in a series of articles looking at how to build and structure a Database Application in Blazor. The articles so far are:

1. Project Structure and Framework.
2. Services - Building the CRUD Data Layers.
3. View Components - CRUD Edit and View Operations in the UI.
4. UI Components - Building HTML/CSS Controls.
5. View Components - CRUD List Operations in the UI.

This article looks in detail at building reusable CRUD presentation layer components, specifically Edit and View functionality.  There are significant changes since the first release.

I find it interesting that most programmers try and automate Edit And View forms by building a control builder rather than boilerplating everything else. Most forms are unique to their record set.  Certain fields can be grouped together and put on the same line.  Text fields change in length depending on how many characters they need.  Building a factory to handle this, plus the added complication of linkages between the control, the dataclass instance and validation, doesn't seem worth it.  The configuration dataset becomes more complicated than the form it's trying to mimic.  For those reasons there's no form builder here, just a set of libary UI component classes to standardise form building.

## Sample Project, Code and 

The repository for the articles has moved to [CEC.Database Repository](https://github.com/ShaunCurtis/CEC.Database).  You can use it as a template for developing your own applications.  Previous repositories are obselete and will be removed.

There's a SQL script in /SQL in the repository for building the database.  The application can use either a real SQL database or an in-memory SQLite database.

[You can see the Server and WASM versions of the project running here on the same site](https://cec-blazor-database.azurewebsites.net/).

Several custom controls are used in the forms.  The detail on these is covered in separate articles:

- [EditFormState Control](https://www.codeproject.com/Articles/5297299/A-Blazor-Edit-Form-State-Control)
- [EditValidationState Control](https://www.codeproject.com/Articles/5297302/A-Blazor-Validation-Control) 
- [InlineDialog Control](https://www.codeproject.com/Articles/5297432/A-Blazor-Inline-Dialog-Control)
- [ModalDialog Control](https://www.codeproject.com/Articles/5294466/A-Blazor-Modal-Dialog-Editor)

## The Base Forms

All UI components inherit from `ComponentBase`.  All source files can be viewed on the Github site, and I include references or links to specific code files at appropriate places in the article.  In most places you need to read through the code for detailed commentary on functionality.

### RecordFormBase

`RecordFormBase` is the base abstract class used by the record forms.  It inherits from `ComponentBase`.  Record forms can be created in several contexts:
 
1. As the root component in a RouteView, where the RouteView passes the form the `Id` via a Parameter.
2. In a modal dialog within a list or other component.  The ID is passed to the form through a `DialogOptions` class.  
3. As an inline editor within another component such as a list, where the component passes the form the `Id` via a Parameter.

`RecordFormBase` is designed to detect it's context, specifically the modal dialog context by checking by checking for a cascaded `IModalDialog` object.  There are two sets of dependancies:  
1. The `Id` of the record.  This is either passed as a `Parameter` if the form is hosted in a RouteView or other component, or in a public `ModalOptions` property of `IModalDialog`.  Note an Id of -1 means new record while 0 means a default record (0 is the default value for `int`).
2. The Exit mechanism.  This is either:
   1.  By calling close on `Modal` if it's in a modal context.
   2.  By calling the `ExitAction` delegate if one if registered.
   3.  A default - exit to root.
 
```csharp
// Blazor.SPA/Components/Base/RecordFormBase.cs
    public class RecordFormBase<TRecord> : ComponentBase  where TRecord : class, IDbRecord<TRecord>, new()
    {
        [CascadingParameter] public IModalDialog Modal { get; set; }
        [Parameter] public int ID {get; set;}
        [Parameter] public EventCallback ExitAction { get; set; }
        [Inject] protected NavigationManager NavManager { get; set; }
        protected IFactoryControllerService<TRecord> Service { get; set; }
        protected virtual bool IsLoaded => this.Service != null && this.Service.Record != null;
        protected virtual bool HasServices => this.Service != null;
        protected bool _isModal => this.Modal != null;
        protected int _modalId = 0;
        protected int _Id => _modalId != 0 ? _modalId : this.ID;

        protected async override Task OnInitializedAsync()
        {
            await LoadRecordAsync();
            await base.OnInitializedAsync();
        }

        protected virtual async Task LoadRecordAsync()
        {
            this.TryGetModalID();
            await this.Service.GetRecordAsync(this._Id);
        }

        protected virtual bool TryGetModalID()
        {
            if (this._isModal && this.Modal.Options.TryGet<int>("Id", out int value))
            {
                this._modalId = value;
                return true;
            }
            return false;
        }

        protected virtual void Exit()
        {
            if (this._isModal)
                this.Modal.Close(ModalResult.OK());
            else if (ExitAction.HasDelegate)
                ExitAction.InvokeAsync();
            else
                this.NavManager.NavigateTo("/");
        }
    }
```

### EditRecordFormBase

`EditRecordFormBase` is the base for editor forms. It inherits from `RecordFormBase` and implements editing functionality.

It:
1. Manages the `EditContext`.
2. Has a set of Boolean Properties to track state and manage button display/disabled state.
3. Saves the record.

The `Dirty` property interfaces with the Modal Dialog to lock it (not allow exit and turn off navigation) when the form is dirty.
 
```csharp
// Blazor.SPA/Components/Base/EditRecordFormBase.cs
public abstract class EditRecordFormBase<TRecord> : RecordFormBase<TRecord>, IDisposable where TRecord : class, IDbRecord<TRecord>, new()
{
    /// Edit Context for the Editor - built from the service record
    protected EditContext EditContext { get; set; }

    /// Property tracking the Edit state of the form
    protected bool IsDirty
    {
        get => this._isDirty;
        set
        {
            if (value != this.IsDirty)
            {
                this._isDirty = value;
                if (this._isModal) this.Modal.Lock(value);
            }
        }
    }

    /// model used by the Edit Context
    protected TRecord Model => this.Service?.Record ?? null;

    /// Reference to the form EditContextState control
    protected EditFormState EditFormState { get; set; }
```

The next set of properties are state properties used in the code and by the razor buttons to control display/disabled state.

```csharp
    protected bool _isNew => this.Service?.IsNewRecord ?? true;
    protected bool _isDirty = false;
    protected bool _isValid = true;
    protected bool _saveDisabled => !this.IsDirty || !this._isValid;
    protected bool _deleteDisabled => this._isNew || this._confirmDelete;
    protected bool _isLoaded = false;
    protected bool _dirtyExit = false;
    protected bool _confirmDelete = false;
    protected bool _isInlineDirty => (!this._isModal) && this._isDirty;
    protected string _saveButtonText => this._isNew ? "Save" : "Update";
```

`LoadRecordAsync` calls the base to get the record, creates the `EditContext` and registers with `EditContext.OnFieldChanged`.  The other methods handle state change.

```csharp
    protected async override Task OnInitializedAsync()
        => await LoadRecordAsync();

    /// Method to load the record
    /// calls the base method to load the record and then sets up the EditContext
    protected override async Task LoadRecordAsync()
    {
        await base.OnInitializedAsync();
        this.EditContext = new EditContext(this.Model);
        _isLoaded = true;
        this.EditContext.OnFieldChanged += FieldChanged;
        if (!this._isNew)
            this.EditContext.Validate();
    }

    /// Event handler for EditContext OnFieldChanged Event
    protected void FieldChanged(object sender, FieldChangedEventArgs e)
    {
        this._dirtyExit = false;
        this._confirmDelete = false;
    }

    /// Method to change edit state
    protected void EditStateChanged(bool dirty)
        => this.IsDirty = dirty;


    /// Method to change the Validation state
    protected void ValidStateChanged(bool valid)
        => this._isValid = valid;

    /// IDisposable Interface Implementation
    public void Dispose()
        => this.EditContext.OnFieldChanged -= FieldChanged;
```

Finally the button event handlers to control save and exiting a dirty form.

```csharp
    /// Method to handle EditForm submission
    protected async void HandleValidSubmit()
    {
        await this.Service.SaveRecordAsync();
        this.EditFormState.UpdateState();
        this._dirtyExit = false;
        await this.InvokeAsync(this.StateHasChanged);
    }

    /// Handler for Delete action
    protected void Delete()
    {
        if (!this._isNew)
            this._confirmDelete = true;
    }

    /// Handler for Delete confirmation
    protected async void ConfirmDelete()
    {
        if (this._confirmDelete)
        {
            await this.Service.DeleteRecordAsync();
            this.IsDirty = false;
            this.DoExit();
        }
    }

    /// Handler for a confirmed exit - i.e. dirty exit
    protected void ConfirmExit()
    {
        this.IsDirty = false;
        this.DoExit();
    }

    /// Handler to Exit the form, dependant on it context
    protected void DoExit(ModalResult result = null)
    {
        result = result ?? ModalResult.OK();
        if (this._isModal)
            this.Modal.Close(result);
        if (ExitAction.HasDelegate)
            ExitAction.InvokeAsync();
        else
            this.NavManager.NavigateTo("/");
    }
}
```

## Implementing Forms

### WeatherForecastViewerForm

The code for the `WeatherForecastViewerForm` is pretty simple.

1. Inherit from `RecordFormBase` and set `TRecord` as `WeatherForecast`.
2. Get the `WeatherForecastControllerService` and assign it to the base `Service` property.

```csharp
public partial class WeatherForecastViewerForm : RecordFormBase<WeatherForecast>
{

    [Inject] private WeatherForecastControllerService ControllerService { get; set; }

    protected async override Task OnInitializedAsync()
    {
        this.Service = this.ControllerService;
        await base.OnInitializedAsync();
    }
}
```

The majority of the work is in the Razor code.  
1. There's no Html code, it's all components.  We'll look at UI components in detail on the next article.  
2. The layout is based on Bootstrap grids.
3. Column size dictated control size.
4. `UILoader` only loads it's content when we have a record to display.

```html
@namespace Blazor.Database.Components
@inherits RecordFormBase<WeatherForecast>

<UIContainer>
    <UIFormRow>
        <UIColumn>
            <h2>Weather Forecast Viewer</h2>
        </UIColumn>
    </UIFormRow>
</UIContainer>
<UILoader Loaded="this.IsLoaded">
    <UIContainer>
        <UIFormRow>
            <UILabelColumn>
                Date
            </UILabelColumn>
            <UIInputColumn Cols="3">
                <InputReadOnlyText Value="@this.ControllerService.Record.Date.ToShortDateString()"></InputReadOnlyText>
            </UIInputColumn>
            <UIColumn Cols="7"></UIColumn>
        </UIFormRow>
        <UIFormRow>
            <UILabelColumn>
                Temperature &deg;C
            </UILabelColumn>
            <UIInputColumn Cols="2">
                <InputReadOnlyText Value="@this.ControllerService.Record.TemperatureC.ToString()"></InputReadOnlyText>
            </UIInputColumn>
            <UIColumn Cols="8"></UIColumn>
        </UIFormRow>
        <UIFormRow>
            <UILabelColumn>
                Temperature &deg;f
            </UILabelColumn>
            <UIInputColumn Cols="2">
                <InputReadOnlyText Value="@this.ControllerService.Record.TemperatureF.ToString()"></InputReadOnlyText>
            </UIInputColumn>
            <UIColumn Cols="8"></UIColumn>
        </UIFormRow>
        <UIFormRow>
            <UILabelColumn>
                Summary
            </UILabelColumn>
            <UIInputColumn Cols="9">
                <InputReadOnlyText Value="@this.ControllerService.Record.Summary"></InputReadOnlyText>
            </UIInputColumn>
        </UIFormRow>
    </UIContainer>
</UILoader>
<UIContainer>
    <UIFormRow>
        <UIButtonColumn>
            <UIButton AdditionalClasses="btn-secondary" ClickEvent="this.Exit">Exit</UIButton>
        </UIButtonColumn>
    </UIFormRow>
</UIContainer>
```

### WeatherForecastEditorForm

`WeatherForecastEditorForm` is similar to `WeatherForecastViewerForm`.

The code is again pretty simple.

1. Inherit from `EditRecordFormBase` and set `TRecord` as `WeatherForecast`.
2. Get the `WeatherForecastControllerService` and assign it to the base `Service` property.

```csharp
public partial class WeatherForecastViewerForm : RecordFormBase<WeatherForecast>
{

    [Inject] private WeatherForecastControllerService ControllerService { get; set; }

    protected async override Task OnInitializedAsync()
    {
        this.Service = this.ControllerService;
        await base.OnInitializedAsync();
    }
}
```
The Razor file is shown below.  It's based on the standard Blazor EditForm with some additional controls.  The same comments made on the Viewer apply here.  In addition:

1. `InlineDialog` is a form locking control.  It's enabled by the `_isInlineDirty` property.  Go to the demo site and edit a record to see it in action. It's only enabled when the form isn't in a modal context.
2. `EditFormState` is a control that tracks the form state, i.e. the state of the record against the original when the form was loaded.  It links with `InlineDialog` to control form locking.
3. `ValidationFormState` is a custom validation control.
4. The buttons are tied to the boolean control properties to manage their state.

The custom controls are covered in separate articles referenced in the Links section.  I haven't abstracted this further so you can see a full form in action.  You can move all the Title, Edit Form and button sections into a FormWrapper component. 

```html
@namespace Blazor.Database.Components
@inherits EditRecordFormBase<WeatherForecast>

<InlineDialog Lock="this._isInlineDirty" Transparent="false">
    <UIContainer>
        <UIFormRow>
            <UIColumn>
                <h2>Weather Forecast Editor</h2>
            </UIColumn>
        </UIFormRow>
    </UIContainer>
    <UILoader Loaded="this._isLoaded">
        <EditForm EditContext="this.EditContext" OnValidSubmit="HandleValidSubmit" class=" px-2 py-3">
            <EditFormState @ref="this.EditFormState"  EditStateChanged="this.EditStateChanged"></EditFormState>
            <ValidationFormState ValidStateChanged="this.ValidStateChanged"></ValidationFormState>
            <UIContainer>
                <UIFormRow>
                    <UILabelColumn>
                        Record ID
                    </UILabelColumn>
                    <UIInputColumn Cols="3">
                        <InputReadOnlyText Value="@this.Model.ID.ToString()" />
                    </UIInputColumn>
                    <UIColumn Cols="3"></UIColumn>
                    <UIValidationColumn>
                        <ValidationMessage For=@(() => this.Model.Date) />
                    </UIValidationColumn>
                </UIFormRow>
                <UIFormRow>
                    <UILabelColumn>
                        Date
                    </UILabelColumn>
                    <UIInputColumn Cols="3">
                        <InputDate class="form-control" @bind-Value="this.Model.Date"></InputDate>
                    </UIInputColumn>
                    <UIColumn Cols="3"></UIColumn>
                    <UIValidationColumn>
                        <ValidationMessage For=@(() => this.Model.Date) />
                    </UIValidationColumn>
                </UIFormRow>
                <UIFormRow>
                    <UILabelColumn>
                        Temperature &deg;C
                    </UILabelColumn>
                    <UIInputColumn Cols="2">
                        <InputNumber class="form-control" @bind-Value="this.Model.TemperatureC"></InputNumber>
                    </UIInputColumn>
                    <UIColumn Cols="4"></UIColumn>
                    <UIValidationColumn>
                        <ValidationMessage For=@(() => this.Model.TemperatureC) />
                    </UIValidationColumn>
                </UIFormRow>
                <UIFormRow>
                    <UILabelColumn>
                        Summary
                    </UILabelColumn>
                    <UIInputColumn>
                        <InputText class="form-control" @bind-Value="this.Model.Summary"></InputText>
                    </UIInputColumn>
                    <UIValidationColumn>
                        <ValidationMessage For=@(() => this.Model.Summary) />
                    </UIValidationColumn>
                </UIFormRow>
            </UIContainer>
            <UIContainer>
                <UIFormRow>
                    <UIButtonColumn>
                        <UIButton Show="true" Disabled="this._deleteDisabled" AdditionalClasses="btn-outline-danger" ClickEvent="() => Delete()">Delete</UIButton>
                        <UIButton Show="this._confirmDelete" AdditionalClasses="btn-danger" ClickEvent="() => this.ConfirmDelete()">Confirm Delete</UIButton>
                        <UIButton Show="true" Disabled="this._saveDisabled" Type="submit" AdditionalClasses="btn-success">@this._saveButtonText</UIButton>
                        <UIButton Show="this._dirtyExit" AdditionalClasses="btn-danger" ClickEvent="() => this.ConfirmExit()">Exit Without Saving</UIButton>
                        <UIButton Show="true" Disabled="this._dirtyExit" AdditionalClasses="btn-dark" ClickEvent="() => this.Exit()">Exit</UIButton>
                    </UIButtonColumn>
                </UIFormRow>
            </UIContainer>
        </EditForm>
    </UILoader>
</InlineDialog>
```

## RouteView Implementations

The RouteView implementation of the viewer is shown below.

1. Declares the `Route` with an ID `Parameter`.
2. Declares the form `WeatherForecastViewerForm`.
3. Passes the `ID` to the form and attaches a delegate to `ExitAction` which returns to the *fetchdata* view.

```html
// WeatherViewer.razor
@page "/weather/view/{ID:int}"

<WeatherForecastViewerForm ID="this.ID" ExitAction="this.ExitToList"></WeatherForecastViewerForm>

@code {
    [Parameter] public int ID { get; set; }

    [Inject] public NavigationManager NavManager { get; set; }

    private void ExitToList()
        => this.NavManager.NavigateTo("/fetchdata");

}
``` 
The editor is exactly the same, but declares the form `WeatherForecastEditorForm`.

```html
// WeatherEditor.razor
@page "/weather/edit/{ID:int}"

<WeatherForecastEditorForm ID="this.ID" ExitAction="this.ExitToList"></WeatherForecastEditorForm>

@code {

    [Inject] public NavigationManager NavManager { get; set; }

    [Parameter] public int ID { get; set; }

    private void ExitToList()
        => this.NavManager.NavigateTo("/fetchdata");
}
```

### Wrap Up
That wraps up this article.  We've shown how to build boilerplate code into base forms and how to implement viewer and editor forms.  We'll look in more detail at the list forms and how the viewer and editors are called in a separate article.   
Some key points to note:
1. The Blazor Server and Blazor WASM code is the same.
2. Almost all the functionality is implemented in library components.  Most of the application code is Razor markup for the individual record fields.
3. The Razor files contains controls, not HTML.
4. Async in used through.

If you're reading this article well into the future, check the readme in the repository for the latest version of the article set.

## History

* 19-Sep-2020: Initial version.
* 17-Nov-2020: Major Blazor.CEC library changes.  Change to ViewManager from Router and new Component base implementation.
* 29-Mar-2021: Major updates to Services, project structure and data editing.
