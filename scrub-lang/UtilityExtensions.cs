using System.Text;

namespace scrub_lang;

public static class UtilityExtensions
{
	//I got this from other projets of mine. if I remember, stack overflow originally. Curious, I was looking into it and it seems to be from the "MoreLINQ" package wholesale. which is apache 2.0.
	//So... I should re-write it anyway. Also I don't need generics for this..
	public static string ToDelimitedString<TSource>(this IEnumerable<TSource> source, string delimiter = ",")
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		if (delimiter == null) throw new ArgumentNullException(nameof(delimiter));
		return ToDelimitedStringImpl(source, delimiter, (sb, e) => sb.Append(e));
	}

	static string ToDelimitedStringImpl<T>(IEnumerable<T> source, string delimiter,
		Func<StringBuilder, T, StringBuilder> append)
		{
			var sb = new StringBuilder();
			var i = 0;

			foreach (var value in source)
			{
				if (i++ > 0)
					_ = sb.Append(delimiter);
				_ = append(sb, value);
			}

			return sb.ToString();
		}
}