using System.Reflection.Metadata;
using scrub_lang.VirtualMachine;
using Terminal.Gui;

namespace MyGuiCsProject.Views;

public class VariableStateView : FrameView
{
	private VMRunner _runner;
	private TableView _table;
	public VariableStateView(string title, VMRunner runner)
	{
		Title = title;
		_runner = runner;
		_table = new TableView()
		{
			X = 0,
			Y = 0,
			Width = Dim.Fill(),
			Height = Dim.Fill(),
			FullRowSelect = true,
			MultiSelect = false,
		};
		_table.Table = new VariableData(_runner);
		Add(_table);
	}
}