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

    public static class Loc
    {
        #region Private Fields

        private static readonly int Language;

        // RESPECT THE ORDER OF THE Local ENUMERATION

        // culture name(used for dates)
        private static readonly string[] CultureNames = { "en-US", "fr-FR", "ru-RU", "de-DE", "it-IT", "zh-Hans" };

        // translation
        private static readonly Dictionary<string, string[]> Translation = new Dictionary<string, string[]>
        {
            {
                "All", new[]
                {
                    "(Select all)",
                    "(Sélectionner tout)",
                    "(Выбрать все)",
                    "(Wählen Sie Alle)",
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
                    "(Rohlinge)",
                    "(Vuoto)",
                    "(空白)",
                }
            },
            {
                "Clear",
                new[]
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
                    "搜索（包含）",
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
                    "Stornieren",
                    "Annulla",
                    "撤消",
                }
            },
            {
                "Status", new[]
                {
                    "{0:### ### ###} record(s) found on {1:### ### ###}",
                    "{0:### ### ###} enregistrement(s) trouvé(s) sur {1:### ### ###}",
                    "{0:### ### ###} записей найдено на {1:### ### ###}",
                    "{0:### ### ###} datensatz(e) gefunden am {1:### ### ###}",
                    "{0:### ### ###} record trovati su {1:### ### ###}",
                    "{0:### ### ###}找到了{1:### ### ###}条记录",
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

        static Loc()
        {
            // change language here
            Language = (int)Local.English;
            Culture = new CultureInfo(CultureNames[Language]);
        }

        #endregion Constructors

        #region Public Properties

        public static CultureInfo Culture { get; }
        public static string CultureName => CultureNames[Language];
        public static string LanguageName => Enum.GetName(typeof(Local), Language);

        public static string All => Translation["All"][Language];
        public static string Cancel => Translation["Cancel"][Language];
        public static string Ok => Translation["Ok"][Language];
        public static string Clear => Translation["Clear"][Language];
        public static string Empty => Translation["Empty"][Language];
        public static string Search => Translation["Search"][Language];
        public static string Status => Translation["Status"][Language];
        public static string ElapsedTime => Translation["ElapsedTime"][Language];

        #endregion Public Properties
    }
}