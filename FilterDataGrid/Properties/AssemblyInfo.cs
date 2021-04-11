using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

// Les informations générales relatives à un assembly dépendent de
// l'ensemble d'attributs suivant. Changez les valeurs de ces attributs pour modifier les informations
// associées à un assembly.
[assembly: AssemblyTitle("FilterDataGrid")]
[assembly: AssemblyDescription("Implements a filter on the columns of a datagrid")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Macabies Gilles")]
[assembly: AssemblyProduct("FilterDataGrid")]
[assembly: AssemblyCopyright("Copyright © 2021 Gilles Macabies")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// L'affectation de la valeur false à ComVisible rend les types invisibles dans cet assembly
// aux composants COM.  Si vous devez accéder à un type dans cet assembly à partir de
// COM, affectez la valeur true à l'attribut ComVisible sur ce type.
[assembly: ComVisible(false)]

//Pour commencer à générer des applications localisables, définissez
//<UICulture>CultureUtiliséePourCoder</UICulture> dans votre fichier .csproj
//dans <PropertyGroup>.  Par exemple, si vous utilisez le français
//dans vos fichiers sources, définissez <UICulture> à fr-FR. Puis, supprimez les marques de commentaire de
//l'attribut NeutralResourceLanguage ci-dessous. Mettez à jour "fr-FR" dans
//la ligne ci-après pour qu'elle corresponde au paramètre UICulture du fichier projet.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //où se trouvent les dictionnaires de ressources spécifiques à un thème
                                     //(utilisé si une ressource est introuvable dans la page,
                                     // ou dictionnaires de ressources de l'application)
    ResourceDictionaryLocation.SourceAssembly //où se trouve le dictionnaire de ressources générique
                                              //(utilisé si une ressource est introuvable dans la page,
                                              // dans l'application ou dans l'un des dictionnaires de ressources spécifiques à un thème)
)]

[assembly: XmlnsDefinition("http://filterdatagrid.control.com/2021", "FilterDataGrid", AssemblyName="FilterDataGrid")]


// Les informations de version pour un assembly se composent des quatre valeurs suivantes :
//
//      Version principale
//      Version secondaire
//      Numéro de build
//      Révision
//
// Vous pouvez spécifier toutes les valeurs ou indiquer les numéros de build et de révision par défaut
// en utilisant '*', comme indiqué ci-dessous :
// [assembly: AssemblyVersion("1.0.*")]

// see https://marinovdh.wordpress.com/2018/10/22/68/
// for the incremental version, modified in the project file, set true to false : <Deterministic>false</Deterministic>

[assembly: AssemblyVersion("1.2.3.0")]
[assembly: AssemblyFileVersion("1.2.3.0")]
[assembly: NeutralResourcesLanguage("")]
