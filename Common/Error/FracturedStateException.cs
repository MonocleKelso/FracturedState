using System;

namespace FracturedState.Game
{
    [Serializable]
    public class FracturedStateException : ApplicationException
    {
        public FracturedStateException() { }
        public FracturedStateException(string message) : base(message) { }
        public FracturedStateException(string message, Exception inner) : base(message, inner) { }
    }

    [Serializable]
    public class SyncException : ApplicationException
    {
        const string msg = "A synchronization error has occurred. You may continue to play but strange behavior might be encountered.";

        public SyncException(Exception inner) : base(msg, inner) { }
    }
}