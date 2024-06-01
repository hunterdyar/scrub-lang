using scrub_lang.VirtualMachine;
using Terminal.Gui;

namespace MyGuiCsProject.Views;

/// <summary>
/// Play/Pause controls, etc.
/// </summary>
public class PlayBar : FrameView
{
	private readonly Button _playButton;
	private readonly Button _pauseButton;
	private readonly Button _stepForwardButton;
	private readonly Button _stepBackwardsButton;
	private readonly VMRunner _runner;

	public PlayBar(VMRunner runner)
	{
		_runner = runner;
		_stepBackwardsButton = new Button()
		{
			X = 0,
			Y = 0,
			Text = "|<",
		};
		_stepBackwardsButton.Clicked += StepBackwardsButtonOnClicked;
		_pauseButton = new Button()
		{
			X = Pos.Right(_stepBackwardsButton),
			Text = "||",
		};
		
		_pauseButton.Clicked += PauseButtonOnClicked;

		_playButton = new Button()
		{
			X = Pos.Right(_pauseButton),
			Text = "|>",
		};
		_playButton.Clicked+= PlayButtonOnClicked;
		
		_stepForwardButton = new Button()
		{
			X = Pos.Right(_playButton),
			Text = ">|",
		};
		_stepForwardButton.Clicked+= StepForwardButtonOnClicked;
		
		Add(_stepBackwardsButton);
		Add(_pauseButton);
		Add(_playButton);
		Add(_stepForwardButton);
		
		//
		// StepBackwardsButton.ClearKeybinding(Command.Accept);
		// StepBackwardsButton.AddKeyBinding(Key.a,Command.Accept);
		// StepBackwardsButton.HotKey = Key.a;
		// StepForwardButton.HotKey = Key.d;
		
	}

	private void StepForwardButtonOnClicked()
	{
		_runner.RunNextOperation();
	}

	private void PlayButtonOnClicked()
	{
		//todo: broadcast play clicked event so we can submit the enter button? 
		_runner.RunUntilStop();
	}

	private void PauseButtonOnClicked()
	{
		_runner.Pause();
	}

	private void StepBackwardsButtonOnClicked()
	{
		_runner.RunPreviousOperation();
	}

	public override bool ProcessHotKey(KeyEvent keyEvent)
	{
		return base.ProcessHotKey(keyEvent);
	}
}