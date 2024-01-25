#region (c) 2022 Gilles Macabies All right reserved

// Solution   : FilterDataGrid
// Projet     : FilterDataGrid
// File       : Loc.cs
// Created    : 23/05/2022

#endregion

#region CONTRIBUTORS

// Turkish              : BEDIRHANSAMSA
// Japanese             : iBadaMorae
// Italian              : aetasoul
// Dutch                : Tenera
// Simplified Chinese   : SWH998
// Traditional Chinese  : BeCare4
// Russian              : anyousib
// Hugarian             : dankovics.jozsef
// Hebrew               : abaye123

// The simplification of the translation is achieved by dankovics.jozsef
// TranslatableElements, ILanguageDictionary, LanguageDictionary

// Improved translations and number format : Apflkuacha

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Local
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
        Hebrew,
        Hungarian,
        Italian,
        Japanese,
        Polish,
        Russian,
        Spanish,
        Turkish
    }

    // Contributor : dankovics.jozsef
    public enum TranslatableElements {

        /// <summary>
        ///     (Select all)
        /// </summary>
        All,

        /// <summary>
        ///     (Blank)
        /// </summary>
        Empty,

        /// <summary>
        ///     Clear filter
        /// </summary>
        Clear,

        /// <summary>
        ///     Search (contains)
        /// </summary>
        Contains,

        /// <summary>
        ///     Search (starts with)
        /// </summary>
        StartsWith,

        /// <summary>
        ///     Toggle contains/starts with
        /// </summary>
        Toggle,

        /// <summary>
        ///     Ok
        /// </summary>
        Ok,

        /// <summary>
        ///     Cancel
        /// </summary>
        Cancel,

        /// <summary>
        ///     {0:n0} record(s) found on {1:n0}
        /// </summary>
        Status,

        /// <summary>
        ///     Elapsed time {0:mm}:{0:ss}.{0:ff}
        /// </summary>
        ElapsedTime,

        /// <summary>
        ///     Checked
        /// </summary>
        True,

        /// <summary>
        ///     Unchecked
        /// </summary>
        False,

        /// <summary>
        ///     Remove all filters
        /// </summary>
        RemoveAll,

        /// <summary>
        ///         Indeterminate
        /// </summary>
        Indeterminate,
    }

    // Contributor : dankovics.jozsef
    public interface ILanguageDictionary
    {
        #region Public Properties

        CultureInfo Culture { get; }
        Dictionary<TranslatableElements, string> Dictionary { get; }
        string Language { get; }

        #endregion Public Properties
    }

    public class Loc
    {
        #region Private Fields

        private Local language;

        #endregion Private Fields

        #region Public Constructors

        public Loc()
        {
            language = Local.English;
            SelectedLanguage = English;
        }

        #endregion Public Constructors

        #region Public Properties

        public CultureInfo Culture => SelectedLanguage.Culture;
        public string DisplayName => SelectedLanguage.Culture.DisplayName;
        public string EnglishName => SelectedLanguage.Language;

        public Local Language {
            get => language;
            set {
                language = value;

                var type = typeof(Loc);
                var propertyInfo =
                    type.GetProperty(Enum.GetName(typeof(Local), language) ?? throw new InvalidOperationException("Language is required"),
                        BindingFlags.Static | BindingFlags.NonPublic);

                if (propertyInfo == null) return;
                SelectedLanguage = (ILanguageDictionary)propertyInfo.GetValue(null);
            }
        }

        #endregion Public Properties

        #region Private Methods

        /// <summary>
        ///     Translate into language
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string Translate(TranslatableElements key) => SelectedLanguage == null ? "unknow" : SelectedLanguage.Dictionary[key];

        #endregion Private Methods

        #region Public Properties

        public string All => Translate(TranslatableElements.All);
        public string Cancel => Translate(TranslatableElements.Cancel);
        public string Clear => Translate(TranslatableElements.Clear);
        public string Contains => Translate(TranslatableElements.Contains);
        public string ElapsedTime => Translate(TranslatableElements.ElapsedTime);
        public string Empty => Translate(TranslatableElements.Empty);
        public string IsFalse => Translate(TranslatableElements.False);
        public string IsTrue => Translate(TranslatableElements.True);
        public string Neutral => "{0}";
        public string Ok => Translate(TranslatableElements.Ok);
        public string RemoveAll => Translate(TranslatableElements.RemoveAll);
        public string StartsWith => Translate(TranslatableElements.StartsWith);
        public string Status => Translate(TranslatableElements.Status);
        public string Toggle => Translate(TranslatableElements.Toggle);
        public string Indeterminate => Translate(TranslatableElements.Indeterminate);

        #endregion Public Properties

        #region Private Properties

        private static ILanguageDictionary Dutch { get; } = new LanguageDictionary("Dutch", new CultureInfo("nl-NL"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(Alles selecteren)" },
                { TranslatableElements.Empty, "(Leeg)" },
                { TranslatableElements.Clear, "Filter \"{0}\" verwijderen" },
                { TranslatableElements.Contains, "Zoek (bevat)" },
                { TranslatableElements.StartsWith, "Zoek (beginnen met)" },
                { TranslatableElements.Toggle, "Toggle bevat/begint met" },
                { TranslatableElements.Ok, "Ok" },
                { TranslatableElements.Cancel, "Annuleren" },
                { TranslatableElements.Status, "{0:n0} rij(en) gevonden op {1:n0}" },
                { TranslatableElements.ElapsedTime, "Verstreken tijd {0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "Aangevinkt" },
                { TranslatableElements.False, "Niet aangevinkt" },
                { TranslatableElements.RemoveAll, "Alle filters verwijderen" },
                { TranslatableElements.Indeterminate, "Onbepaald" },
            });

        private static ILanguageDictionary English { get; } = new LanguageDictionary("English", new CultureInfo("en-US"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(Select all)" },
                { TranslatableElements.Empty, "(Blank)" },
                { TranslatableElements.Clear, "Clear filter \"{0}\"" },
                { TranslatableElements.Contains, "Search (contains)" },
                { TranslatableElements.StartsWith, "Search (startswith)" },
                { TranslatableElements.Toggle, "Toggle contains/startswith" },
                { TranslatableElements.Ok, "Ok" },
                { TranslatableElements.Cancel, "Cancel" },
                { TranslatableElements.Status, "{0:n0} record(s) found on {1:n0}" },
                { TranslatableElements.ElapsedTime, "Elapsed time {0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "Checked" },
                { TranslatableElements.False, "Unchecked" },
                { TranslatableElements.RemoveAll, "Remove all filters" },
                { TranslatableElements.Indeterminate, "Indeterminate" },
            });

        private static ILanguageDictionary French { get; } = new LanguageDictionary("French", new CultureInfo("fr-FR"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(Sélectionner tout)" },
                { TranslatableElements.Empty, "(Vides)" },
                { TranslatableElements.Clear, "Effacer le filtre \"{0}\"" },
                { TranslatableElements.Contains, "Rechercher (contient)" },
                { TranslatableElements.StartsWith, "Rechercher (commence par)" },
                { TranslatableElements.Toggle, "Basculer contient/commence par" },
                { TranslatableElements.Ok, "Ok" },
                { TranslatableElements.Cancel, "Annuler" },
                { TranslatableElements.Status, "{0:n0} enregistrement(s) trouvé(s) sur {1:n0}" },
                { TranslatableElements.ElapsedTime, "Temps écoulé {0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "Coché" },
                { TranslatableElements.False, "Décoché" },
                { TranslatableElements.RemoveAll, "Supprimer tous les filtres" },
                { TranslatableElements.Indeterminate, "Indéterminée" },
            });

        private static ILanguageDictionary German { get; } = new LanguageDictionary("German", new CultureInfo("de-DE"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(Alle auswählen)" },
                { TranslatableElements.Empty, "(Leer)" },
                { TranslatableElements.Clear, "Filter löschen \"{0}\"" },
                { TranslatableElements.Contains, "Suche (enthält)" },
                { TranslatableElements.StartsWith, "Suche (beginnen mit)" },
                { TranslatableElements.Toggle, "Toggle enthält/beginnt mit" },
                { TranslatableElements.Ok, "Ok" },
                { TranslatableElements.Cancel, "Abbrechen" },
                { TranslatableElements.Status, "{0:n0} zeilen angezeigt von {1:n0}" },
                { TranslatableElements.ElapsedTime, "Verstrichene Zeit {0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "Ausgewählt" },
                { TranslatableElements.False, "Nicht ausgewählt" },
                { TranslatableElements.RemoveAll, "Alle Filter entfernen" },
                { TranslatableElements.Indeterminate, "Unbestimmt" },
            });

        private static ILanguageDictionary Hebrew { get; } = new LanguageDictionary("Hebrew", new CultureInfo("he-IL"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(בחר הכל)" },
                { TranslatableElements.Empty, "(ריק)" },
                { TranslatableElements.Clear, "נקה מסנן \"{0}\"" },
                { TranslatableElements.Contains, "חיפוש (מכיל)" },
                { TranslatableElements.StartsWith, "חיפוש (מתחיל ב)" },
                { TranslatableElements.Toggle, "החלף מכיל/מתחיל ב" },
                { TranslatableElements.Ok, "אישור" },
                { TranslatableElements.Cancel, "בטל" },
                { TranslatableElements.Status, "{0:n0} רשומות נמצאו ב {1:n0}" },
                { TranslatableElements.ElapsedTime, "הזמן שחלף {0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "נבחר" },
                { TranslatableElements.False, "לא נבחר" },
                { TranslatableElements.RemoveAll, "הסר את כל המסננים" },
                { TranslatableElements.Indeterminate, "לא מוגדר" },
            });

        private static ILanguageDictionary Hungarian { get; } = new LanguageDictionary("Hungarian",
            new CultureInfo("hu-HU"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(Összes kijelölése)" },
                { TranslatableElements.Empty, "(Üres)" },
                { TranslatableElements.Clear, "Szűrő törlése \"{0}\"" },
                { TranslatableElements.Contains, "Keresés (tartalmaz)" },
                { TranslatableElements.StartsWith, "Keresés (kezdődik)" },
                { TranslatableElements.Toggle, "Váltás tartalmaz/kezdődik" },
                { TranslatableElements.Ok, "Ok" },
                { TranslatableElements.Cancel, "Mégsem" },
                { TranslatableElements.Status, "{0:n0} találat, összes: {1:n0}" },
                { TranslatableElements.ElapsedTime, "Eltelt idő {0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "True" },
                { TranslatableElements.False, "False" },
                { TranslatableElements.RemoveAll, "Összes szűrő törlése" },
                { TranslatableElements.Indeterminate, "Határozatlan" },
            });

        private static ILanguageDictionary Italian { get; } = new LanguageDictionary("Italian", new CultureInfo("it-IT"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(Seleziona tutto)" },
                { TranslatableElements.Empty, "(Vuoto)" },
                { TranslatableElements.Clear, "Cancella filtro \"{0}\"" },
                { TranslatableElements.Contains, "Cerca (contiene)" },
                { TranslatableElements.StartsWith, "Cerca (inizia con)" },
                { TranslatableElements.Toggle, "Toggle contiene/inizia con" },
                { TranslatableElements.Ok, "Ok" },
                { TranslatableElements.Cancel, "Annulla" },
                { TranslatableElements.Status, "{0:n0} oggetti trovati su {1:n0}" },
                { TranslatableElements.ElapsedTime, "Tempo trascorso {0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "Controllato" },
                { TranslatableElements.False, "Deselezionato" },
                { TranslatableElements.RemoveAll, "Rimuovi tutti i filtri" },
                { TranslatableElements.Indeterminate, "Indeterminato" },
            });

        private static ILanguageDictionary Japanese { get; } = new LanguageDictionary("Japanese",
            new CultureInfo("ja-JP"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(すべて選択)" },
                { TranslatableElements.Empty, "(空白)" },
                { TranslatableElements.Clear, "\"{0}\" からフィルターをクリア" },
                { TranslatableElements.Contains, "検索 (含む)" },
                { TranslatableElements.StartsWith, "検索 (で始まる)" },
                { TranslatableElements.Toggle, "含む/で始まるの切り替え" },
                { TranslatableElements.Ok, "确定" },
                { TranslatableElements.Cancel, "取り消し" },
                { TranslatableElements.Status, "{1:n0} レコード中 {0:n0}個が見つかりました " },
                { TranslatableElements.ElapsedTime, "経過時間{0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "選択済み" },
                { TranslatableElements.False, "未選択" },
                { TranslatableElements.RemoveAll, "すべてのフィルターをクリア" },
                { TranslatableElements.Indeterminate, "不定" },
            });

        private static ILanguageDictionary Polish { get; } = new LanguageDictionary("Polish", new CultureInfo("pl-PL"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(Zaznacz wszystkie)" },
                { TranslatableElements.Empty, "(Pusty)" },
                { TranslatableElements.Clear, "Wyczyść filtr \"{0}\"" },
                { TranslatableElements.Contains, "Szukaj (zawiera)" },
                { TranslatableElements.StartsWith, "Szukaj (zaczyna się od)" },
                { TranslatableElements.Toggle, "Przełącz zawiera/zaczyna się od" },
                { TranslatableElements.Ok, "Ok" },
                { TranslatableElements.Cancel, "Anuluj" },
                { TranslatableElements.Status, "{0:n0} rekord(y) znaleziony(e) w {1:n0}" },
                { TranslatableElements.ElapsedTime, "Zajęło {0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "Zaznaczone" },
                { TranslatableElements.False, "Niezaznaczone" },
                { TranslatableElements.RemoveAll, "Usuń wszystkie filtry" },
                { TranslatableElements.Indeterminate, "Nieokreślony" },
            });

        private static ILanguageDictionary Russian { get; } = new LanguageDictionary("Russian", new CultureInfo("ru-RU"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(Выбрать все)" },
                { TranslatableElements.Empty, "(Пусто)" },
                { TranslatableElements.Clear, "Очистить фильтр \"{0}\"" },
                { TranslatableElements.Contains, "Искать (содержит)" },
                { TranslatableElements.StartsWith, "Искать (начинается)" },
                { TranslatableElements.Toggle, "Переключить содержит/начинается" },
                { TranslatableElements.Ok, "Ok" },
                { TranslatableElements.Cancel, "Отменить" },
                { TranslatableElements.Status, "{0:n0} записей найдено из {1:n0}" },
                { TranslatableElements.ElapsedTime, "Затрачено времени {0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "Проверено" },
                { TranslatableElements.False, "Не проверено" },
                { TranslatableElements.RemoveAll, "Сбросить все фильтры" },
                { TranslatableElements.Indeterminate, "Неопределенный" },
            });

        private static ILanguageDictionary SimplifiedChinese { get; } = new LanguageDictionary("SimplifiedChinese",
            new CultureInfo("zh-Hans"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(全选)" },
                { TranslatableElements.Empty, "(空白)" },
                { TranslatableElements.Clear, "清除过滤器 \"{0}\"" },
                { TranslatableElements.Contains, "搜索(包含)" },
                { TranslatableElements.StartsWith, "搜索 (来自)" },
                { TranslatableElements.Toggle, "切換包含/開始於" },
                { TranslatableElements.Ok, "确定" },
                { TranslatableElements.Cancel, "取消" },
                { TranslatableElements.Status, "{0:n0} 找到了 {1:n0} 条记录" },
                { TranslatableElements.ElapsedTime, "经过时间{0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "已選中" },
                { TranslatableElements.False, "未選中" },
                { TranslatableElements.RemoveAll, "删除所有过滤器" },
                { TranslatableElements.Indeterminate, "不定" },
            });

        private static ILanguageDictionary Spanish { get; } = new LanguageDictionary("Spanish", new CultureInfo("es-ES"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(Seleccionar todos)" },
                { TranslatableElements.Empty, "(Vacio)" },
                { TranslatableElements.Clear, "Limpiar filtros \"{0}\"" },
                { TranslatableElements.Contains, "Buscar (contiene)" },
                { TranslatableElements.StartsWith, "Buscar (comienza con)" },
                { TranslatableElements.Toggle, "Toggle contiene/comienza con" },
                { TranslatableElements.Ok, "Aceptar" },
                { TranslatableElements.Cancel, "Cancelar" },
                { TranslatableElements.Status, "{0:n0} registro(s) encontrado(s) de {1:n0}" },
                { TranslatableElements.ElapsedTime, "Tiempo transurrido {0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "Comprobado" },
                { TranslatableElements.False, "Sin marcar" },
                { TranslatableElements.RemoveAll, "Eliminar todos los filtros" },
                { TranslatableElements.Indeterminate, "Indeterminado" },
            });

        private static ILanguageDictionary TraditionalChinese { get; } = new LanguageDictionary("TraditionalChinese",
                                                                                                    new CultureInfo("zh-Hant"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(全選)" },
                { TranslatableElements.Empty, "(空白)" },
                { TranslatableElements.Clear, "清除篩選 \"{0}\"" },
                { TranslatableElements.Contains, "搜尋(包含)" },
                { TranslatableElements.StartsWith, "搜尋(字元開頭)" },
                { TranslatableElements.Toggle, "切換包含/開始於" },
                { TranslatableElements.Ok, "確定" },
                { TranslatableElements.Cancel, "取消" },
                { TranslatableElements.Status, "{0:n0} 找到 {1:n0} 條記錄" },
                { TranslatableElements.ElapsedTime, "經過時間{0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "已選中" },
                { TranslatableElements.False, "未選中" },
                { TranslatableElements.RemoveAll, "清除所有篩選" },
                { TranslatableElements.Indeterminate, "不定" },
            });

        private static ILanguageDictionary Turkish { get; } = new LanguageDictionary("Turkish", new CultureInfo("tr-TR"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(Hepsini seç)" },
                { TranslatableElements.Empty, "(Boş)" },
                { TranslatableElements.Clear, "\"{0}\" filtresini temizle" },
                { TranslatableElements.Contains, "Ara (içerir)" },
                { TranslatableElements.StartsWith, "Ara (ile başlar)" },
                { TranslatableElements.Toggle, "İçerir/ile başlar arasında geçiş yap" },
                { TranslatableElements.Ok, "Tamam" },
                { TranslatableElements.Cancel, "İptal" },
                { TranslatableElements.Status, "{1:n0} kayıt içerisinden {0:n0} kayıt bulundu" },
                { TranslatableElements.ElapsedTime, "Geçen süre {0:dd}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "Seçili" },
                { TranslatableElements.False, "Seçili değil" },
                { TranslatableElements.RemoveAll, "Tüm filtreleri kaldır" },
                { TranslatableElements.Indeterminate, "Belirsiz" },
            });

        private ILanguageDictionary SelectedLanguage { get; set; }

        #endregion Private Properties
    }

    // Contributor : dankovics.jozsef
    internal class LanguageDictionary : ILanguageDictionary {

        #region Public Constructors

        public LanguageDictionary(string language, CultureInfo culture,
            Dictionary<TranslatableElements, string> dictionary) {
            Language = language;
            Culture = culture;
            Dictionary = dictionary;
        }

        #endregion Public Constructors

        #region Public Properties

        public CultureInfo Culture { get; }
        public Dictionary<TranslatableElements, string> Dictionary { get; }
        public string Language { get; }

        #endregion Public Properties
    }
}
