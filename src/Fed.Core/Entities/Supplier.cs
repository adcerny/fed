namespace Fed.Core.Entities
{
    public class Supplier
    {
        public Supplier(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string Name { get; set; }
    }
}
