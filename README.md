# WPF Filterable DataGrid, multi language

<!--
![GitHub release (latest by date)](https://img.shields.io/github/v/release/macgile/DataGridFilter)
-->

![Nuget package](https://img.shields.io/nuget/v/FilterDataGrid)
![GitHub release (latest by date)](https://img.shields.io/github/v/release/macgile/DataGridFilter?include_prereleases)
[![MIT license](https://img.shields.io/badge/License-MIT-blue.svg)](https://lbesson.mit-license.org/)
[![Net Version](https://img.shields.io/badge/net%20version-net4.7.2%20netcore3.1%20net5.0%20net6.0%20net7.0-blue)](https://shields.io)

![datagrid image demo](https://raw.githubusercontent.com/macgile/DataGridFilter/master/FilterDataGrid.png)  

A DataGrid control that inherits from the base DataGrid control class and override some methods to implement filters  
for each column like Excel, in automatic or custom generation.  

Translation of labels and formatting of dates in the following languages: **Chinese(traditional and simplified), Dutch, English, French, German, Hungarian, Italian, Japanese, Polish, Russian, Spanish, Turkish.**

 > *The translations are from google translate, if you find any errors or want to add other languages, please let me know.*

The **Nuget package** is available [here](https://www.nuget.org/packages/FilterDataGrid/).

To understand how the filter works, you can consult the article posted on [CodeProject](https://www.codeproject.com/Articles/5292782/WPF-DataGrid-Filterable-multi-language).  

> **For operation closer to that of Excel, see the version available in the repository [FilterDataGrid-Beta](https://github.com/macgile/FilterDataGrid-Beta).  
This version uses a new filtering method that is completely different from the current version or the one commonly used to filter data (as far as I know).**  

:warning: **Attention in version 1.2.6 you must add the global style so that the FilterDataGrid is displayed correctly, this is a bug fixed in the current version.**  

```xml
<Style x:Key="FilterDatagridStyle" 
BasedOn="{StaticResource {ComponentResourceKey TypeInTargetAssembly=control:FilterDataGrid, 
ResourceId=FilterDataGridStyle}}" 
TargetType="{x:Type control:FilterDataGrid}">
</Style>
```

## How to use

- There are two ways to install :

- NuGet command : **Install-Package FilterDataGrid**

- Or manually add **FilterDataGrid.dll** as reference in your project

- Add **Namespace** into your xaml :  

```xml
    <Window xmlns:control="http://filterdatagrid.control.com/2021" ..
```

- **Control**  

```xml
  <control:FilterDataGrid 
   FilterLanguage="English" DateFormatString="d" ShowStatusBar="True" ShowElapsedTime="False"
   ExcludeFields="lastname,age,manager" ...
```

- Properties
  - **ShowStatusBar** : *displays the status bar*, default : false  
  - **ShowElapsedTime** : *displays the elapsed time of filtering in status bar*, default : false  
  - **ShowRowsCount** : *display the number of rows*, default : false  
  *- If the value passed to **RowHeaderWidth** is greater than the calculation of the width of the column, this is the one that is used.*  
  - **FilterLanguage** : *translation into available language*, default : English  
  - **ExcludeFields** : *comma separated fields to exclude from filter, only works in AutoGenerateColumns mode*  
  - **DateFormatString** : *date display format*, default : "d"  

  > :warning: **Before version 1.2.5.2**, you must set the "Time" part of the DateTime fields to zero, otherwise the filter doesn't work.  
[see the documentation "Standard date and time format strings"](https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-date-and-time-format-strings)

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

## Global Style

>You can define a global style which overrides the default style of "FilterDataGrid"  
:warning: **The ComponentResourceKey is obsolete from version 1.2.7**   
~~BasedOn="{StaticResource {ComponentResourceKey TypeInTargetAssembly=control:FilterDataGrid,
        ResourceId=FilterDataGridStyle}}"~~

```xml
<Style
            x:Key="FilterDatagridStyle" TargetType="{x:Type control:FilterDataGrid}">
            <Setter Property="Margin" Value="10" />
            <Setter Property="RowHeaderWidth" Value="40" />
     ...
</Style>
```

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


<!-- Made with [contributors-img](https://contrib.rocks). -->
<a href="https://github.com/macgile/DataGridFilter/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=macgile/DataGridFilter" />
</a>
