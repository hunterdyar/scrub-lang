using scrub_lang.VirtualMachine;
using Terminal.Gui;

namespace MyGuiCsProject.Views;

public class TimelineView : FrameView
{
	private VMRunner _runner;
	private ProgressBar _bar;
	//static height var?
	//we want to put some opcounters on top of the bar in the inverse color as the bar.
	public TimelineView(VMRunner runner)
	{
		Title = "Timeline";
		_runner = runner;
		_bar = new ProgressBar()
		{
			Width = Dim.Fill(),
			Height = Dim.Fill(),
			ColorScheme = Colors.Dialog
		};
		this.Border = new Border(){
			BorderStyle = BorderStyle.None
		};
		_runner.OnNewResult += (a,b)=> UpdateProgress();
		_runner.OnComplete += () => UpdateProgress();
		_runner.OnPaused += () => UpdateProgress();
		_runner.OnError += () => UpdateProgress();
		Add(_bar);
	}

	private void UpdateProgress()
	{
		var p = _runner.Percentage;
		_bar.Fraction = p;
	}
}