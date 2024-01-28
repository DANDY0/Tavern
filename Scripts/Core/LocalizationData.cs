using GrandDevs.Tavern.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GrandDevs.Tavern
{
	[CreateAssetMenu(fileName = "LocalizationData", menuName = "GrandDevs/LocalizationData", order = 2)]
	public class LocalizationData : ScriptableObject
	{
		[SerializeField]
		public List<LocalizationLanguageData> languages;

		public Enumerators.Language defaultLanguage;

		public string localizationGoogleSpreadsheet;
		public bool refreshLocalizationAtStart = true;

		[Serializable]
		public class LocalizationLanguageData
		{
			public Enumerators.Language language;
			[SerializeField]
			public List<LocalizationDataInfo> localizedTexts;
		}

		[Serializable]
		public class LocalizationDataInfo
		{
			public string key;
			[TextArea(1, 9999)]
			public string value;
		}
	}

	public class LocalizationSheetData
	{
		public string Key;

		// each new language require new parameter in spreadsheet at this moment.(could be more fliexible in future revisions)
		public string English;
		public string German;
	}
}