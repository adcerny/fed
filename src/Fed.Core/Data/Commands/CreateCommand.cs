namespace Fed.Core.Data.Commands
{
    public class CreateCommand<T> : IDataOperation<T>
    {
        public CreateCommand(T obj)
        {
            Object = obj;
        }

        public T Object { get; }
    }
}