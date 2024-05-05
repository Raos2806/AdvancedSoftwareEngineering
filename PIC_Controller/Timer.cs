using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace PIC_Controller
{
    public interface Itimer
    {
        void UpdateLaufzeit(int cycle);
        void TMR0();
        int SetVorteiler();
    }

    public class Timer: Itimer
    {
        Variablen var;

        public Timer(Variablen var)
        {
            this.var = var;
        }

        public void UpdateLaufzeit(int cycles)
        {
            var.zykluszeit = (float)Math.Round(cycles * 4 / var.quarzFrequenz, 1);

            var.laufzeit += var.zykluszeit;

            return;
        }

        public int SetVorteiler()
        {
            string value = var.register[1, 1].ToString();
            if (var.register[1, 1].ToString() == "0") // PSA 0 für TMR0

                switch (value)
                {
                    case "000":
                        return 2;
                    case "001":
                        return 4;
                    case "010":
                        return 8;
                    case "011":
                        return 16;
                    case "100":
                        return 32;
                    case "101":
                        return 64;
                    case "110":
                        return 128;
                    case "111":
                        return 256;
                    default:
                        MessageBox.Show("Vorteiler inkorrekt!");
                        return 0;
                }
            else // PSA 1 für Watchdog
            {
                switch (value)
                {
                    case "000":
                        return 1;
                    case "001":
                        return 2;
                    case "010":
                        return 4;
                    case "011":
                        return 8;
                    case "100":
                        return 16;
                    case "101":
                        return 32;
                    case "110":
                        return 64;
                    case "111":
                        return 128;
                    default:
                        MessageBox.Show("Vorteiler inkorrekt!");
                        return 0;
                }
            }
        }

        public void TMR0()
        {
            if (var.register[1, 1].ToString() == "1") // T0CS-Bit
            {
                if (var.register[1, 1].ToString() == "1") // PSA-Bit
                {
                    var.register[0, 1]++;
                }
                else
                {
                    var.vorteiler++;

                    if (var.vorteiler == var.vorteiler_max)
                    {
                        var.vorteiler = 0;
                        var.register[0, 1]++;
                        var.vorteiler_max = SetVorteiler();
                    }

                    if (var.register[0, 1] > 0xff)
                    {
                        var.register[0, 11] |= 0b00000100;
                        var.register[0, 1] = 0x00;
                        var.T0_interrupt = true;
                    }
                }
            }
        }
    }
}
