using System.Text;

namespace scrub_lang.Objects;

public class Array : Objects.Object
{
	public Object[] NativeArray => _array;
	private readonly Object[] _array;
	public Array(Object[] elements)
	{
		_array = elements;
		Length = new Integer(elements.Length);
	}

	public Integer Length { get; set; }

	public override ScrubType GetType() => ScrubType.Array;

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		sb.Append('[');
		for (var i = 0; i < _array.Length; i++)
		{
			var o = _array[i];
			sb.Append(o.ToString());
			if (i < _array.Length - 1)
			{
				sb.Append(", ");
			}
		}

		sb.Append(']');
		return sb.ToString();
	}
}