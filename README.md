<!--
Edited
https://dillinger.io/
-->
![GitHub release (latest by date)](https://img.shields.io/github/v/release/macgile/DataGridFilter)

# WPF Filterable DataGrid, multi language
![datagrid image demo](FilterDataGrid.png)


A DataGrid control that inherits from the base DataGrid control class and override some methods to implement filters  
for each column like Excel, in automatic or custom generation.  

Translation of labels and formatting of dates in the following languages: English, French, Russian, German, Italian, Chinese, Dutch.

 > *The translations are from google translate, if you find any errors or want to add other languages, please let me know.*

The **Nuget package** is available [here](https://www.nuget.org/packages/FilterDataGrid/).

## How to use
 - There are two ways to install :
   + NuGet command : **Install-Package FilterDataGrid**
   + Or manually add **FilterDataGrid.dll** as reference in your project
   
 - Add **FilterDatagrid** into your xaml :   
 
      - **Namespace**  
		```xml 
		<Window xmlns:control="http://filterdatagrid.control.com/2021" ...
		```
	  - **Control**   
		```xml 
		<control:FilterDataGrid 
		 FilterLanguage="English" DateFormatString="d" ShowStatusBar="True" ShowElapsedTime="False" ...
		```   
		- Properties
		  - **ShowStatusBar** : *displays the status bar*, default : false
		  - **ShowElapsedTime** : *displays the elapsed time of filtering in status bar*, default : false
		  - **DateFormatString** : *date display format*, default : "d"
		  - **FilterLanguage** : *translation into available language*, default : English   

		>  

 	> *If you add custom columns, you must set **AutoGenerateColumns="False"*** 
		
	  - **Custom TextColumn**   
		```xml
		<control:FilterDataGrid.Columns>   
		    <control:DataGridTextColumn IsColumnFiltered="True" ...
		```
	  - **Custom TemplateColumn**  
	    > ***FieldName** property of **DataGridTemplateColumn** is required*   
	    
		```xml
		<control:FilterDataGrid.Columns>   
		    <control:DataGridTemplateColumn IsColumnFiltered="True"
			                            FieldName="LastName" ...
		```

## Benchmark ##

> Intel Core i7, 2.93 GHz, 16 GB, Windows 10, 64 bits.  
> Tested on the "Last name" column of the demo application using a random distinct name generator, between 5 and 8 letters in length.  
> *The elapsed time decreases based on the number of columns and filtered items.*


Number of rows | Opening of the PopUp | Applying the filter | Total (PopUp + Filter)
 --- | --- | --- | ---
10 000 | < 1 second | < 1 second | < 1 second 
100 000 | < 1 second | < 1 second | < 1 second 
500 000 | ± 1.5 second | ± 1 second	| ± 2.5 seconds 
1 000 000 | ± 3 seconds	| ± 1.5 seconds	| ± 4.5 seconds 

