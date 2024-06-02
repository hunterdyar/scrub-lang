using System.Text.Json;
using System.Xml;
using ScrubTUI;
using Terminal.Gui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.CommandLine;
using ScrubTUI.Configuration;

public static class ScrubTUIProgram
{
    public static UserPreferences Preferences = new UserPreferences(); 
    public static List<string> RecentFiles = new List<string>();
    public static IConfigurationRoot ConfigurationRoot;
    public static void Main(string[] args)
    {
        //load config
        ConfigurationRoot = new ConfigurationManager().AddCommandLine(args).AddJsonFile("usersettings.json",false).Build();
        var pref = ConfigurationRoot.GetSection("Settings").GetSection("Preferences");
        var rf = ConfigurationRoot.GetSection("Settings").GetSection("RecentFiles");
        var sett = ConfigurationRoot.GetSection("Settings");
        
		var p = pref.Get<UserPreferences>();
		if (p != null)
		{
			Preferences = p;
		}

		RecentFiles = rf.Get<string[]>()?.ToList();
		
		//run
        try
        {
	        Application.Init();
	        Application.Run<ScrubTUI.ScrubTUI>();
        }
        finally
        { 
	        SaveSettings();
           Application.Shutdown();
        }
    }

    //todo: this belongs... somewhere... in settings.cs?
    public static void AddRecentFile(string path)
    {
	    //remove from wherever and insert at beginning of list.
	    if (ScrubTUIProgram.RecentFiles.Contains(path))
	    {
		    if (ScrubTUIProgram.RecentFiles.IndexOf(path) == 0)
		    {
			    //don't bother updating most recent.
			    return;
		    }
		    
		    ScrubTUIProgram.RecentFiles.Remove(path);
	    }
	    
	    ScrubTUIProgram.RecentFiles.Insert(0, path);

	    //trim to length.
	    if (RecentFiles.Count>Preferences.NumberRecentFiles)
	    {
		    RecentFiles.RemoveAt(RecentFiles.Count-1);
	    }
	    SaveSettings();
    }
    
    private static void SaveSettings()
    {
	    JsonConfigurationProvider? jprov = null;
	    foreach (var provider in ConfigurationRoot.Providers)
	    {
		    if (provider is JsonConfigurationProvider j)
		    {
			    jprov = j;
			    break;
		    }
	    }

	    if (jprov == null)
	    {
		    return;
	    }

	    var settings = new Settings()
	    {
		    Preferences = Preferences,
		    RecentFiles = RecentFiles.ToArray(),
	    };
	    
	    var path = jprov.Source.Path;

	    string output = JsonSerializer.Serialize<Settings>(settings);
	    output = "{\"Settings\":" + output + "}";
	    File.WriteAllText(path, output);
    }
}
