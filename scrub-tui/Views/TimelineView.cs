using scrub_lang.VirtualMachine;
using Terminal.Gui;

namespace MyGuiCsProject.Views;

public class TimelineView : FrameView
{
	private ScrubTUI _tui;
	private ProgressBar _bar;
	//static height var?
	//we want to put some opcounters on top of the bar in the inverse color as the bar.
	public TimelineView(ScrubTUI tui)
	{
		Title = "Timeline";
		_tui = tui;
		_bar = new ProgressBar()
		{
			Width = Dim.Fill(),
			Height = Dim.Fill(),
			ColorScheme = Colors.Dialog
		};
		this.Border = new Border(){
			BorderStyle = BorderStyle.None
		};
		
		_tui.Runner.OnNewResult += (a,b)=> UpdateProgress();
		_tui.Runner.OnComplete += UpdateProgress;
		_tui.Runner.OnPaused += UpdateProgress;
		_tui.Runner.OnError += UpdateProgress;
		Add(_bar);
	}

	private void UpdateProgress()
	{
		var p = _tui.Runner.Percentage;
		_bar.Fraction = p;
		if (p == 1)
		{
			_bar.ColorScheme = Colors.Base;
		}else if (p == 0)
		{
			_bar.ColorScheme = Colors.Menu;
		}
		else
		{
			_bar.ColorScheme = Colors.TopLevel;
		}
	}
}