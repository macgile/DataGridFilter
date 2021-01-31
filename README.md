<!--
Edited
https://dillinger.io/
-->

# WPF Filterable DataGrid, multi language
![datagrid image demo](FilterDataGrid.png)


A custom DataGrid control that inherits from the base DataGrid control class and override some methods to implement filters for each column, like Excel.  
The translation of the displayed information is available in several languages.  

<font size="2" color="gray">
* The translations are from google translate, if you find any errors please let me know.
</font>

## How to use
 - Include the **FilterDataGrid** folder in your project   

 - Add **FilterDataGrid.xaml** to App.xaml as MergedDictionaries   
```xml
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
	    	<ResourceDictionary Source="FilterDataGrid/FilterDataGrid.xaml" />
		...
```  
   
 - Add **FilterDatagrid** into your xaml :   
 
      - **Namespace**  
		```xml 
		<Window xmlns:control="clr-namespace:FilterDataGrid" ...
		```
	  - **Control**   
		```xml 
		<control:FilterDataGrid FilterLanguage="English" DateFormatString="d" ShowStatusBar="True" ShowElapsedTime="False"
		...
		```   
		- Properties
		  - **ShowStatusBar** : *displays the status bar*, default : false
		  - **ShowElapsedTime** : *displays the elapsed time of filtering in status bar*, default : false
		  - **DateFormatString** : *date display format*, default : "d"
		  - **FilterLanguage** : *translation into available language*, default : English   
		*Languages available : English, French, Russian, German, Italian, Chinese*  
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
> Tested on the first column of the demo application using a random distinct name generator between 5 and 8 letters in length.  
> The elapsed time decreases according to the number of columns and the number of filtered elements.


Number of lines | Opening of the PopUp | Applying the filter | Total (PopUp + Filter)
 --- | --- | --- | ---
1000 | < 1 second | < 1 second | < 1 second 
100,000 | < 1 second | < 1 second | < 1 second 
500,000 | ± 1.5 second | < 1 second	| ± 2.5 seconds 
1 000 000 | ± 3 seconds	| ± 1.5 seconds	| ± 4.5 seconds 

