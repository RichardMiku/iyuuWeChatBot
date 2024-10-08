namespace ricwxbot.Functions
{
    public class loginfo
    {
        private readonly ILogger<loginfo> _logger;
        public loginfo (Logger<loginfo> logger)
        {
            _logger = logger;
        }

        public void InfoLogOut(string message)
        {
            _logger.LogInformation(message);
        }
    }
}
