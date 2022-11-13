using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;


[assembly: AssemblyTitle("FilterDataGrid")]
[assembly: AssemblyDescription("A DataGrid control that inherits from the base DataGrid control class and override some methods to\r\nImplement filters for each column like Excel, in automatic or custom generation.\r\nTranslation of labels and formatting of dates in the available languages : SimplifiedChinese, Dutch, English, French, German, Italian, Russian.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Macabies Gilles")]

#if NET472
[assembly: AssemblyProduct("FilterDataGrid .NET Framework 4.7.2")]
#elif NET6_0
[assembly: AssemblyProduct("FilterDataGrid .NET 6.0")]
#elif NET5_0
[assembly: AssemblyProduct("FilterDataGrid .NET 5.0")]

#elif NETCOREAPP3_1
[assembly: AssemblyProduct("FilterDataGrid .NET Core 3.1")]
#else
[assembly: AssemblyProduct("FilterDataGrid unknow")]
#endif

[assembly: AssemblyCopyright("Copyright © 2021 Gilles Macabies")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, 
    ResourceDictionaryLocation.SourceAssembly 
)]

[assembly: XmlnsDefinition("http://filterdatagrid.control.com/2021", "FilterDataGrid", AssemblyName="FilterDataGrid")]


[assembly: AssemblyVersion("1.2.7.0")]
[assembly: AssemblyFileVersion("1.2.7.0")]
[assembly: NeutralResourcesLanguage("")]
