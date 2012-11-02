using System;

namespace Orvid.Assembler.x86.IstructionGen
{
	/// <summary>
	/// The different types of cases the <see cref="StringCaser"/> will
	/// output in.
	/// </summary>
	public enum StringCase
	{
		/// <summary>
		/// A variation upon Camel Case, where-by
		/// the first letter is always capitolized.
		/// </summary>
		PascalCase,
		/// <summary>
		/// A style of casing where the first letter
		/// of every word, including single letter words,
		/// are uppercase, except for the first letter
		/// of the first word, which is lowercase.
		/// </summary>
		camelCase,
		/// <summary>
		/// A style where all letters are uppercase.
		/// </summary>
		UPPERCASE,
		/// <summary>
		/// A style where all letters are lowercase.
		/// </summary>
		lowercase,
	}

	public static class StringCaser
	{

		public static string Transform(string input, StringCase targetCase)
		{
			if (input.Length < 1)
				return input;
			switch (targetCase)
			{
				case StringCase.PascalCase:
					return input[0].ToString().ToUpper() + input.Substring(1);
				case StringCase.camelCase:
					return input[0].ToString().ToLower() + input.Substring(1);
				case StringCase.lowercase:
					return input.ToLower();
				case StringCase.UPPERCASE:
					return input.ToUpper();
				default:
					throw new Exception("Unknown string case!");
			}
		}

	}
}

