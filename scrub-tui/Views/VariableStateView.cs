using System.Reflection.Metadata;
using scrub_lang.VirtualMachine;
using Terminal.Gui;

namespace ScrubTUI.Views;

public class VariableStateView : FrameView
{
	private ScrubTUI _tui;
	private TableView _table;
	public VariableStateView(string title, ScrubTUI tui)
	{
		Title = title;
		_tui = tui;
		_table = new TableView()
		{
			X = 0,
			Y = 0,
			Width = Dim.Fill(),
			Height = Dim.Fill(),
			FullRowSelect = true,
			MultiSelect = false,
		};
		_table.Table = new VariableData(_tui.Runner);
		Add(_table);
	}
}