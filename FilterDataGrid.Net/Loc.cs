#region (c) 2022 Gilles Macabies All right reserved

// Author     : Gilles Macabies
// Solution   : FilterDataGrid
// Projet     : FilterDataGrid.Net
// File       : Loc.cs
// Created    : 03/04/2024
//

#endregion

#region CONTRIBUTORS

// Czech                : hanyscz
// Dutch                : Tenera
// Hebrew               : abaye123
// Hugarian             : dankovics.jozsef
// Italian              : aetasoul
// Japanese             : iBadaMorae
// Polish               : mgaszynski
// Portuguese           : eduardoghi
// Russian              : anyousib
// Simplified Chinese   : SWH998
// Traditional Chinese  : BeCare4
// Turkish              : BEDIRHANSAMSA
// Ukrainian            : oleluo
// Korean               : hyuckkim

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
        Czech,
        Dutch,
        English,
        French,
        German,
        Hungarian,
        Italian,
        Japanese,
        Polish,
        Portuguese,
        Russian,
        SimplifiedChinese,
        Spanish,
        TraditionalChinese,
        Turkish,
        Ukrainian,
        Korean
    }

    // Contributor : dankovics.jozsef
    public enum TranslatableElements
    {
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
        ///     Indeterminate
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

        private Local language = Local.English;

        #endregion Private Fields

        #region Private Methods

        /// <summary>
        ///     Translate into language
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string Translate(TranslatableElements key)
        {
            return SelectedLanguage == null ? "unknow" : SelectedLanguage.Dictionary[key];
        }

        #endregion Private Methods

        #region Public Properties

        public CultureInfo Culture => SelectedLanguage.Culture;
        public string DisplayName => SelectedLanguage.Culture.DisplayName;
        public string EnglishName => SelectedLanguage.Language;

        public Local Language
        {
            get => language;
            set
            {
                language = value;

                Type type = typeof(Loc);
                string enumName = Enum.GetName(typeof(Local), language) ?? throw new ArgumentException("Language is required", nameof(Local));
                PropertyInfo propertyInfo = type.GetProperty(enumName, BindingFlags.Static | BindingFlags.NonPublic);

                if (propertyInfo == null) return;
                SelectedLanguage = (ILanguageDictionary)propertyInfo.GetValue(null);
            }
        }

        #endregion Public Properties

        #region Public Properties

        public string All => Translate(TranslatableElements.All);
        public string Cancel => Translate(TranslatableElements.Cancel);
        public string Clear => Translate(TranslatableElements.Clear);
        public string Contains => Translate(TranslatableElements.Contains);
        public string ElapsedTime => Translate(TranslatableElements.ElapsedTime);
        public string Empty => Translate(TranslatableElements.Empty);
        public string IsFalse => Translate(TranslatableElements.False);
        public string IsTrue => Translate(TranslatableElements.True);
        public static string Neutral => "{0}";
        public string Ok => Translate(TranslatableElements.Ok);
        public string RemoveAll => Translate(TranslatableElements.RemoveAll);
        public string StartsWith => Translate(TranslatableElements.StartsWith);
        public string Status => Translate(TranslatableElements.Status);
        public string Toggle => Translate(TranslatableElements.Toggle);
        public string Indeterminate => Translate(TranslatableElements.Indeterminate);

        #endregion Public Properties

        #region Private Properties

        private static ILanguageDictionary Czech { get; } = new LanguageDictionary("Czech",
            new CultureInfo("cs-CZ"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(Vybrat vše)" },
                { TranslatableElements.Empty, "(Prázdné)" },
                { TranslatableElements.Clear, "Vymazat filtr \"{0}\"" },
                { TranslatableElements.Contains, "Hledat (obsahuje)" },
                { TranslatableElements.StartsWith, "Hledat (začíná na)" },
                { TranslatableElements.Toggle, "Přepnout obsahuje/začíná na" },
                { TranslatableElements.Ok, "Ok" },
                { TranslatableElements.Cancel, "Zrušit" },
                { TranslatableElements.Status, "{0:n0} záznam(ů) nalezeno z {1:n0}" },
                { TranslatableElements.ElapsedTime, "Uplynulý čas {0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "Zaškrtnuto" },
                { TranslatableElements.False, "Nezaškrtnuto" },
                { TranslatableElements.RemoveAll, "Odebrat všechny filtry" },
                { TranslatableElements.Indeterminate, "Neurčeno" }
            });

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

        private static ILanguageDictionary English { get; } = new LanguageDictionary("English",
            new CultureInfo("en-US"),
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
                { TranslatableElements.StartsWith, "Suche (beginnt mit)" },
                { TranslatableElements.Toggle, "Toggle enthält/beginnt mit" },
                { TranslatableElements.Ok, "Ok" },
                { TranslatableElements.Cancel, "Abbrechen" },
                { TranslatableElements.Status, "{0:n0} Zeilen angezeigt von {1:n0}" },
                { TranslatableElements.ElapsedTime, "Verstrichene Zeit {0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "Ausgewählt" },
                { TranslatableElements.False, "Nicht ausgewählt" },
                { TranslatableElements.RemoveAll, "Alle Filter entfernen" },
                { TranslatableElements.Indeterminate, "Unbestimmt" },
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

        private static ILanguageDictionary Italian { get; } = new LanguageDictionary("Italian",
            new CultureInfo("it-IT"),
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
                { TranslatableElements.True, "Vero" },
                { TranslatableElements.False, "Falso" },
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
                { TranslatableElements.Ok, "確定" },
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

        private static ILanguageDictionary Portuguese { get; } = new LanguageDictionary("Portuguese",
            new CultureInfo("pt-BR"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(Selecionar todos)" },
                { TranslatableElements.Empty, "(Vazio)" },
                { TranslatableElements.Clear, "Limpar filtro de \"{0}\"" },
                { TranslatableElements.Contains, "Pesquisar (contém)" },
                { TranslatableElements.StartsWith, "Pesquisar (começa com)" },
                { TranslatableElements.Toggle, "Alternar contém/começa com" },
                { TranslatableElements.Ok, "Ok" },
                { TranslatableElements.Cancel, "Cancelar" },
                { TranslatableElements.Status, "{0:n0} registro(s) encontrado(s) de {1:n0}" },
                { TranslatableElements.ElapsedTime, "Tempo decorrido {0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "Marcado" },
                { TranslatableElements.False, "Desmarcado" },
                { TranslatableElements.RemoveAll, "Remover todos os filtros" },
                { TranslatableElements.Indeterminate, "Indeterminado" },
            });

        private static ILanguageDictionary Russian { get; } = new LanguageDictionary("Russian",
            new CultureInfo("ru-RU"),
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

        private static ILanguageDictionary Spanish { get; } = new LanguageDictionary("Spanish",
            new CultureInfo("es-ES"),
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

        private static ILanguageDictionary Turkish { get; } = new LanguageDictionary("Turkish",
            new CultureInfo("tr-TR"),
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

        private static ILanguageDictionary Ukrainian { get; } = new LanguageDictionary("Ukrainian",
            new CultureInfo("uk-UA"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(Вибрати все)" },
                { TranslatableElements.Empty, "(Порожньо)" },
                { TranslatableElements.Clear, "Очистити фільтр \"{0}\"" },
                { TranslatableElements.Contains, "Пошук (містить)" },
                { TranslatableElements.StartsWith, "Пошук (починається з)" },
                { TranslatableElements.Toggle, "Перемкнути містить/починається з" },
                { TranslatableElements.Ok, "Ok" },
                { TranslatableElements.Cancel, "Скасувати" },
                { TranslatableElements.Status, "{0:n0} запис(ів) знайдено з {1:n0}" },
                { TranslatableElements.ElapsedTime, "Витрачений час {0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "Вибрано" },
                { TranslatableElements.False, "Не вибрано" },
                { TranslatableElements.RemoveAll, "Очистити всі фільтри" },
                { TranslatableElements.Indeterminate, "Невизначений" },
            });

        private static ILanguageDictionary Korean { get; } = new LanguageDictionary("Korean",
            new CultureInfo("ko-KR"),
            new Dictionary<TranslatableElements, string>
            {
                { TranslatableElements.All, "(전체 선택)" },
                { TranslatableElements.Empty, "(비어 있음)" },
                { TranslatableElements.Clear, "필터 초기화 \"{0}\"" },
                { TranslatableElements.Contains, "검색 (포함)" },
                { TranslatableElements.StartsWith, "검색 (시작)" },
                { TranslatableElements.Toggle, "포함/시작 토글" },
                { TranslatableElements.Ok, "확인" },
                { TranslatableElements.Cancel, "취소" },
                { TranslatableElements.Status, "{1:n0}에서 {0:n0}개 요소 찾음" },
                { TranslatableElements.ElapsedTime, "걸린 시간 {0:mm}:{0:ss}.{0:ff}" },
                { TranslatableElements.True, "선택됨" },
                { TranslatableElements.False, "선택되지 않음" },
                { TranslatableElements.RemoveAll, "필터 모두 제거" },
                { TranslatableElements.Indeterminate, "불완전함" },
            });

        private ILanguageDictionary SelectedLanguage { get; set; } = English;

        #endregion Private Properties
    }

    // Contributor : dankovics.jozsef
    internal class LanguageDictionary(string language, CultureInfo culture,
        Dictionary<TranslatableElements, string> dictionary) : ILanguageDictionary
    {
        #region Public Properties

        public CultureInfo Culture { get; } = culture;
        public Dictionary<TranslatableElements, string> Dictionary { get; } = dictionary;
        public string Language { get; } = language;

        #endregion Public Properties
    }
}