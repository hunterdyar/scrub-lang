using scrub_lang.VirtualMachine;
using Terminal.Gui;

namespace MyGuiCsProject.Views;

public class LastResultView : FrameView
{
	private ScrubTUI _tui;
	private Label _resultLabel;
	private Label _timeLabel;
	public LastResultView(ScrubTUI tui)
	{
		_tui = tui;
		_tui.Runner.OnNewResult += OnNewResult;
		
		Title = "Last Result";
		int timeWidth = 7;
		_resultLabel = new Label()
		{
			X = 0,
			Y = 0,
			Width = Dim.Fill(timeWidth),
			Height = Dim.Fill()
		};
		_timeLabel = new Label()
		{
			X = Pos.Right(_resultLabel),
			Y = 0,
			Width = timeWidth,
			Height = Dim.Fill(),
			TextAlignment = TextAlignment.Right
		};
		Add(_resultLabel);
		Add(_timeLabel);
	}

	private void OnNewResult(string result, TimeSpan time)
	{
		_resultLabel.Text = result;
		_timeLabel.Text = time.TotalMilliseconds.ToString("N1") + "ms";
	}
}