using System;

namespace Fed.Core.Data.Commands
{
    public class UpdateCommand<T> : IDataOperation<T>
    {
        public UpdateCommand(Guid id, T obj)
        {
            Id = id.ToString();
            Object = obj;
        }

        public UpdateCommand(string id, T obj)
        {
            Id = id;
            Object = obj;
        }

        public string Id { get; }
        public T Object { get; }
    }
}