#region (c) 2022 Gilles Macabies All right reserved

// Solution   : FilterDataGrid
// Projet     : FilterDataGrid
// File       : Loc.cs
// Created    : 23/05/2022

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
// ReSharper disable UnusedMember.Global

namespace FilterDataGrid
{
    public enum Local
    {
        TraditionalChinese,
        SimplifiedChinese,
        Dutch,
        English,
        French,
        German,
        Italian,
        Polish,
        Russian,
        Spanish,
        Japanese
    }

    public class Loc
    {
        #region Private Fields

        private Local language;

        // culture name(used for dates)
        private static readonly Dictionary<Local, string> CultureNames = new Dictionary<Local, string>
        {
            { Local.TraditionalChinese, "zh-Hant" },
            { Local.SimplifiedChinese, "zh-Hans" },
            { Local.Dutch,   "nl-NL" },
            { Local.English, "en-US" },
            { Local.French,  "fr-FR" },
            { Local.German,  "de-DE" },
            { Local.Italian, "it-IT" },
            { Local.Polish,  "pl-PL" },
            { Local.Russian, "ru-RU" },
            { Local.Spanish, "es-ES" },
            { Local.Japanese, "ja-JP" }
        };

        /// <summary>
        /// Translation dictionary
        /// </summary>
        private static readonly Dictionary<string, Dictionary<Local, string>> Translation =
            new Dictionary<string, Dictionary<Local, string>>
            {
                {
                    "All", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "(全選)" },
                        { Local.SimplifiedChinese, "(全选)" },
                        { Local.Dutch,   "(Alles selecteren)" },
                        { Local.English, "(Select all)" },
                        { Local.French,  "(Sélectionner tout)" },
                        { Local.German,  "(Alle auswählen)" },
                        { Local.Italian, "(Seleziona tutto)" },
                        { Local.Polish, "(Zaznacz wszystkie)" },
                        { Local.Russian, "(Выбрать все)" },
                        { Local.Spanish, "(Seleccionar todos)" },
                        { Local.Japanese, "(すべて選択)" }
                    }
                },
                {
                    "Empty", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "(空白)" },
                        { Local.SimplifiedChinese, "(空白)" },
                        { Local.Dutch,   "(Leeg)" },
                        { Local.English, "(Blank)" },
                        { Local.French,  "(Vides)" },
                        { Local.German,  "(Leer)" },
                        { Local.Italian, "(Vuoto)" },
                        { Local.Polish, "(Pusty)" },
                        { Local.Russian, "(Заготовки)" },
                        { Local.Spanish, "(Vacio)"},
                        { Local.Japanese, "(空白)" }
                    }
                },
                {
                    "Clear", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "清除篩選 \"{0}\"" },
                        { Local.SimplifiedChinese, "清除过滤器 \"{0}\"" },
                        { Local.Dutch,   "Filter \"{0}\" verwijderen" },
                        { Local.English, "Clear filter \"{0}\"" },
                        { Local.French,  "Effacer le filtre \"{0}\"" },
                        { Local.German,  "Filter löschen \"{0}\"" },
                        { Local.Italian, "Cancella filtro \"{0}\"" },
                        { Local.Polish, "Wyczyść filtr \"{0}\"" },
                        { Local.Russian, "Очистить фильтр \"{0}\"" },
                        { Local.Spanish, "Limpiar filtros \"{0}\"" },
                        { Local.Japanese, "\"{0}\"からフィルターをクリア" }
                    }
                },

                {
                    "Contains", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "搜尋(包含)" },
                        { Local.SimplifiedChinese, "搜索(包含)" },
                        { Local.Dutch,   "Zoek (bevat)" },
                        { Local.English, "Search (contains)" },
                        { Local.French,  "Rechercher (contient)" },
                        { Local.German,  "Suche (enthält)" },
                        { Local.Italian, "Cerca (contiene)" },
                        { Local.Polish, "Szukaj (zawiera)" },
                        { Local.Russian, "Искать (содержит)" },
                        { Local.Spanish, "Buscar (contiene)" },
                        { Local.Japanese, "検索(含む)" }
                    }
                },

                {
                    "StartsWith", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "搜尋(字元開頭)" },
                        { Local.SimplifiedChinese, "搜索 (来自)" },
                        { Local.Dutch,   "Zoek (beginnen met)" },
                        { Local.English, "Search (startswith)" },
                        { Local.French,  "Rechercher (commence par)" },
                        { Local.German,  "Suche (beginnen mit)" },
                        { Local.Italian, "Cerca (inizia con)" },
                        { Local.Polish, "Szukaj (zaczyna się od)" },
                        { Local.Russian, "Искать (hачни с)" },
                        { Local.Spanish, "Buscar (comienza con)" },
                        { Local.Japanese, "検索 (で始まる)" }
                    }
                },

                {
                    "Toggle", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "切換包含/開始於" },
                        { Local.SimplifiedChinese, "切換包含/開始於" },
                        { Local.Dutch,   "Toggle bevat/begint met" },
                        { Local.English, "Toggle contains/startswith" },
                        { Local.French,  "Basculer contient/commence par" },
                        { Local.German,  "Toggle enthält/beginnt mit" },
                        { Local.Italian, "Toggle contiene/inizia con" },
                        { Local.Polish,  "Przełącz zawiera/zaczyna się od" },
                        { Local.Russian, "Переключить содержит/начинается с" },
                        { Local.Spanish, "Toggle contiene/comienza con" },
                        { Local.Japanese, "含む/で始まるの切り替え" }
                    }
                },

                {
                    "Ok", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "確定" },
                        { Local.SimplifiedChinese, "确定" },
                        { Local.Dutch,   "Ok" },
                        { Local.English, "Ok" },
                        { Local.French,  "Ok" },
                        { Local.German,  "Ok" },
                        { Local.Italian, "Ok" },
                        { Local.Polish,  "Ok" },
                        { Local.Russian, "Ok" },
                        { Local.Spanish, "Aceptar" },
                        { Local.Japanese, "确定" }
                    }
                },
                {
                    "Cancel", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "取消" },
                        { Local.SimplifiedChinese, "取消" },
                        { Local.Dutch,   "Annuleren" },
                        { Local.English, "Cancel" },
                        { Local.French,  "Annuler" },
                        { Local.German,  "Abbrechen" },
                        { Local.Italian, "Annulla" },
                        { Local.Polish,  "Anuluj" },
                        { Local.Russian, "Отмена" },
                        { Local.Spanish, "Cancelar" },
                        { Local.Japanese, "取り消し" }
                    }
                },
                {
                    "Status", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "{0:n0} 找到 {1:n0} 條記錄" },
                        { Local.SimplifiedChinese, "{0:n0} 找到了 {1:n0} 条记录" },
                        { Local.Dutch,   "{0:n0} rij(en) gevonden op {1:n0}" },
                        { Local.English, "{0:n0} record(s) found on {1:n0}" },
                        { Local.French,  "{0:n0} enregistrement(s) trouvé(s) sur {1:n0}" },
                        { Local.German,  "{0:n0} zeilen angezeigt von {1:n0}" },
                        { Local.Italian, "{0:n0} oggetti trovati su {1:n0}" },
                        { Local.Polish,  "{0:n0} rekord(y) znaleziony(e) w {1:n0}" },
                        { Local.Russian, "{0:n0} записей найдено на {1:n0}" },
                        { Local.Spanish, "{0:n0} registro(s) encontrado(s) de {1:n0}"},
                        { Local.Japanese, "{1:n0} レコード中 {0:n0}個が見つかりました " }
                    }
                },
                {
                    "ElapsedTime", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "經過時間{0:mm}:{0:ss}.{0:ff}" },
                        { Local.SimplifiedChinese, "经过时间{0:mm}:{0:ss}.{0:ff}" },
                        { Local.Dutch,   "Verstreken tijd {0:mm}:{0:ss}.{0:ff}" },
                        { Local.English, "Elapsed time {0:mm}:{0:ss}.{0:ff}" },
                        { Local.French,  "Temps écoulé {0:mm}:{0:ss}.{0:ff}" },
                        { Local.German,  "Verstrichene Zeit {0:mm}:{0:ss}.{0:ff}" },
                        { Local.Italian, "Tempo trascorso {0:mm}:{0:ss}.{0:ff}" },
                        { Local.Polish,  "Zajęło {0:mm}:{0:ss}.{0:ff}" },
                        { Local.Russian, "Пройденное время {0:mm}:{0:ss}.{0:ff}" },
                        { Local.Spanish, "Tiempo transurrido {0:mm}:{0:ss}.{0:ff}"},
                        { Local.Japanese, "経過時間{0:mm}:{0:ss}.{0:ff}" }
                    }
                },
                {
                    "True", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "已選中" },
                        { Local.SimplifiedChinese, "已選中" },
                        { Local.Dutch,   "Aangevinkt" },
                        { Local.English, "Checked" },
                        { Local.French,  "Coché" },
                        { Local.German,  "Ausgewählt" },
                        { Local.Italian, "Controllato" },
                        { Local.Polish,  "Zaznaczone" },
                        { Local.Russian, "Проверено" },
                        { Local.Spanish, "Comprobado" },
                        { Local.Japanese, "選択済み" },
                    }
                },
                {
                    "False", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "未選中" },
                        { Local.SimplifiedChinese, "未選中" },
                        { Local.Dutch,   "Niet aangevinkt" },
                        { Local.English, "Unchecked" },
                        { Local.French,  "Décoché" },
                        { Local.German,  "Nicht ausgewählt" },
                        { Local.Italian, "Deselezionato" },
                        { Local.Polish,  "Niezaznaczone" },
                        { Local.Russian, "непроверено" },
                        { Local.Spanish, "Sin marcar" },
                        { Local.Japanese, "未選択" }
                    }
                },
                {
                    "RemoveAll", new Dictionary<Local, string>
                    {
                        { Local.TraditionalChinese, "清除所有篩選" },
                        { Local.SimplifiedChinese, "删除所有过滤器" },
                        { Local.Dutch,   "Alle filters verwijderen" },
                        { Local.English, "Remove all filters" },
                        { Local.French,  "Supprimer tous les filtres" },
                        { Local.German,  "Alle Filter entfernen" },
                        { Local.Italian, "Rimuovi tutti i filtri" },
                        { Local.Polish,  "Usuń wszystkie filtry" },
                        { Local.Russian, "Удалить все фильтры" },
                        { Local.Spanish, "Eliminar todos los filtros" },
                        { Local.Japanese, "すべてのフィルターをクリア" },
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

        public string CultureName => CultureNames[Language];

        public string LanguageName => Enum.GetName(typeof(Local), Language);

        public string All => Translate("All");

        public string Cancel => Translate("Cancel");

        public string Clear => Translate("Clear");

        public string Contains => Translate("Contains");

        public string ElapsedTime => Translate("ElapsedTime");

        public string Empty => Translate("Empty");

        public string Ok => Translate("Ok");

        public string StartsWith => Translate("StartsWith");

        public string Status => Translate("Status");

        public string Toggle => Translate("Toggle");

        public string RemoveAll => Translate("RemoveAll");

        public string IsTrue => Translate("True");

        public string IsFalse => Translate("False");

        public string Neutral => "{0}";

        #endregion Public Properties

        #region Private Methods

        /// <summary>
        /// Translated into the language
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string Translate(string key)
        {
            return Translation.ContainsKey(key) && Translation[key].ContainsKey(Language)
                ? Translation[key][Language]
                : "unknow";
        }

        #endregion Private Methods
    }
}