/*
 * File: GroupException.cs
 *
 * Author: James Brown
 * Harvey Mudd College, Claremont, CA 91711.
 * Sketchers 2008.
 * 
 * Use at your own risk.  This code is not maintained and not guaranteed to work.
 * We take no responsibility for any harm this code may cause.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Grouper
{
	[Serializable]
	public class GroupException : Exception
	{
		/// <summary>
		/// Instanatiates a default GroupException
		/// </summary>
		public GroupException()
			: base()
		{
			// Nothing to do here
		}

		/// <summary>
		/// Create a GroupException with the given error message
		/// </summary>
		/// <param name="message">The error message</param>
		public GroupException(string message)
			: base(message)
		{
			// Nothing to do here
		}
	}

	[Serializable]
	public class NoGroupingsFoundException : GroupException
	{
		public NoGroupingsFoundException()
			: base()
		{ }

		public NoGroupingsFoundException(string message)
			: base(message)
		{ }
	}

	public class MaximumGroupTimeExceededException : GroupException
	{
		public MaximumGroupTimeExceededException()
			: base()
		{ }

		public MaximumGroupTimeExceededException(string message)
			: base(message)
		{ }
	}
}
