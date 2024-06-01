using scrub_lang.VirtualMachine;
using scrub_lang.VirtualMachine.ExecutionLog;
using Terminal.Gui;

namespace MyGuiCsProject.Views;

public class OpLogView : FrameView
{
	private ListView _listView;
	private ScrubTUI _tui;
	
	public OpLogView(ScrubTUI tui)
	{
		_tui = tui;
		_tui.Runner.OnInitialized += OnInitialized;
		_tui.Runner.OnComplete += OnComplete;
		_listView = new ListView()
		{
			Height = Dim.Fill(),
			Width = Dim.Fill(),
			CanFocus = false,
			AllowsMultipleSelection = false,
			AllowsMarking = true,
		};
		_listView.OpenSelectedItem += ListViewOnOpenSelectedItem;
		Add(_listView);
	}

	private void ListViewOnOpenSelectedItem(ListViewItemEventArgs obj)
	{
		var log = (OpLog)obj.Value;
		_tui.Runner.RunTo(log.OpNumber);
	}

	private void OnComplete()
	{
		_listView.SetSource(_tui.Runner.Log.Log);
	}

	void OnInitialized()
	{
		_listView.SetSource(_tui.Runner.Log.Log);
	}
	
}