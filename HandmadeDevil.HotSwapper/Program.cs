using System;

namespace HandmadeDevil.HotSwapper
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Look for game executable and associated resources in the main game build output dir.
            // CreateInstanceAndUnwrap a MarshalByRefObject proxy loader that will instantiate the game class
            // in a separate AppDomain (that has shadow copy enabled)

            // http://stackoverflow.com/questions/17225276/create-custom-appdomain-and-add-assemblies-to-it/17324102#17324102
            // http://stackoverflow.com/questions/658498/how-to-load-an-assembly-to-appdomain-with-all-references-recursively
            // http://stackoverflow.com/questions/2100296/how-can-i-switch-net-assembly-for-execution-of-one-method/2101048#2101048

        }
    }
}
