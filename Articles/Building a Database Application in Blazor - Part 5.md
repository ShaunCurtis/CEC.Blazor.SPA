---
title: Part 5 - View Components - CRUD List Operations in the UI
oneliner: This article describes how to build the View Components.
precis: This article looks in detail at building reusable List Presentation Layer components and deploying them in both Server and WASM projects.
date: 2020-10-05
published: 2020-10-03
---

# Building a Database Application in Blazor 
## Part 5 - View Components - CRUD List Operations in the UI

## Introduction

This article is the fifth in a series on Building Blazor Database Applications. The articles so far are:

1. Project Structure and Framework.
2. Services - Building the CRUD Data Layers.
3. View Components - CRUD Edit and View Operations in the UI.
4. UI Components - Building HTML/CSS Controls.
5. View Components - CRUD List Operations in the UI.

This article looks in detail at building reusable List UI components and deploying them in both Server and WASM projects.

## Repository and Database

The repository for the articles has moved to [Blazor.Database Repository](https://github.com/ShaunCurtis/Blazor.Database).  The older repositories are now obselete and will be removed soon.

There's a SQL script in /SQL in the repository for building the database.

[You can see the Server and WASM versions of the project running here on the same site](https://cec-blazor-database.azurewebsites.net/).

## List Functionality

List components present more challenges than other CRUD components.  Functionality expected in a production level list control includes:
* Paging -  to handle large data sets
* Column formatting - to control column width and data overflow
* Sorting - on columns
* Filtering - not covered here.


## The Base Forms

`ListFormBase` is the base abstract form for all lists. It inherits from `ComponentBase`, and contains all the boilerplate code.  `TRecord` is the dataclass it operates on.  The form uses the 

The code is shown below
```csharp
    public abstract class ListFormBase<TRecord> : ComponentBase, IDisposable where TRecord : class, IDbRecord<TRecord>, new()
    {
        /// Callbacks for Edit/View/New/Exit Actions
        [Parameter] public EventCallback<int> EditRecord { get; set; }
        [Parameter] public EventCallback<int> ViewRecord { get; set; }
        [Parameter] public EventCallback<int> NewRecord { get; set; }
        [Parameter] public EventCallback ExitAction { get; set; }

        /// Controller Data Service
        [Inject] protected IFactoryControllerService<TRecord> Service { get; set; }
        [Inject] protected NavigationManager NavManager { get; set; }

        /// Booleans for Service and Recordlist state
        protected bool IsLoaded => this.Service?.HasRecords ?? false;
        protected bool HasService => this.Service != null;

        protected override async Task OnInitializedAsync()
        {
            if (this.HasService)
            {
                await this.Service.GetRecordsAsync();
                this.Service.ListHasChanged += OnListChanged;
            }
        }

        /// Call StatehasChanged if list changed
        protected void OnListChanged(object sender, EventArgs e)
            => this.InvokeAsync(this.StateHasChanged);

        /// Event handlers to call EventCallbacks
        protected virtual void Edit(int id)
            => this.EditRecord.InvokeAsync(id);
        protected virtual void View(int id)
            => this.ViewRecord.InvokeAsync(id);
        protected virtual void New()
            => this.NewRecord.InvokeAsync();
        protected virtual void Exit()
        {
            if (ExitAction.HasDelegate)
                ExitAction.InvokeAsync();
            else
                this.NavManager.NavigateTo("/");
        }

        /// IDisosable Interface implementation
        public void Dispose()
            => this.Service.ListHasChanged -= OnListChanged;
    }
```

### Paging and Sorting

Paging and sorting is implemented by a `Paginator` class that resides in the ControllerService.  There are UI components that interact with the `Paginator`: `PaginatorControl` and `SortControl`.

You can see `PaginatorControl` in use in a list form - here in the left side of a button row at the bottom of the form

```csharp
<UIContainer>
    <UIFormRow>
        <UIColumn Cols="8">
            <PaginatorControl Paginator="this.Service.Paginator"></PaginatorControl>
        </UIColumn>
        <UIButtonColumn Cols="4">
            <UIButton Show="true" AdditionalClasses="btn-success" ClickEvent="() => this.New()">New Record</UIButton>
            <UIButton AdditionalClasses="btn-secondary" ClickEvent="this.Exit">Exit</UIButton>
        </UIButtonColumn>
    </UIFormRow>
</UIContainer>
```

And `SortControl` in action in the header row of a list form.
  
```csharp
<head>
    <SortControl Paginator="this.Service.Paginator">
        <UIDataTableHeaderColumn SortField="ID">ID</UIDataTableHeaderColumn>
        <UIDataTableHeaderColumn SortField="Date">Date</UIDataTableHeaderColumn>
        ...
    </SortControl>
</head>
```

#### Paginator

The Controller Service holds the `Paginator` instance used by list forms.  The code is self explanatory, providing the functionality for paging operations.  It's passed to the Data Service to retrieve the correct sorted page through the `PaginatorData` class.

```csharp
public class Paginator
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int BlockSize { get; set; } = 10;
    public int RecordCount { get; set; } = 0;
    public event EventHandler PageChanged;
    public string SortColumn
    {
        get => (!string.IsNullOrWhiteSpace(_sortColumn)) ? _sortColumn : DefaultSortColumn;
        set => _sortColumn = value;
    }
    private string _sortColumn = string.Empty;
    public string DefaultSortColumn { get; set; } = "ID";
    public bool SortDescending { get; set; }

    public int LastPage => (int)((RecordCount / PageSize) + 0.5);
    public int LastBlock => (int)((LastPage / BlockSize) + 1.5);
    public int CurrentBlock => (int)((Page / BlockSize) + 1.5);
    public int StartBlockPage => ((CurrentBlock - 1) * BlockSize) + 1;
    public int EndBlockPage => StartBlockPage + BlockSize;
    public bool HasBlocks => ((RecordCount / (PageSize * BlockSize)) + 0.5) > 1;
    public bool HasPagination => (RecordCount / PageSize) > 1;


    public Paginator(int pageSize, int blockSize)
    {
        this.BlockSize = blockSize;
        this.PageSize = pageSize;
    }

    public void ToPage(int page, bool forceUpdate = false)
    {
        if ((forceUpdate | !this.Page.Equals(page)) && page > 0)
        {
            this.Page = page;
            this.PageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void NextPage()
        => this.ToPage(this.Page + 1);

    public void PreviousPage()
                => this.ToPage(this.Page - 1);
    public void ToStart()
        => this.ToPage(1);

    public void ToEnd()
        => this.ToPage((int)((RecordCount / PageSize) + 0.5));

    public void NextBlock()
    {
        if (CurrentBlock != LastBlock)
        {
            var calcpage = (CurrentBlock * PageSize * BlockSize) + 1;
            this.Page = calcpage > LastPage ? LastPage : LastPage;
            this.PageChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public void PreviousBlock()
    {
        if (CurrentBlock != 1)
        {
            this.Page = ((CurrentBlock - 1) * PageSize * BlockSize) - 1;
            this.PageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void NotifySortingChanged()
        => this.ToPage(1, true);

    public PaginatorData GetData => new PaginatorData()
    {
        Page = this.Page,
        PageSize = this.PageSize,
        BlockSize = this.BlockSize,
        RecordCount = this.RecordCount,
        SortColumn = this.SortColumn,
        SortDescending = this.SortDescending
    };
}
```

#### PaginatorData

This is the class used to pass data into the dat services.  This has to be passed via json though the api so "keep it simple"/

```csharp
    public class PaginatorData
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int BlockSize { get; set; } = 10;
        public int RecordCount { get; set; } = 0;
        public string SortColumn { get; set; } = string.Empty;
        public bool SortDescending { get; set; } = false;
    }
```

#### PaginatorControl

The code again is self-explanatory, building out a Bootstrap ButtonGroup.  I've kept away from using icons, you can if you wish.

```html
@namespace Blazor.SPA.Components

@if (this.hasPaginator)
{
    <nav aria-label="...">
        <ul class="pagination">
            <li class="page-item">
                <a class="page-link" @onclick="() => this.Paginator.ToStart()">&vert;&lt;</a>
            </li>
            @if (this.Paginator.HasBlocks)
            {
                <li class="page-item">
                    <a class="page-link" @onclick="() => this.Paginator.PreviousBlock()">&lt;&lt;</a>
                </li>
            }
            @for (var i = this.Paginator.StartBlockPage; i < this.Paginator.EndBlockPage; i++)
            {
                var pageNo = i;
                @if (pageNo > this.Paginator.LastPage) break;
                @if (pageNo == this.Paginator.Page)
                {
                    <li class="page-item active">
                        <span class="page-link">
                            @pageNo
                            <span class="sr-only">(current)</span>
                        </span>
                    </li>
                }
                else
                {
                    <li class="page-item">
                        <a class="page-link" @onclick="() => this.Paginator.ToPage(pageNo)">@pageNo</a>
                    </li>
                }
            }
            @if (this.Paginator.HasBlocks)
            {
                <li class="page-item">
                    <a class="page-link" @onclick="() => this.Paginator.NextBlock()">&gt;&gt;</a>
                </li>
            }
            <li class="page-item">
                <a class="page-link" @onclick="() => this.Paginator.ToEnd()">&gt;&vert;</a>
            </li>
        </ul>
    </nav>
}

@code {
    [Parameter] public Paginator Paginator { get; set; }
    private bool hasPaginator => this.Paginator != null && this.Paginator.HasPagination;
}
```

#### SortControl

The `SortControl` is used in a list header. It cascades itself and provides the interface into the Paginator for the header columns through a set of public helper methods.

```csharp
@namespace Blazor.SPA.Components

<CascadingValue Value="this">
    @ChildContent
</CascadingValue>

@code {
    [Parameter] public RenderFragment ChildContent { get; set; }
    [Parameter] public string NotSortedClass { get; set; } = "sort-column oi oi-resize-height";
    [Parameter] public string AscendingClass { get; set; } = "sort-column oi oi-sort-ascending";
    [Parameter] public string DescendingClass { get; set; } = "sort-column oi oi-sort-descending";
    [Parameter] public EventCallback<SortingEventArgs> Sort { get; set; }
    [Parameter] public Paginator Paginator { get; set; }
    public string SortColumm { get; private set; } = string.Empty;
    public bool Descending { get; private set; } = false;

    public string GetIcon(string columnName)
        => !this.SortColumm.Equals(columnName)
        ? this.NotSortedClass
        : this.Descending
            ? this.AscendingClass
            : this.DescendingClass;

    public void NotifySortingChanged(string sortColumn, bool descending = false)
    {
        this.SortColumm = sortColumn;
        this.Descending = descending;
        this.Notify();
    }

    public void NotifySortingDirectionChanged()
    {
        this.Descending = !this.Descending;
        this.Notify();
    }

    private void Notify()
    {
        if (Paginator != null)
            {
            Paginator.SortDescending = this.Descending;
            Paginator.SortColumn = this.SortColumm;
            Paginator.NotifySortingChanged();
            }
        var args = SortingEventArgs.Get(this.SortColumm, this.Descending);
        if (Sort.HasDelegate) this.Sort.InvokeAsync(args);
    }
}
```

#### UIDataTableHeaderColumn


This is the UI control that builds out each header column in a list.  It builds out the razor and Css class for the header and notifies the captured  SortControl on any mouse click events.
```csharp
@namespace Blazor.SPA.Components

@if (_isSortField)
{
    <th class="@this.CssClass" @attributes="UserAttributes" @onclick="SortClick">
        <span class="@_iconclass"></span>
        @this.ChildContent
    </th>
}
else
{
    <th class="@this.CssClass" @attributes="UserAttributes">
        @this.ChildContent
    </th>
}

@code {

    [CascadingParameter] public SortControl SortControl { get; set; }
    [Parameter] public RenderFragment ChildContent { get; set; }
    [Parameter] public string SortField { get; set; } = string.Empty;
    [Parameter(CaptureUnmatchedValues = true)] public IDictionary<string, object> UserAttributes { get; set; } = new Dictionary<string, object>();
    private bool _hasSortControl => this.SortControl != null;
    private bool _isSortField => !string.IsNullOrWhiteSpace(this.SortField);
    private string _iconclass => _hasSortControl && _isSortField ? this.SortControl.GetIcon(SortField) : string.Empty;

    private string CssClass => CSSBuilder.Class("grid-col")
        .AddClass("cursor-hand", _isSortField)
        .AddClassFromAttributes(this.UserAttributes)
        .Build();

    private void SortClick(MouseEventArgs e)
    {
        if (this.SortControl.SortColumm.Equals(this.SortField))
            this.SortControl.NotifySortingDirectionChanged();
        else
            this.SortControl.NotifySortingChanged(this.SortField);
    }
}
```


### Weather Forecast List Forms

There are three list forms in the solution.  They demonstrate different UI approaches.

1. The classic web page approach using different RouteViews (Pages) for the record viewer and editor.
2. The modal dialog approach - opening and closing modal dialogs within the list RouteView.
3. The inline dialog approach - opening and closing a section within the RouteView to display/edit the record.

The standard `WeatherForecastListForm` looks like this.  It inherits from `ListFormBase` with `WeatherForecast` as `TRecord`.  It assigns the `WeatherForecastControllerService` to the base `IFactoryControllerService` property `Service`.  Note it has a component Css file defining the custom Css used in the component.

```csharp
// Blazor.Database/Components/Forms/WeatherForecast/WeatherForecastListForm.razor.cs
public partial class WeatherForecastListForm : ListFormBase<WeatherForecast>
{
    [Inject] private WeatherForecastControllerService ControllerService { get; set; }
    [Parameter] public bool IsModal {get; set;}
    private BaseModalDialog Modal { get; set; }

    protected override async Task OnInitializedAsync()
    {
        this.Service = this.ControllerService;
        await base.OnInitializedAsync();
    }

    protected override async void Edit(int id)
    {
        if (this.IsModal)
        {
            var options = new ModalOptions();
            options.Set("Id", id);
            await this.Modal.ShowAsync<WeatherForecastEditorForm>(options);
        }
        else
            base.Edit(id);
    }
    protected override async void View(int id)
    {
        if (this.IsModal)
        {
            var options = new ModalOptions();
            options.Set("Id", id);
            await this.Modal.ShowAsync<WeatherForecastViewerForm>(options);
        }
        else
            base.View(id);
    }

    protected override async void New()
    {
        if (this.IsModal)
        {
            var options = new ModalOptions();
            options.Set("Id", -1);
            await this.Modal.ShowAsync<WeatherForecastEditorForm>(options);
        }
        else
            base.New();
    }
}
```

The razor markup.  Note:
1. The `SortControl` in the header and the `UIDataTableHeaderColumn` components building the header with the sortable columns.
2. The `PaginatorControl` in the botton button row linked to the `Service.Paginator`.  Paging is event driven.  `PaginatorControl` paging requests are handled directly by `Paginator` in the controller service.  Updates trigger a `ListChanged` event in the service which triggers a UI update in the List Form.
3. The `BaseModalDialog` added if the Form is using Modal Dialogs.

```html
@namespace Blazor.Database.Components
@inherits ListFormBase<WeatherForecast>

<h1>Weather Forecasts</h1>

<UILoader Loaded="this.IsLoaded">
    <UIDataTable TRecord="WeatherForecast" Records="this.ControllerService.Records" class="table">
        <Head>
            <SortControl Paginator="this.Service.Paginator">
                <UIDataTableHeaderColumn SortField="ID">ID</UIDataTableHeaderColumn>
                <UIDataTableHeaderColumn SortField="Date">Date</UIDataTableHeaderColumn>
                <UIDataTableHeaderColumn SortField="TemperatureC">Temp. (C)</UIDataTableHeaderColumn>
                <UIDataTableHeaderColumn>Temp. (F)</UIDataTableHeaderColumn>
                <UIDataTableHeaderColumn SortField="Summary">Summary</UIDataTableHeaderColumn>
                <UIDataTableHeaderColumn class="max-column">Description</UIDataTableHeaderColumn>
                <UIDataTableHeaderColumn class="text-right">Actions</UIDataTableHeaderColumn>
            </SortControl>
        </Head>
        <RowTemplate>
            <UIDataTableRow>
                <UIDataTableColumn>@context.ID</UIDataTableColumn>
                <UIDataTableColumn> @context.Date.ToShortDateString()</UIDataTableColumn>
                <UIDataTableColumn>@context.TemperatureC</UIDataTableColumn>
                <UIDataTableColumn>@context.TemperatureF</UIDataTableColumn>
                <UIDataTableColumn>@context.Summary</UIDataTableColumn>
                <UIDataTableMaxColumn>@context.Description</UIDataTableMaxColumn>
                <UIDataTableColumn class="text-right text-nowrap">
                    <UIButton AdditionalClasses="btn-sm btn-secondary" ClickEvent="() => this.View(context.ID)">View</UIButton>
                    <UIButton AdditionalClasses="btn-sm btn-primary" ClickEvent="() => this.Edit(context.ID)">Edit</UIButton>
                </UIDataTableColumn>
            </UIDataTableRow>
        </RowTemplate>
    </UIDataTable>
    <UIContainer>
        <UIFormRow>
            <UIColumn Cols="8">
                <PaginatorControl Paginator="this.ControllerService.Paginator"></PaginatorControl>
            </UIColumn>
            <UIButtonColumn Cols="4">
                <UIButton Show="true" AdditionalClasses="btn-success" ClickEvent="() => this.New()">New Record</UIButton>
                <UIButton AdditionalClasses="btn-secondary" ClickEvent="this.Exit">Exit</UIButton>
            </UIButtonColumn>
        </UIFormRow>
    </UIContainer>
</UILoader>
@if (this.IsModal)
{
    <BaseModalDialog @ref="this.Modal"></BaseModalDialog>
}
```

### The Views

The application declares a set of intermediate Views for the list forms.  These are common to both the WASM and Server SPAs

#### WeatherForecastComponent

This is the multi RouteView implementation.  Event handlers are hooked up `WeatherForecastListForm` to route to the different RouteViews through the `NavigationManager`.

```html
@namespace Blazor.Database.Components

<WeatherForecastListForm EditRecord="this.GoToEditor" ViewRecord="this.GoToViewer" NewRecord="this.GoToNew"></WeatherForecastListForm>

@code {

    [Inject] NavigationManager NavManager { get; set; }

    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }

    public void GoToEditor(int id)
    => this.NavManager.NavigateTo($"/weather/edit/{id}");

    public void GoToNew()
    => this.NavManager.NavigateTo($"/weather/edit/-1");

    public void GoToViewer(int id)
    => this.NavManager.NavigateTo($"/weather/view/{id}");

}
```
The modal implementation is simple.  It already handles editor/viewer state by enabling `IsModal`.  You don't really need it as you could declare `WeatherForecastListForm` directly in the RouteView.

```html
@namespace Blazor.Database.Components

<WeatherForecastListForm IsModal="true"></WeatherForecastListForm>
```

The inline dialog is the most complex.  It uses Ids to show/hide the Editor/Viewer through `UIBase`.

```html
@namespace Blazor.Database.Components

<UIBase Show="this.ShowEditor">
    <WeatherForecastEditorForm ID="this.editorId" ExitAction="this.CloseDialog"></WeatherForecastEditorForm>
</UIBase>
<UIBase Show="this.ShowViewer">
    <WeatherForecastViewerForm ID="this.editorId" ExitAction="this.CloseDialog"></WeatherForecastViewerForm>
</UIBase>

<WeatherForecastListForm EditRecord="this.GoToEditor" ViewRecord="this.GoToViewer" NewRecord="this.GoToNew" ExitAction="Exit"></WeatherForecastListForm>

```
```csharp
@code {

    [Inject] NavigationManager NavManager { get; set; }

    private int editorId = 0;
    private int viewerId = 0;

    private bool ShowViewer => this.viewerId != 0;
    private bool ShowEditor => this.editorId != 0;

    public void GoToEditor(int id)
        => SetIds(id, 0);

    public void GoToNew()
        => SetIds(-1, 0);

    public void GoToViewer(int id)
        => SetIds(0, id);

    public void CloseDialog()
        => SetIds(0, 0);

    public void Exit()
        => this.NavManager.NavigateTo("/");

    private void SetIds(int editorId, int viewerId)
    {
        this.editorId = editorId;
        this.viewerId = viewerId;
    }
}
```

### The RouteViews (aka Pages)

These simply declare routes and the top level form component.

 - Blazor.Database.WASM/RouteViews/Weather/xxx.razor
 - Blazor.Database.Server/RouteViews/Weather/xxx.razor

```html
@page "/fetchdata"
<WeatherForecastComponent></WeatherForecastComponent>
```

```html
@page "/fetchdataInline"
<WeatherForecastInlineComponent></WeatherForecastInlineComponent>
```

```html
@page "/fetchdataModal"
<WeatherForecastListModal></WeatherForecastListModal>
```


## Wrap Up
That wraps up this article.  Some key points to note:
1. There's no differences between the Blazor Server and Blazor WASM code base.
2. 90% plus functionality is implemented in the library components as boilerplate generic code.  Most of the application code is Razor markup for the individual record forms.
3. Async functionality is used throughout.

If you're reading this article well into the future, check the readme in the repository for the latest version of the article set.
    
## History

* 25-Sep-2020: Initial version.
* 17-Nov-2020: Major Blazor.CEC library changes.  Change to ViewManager from Router and new Component base implementation.
* 31-Mar-2021: Major updates to Services, project structure and data editing.
