namespace MusCat.Core.Services
{
    public class Result<T>
    {
        public ResultType Type { get; set; }
        public string Error { get; set; }
        public T Data { get; set; }

        public Result(T data)
        {
            Data = data;
            Type = ResultType.Ok;
        }

        public Result(ResultType type, string error)
        {
            Type = type;
            Error = error;
        }
    }
}
