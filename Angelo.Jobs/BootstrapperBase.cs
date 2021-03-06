using System;
using System.Linq;
using System.Threading.Tasks;
using Angelo.Jobs.Models;
using Angelo.Jobs.Server;

namespace Angelo.Jobs
{
	public abstract class BootstrapperBase : IBootstrapper
	{
		public BootstrapperBase(
			JobsOptions options,
			IStorage storage,
			IProcessingServer server)
		{
			Options = options;
			Storage = storage;
			Server = server;
		}

		protected JobsOptions Options { get; }

		protected IStorage Storage { get; }

		protected IProcessingServer Server { get; }

		public async Task BootstrapAsync()
		{
			await Storage.InitializeAsync();
			await WorkOutCronJobs();
			await BootstrapCore();
			Server.Start();
		}

		public async Task WorkOutCronJobs()
		{
			var entries = Options.CronJobRegistry?.Build() ?? Enumerable.Empty<CronJobRegistry.Entry>().ToArray();
			using (var connection = Storage.GetConnection())
			{
				var currentJobs = await connection.GetCronJobsAsync();
				await WorkOutCronJobsCore(connection, entries, currentJobs);
			}
		}

		public virtual async Task WorkOutCronJobsCore(IStorageConnection connection, CronJobRegistry.Entry[] entries, CronJob[] currentJobs)
		{
			if (entries.Length != 0)
			{
				// Add or update jobs
				foreach (var entry in entries)
				{
					var cronJob = currentJobs.FirstOrDefault(j => j.Name == entry.Name);
					var updating = cronJob != null;
					if (!updating)
					{
						cronJob = new CronJob
						{
							Name = entry.Name,
							TypeName = entry.JobType.AssemblyQualifiedName,
							Cron = entry.Cron,
							LastRun = DateTime.MinValue
						};
						await connection.StoreJobAsync(cronJob);
					}
					else
					{
						cronJob.TypeName = entry.JobType.AssemblyQualifiedName;
						cronJob.Cron = entry.Cron;
						await connection.UpdateCronJobAsync(cronJob);
					}
				}
			}

			// Delete old jobs
			foreach (var oldJob in currentJobs.Where(j => !entries.Any(e => e.Name == j.Name)))
			{
				await connection.RemoveCronJobAsync(oldJob.Name);
			}
		}

		public virtual Task BootstrapCore() => Task.FromResult(0);
	}
}
