using Fed.Core.Common.Interfaces;
using System;

namespace Fed.Core.Common
{
    public class RandomIdGenerator : IIdGenerator
    {
        public string GenerateId() =>
            Guid.NewGuid().ToString("n").Substring(0, 8).ToUpper();
    }
}