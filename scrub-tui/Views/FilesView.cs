using scrub_lang.Compiler;
using Terminal.Gui;

namespace ScrubTUI.Views;

public class FilesView : FrameView
{
	//THis is the files tab, and the landing page.
	//It should have some greeting text up top. "Welcome to Scrub!" or some ascii art.
	private OpenDialog _openDialog;
	private Button _openFileButton;
	private ListView _recentFilesListView;
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

		_openDialog = new OpenDialog()
		{
			Width = Dim.Fill(),
			Height = Dim.Fill(),
			X = 0,
			Y = 0,
			AllowedFileTypes = ["scrub", "txt", "text"],
			AllowsMultipleSelection = false,
			CanChooseDirectories = false,
			CanChooseFiles = true,
			ColorScheme = Colors.TopLevel,
		};


		_openFileButton = new Button()
		{
			X = 0,
			Y = Pos.Bottom(welcome),
			TextAlignment = TextAlignment.Centered,
			Text = "Open",
		};
		Add(_openFileButton);


		_recentFilesListView = new ListView(ScrubTUIProgram.RecentFiles)
		{
			X = 0,
			Width = Dim.Fill(),
			TextAlignment = TextAlignment.Centered,
			Y = Pos.Bottom(_openFileButton)+1,
			Height = Dim.Fill(),
		};
		Add(_recentFilesListView);

		_openFileButton.Clicked += OpenFileButtonOnClicked;
		_openDialog.Closed += OpenDialogOnClosed;
		_recentFilesListView.OpenSelectedItem += RecentFilesListViewOnOpenSelectedItem;
	}

	private void RecentFilesListViewOnOpenSelectedItem(ListViewItemEventArgs obj)
	{
		var path = obj.Value.ToString();
		_tui.RunFile(path);
		//move file to top of list... todo: probably a cleaner way to do this.
		ScrubTUIProgram.AddRecentFile(path);
	}

	private void OpenDialogOnClosed(Toplevel obj)
	{
		if (_openDialog.FilePaths.Count > 0)
		{
			_tui.RunFile(_openDialog.FilePaths[0]);
		}

		ScrubTUIProgram.AddRecentFile(_openDialog.FilePaths[0]);
	}

	private void OpenFileButtonOnClicked()
	{
		Terminal.Gui.Application.Run(_openDialog, OnFileSelectedError);
	}

	private bool OnFileSelectedError(Exception exception)
	{
		//dang
		return true;//resume on true, exit on false.
	}

//It should look like the nvim launcher I use. "Recent files" and a list of 5 or so (...more on bottom to expand to however many we save).
	//then below that, a "Open file..." button that opens a dialogue.
}