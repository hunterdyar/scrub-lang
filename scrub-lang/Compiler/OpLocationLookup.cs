using scrub_lang.Tokenizer.Tokens;

namespace scrub_lang.Compiler;

public class OpLocationLookup
{
	private Dictionary<int, Location> _locations;
	public OpLocationLookup()
	{
		_locations = new Dictionary<int, Location>();
	}

	public void Add(int instruction, Token token)
	{
		_locations.Add(instruction,token.Location);
	}

	public void Add(int instruction, Location location)
	{
		_locations.Add(instruction, location);
	}
	public Location GetLocation(int instructionPointer)
	{
		if (_locations.Count == 0)
		{
			return new Location(-1,-1);
		}
		
		//todo: write a faster solution. I mean, binary search against tuples would be faster. whatever.
		//searches for the instruction pointer in it's table that is below this.
		for (int i = instructionPointer; i >=0; i--)
		{
			if (_locations.TryGetValue(i, out var loc))
			{
				return loc;
			}
		}

		return new Location(-1, -1);

	}
}