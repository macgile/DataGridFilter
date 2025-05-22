using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

[assembly: AssemblyTitle("FilterDataGrid")]
[assembly: AssemblyDescription("A DataGrid control that inherits from the base DataGrid control class and override some methods to\r\nImplement filters for each column like Excel, in automatic or custom generation.\r\nTranslation of labels and formatting of dates in the available languages : Chinese, Dutch, English, French, German, Italian, Russian.")]
[assembly: AssemblyCompany("Macabies Gilles")]
[assembly: AssemblyCopyright("Copyright © 2021 Gilles Macabies")]
[assembly: ComVisible(false)]


#if NET48
[assembly: AssemblyProduct("FilterDataGrid Net48-windows")]
#elif NETCOREAPP3_1
[assembly: AssemblyProduct("FilterDataGrid NetCoreApp3.1")]
#elif NET5_0
[assembly: AssemblyProduct("FilterDataGrid Net5.0-windows")]
#elif NET6_0
[assembly: AssemblyProduct("FilterDataGrid Net6.0-windows")]
#elif NET7_0
[assembly: AssemblyProduct("FilterDataGrid Net7.0-windows")]
#elif NET8_0
[assembly: AssemblyProduct("FilterDataGrid Net8.0-windows")]
#elif NET9_0
[assembly: AssemblyProduct("FilterDataGrid Net9.0-windows")]
#else
[assembly: AssemblyProduct("FilterDataGrid")]
#endif


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, 
    ResourceDictionaryLocation.SourceAssembly 
)]

[assembly: XmlnsDefinition("http://filterdatagrid.control.com/2021", "FilterDataGrid", AssemblyName="FilterDataGrid")]

[assembly: Guid("20c6b7ba-9949-43a4-ada5-047c10ccd899")]
[assembly: AssemblyVersion("1.2.9.0")]
[assembly: AssemblyFileVersion("1.2.9.0")]
