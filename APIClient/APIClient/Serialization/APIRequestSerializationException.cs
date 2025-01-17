using System;
using System.Collections.Generic;

namespace APIClient.Serialization
{
    public class APIRequestSerializationException : Exception
    { 
        public IReadOnlyList<string> MissingParameters { get; }
        
        public APIRequestSerializationException(string message, IReadOnlyList<string> missingParameters = null)
        : base(message)
        {
            MissingParameters = missingParameters ?? new List<string>();
        }
    }
}