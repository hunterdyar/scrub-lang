using Terminal.Gui;

namespace MyGuiCsProject.Views;

public class FilesView : FrameView
{
	//THis is the files tab, and the landing page.
	//It should have some greeting text up top. "Welcome to Scrub!" or some ascii art.
	private OpenDialog openDialog;
	private Button openFileButton;

	public Action<string> OnFileSelected;
	public FilesView()
	{
		openDialog = new OpenDialog()
		{
			Width = Dim.Fill(),
			Height = Dim.Fill(),
			AllowedFileTypes = ["scrub", "txt", "text"],
			AllowsMultipleSelection = false,
			CanChooseDirectories = false,
			ColorScheme = Colors.TopLevel,
		};
		openFileButton = new Button()
		{
			Text = "Open"
		};
		openFileButton.Clicked += OpenFileButtonOnClicked;
		Add(openFileButton);
	}

	private void OpenFileButtonOnClicked()
	{
		Terminal.Gui.Application.Run(openDialog, OnFileSelectedError);
		var selected = openDialog.FilePaths[0];
	}

	private bool OnFileSelectedError(Exception exception)
	{
		//dang
		return true;//resume on true, exit on false.
	}

//It should look like the nvim launcher I use. "Recent files" and a list of 5 or so (...more on bottom to expand to however many we save).
	//then below that, a "Open file..." button that opens a dialogue.
}