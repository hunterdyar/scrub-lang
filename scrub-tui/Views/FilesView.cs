using Terminal.Gui;

namespace MyGuiCsProject.Views;

public class FilesView : FrameView
{
	//THis is the files tab, and the landing page.
	//It should have some greeting text up top. "Welcome to Scrub!" or some ascii art.
	private OpenDialog openDialog;
	private Button openFileButton;
	private ScrubTUI _tui;
	public Action<string> OnFileSelected;
	public FilesView(ScrubTUI tui)
	{
		_tui = tui;
		var welcome = new Label("Welcome to Scrub!")
		{
			X = 0,
			Y = 0,
			Width = Dim.Fill(),
			Height = Dim.Sized(2),
			TextAlignment = TextAlignment.Centered
		};
		Add(welcome);

		openDialog = new OpenDialog()
		{
			Width = Dim.Fill(),
			Height = Dim.Fill(),
			X = 0,
			Y = Pos.Bottom(welcome),
			AllowedFileTypes = ["scrub", "txt", "text"],
			AllowsMultipleSelection = false,
			CanChooseDirectories = false,
			CanChooseFiles = true,
			
			ColorScheme = Colors.TopLevel,
		};
		
		
		openFileButton = new Button()
		{
			X = 0,
			Y = Pos.Bottom(welcome),
			TextAlignment = TextAlignment.Centered,
			Text = "Open",
		};
		
		openFileButton.Clicked += OpenFileButtonOnClicked;
		openDialog.Closed += OpenDialogOnClosed;
		Add(openFileButton);
	}

	private void OpenDialogOnClosed(Toplevel obj)
	{
		if (openDialog.FilePaths.Count > 0)
		{
			_tui.RunFile(openDialog.FilePaths[0]);
		}
	}

	private void OpenFileButtonOnClicked()
	{
		Terminal.Gui.Application.Run(openDialog, OnFileSelectedError);
	}

	private bool OnFileSelectedError(Exception exception)
	{
		//dang
		return true;//resume on true, exit on false.
	}

//It should look like the nvim launcher I use. "Recent files" and a list of 5 or so (...more on bottom to expand to however many we save).
	//then below that, a "Open file..." button that opens a dialogue.
}