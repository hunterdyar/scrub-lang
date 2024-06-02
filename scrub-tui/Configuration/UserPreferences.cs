using System.Text.Json.Serialization;

namespace ScrubTUI.Configuration;

[System.Serializable]
public class UserPreferences
{
	[JsonInclude]
	public int NumberRecentFiles = 5;
	[JsonInclude]
	public string Theme;
}