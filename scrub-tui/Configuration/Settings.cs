using System.Text.Json.Serialization;

namespace ScrubTUI.Configuration;

//class representation of the base JSON file. 
[Serializable]
public class Settings
{
	[JsonInclude]
	public UserPreferences Preferences { get; set; }
	[JsonInclude]
	public string[] RecentFiles;
}