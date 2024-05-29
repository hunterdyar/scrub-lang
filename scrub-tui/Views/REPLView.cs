using System.Runtime.CompilerServices;
using scrub_lang.VirtualMachine;
using Terminal.Gui;

namespace MyGuiCsProject.Views;

public class REPLView : FrameView
{
	// var container = new FrameView("Read-Execute-Print-Loop")
	// {
	// 	X = 0,
	// 	Y = 0,
	// 	Width = Dim.Fill(),
	// 	Height = Dim.Fill()
	// };
	public TextView OutputView;
	public TextField ReplInput;
	private Button _doItButton;
	private VMRunner _runner;
	private List<string> _replHistory = new List<string>();
	private int _historyPos;
	public REPLView(VMRunner runner)
	{
		_runner = runner;
		_runner.Output.OnUpdate += OnOutputUpdate;

		Title = "Read-Execute-Print-Loop";
		OutputView = new TextView()
		{
			X = 0,
			Y = 0,
			Width = Dim.Fill(),
			Height = Dim.Fill(1),
			WordWrap = true,
			ReadOnly = true,
		};
		OutputView.ColorScheme = Colors.Base;
		ReplInput = new TextField("")
		{
			X = 0,
			Y = Pos.Bottom(OutputView),
			Width = Dim.Fill(8),
			Height = 1,
		};
		ReplInput.ColorScheme = Colors.Dialog;

		_doItButton = new Button()
		{
			X = Pos.Right(ReplInput),
			Y = Pos.Bottom(OutputView),
			Width = 8,
			Height = 1,
			Text = "run"
		};

		_doItButton.ClearKeybinding(Key.Enter);
		_doItButton.AddKeyBinding(Key.Enter,Command.Accept);
		
		_doItButton.Clicked += Submit;
		// _doItButton.Clicked += DoItButtonOnClicked;
		
		Add(OutputView);
		Add(ReplInput);
		Add(_doItButton);
	}

	public void ProcessKeyEvent(KeyEvent key)
	{
		if (key.Key == Key.Enter)
		{
			Submit();
			return;
		}

		if (key.Key == Key.CursorUp)
		{
			PreviousHistory();
		}

		if (key.Key == Key.CursorDown)
		{
			NextHistory();
		}
	}

	private void OnOutputUpdate()
	{
		OutputView.Text = _runner.Output.ToString();
		ReplInput.Enabled = _runner.State != VMState.Paused || _runner.State != VMState.Paused;
		OutputView.MoveEnd();
	}

	public void Submit()
	{
		// if (k.KeyEvent.Key != Key.Enter)
		// {
		// 	return;
		// }
		
		if (_runner.State == VMState.Paused)
		{
			_runner.RunUntilStop();
			return;
		}

		if (ReplInput.Text == "")
		{
			//resume if it's running. maybe do this no matter what

		}

		var program = ReplInput.Text.ToString();
		ReplInput.Text = "";
		RunLine(program);
		_replHistory.Add(program);
		_historyPos++;
	}

	//pressing 'up' in the terminal
	private void PreviousHistory()
	{
		if (_historyPos > 0)
		{
			var current = _replHistory[_historyPos - 1];
			_historyPos--;
			ReplInput.Text = current;
		}
	}

	//pressing 'down' in the terminal
	private void NextHistory()
	{
		if (_historyPos < _replHistory.Count)
		{
			var current = _replHistory[_historyPos];
			_historyPos++;
			ReplInput.Text = current;
		}
	}

	private void RunLine(string program)
	{
		_runner.RunWithEnvironment(program);
	}
}