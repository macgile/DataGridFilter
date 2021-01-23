
General use:

1) Include the FilterDataGrid folder in your project

2) Add "FilterableDatagrid.xaml" to App.xaml as MergedDictionaries
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="FilterDataGrid/FilterDataGrid.xaml" />

3) 3) Add FilterDataGrid control into your xaml :

    Namespace : 
        xmlns:control="clr-namespace:FilterDataGrid"

    Control : 
        <control:FilterDataGrid ...
        
        Available properties :
          (bool) ShowStatusBar      : Displays the status bar, default : false
          (bool) ShowElapsedTime    : Displays the elapsed time of filtering in status bar, default : false
          (string) DateFormatString : Date display format, default : "d"

        * If you add custom columns, you must set the property AutoGenerateColumns to "False"

        Custom colum :  
        <control:FilterDataGrid.Columns>
            <control:DataGridTextColumn ...

        * The property "FiledName" in DataGridTemplateColumn is mandatory
        Custom template colum :
        <control:FilterDataGrid.Columns>   
            <control:DataGridTemplateColumn IsColumnFiltered="True" FieldName="LastName" ...

4) You can change the language in the constructor of the "Loc.cs" file, default : English
	Language = (int)Local.English;
	Languages available : English, French, Russian, German, Italian, Chinese

