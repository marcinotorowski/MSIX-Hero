namespace Otor.MsixHero.Cli.Executors
{
    public static class StandardExitCodes
    {
        public static int ErrorSuccess = 0;

        public static int ErrorGeneric = 1;

        public static int ErrorParameter = 2;
        
        public static int ErrorFormat = 3;

        public static int ErrorSettings = 10;

        public static int ErrorMustBeAdministrator = 11;

        public static int ErrorResourceExists = 12;

        public static int ErrorNotSupported = 13;
    }
}
