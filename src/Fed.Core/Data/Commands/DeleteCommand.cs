using System;

namespace Fed.Core.Data.Commands
{
    public class DeleteCommand<T> : IDataOperation<bool>
    {
        public DeleteCommand(Guid id)
        {
            Id = id.ToString();
        }

        public DeleteCommand(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}
