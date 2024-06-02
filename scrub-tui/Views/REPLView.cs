using scrub_lang.VirtualMachine;
using Terminal.Gui;

namespace ScrubTUI.Views;

public class REPLView : FrameView
{
	private readonly TextView _outputView;
	private readonly TextField _replInput;
	private readonly Button _doItButton;
	private readonly ScrubTUI _tui;
	private readonly List<string> _replHistory = new List<string>();
	private int _historyPos;
	public REPLView(ScrubTUI tui)
	{
		_tui = tui;
		_tui.Runner.Output.OnUpdate += ForceOutputUpdate;

		Title = "Read-Execute-Print-Loop";
		_outputView = new TextView()
		{
			X = 0,
			Y = 0,
			Width = Dim.Fill(),
			Height = Dim.Fill(1),
			WordWrap = true,
			ReadOnly = true,
		};
		_outputView.ColorScheme = Colors.Base;
		_replInput = new TextField("")
		{
			X = 0,
			Y = Pos.Bottom(_outputView),
			Width = Dim.Fill(8),
			Height = 1,
		};
		_replInput.ColorScheme = Colors.Dialog;

		_doItButton = new Button()
		{
			X = Pos.Right(_replInput),
			Y = Pos.Bottom(_outputView),
			Width = 8,
			Height = 1,
			Text = "run"
		};

		_doItButton.ClearKeybinding(Key.Enter);
		_doItButton.AddKeyBinding(Key.Enter,Command.Accept);
		
		_doItButton.Clicked += Submit;
		// _doItButton.Clicked += DoItButtonOnClicked;
		
		Add(_outputView);
		Add(_replInput);
		Add(_doItButton);
	}

	public void HandleKey(KeyEventEventArgs eventArgs)
	{
		if (eventArgs.KeyEvent.Key == Key.Enter)
		{
			Submit();
			eventArgs.Handled = true;
			return;
		}

		if (eventArgs.KeyEvent.Key == Key.CursorUp)
		{
			PreviousHistory();
			eventArgs.Handled = true;
			return;
		}

		if (eventArgs.KeyEvent.Key == Key.CursorDown)
		{
			NextHistory();
			eventArgs.Handled = true;
			return;
		}
	}

	public void ForceOutputUpdate()
	{
		_replInput.Enabled = _tui.Runner.State != VMState.Paused || _tui.Runner.State != VMState.Paused;
		_outputView.Text = _tui.Runner.Output.ToString();
		_outputView.MoveEnd();
	}

	public void Submit()
	{
		// if (k.KeyEvent.Key != Key.Enter)
		// {
		// 	return;
		// }
		
		if (_tui.Runner.State == VMState.Paused)
		{
			_tui.Runner.RunUntilStop();
			return;
		}

		if (_replInput.Text == "")
		{
			//resume if it's running. maybe do this no matter what
			return;
		}

		var program = _replInput.Text.ToString();
		_replInput.Text = "";
		RunLine(program);
		_replHistory.Add(program);
		_historyPos = _replHistory.Count;
	}

	//pressing 'up' in the terminal
	private void PreviousHistory()
	{
		if (_historyPos > 0)
		{
			var current = _replHistory[_historyPos - 1];
			_historyPos--;
			_replInput.Text = current;
		}
	}

	//pressing 'down' in the terminal
	private void NextHistory()
	{
		if (_historyPos < _replHistory.Count)
		{
			var current = _replHistory[_historyPos];
			_historyPos++;
			_replInput.Text = current;
		}
	}

	private void RunLine(string program)
	{
		_tui.Runner.RunWithEnvironment(program);
	}
}