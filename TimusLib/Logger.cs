using log4net;
using log4net.Config;

namespace TimusLib
{
    public static class Logger
    {
        public static ILog Log = LogManager.GetLogger("LOGGER");

        public static void InitLogger()
        {
            XmlConfigurator.Configure();
        }
    }
}