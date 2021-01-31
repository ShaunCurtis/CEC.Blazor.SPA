/// =================================
/// Author: Shaun Curtis, Cold Elm
/// License: MIT
/// ==================================

namespace CEC.Blazor.Data
{
    /// <summary>
    /// Class defining the return information from a CRUD database operation
    /// </summary>
    public class DbTaskResult
    {
        public string Message { get; set; } = "New Object Message";

        public MessageType Type { get; set; } = MessageType.None;

        public bool IsOK { get; set; } = true;

        public int NewID { get; set; } = 0;

    }
}
