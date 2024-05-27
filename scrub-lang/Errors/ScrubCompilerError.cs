﻿using scrub_lang.Objects;

namespace scrub_lang.Evaluator;

public class ScrubCompilerError  : ScrubError
{
	public ScrubCompilerError(string message) : base(message)
	{
	}

	public override ScrubType GetType()
	{
		return ScrubType.Error;
	}
}