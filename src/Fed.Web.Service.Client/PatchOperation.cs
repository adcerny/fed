using Newtonsoft.Json;

namespace Fed.Web.Service.Client
{
    public class PatchOperation
    {
        private readonly string Op;
        private readonly string Path;
        private readonly object Value;

        private PatchOperation(string op, string path, object value)
        {
            Op = op;
            Path = path;
            Value = value;
        }

        public static PatchOperation CreateRemove(string path, object value) => new PatchOperation("remove", path, value);
        public static PatchOperation CreateAdd(string path, object value) => new PatchOperation("add", path, value);
        public static PatchOperation CreateReplace(string path, object value) => new PatchOperation("replace", path, value);

        public override string ToString() =>
            $"{{ \"op\": \"{Op}\", \"path\": \"{Path}\", \"value\":  {JsonConvert.SerializeObject(Value)}}}";
    }
}
