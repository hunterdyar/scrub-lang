using System.Data;
using scrub_lang.VirtualMachine;

namespace MyGuiCsProject.Views;


public class VariableData : DataTable
{
	private VMRunner _runner;
	
	public VariableData(VMRunner runner)
	{
		_runner = runner;
		_runner.OnComplete += UpdateState;
		_runner.OnPaused += UpdateState;
		_runner.OnError += UpdateState;
		//_table = new DataTable();

		var nameCol = new DataColumn();
		nameCol.ColumnName = "name";
		nameCol.Caption = "Name";
		nameCol.AutoIncrement = false;
		nameCol.ReadOnly = true;
		Columns.Add(nameCol);
		
		var valueCol = new DataColumn();
		valueCol.ColumnName = "value";
		valueCol.Caption = "Value";
		valueCol.ReadOnly = false;
		Columns.Add(valueCol);
		
	}
	

	private void UpdateState()
	{
		//empty everything and recreate everything. Not ideal, but we are just getting things moving. change to hash setup, or smart updates from VM?
		Rows.Clear();
		if (_runner == null)
		{
			return;
		}

		if (_runner.State == null)
		{
			return;
		}
		foreach(var variable in _runner.Status.GetVariables())
		{
			if (variable.Object == null)
			{
				continue;
			}
			var r = NewRow();
			r["name"] = variable.Name;
			r["value"] = variable.Object.ToString();
			Rows.Add(r);
		}
		this.AcceptChanges();
	}

}