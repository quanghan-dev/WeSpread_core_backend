namespace BLL.Service
{
    public interface ILogger
    {
        void Information(string message);

        void Warning(string message);

        void Debug(string message);

        void Error(string message);

    }
}
