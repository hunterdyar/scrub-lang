using scrub_lang.VirtualMachine;
using Terminal.Gui;

namespace MyGuiCsProject.Views;

public class LastResultView : FrameView
{
	private VMRunner _runner;
	private Label _resultLabel;
	
	public LastResultView(VMRunner runner)
	{
		_runner = runner;
		_runner.OnNewResult += OnNewResult;
		_resultLabel = new Label()
		{
			X = 0,
			Y = 0,
			Width = Dim.Fill(),
			Height = Dim.Fill()
		};
		Add(_resultLabel);
	}

	private void OnNewResult(string result)
	{
		_resultLabel.Text = result;
	}
}