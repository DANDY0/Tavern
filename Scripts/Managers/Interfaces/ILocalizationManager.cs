using GrandDevs.Tavern.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.Tavern
{
    public interface ILocalizationManager
    {
        event Action<Enumerators.Language> LanguageWasChangedEvent;

        Dictionary<SystemLanguage, Enumerators.Language> SupportedLanguages { get; }
        Enumerators.Language CurrentLanguage { get; }
        Enumerators.Language DefaultLanguage { get; }
        LocalizationData LocalizationData { get; }

        void SetLanguage(Enumerators.Language language, bool forceUpdate = false);

        string GetUITranslation(string key);
    }
}