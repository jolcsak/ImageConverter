using ImageConverter.Domain;
using Quartz;

namespace ImageConverter.Web.Server
{
    public class ExecutionContext : IExecutionContext
    {
        public JobKey? JobKey { get; set; }
        public ITrigger? Trigger { get; set; }

        public ExecutionState ExecutionState { get; set; }
    }
}
