using scrub_lang.VirtualMachine;
using Terminal.Gui;

namespace MyGuiCsProject.Views;

public class PlayBar : FrameView
{
	public Button PlayButton;
	public Button PauseButton;
	public Button StepForwardButton;
	public Button StepBackwardsButton;
	private VMRunner _runner;

	public PlayBar(VMRunner runner)
	{
		_runner = runner;
		StepBackwardsButton = new Button()
		{
			X = 0,
			Y = 0,
			Text = "|<",
		};
		StepBackwardsButton.Clicked += StepBackwardsButtonOnClicked;
		PauseButton = new Button()
		{
			X = Pos.Right(StepBackwardsButton),
			Text = "||",
		};
		PauseButton.Clicked += PauseButtonOnClicked;

		PlayButton = new Button()
		{
			X = Pos.Right(PauseButton),
			Text = "|>",
		};
		PlayButton.Clicked+= PlayButtonOnClicked;
		
		StepForwardButton = new Button()
		{
			X = Pos.Right(PlayButton),
			Text = ">|",
		};
		StepForwardButton.Clicked+= StepForwardButtonOnClicked;
		
		Add(StepBackwardsButton);
		Add(PauseButton);
		Add(PlayButton);
		Add(StepForwardButton);
		
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
	
}