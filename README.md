# WPF Filterable DataGrid, multi language

<!--
![GitHub release (latest by date)](https://img.shields.io/github/v/release/macgile/DataGridFilter)
https://github.com/ikatyang/emoji-cheat-sheet/tree/master?tab=readme-ov-file#other-symbol
-->

![Nuget package](https://img.shields.io/nuget/v/FilterDataGrid)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/macgile/DataGridFilter?include_prereleases)
[![MIT license](https://img.shields.io/badge/License-MIT-blue.svg)](https://lbesson.mit-license.org/)
[![Net Version](https://img.shields.io/badge/net%20version-net4.8%20netcore3.1%20net5.0%20net6.0%20net7.0-blue)](https://shields.io)

<!-- FilterDataGrid.png -->
![datagrid image demo](https://raw.githubusercontent.com/macgile/DataGridFilter/master/filterdatagrid.png)  

A DataGrid control that inherits from the base DataGrid control class and override some methods to implement filters  
for each column like Excel, in automatic or custom generation.  

Support for nested objects and filter persistence.  

Translation of labels and formatting of dates in the following languages: **Czech, Chinese(traditional and simplified), Dutch, English, French, German, Hebrew, Hungarian, Italian, Japanese, Polish, Portuguese, Russian, Spanish, Turkish, Ukrainian.**

 > *The translations are from google translate, if you find any errors or want to add other languages, please let me know.*

The **Nuget package** is available [here](https://www.nuget.org/packages/FilterDataGrid/).

To understand how the filter works, you can consult the article posted on [CodeProject](https://www.codeproject.com/Articles/5292782/WPF-DataGrid-Filterable-multi-language).  

## How to use

- There are two ways to install :

- NuGet command : **Install-Package FilterDataGrid**

- Or manually add **FilterDataGrid.dll** as reference in your project

- Add **Namespace** :

  ```xml
      xmlns:control="http://filterdatagrid.control.com/2021"  
  ```  
  or  
  ```xml
      xmlns:control="clr-namespace:FilterDataGrid;assembly=FilterDataGrid"  
  ```
- **Control**  

  ```xml
    <control:FilterDataGrid 
    FilterLanguage="English" DateFormatString="d" ShowStatusBar="True" ShowElapsedTime="False"
    ExcludeFields="lastname,age,manager" ...
  ```
- **Properties**

  - **DateFormatString** : *date display format*, default : "d"  
    > warning: **Before version 1.2.5.2**, you must set the "Time" part of the DateTime fields to zero, otherwise the filter doesn't work.
  [see the documentation](https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings)

  - **ExcludeColumns** :new: : *name of fields to exclude from displayed columns, separated by commas, only works in AutoGenerateColumns mode*  
  - **ExcludeFields** : *name of fields separated by commas to exclude from the filter, only works in AutoGenerateColumns mode*  

  - **FilterLanguage** : *translation into available language*, default : English  

  - **PersistentFilter** : *enable/disable filter persistence* , default : false  

    > [See below the detail of this feature](#persistence-of-filters)
  - **ShowElapsedTime** : *displays the elapsed time of filtering in status bar*, default : false  

  - **ShowRowsCount** : *display the number of rows*, default : false  
    > *If the value passed to **RowHeaderWidth** is greater than the calculation of the width of the column, this is the one that is used.*  

  - **ShowStatusBar** : *displays the status bar*, default : false  
  - **FilterPopupBackground** : set a custom background color for the filter popup, default : Background color of host window.  
    > *If **FilterPopupBackground** is not set, the background color of the host window is used.*  

- **Custom TextColumn / CheckBoxColumn**
     > *If you add custom columns, you must set **AutoGenerateColumns="False"***

    ```xml
    <control:FilterDataGrid.Columns>   
        <control:DataGridTextColumn     IsColumnFiltered="True" ... />
        <control:DataGridCheckBoxColumn IsColumnFiltered="True" ... />
    ```

- **Custom TemplateColumn**  
     > :warning: ***FieldName** property of **DataGridTemplateColumn** is required*

  ```xml
  <control:FilterDataGrid.Columns>   
      <control:DataGridTemplateColumn IsColumnFiltered="True"
                                      FieldName="LastName" ... />  
  ```
  
- **Custom ComboBoxColumn**  :new:
    > :warning: ***SelectedItemBinding** or ***SelectedValueBinding*** property is required.    
    The **SelectedValuePath** property is not required in case **ItemsSource** is an enumeration.  
    In the case where the field is a nested object, enter the entire path of this field, e.g: **Country.Name***
    >
    > 
    ***Demo application** contains several types of combo-box implementation, for greater clarity  
    some are commented but completely operational.*  

  ```xml
  <control:FilterDataGrid.Columns>   
      <control:DataGridComboBoxColumn IsColumnFiltered="True"
      DisplayMemberPath="<name of the field to display the value (from the datagrid list)>"
      ItemsSource="{Binding <external list or enum>}"
      SelectedValueBinding="{Binding <unique identifier (from the datagrid list)>}"  
      SelectedValuePath="<unique identifier of external list>"
      ... />  
  ```

## Global Style

> You can define a global style which overrides the default style of "FilterDataGrid"  
:warning: **The ComponentResourceKey is obsolete from version 1.2.7**
~~BasedOn="{StaticResource {ComponentResourceKey TypeInTargetAssembly=control:FilterDataGrid,
        ResourceId=FilterDataGridStyle}}"~~

```xml
 <Application.Resources>
  <Style
            x:Key="FilterDatagridStyle" TargetType="{x:Type control:FilterDataGrid}">
            <Setter Property="Margin" Value="10" />
            <Setter Property="RowHeaderWidth" Value="40" />
            ...
  </Style>
 </Application.Resources>

  <!-- usage -->
  <control:FilterDataGrid Style="{StaticResource FilterDatagridStyle}" ...
```
## Persistence of filters

>Enabling the **PersistentFilter** property saves a json file in the application folder that contains the status of active filters, any modification of the filters is immediately saved until the closing of the application, it is this state of the filters which will be loaded during the new launch of the application.  
The name of the json file is inferred from the name you give to your FilterDataGrid control.

<!-- >:warning: This feature don't work with ComboBox Column. -->

```xml
 <control:FilterDataGrid x:Name="MyFilterDatagrid"
```

>If it is not provided, the type name of the source collection is used, for example for a generic list of type Employees, **Employees** will be used  

```csharp
// collection used as ItemsSource
List<Employees>
```

>Two methods (LoadPreset/SavePreset) are exposed to be able to manually manage loading and saving from the host application.  
These methods are independent of the **PersistentFilter** property whatever its state.

```csharp
// Load Preset
MyFilterDatagrid.LoadPreset();

// Save Preset
MyFilterDatagrid.SavePreset();
```

>Be aware that the filters must remain consistent with your source collection, for example if you filter on a "True" Boolean field and when the application is launched your collection no longer contains the "True" value for this field, the filter application may cause unexpected results.  
This is especially true for demo applications that use random data generation.

## Benchmark

> Intel Core i7, 2.93 GHz, 16 GB, Windows 10, 64 bits.  
> Tested on the "Last name" column of the demo application using a random distinct name generator, between 5 and 8 letters in length.  
> *The elapsed time decreases based on the number of columns and filtered items.*

Number of rows | Opening of the PopUp | Applying the filter | Total (PopUp + Filter)
 --- | --- | --- | ---
10 000 | < 1 second | < 1 second | < 1 second
100 000 | < 1 second | < 1 second | < 1 second
500 000 | ± 1.5 second | ± 1 second | ± 2.5 seconds
1 000 000 | ± 3 seconds | ± 1.5 seconds | ± 4.5 seconds

## Demonstration

![datagrid image demo](https://raw.githubusercontent.com/macgile/DataGridFilter/master/capture.gif)  

## Contributors

<a href="https://github.com/Apflkuacha" target="_blank">
  <img src="https://images.weserv.nl/?url=avatars.githubusercontent.com/u/31316050?v=4&h=64&w=64&fit=cover&mask=circle&maxage=7d" />
</a>
<a href="https://github.com/wordiboi" target="_blank">
  <img src="https://images.weserv.nl/?url=avatars.githubusercontent.com/u/15075279?v=4&h=64&w=64&fit=cover&mask=circle&maxage=7d" />
</a>
<a href="https://github.com/ottosson" target="_blank">
  <img src="https://images.weserv.nl/?url=avatars.githubusercontent.com/u/3355320?v=4&h=64&w=64&fit=cover&mask=circle&maxage=7d" />
</a>
<a href="https://github.com/livep2000" target="_blank">
  <img src="https://images.weserv.nl/?url=avatars.githubusercontent.com/u/2779309?v=4&h=64&w=64&fit=cover&mask=circle&maxage=7d" />
</a>
<a href="https://github.com/mcboothy" target="_blank">
  <img src="https://images.weserv.nl/?url=avatars.githubusercontent.com/u/7164916?v=4&h=64&w=64&fit=cover&mask=circle&maxage=7d" />
</a>

<!-- Made with [contributors-img](https://contrib.rocks). -->
<a href="https://github.com/macgile/DataGridFilter/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=macgile/DataGridFilter" />
</a>
