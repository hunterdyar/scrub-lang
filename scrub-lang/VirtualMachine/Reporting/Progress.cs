namespace scrub_lang.VirtualMachine;

public class Progress
{
	public long OpCounter => _opCounter;
	private long _opCounter = 0;
	public long MaxCounter => long.Max(_opCounter, _completeCount);
	private long _completeCount = 0;
	public float Percentage => GetPercentage();

	private float GetPercentage()
	{
		var max = MaxCounter;
		if (max != 0 && _opCounter != 0)
		{
			return _opCounter / max;
		}

		return 0;
	}

	public void SetCompleteToCurrent()
	{
		_completeCount = _opCounter;
	}

	public void IncrementCount()
	{
		_opCounter++;
	}

	public void DecrementCount()
	{
		_opCounter--;
	}
}