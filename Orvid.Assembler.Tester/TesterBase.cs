using System;
using System.IO;
using System.Collections.Generic;
using Orvid.Assembler;

namespace Orvid.Assembler.Tester
{
	public enum ResultType
	{
		Success,
		BytesMismatch,
		LengthMismatch,
		ExceptionThrown,
	}

	public sealed class TestResult
	{
		public readonly ResultType Result;
		// This is only set if an exception is thrown
		// somewhere along the way. (aka. Result is ExceptionThrow)
		public readonly string FailureDetails;
		public readonly byte[] ActualBytes;
		public readonly byte[] ExpectedBytes;

		public TestResult(ResultType res, byte[] actualBytes, byte[] expectedBytes, string failureDetails = null)
		{
			Result = res;
			ActualBytes = actualBytes;
			ExpectedBytes = expectedBytes;
			FailureDetails = failureDetails;
		}
	}

	public abstract class TesterBase
	{
		private static List<TestResult> Results = new List<TestResult>(4096);
		public abstract string Name { get; }

		public void Test(Instruction instr, params byte[] expectedBytes)
		{
			try
			{
				MemoryStream strm = new MemoryStream(expectedBytes.Length);
				instr.Emit(strm);
				byte[] actualBytes = strm.ToArray();
				if (actualBytes.Length != expectedBytes.Length)
				{
					Results.Add(new TestResult(ResultType.LengthMismatch, actualBytes, expectedBytes));
					return;
				}

				for (int i = 0; i < actualBytes.Length; i++)
				{
					if (actualBytes[i] != expectedBytes[i])
					{

					}
				}
			}
			catch (Exception e)
			{
				Results.Add(
					new TestResult(
						ResultType.ExceptionThrown, 
						null, 
						expectedBytes, 
						String.Format("An exception of the type '{0}' was thrown while running the test. The message it gave was:\r\n{1}\r\n\r\nAnd the stack trace is:\r\n{2}", e.GetType().ToString(), e.Message, e.StackTrace)
					)
				);
				return;
			}
		}
	}
}

