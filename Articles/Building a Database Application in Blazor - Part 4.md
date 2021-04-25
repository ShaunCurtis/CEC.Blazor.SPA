---
title: Part 4 - UI Components
oneliner: This article describes how to build UI Components.
precis: This article looks at the components we use in the UI and then focuses on how to build generic UI Components from HTML and CSS.
date: 2020-10-03
published: 2020-10-03
---

# Building a Database Application in Blazor 
## Part 4 - UI Components

## Introduction

This article is the fourth in a series on Building Blazor Database Applications.  This article looks at the components we use in the UI and then focuses on how to build generic UI Components from HTML and CSS.

1. Project Structure and Framework.
2. Services - Building the CRUD Data Layers.
3. View Components - CRUD Edit and View Operations in the UI.
4. UI Components - Building HTML/CSS Controls.
5. View Components - CRUD List Operations in the UI.

## Repository and Database

The repository for the articles has move to [CEC.Blazor.SPA Repository](https://github.com/ShaunCurtis/CEC.Blazor.SPA).  [CEC.Blazor GitHub Repository](https://github.com/ShaunCurtis/CEC.Blazor) is obselete and will be removed.

There's a SQL script in /SQL in the repository for building the database.

[You can see the Server and WASM versions of the project running here on the same site](https://cec-blazor-server.azurewebsites.net/).

### Components

For a detailed look at components read my article [A Dive into Blazor Components](https://www.codeproject.com/Articles/5277618/A-Dive-into-Blazor-Components).

Everything in the Blazor UI, other than the start page, is a component.  Yes App, Router,... they're all components.  Not all components emit Html.

You can divide components into four categories:
1. RouteViews - these are the top level components.  Views are combined with a Layout to make the display window.
2. Layouts - Layouts combine with Views to make up the display window.
3. Forms - Forms are logical collections of controls.  Edit forms, display forms, list forms, data entry wizards are all classic forms.  Forms contain controls - not HTML.
4. Controls - Controls either display something - emit HTML - or do some unit of work.  Text boxes, dropdowns, buttons, grids are all classic Hrtml emitting controls. App, Router, Validation are controls that do units of work.

### RouteViews

RouteViews are application specific, the only difference between a RouteView and a Form is a RouteView declares one or more routes through the `@Page` directive.  The `Router` component declared in the root `App` sets the `AppAssembly` to a secific code assembly.  This is the assembly that `Router` trawls though on startup to find all the declared routes.

In the application RouteViews are decalred in the WASM application library. 

The Weather Forecast Viewer and List Views are shown below.

```csharp
// Blazor.Database/RouteViews/Weather/WeatherViewer.cs
@page "/weather/view/{ID:int}"

<WeatherForecastViewerForm ID="this.ID" ExitAction="this.ExitToList"></WeatherForecastViewerForm>

@code {
    [Parameter] public int ID { get; set; }

    [Inject] public NavigationManager NavManager { get; set; }

    private void ExitToList()
        => this.NavManager.NavigateTo("/fetchdata");
}
```


```csharp
// Blazor.Database/RouteViews/Weather/FetchData.cs
@page "/fetchdata"

<WeatherForecastComponent></WeatherForecastComponent>
```

### Forms

We saw Forms in the last article.  They're specific to the application.

The code below shows the Weather Viewer.  It's all UI Controls, no HTML markup.

```html
// Blazor.Database/Components/Forms/WeatherForecastViewerForm.razor
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

The code behind page is relatively simple - the complexity is in the boilerplate code in the parent classes.  It loads the record specific Controller service.

```csharp
// Blazor.Database/Components/Forms/WeatherForecastViewerForm.razor.cs
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

## UI Controls

UI Controls emit HTML and CSS markup.  All the controls here are based on the Bootstrap CSS Framework.  All controls inherit from `ComponentBase` and UI Controls inherit from `UIBase`.

#### UIBase

`UIBase` inherits from `Component`.  It builds an HTML DIV block that you can turn on or off.

Lets look at some of `UIBase` in detail.

The HTML block tag can be set using the `Tag` parameter.  It can only be set by inherited classes.

```csharp
protected virtual string HtmlTag => "div";
```

The control Css class is built using a `CssBuilder` class.  Inheriting classes can set a primary Css value and add as many secondary values they wish.  Add on CSS classes can be added either through the `AdditionalClasses` parameter or by defining a `class` attribute.

```csharp
[Parameter] public virtual string AdditionalClasses { get; set; } = string.Empty;
protected virtual string PrimaryClass => string.Empty;
protected List<string> SecondaryClass { get; private set; } = new List<string>();

protected string CssClass
=> CSSBuilder.Class(this.PrimaryClass)
    .AddClass(SecondaryClass)
    .AddClass(AdditionalClasses)
    .AddClassFromAttributes(this.UserAttributes)
    .Build();
```

The control can be hidden or disabled with two parameters.  When `Show` is true `ChildContent` is displayed.  When `Show` is false `HideContent` is displayed if it isn't `null`, otherwise nothing is displayed.

```csharp
[Parameter] public bool Show { get; set; } = true;
[Parameter] public bool Disabled { get; set; } = false;
[Parameter] public RenderFragment ChildContent { get; set; }
[Parameter] public RenderFragment HideContent { get; set; }
```


Finally the control captures any additional attributes and adds them to the markup element.

```csharp
[Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object> UserAttributes { get; set; } = new Dictionary<string, object>();
```

The control builds the `RenderTree` in code.

```csharp
protected override void BuildRenderTree(RenderTreeBuilder builder)
{
    if (this.Show)
    {
        builder.OpenElement(0, this.HtmlTag);
        if (!string.IsNullOrWhiteSpace(this.CssClass)) builder.AddAttribute(1, "class", this.CssClass);
        if (Disabled) builder.AddAttribute(2, "disabled");
        builder.AddMultipleAttributes(3, this.UserAttributes);
        if (this.ChildContent != null) builder.AddContent(4, ChildContent);
        else if (this.HideContent != null) builder.AddContent(5, HideContent);
        builder.CloseElement();
    }
}
```

### Some Examples

The rest of the article looks at a few of the UI controls in more detail.

#### UIButton

This is a standard Bootstrap Button. 
1. `Type` sets the button type.
2. `PrimaryClass` is set.
3. `ButtonClick` handles the button click event and calls the EventCallback.
5. `Show` and `Disabled` handle button state.

```csharp
// Blazor.SPA/Components/UIComponents/Base/UIButtons.cs
@namespace Blazor.SPA.Components
@inherits UIBase
@if (this.Show)
{
    <button class="@this.CssClass" @onclick="ButtonClick" type="@Type" disabled="@this.Disabled" @attributes="UserAttributes">
        @this.ChildContent
    </button>
}
@code {
    [Parameter] public string Type { get; set; } = "button";
    [Parameter] public EventCallback<MouseEventArgs> ClickEvent { get; set; }
    protected override string PrimaryClass => "btn mr-1";
    protected async Task ButtonClick(MouseEventArgs e) => await this.ClickEvent.InvokeAsync(e);
}
```

Here's some code showing the control in use.

```html
<UIButton Show="true" Disabled="this._dirtyExit" AdditionalClasses="btn-dark" ClickEvent="() => this.Exit()">Exit</UIButton>
```

#### UIColumn

This is a standard Bootstrap Column. 
1. `Cols` defines the number of columns
2. `PrimaryCss` is built from`Cols`.
3. `Base` `RenderTreeBuilder` builds out the control as a *div*. 

```csharp
// Blazor.SPA/Components/UIControls/Base/UIColumn.cs
public class UIColumn : UIBase
{
    [Parameter] public virtual int Cols { get; set; } = 0;
    protected override string PrimaryClass => this.Cols > 0 ? $"col-{this.Cols}" : $"col";
}
```

#### UILoader

This is a wrapper control designed to save implementing error checking in child content. It only renders it's child content when `IsLoaded` is true. The control saves implementing a lot of error checking in the child content.

```csharp
@namespace Blazor.SPA.Components
@inherits UIBase

@if (this.Loaded)
{
    @this.ChildContent
}
else
{
    <div>Loading....</div>
}

@code {
    [Parameter] public bool Loaded { get; set; }
}
```

You can see the control in use in the Edit and View forms.

#### UIContainer/UIRow/UIColumn

These controls create the BootStrap grid system - i.e. container, row and column - by building out DIVs with the correct Css.

```csharp
public class UIContainer : UIBase
{
    protected override string PrimaryClass => "container-fluid";
}
```

```csharp
class UIRow : UIBase
{
    protected override string PrimaryClass => "row";
}
```

```csharp
public class UIColumn : UIBase
{
    [Parameter] public virtual int Cols { get; set; } = 0;
    protected override string PrimaryClass => this.Cols > 0 ? $"col-{this.Cols}" : $"col";
}
```

```csharp
// CEC.Blazor/Components/UIControls/UIBootstrapContainer/UILabelColumn.cs
public class UILabelColumn : UIColumn
{
    protected override string _BaseCss => $"col-{Columns} col-form-label";
}
```

Here's some code showing the controls in use.

```html
<UIContainer>
    <UIRow>
        <UILabelColumn Columns="2">
            Date
        </UILabelColumn>
        ............
    </UIRow>
..........
</UIContainer>
```

## Wrap Up
This article provides an overview on how to build UI Controls with components, and examines some example components in detail.  You can see all the library UIControls in the GitHub Repository

Some key points to note:
1. UI Controls let you abstract markup from higher level components such as Forms and Views.
2. UI Controls give you control and applies some discipline over the HTML and CSS markup.
3. View and Form components are much cleaner and easier to view.
4. Use as little or as much abstraction as you wish.
5. Controls, such as `UILoader`, just make life easier!

If you're reading this article well into the future, check the readme in the repository for the latest version of the article set.

## History

* 21-Sep-2020: Initial version.
* 17-Nov-2020: Major Blazor.CEC library changes.  Change to ViewManager from Router and new Component base implementation.
* 29-Mar-2021: Major updates to Services, project structure and data editing.

