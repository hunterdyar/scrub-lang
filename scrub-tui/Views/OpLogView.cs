using scrub_lang.VirtualMachine;
using scrub_lang.VirtualMachine.ExecutionLog;
using Terminal.Gui;

namespace MyGuiCsProject.Views;

public class OpLogView : FrameView
{
	private ListView _listView;
	private VMRunner _runner;
	
	public OpLogView(VMRunner runner)
	{
		_runner = runner;
		_runner.OnInitialized += OnInitialized;
		_runner.OnComplete += OnComplete;
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
		_runner.RunTo(log.OpNumber);
	}

	private void OnComplete()
	{
		_listView.SetSource(_runner.Log.Log);
	}

	void OnInitialized()
	{
		_listView.SetSource(_runner.Log.Log);
	}
	
}