
General use:

1) Add FilterDataGrid.dll as reference in your project 
   or install NuGet package : Install-Package FilterDataGrid

2) Add FilterDataGrid control into your xaml :

    Namespace : 
        xmlns:control="http://filterdatagrid.control.com/2021"
        
    Control : 
        <control:FilterDataGrid ShowStatusBar="False" ShowElapsedTime="False" 
                                DateFormatString="d"  FilterLanguage="English"
                                ...
        
        - Properties :
            ShowStatusBar      : Displays the status bar, default : false
            ShowElapsedTime    : Displays the elapsed time of filtering in status bar, default : false
            DateFormatString   : Date display format, default : "d"
            FilterLanguage     : Translation into available language, default : English
                                 Languages available : English, French, Russian, German, Italian, Chinese

          * If you add custom columns, you must set the property AutoGenerateColumns to "False"

        - Custom TextColumn :  
            <control:FilterDataGrid.Columns>
                <control:DataGridTextColumn IsColumnFiltered="True" ...

          * FieldName property of TemplateColumn is required
        - Custom TemplateColumn :
            <control:FilterDataGrid.Columns>   
                <control:DataGridTemplateColumn IsColumnFiltered="True" FieldName="LastName" ...
