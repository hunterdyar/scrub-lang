using scrub_lang.Evaluator;

namespace scrub_lang.Memory;

//represents a block of heap memory.
public class Memory
{
	// public void CreateVariable(IdentifierExpression ident, )
	public Byte[] Heap => _heap;
	public byte[] _heap;
	public bool[] Used => _used;
	public bool[] _used;

	private int _memoryused = 0;
	private int _memContainerSize = 8;//2^this. Whenever we need more memory we copy the array into an arry that's the next power of two up.
	
	//Object GetGlobal(int)
	//void SetGlobal(int, object)
	public Memory()
	{
		var size = 1 << _memContainerSize;
		_heap = new byte[size];
		_used = new bool[size];
	}

	public ScrubMemoryError? SetByte(byte b, int pos)
	{
		if (!_used[pos])
		{
			_heap[pos] = b;
			_used[pos] = true;
			return null;
		}
		return new ScrubMemoryError("Memory at location {pos} is use"!);
	}

	public ScrubMemoryError? SetBytes(byte[] b, int pos)
	{
		//obviously this could be way faster with array.copy or such.
		//but i haven't actually decided how memory works yet.
		//this is all basically placeholder so i can tinker elsewhere.
		if (pos + b.Length > _memContainerSize)
		{
			GrowContainer();
		}
		for (int i = 0; i < b.Length; i++)
		{
			SetByte(b[i], pos + i);
		}

		return null;
	}

	public byte GetByte(int pos)
	{
		return _heap[pos];
	}
	public byte[] GetBytes(int pos, int length)
	{
		var b = new ArraySegment<byte>(_heap, pos, length);
		//creates a copy. we don't want to bother doing this.
		return b.ToArray();
	}

	private void GrowContainer()
	{
		_memContainerSize++;
		long size = 1 << _memContainerSize;
		var h = new byte[size];
		var u = new bool[size];
		Array.Copy(_heap,h,_heap.Length);
		Array.Copy(_used, u, _heap.Length);
		_heap = h;
		_used = u;
	}
}