using Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Hangfire;
using Hangfire.Storage;

namespace DummyClassLib
{
    public class Job : IJob
    {
        private readonly ILogger<Job> _logger;
        private readonly IOptionsMonitor<SubConfig> test;


        public Job(ILogger<Job> logger, IOptionsMonitor<SubConfig> test)
        {
            _logger = logger;
            this.test = test;

        }

        public string Deneme()
        {
            Console.WriteLine(test.CurrentValue.JobName + "_____" + test.CurrentValue.JobInterval);
            return JsonSerializer.Serialize(test.CurrentValue);
        }
    }

    public class FakeService : IFakeService
    {
        private readonly IOptionsMonitor<SubConfig> _test;
        private readonly IJob _job;
        private SubConfig json;
        public FakeService(IOptionsMonitor<SubConfig> test, IJob job)
        {
            _test = test;
            _job = job;
            _test.OnChange(ConfigChanged);
        }

        private void ConfigChanged(SubConfig arg1, string arg2)
        {
            var serialize = arg1;
            if (json != serialize)
            {
                
                using (var connection = JobStorage.Current.GetConnection())
                {
                    foreach (var recurringJob in connection.GetRecurringJobs())
                    {
                        RecurringJob.RemoveIfExists(recurringJob.Id);
                    }
                }
                RegisterJob();
                json = serialize;
            }
        }

        public void RegisterJob()
        {

            RecurringJob.AddOrUpdate(_test.CurrentValue.JobName, () => _job.Deneme()
                , _test.CurrentValue.JobInterval);
        }
    }
}
