// Author : Gilles Macabies Solution : DataGridFilter Projet : DataGridFilter File : Loc.cs Created : 18/12/2019

using System;
using System.Collections.Generic;
using System.Globalization;

namespace FilterDataGrid
{
    public enum Local
    {
        Chinese,
        Dutch,
        English,
        French,
        German,
        Italian,
        Russian,
    }

    public class Loc
    {
        #region Private Fields

        private Local language;

        // culture name(used for dates)
        private static readonly Dictionary<Local, string> CultureNames = new Dictionary<Local, string>
        {
            { Local.Chinese, "zh-Hans" },
            { Local.Dutch,   "nl-NL" },
            { Local.English, "en-US" },
            { Local.French,  "fr-FR" },
            { Local.German,  "de-DE" },
            { Local.Italian, "it-IT" },
            { Local.Russian, "ru-RU" },
        };

        // Translation
        private static readonly Dictionary<string, Dictionary<Local, string>> Translation =
            new Dictionary<string, Dictionary<Local, string>>
            {
                {
                    "All", new Dictionary<Local, string>
                    {
                        { Local.Chinese, "(全选)" },
                        { Local.Dutch,   "(Alles selecteren)" },
                        { Local.English, "(Select all)" },
                        { Local.French,  "(Sélectionner tout)" },
                        { Local.German,  "(Alle auswählen)" },
                        { Local.Italian, "(Seleziona tutto)" },
                        { Local.Russian, "(Выбрать все)" },
                    }
                },
                {
                    "Empty", new Dictionary<Local, string>
                    {
                        { Local.Chinese, "(空白)" },
                        { Local.Dutch,   "(Leeg)" },
                        { Local.English, "(Blank)" },
                        { Local.French,  "(Vides)" },
                        { Local.German,  "(Leer)" },
                        { Local.Italian, "(Vuoto)" },
                        { Local.Russian, "(Заготовки)" },
                    }
                },
                {
                    "Clear", new Dictionary<Local, string>
                    {
                        { Local.Chinese, "清除过滤器 \"{0}\"" },
                        { Local.Dutch,   "Filter \"{0}\" verwijderen" },
                        { Local.English, "Clear filter \"{0}\"" },
                        { Local.French,  "Effacer le filtre \"{0}\"" },
                        { Local.German,  "Filter löschen \"{0}\"" },
                        { Local.Italian, "Cancella filtro \"{0}\"" },
                        { Local.Russian, "Очистить фильтр \"{0}\"" },
                    }
                },

                {
                    "Search", new Dictionary<Local, string>
                    {
                        { Local.Chinese, "搜索(包含)" },
                        { Local.Dutch,   "Zoek (bevat)" },
                        { Local.English, "Search (contains)" },
                        { Local.French,  "Rechercher (contient)" },
                        { Local.German,  "Suche (enthält)" },
                        { Local.Italian, "Cerca (contiene)" },
                        { Local.Russian, "Искать (содержит)" },
                    }
                },


                {
                    "Contains", new Dictionary<Local, string>
                    {
                        { Local.Chinese, "搜索(包含)" },
                        { Local.Dutch,   "Zoek (bevat)" },
                        { Local.English, "Search (contains)" },
                        { Local.French,  "Rechercher (contient)" },
                        { Local.German,  "Suche (enthält)" },
                        { Local.Italian, "Cerca (contiene)" },
                        { Local.Russian, "Искать (содержит)" },
                    }
                },

                {
                    "StartsWith", new Dictionary<Local, string>
                    {
                        { Local.Chinese, "搜索 (来自)" },
                        { Local.Dutch,   "Zoek (beginnen met)" },
                        { Local.English, "Search (startswith)" },
                        { Local.French,  "Rechercher (commence par)" },
                        { Local.German,   "Suche (beginnen mit)" },
                        { Local.Italian, "Cerca (iniziare con)" },
                        { Local.Russian, "Искать (hачни с)" },
                    }
                },

                {
                    "Ok", new Dictionary<Local, string>
                    {
                        { Local.Chinese, "确定" },
                        { Local.Dutch,   "Ok" },
                        { Local.English, "Ok" },
                        { Local.French,  "Ok" },
                        { Local.German,  "Ok" },
                        { Local.Italian, "Ok" },
                        { Local.Russian, "Ok" },
                    }
                },
                {
                    "Cancel", new Dictionary<Local, string>
                    {
                        { Local.Chinese, "取消" },
                        { Local.Dutch,   "Annuleren" },
                        { Local.English, "Cancel" },
                        { Local.French,  "Annuler" },
                        { Local.German,  "Abbrechen" },
                        { Local.Italian, "Annulla" },
                        { Local.Russian, "Отмена" },
                    }
                },
                {
                    "Status", new Dictionary<Local, string>
                    {
                        { Local.Chinese, "{0:n0} 找到了 {1:n0} 条记录" },
                        { Local.Dutch,   "{0:n0} rij(en) gevonden op {1:n0}" },
                        { Local.English, "{0:n0} record(s) found on {1:n0}" },
                        { Local.French,  "{0:n0} enregistrement(s) trouvé(s) sur {1:n0}" },
                        { Local.German,  "{0:n0} zeilen angezeigt von {1:n0}" },
                        { Local.Italian, "{0:n0} record trovati su {1:n0}" },
                        { Local.Russian, "{0:n0} записей найдено на {1:n0}" },
                    }
                },
                {
                    "ElapsedTime", new Dictionary<Local, string>
                    {
                        { Local.Chinese, "经过时间{0:mm}:{0:ss}.{0:ff}" },
                        { Local.Dutch,   "Verstreken tijd {0:mm}:{0:ss}.{0:ff}" },
                        { Local.English, "Elapsed time {0:mm}:{0:ss}.{0:ff}" },
                        { Local.French,  "Temps écoulé {0:mm}:{0:ss}.{0:ff}" },
                        { Local.German,  "Verstrichene Zeit {0:mm}:{0:ss}.{0:ff}" },
                        { Local.Italian, "Tempo trascorso {0:mm}:{0:ss}.{0:ff}" },
                        { Local.Russian, "Пройденное время {0:mm}:{0:ss}.{0:ff}" },
                    }
                }
            };

        #endregion Private Fields

        #region Constructors

        public Loc()
        {
            Language = Local.English;
        }

        #endregion Constructors

        #region Public Properties

        public Local Language
        {
            get => language;
            set
            {
                language = value;
                Culture = new CultureInfo(CultureNames[value]);
            }
        }

        public CultureInfo Culture { get; private set; }

        public string All => Translation["All"][Language];

        public string Cancel => Translation["Cancel"][Language];

        public string Clear => Translation["Clear"][Language];

        public string Contains => Translation["Contains"][Language];

        public string CultureName => CultureNames[Language];

        public string ElapsedTime => Translation["ElapsedTime"][Language];

        public string Empty => Translation["Empty"][Language];

        public string LanguageName => Enum.GetName(typeof(Local), Language);

        public string Ok => Translation["Ok"][Language];

        public string Search => Translation["Search"][Language];

        public string StartsWith => Translation["StartsWith"][Language];

        public string Status => Translation["Status"][Language];

        #endregion Public Properties
    }
}