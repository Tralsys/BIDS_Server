namespace BIDS.Parser.Internals;

public static class ValueListGetters
{
	public static List<int> GetIntList(ReadOnlySpan<char> str)
	{
		List<int> ret = new();

		while (!str.IsEmpty)
		{
			int sepIndex = str.IndexOf('X');

			var numStr = sepIndex < 0 ? str : str[..sepIndex];

			if (int.TryParse(numStr, out var num))
				ret.Add(num);
			else
				ret.Add(0);

			if (sepIndex < 0)
				break;

			str = str[(sepIndex + 1)..];
		}

		return ret;
	}

	public static List<double> GetDoubleList(ReadOnlySpan<char> str)
	{
		List<double> ret = new();

		while (!str.IsEmpty)
		{
			int sepIndex = str.IndexOf('X');

			var numStr = sepIndex < 0 ? str : str[..sepIndex];

			if (double.TryParse(numStr, out var num))
				_ = ret.Append(num);
			else
				_ = ret.Append(0);

			if (sepIndex < 0)
				break;

			str = str[(sepIndex + 1)..];
		}

		return ret;
	}
}

