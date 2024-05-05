namespace PIC_Controller
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Variablen var = new Variablen();
            IstackPush stackPush = new StackPush(var);
            IstackPop stackPop = new StackPop(var);
            Itimer timer = new Timer(var);
            Iinterrupt interrupt = new Interrupt(var, stackPush, timer);
            Iflag flag = new Flag();
            IfileHandler file = new FileHandler(var);
            IcommandManager command = new CommandManager(var, timer, stackPush, stackPop, flag);
            Iclicker clicker = new Clicker(var);
            Ipin pin = new Pin(var, timer);
            


            ApplicationConfiguration.Initialize();
            Application.Run(new Form1(var, command, file, clicker, pin, flag));
        }
    }
}