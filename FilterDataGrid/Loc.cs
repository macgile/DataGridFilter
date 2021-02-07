// Author     : Gilles Macabies
// Solution   : DataGridFilter
// Projet     : DataGridFilter
// File       : Loc.cs
// Created    : 18/12/2019

using System;
using System.Collections.Generic;
using System.Globalization;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable CheckNamespace

namespace FilterDataGrid
{
    public enum Local
    {
        English = 0,
        French,
        Russian,
        German,
        Italian,
        Chinese
    }

    public class Loc
    {
        #region Private Fields

        private int language;

        #endregion Private Fields

        #region Private Fields

        // culture name(used for dates)
        private static readonly string[] CultureNames = { "en-US", "fr-FR", "ru-RU", "de-DE", "it-IT", "zh-Hans" };

        // RESPECT THE ORDER OF THE Local ENUMERATION
        // translation
        private static readonly Dictionary<string, string[]> Translation = new Dictionary<string, string[]>
        {
            {
                "All", new[]
                {
                    "(Select all)",
                    "(Sélectionner tout)",
                    "(Выбрать все)",
                    "(Alle auswählen)",
                    "(Seleziona tutto)",
                    "(全选)"
                }
            },
            {
                "Empty", new[]
                {
                    "(Blank)",
                    "(Vides)",
                    "(Заготовки)",
                    "(Leer)",
                    "(Vuoto)",
                    "(空白)",
                }
            },
            {
                "Clear", new[]
                {
                    "Clear filter \"{0}\"",
                    "Effacer le filtre \"{0}\"",
                    "Очистить фильтр \"{0}\"",
                    "Filter löschen \"{0}\"",
                    "Cancella filtro \"{0}\"",
                    "清除过滤器 \"{0}\"",
                }
            },
            {
                "Search", new[]
                {
                    "Search (contains)",
                    "Rechercher (contient)",
                    "Искать (содержит)",
                    "Suche (enthält)",
                    "Cerca (contiene)",
                    "搜索（包含)",
                }
            },
            {
                "Ok", new[]
                {
                    "Ok",
                    "Ok",
                    "Ok",
                    "Ok",
                    "Ok",
                    "好",
                }
            },
            {
                "Cancel", new[]
                {
                    "Cancel",
                    "Annuler",
                    "Отмена",
                    "Abbrechen",
                    "Annulla",
                    "撤消",
                }
            },
            {
                "Status", new[]
                {
                    "{0:n0} record(s) found on {1:n0}",
                    "{0:n0} enregistrement(s) trouvé(s) sur {1:n0}",
                    "{0:n0} записей найдено на {1:n0}",
                    "{0:n0} Zeilen angezeigt von {1:n0}",
                    "{0:n0} record trovati su {1:n0}",
                    "{0:n0} 找到了 {1:n0} 条记录",
                }
            },
            {
                "ElapsedTime", new[]
                {
                    "Elapsed time {0:mm}:{0:ss}.{0:ff}",
                    "Temps écoulé {0:mm}:{0:ss}.{0:ff}",
                    "Пройденное время {0:mm}:{0:ss}.{0:ff}",
                    "Verstrichene Zeit {0:mm}:{0:ss}.{0:ff}",
                    "Tempo trascorso {0:mm}:{0:ss}.{0:ff}",
                    "经过时间{0:mm}:{0:ss}.{0:ff}",
                }
            }
        };

        #endregion Private Fields

        #region Constructors

        public Loc()
        {
            Language = (int)Local.English;
        }

        #endregion Constructors

        #region Public Properties

        public string All => Translation["All"][Language];

        public string Cancel => Translation["Cancel"][Language];

        public string Clear => Translation["Clear"][Language];

        public CultureInfo Culture { get; set; }

        public string CultureName => CultureNames[Language];

        public string ElapsedTime => Translation["ElapsedTime"][Language];

        public string Empty => Translation["Empty"][Language];

        public int Language
        {
            get => language;
            set
            {
                language = value;
                Culture = new CultureInfo(CultureNames[Language]);
            }
        }

        public string LanguageName => Enum.GetName(typeof(Local), Language);
        public string Ok => Translation["Ok"][Language];
        public string Search => Translation["Search"][Language];
        public string Status => Translation["Status"][Language];

        #endregion Public Properties
    }
}
