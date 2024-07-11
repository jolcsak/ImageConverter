using ImageConverter.Domain;

namespace ImageConverter.Web.Server
{
    public class ExecutionContext : IExecutionContext
    {
        public ExecutionState ExecutionState { get; set; }
    }
}
