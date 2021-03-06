using System;

namespace Angelo.Jobs
{
	public interface IJobFactory
	{
		object Create(Type type);
	}

	public static class JobFactoryExtensions
	{
		public static T Create<T>(this IJobFactory @this)
			=> (T)@this.Create(typeof(T));
	}
}
