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
    }
}