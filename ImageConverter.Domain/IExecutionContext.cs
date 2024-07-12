using Quartz;

namespace ImageConverter.Domain
{
    public enum ExecutionState
    {
        Collecting,
        Compressing,
        Done
    }

    public interface IExecutionContext
    {
        ExecutionState ExecutionState { get; set; }
        JobKey? JobKey { get; set; }
        ITrigger? Trigger { get; set; }
    }
}