using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIC_Controller
{
    public interface Iclicker
    {
        void WDTEchange(bool WDTEValue);
        void UpdateQuarzFrequenz(int selectedIndex);
        void wRegUpdate(string txt);
    }

    public class Clicker: Iclicker
    {
        Variablen var;

        public Clicker(Variablen var)
        {
            this.var = var;
        }

        public void WDTEchange(bool WDTEValue)
        {
            var.WDTimer_aktiv = WDTEValue;
        }

        public void UpdateQuarzFrequenz(int selectedIndex)
        {
            switch (selectedIndex)
            {
                case 0: //1 kHz
                    var.quarzFrequenz = 0.001f;
                    break;
                case 1: //25 kHz
                    var.quarzFrequenz = 0.025f;
                    break;
                case 2: //100 kHz
                    var.quarzFrequenz = 0.1f;
                    break;
                case 3: //200 kHz
                    var.quarzFrequenz = 0.2f;
                    break;
                case 4: //455 kHz
                    var.quarzFrequenz = 0.455f;
                    break;
                case 5: //2 MHz
                    var.quarzFrequenz = 2.0f;
                    break;
                case 6: //4 MHz
                    var.quarzFrequenz = 4.0f;
                    break;
                case 7: //10 MHz
                    var.quarzFrequenz = 10.0f;
                    break;

                default:
                    MessageBox.Show("Falsche Eingabe!");
                    break;
            }

            return;
        }

        public void wRegUpdate(string txt)
        {
            int wRegUpdated;

            if (txt.Length == 1 && int.TryParse(txt, System.Globalization.NumberStyles.HexNumber, null, out wRegUpdated))   //Überprüfung, ob in Hex umgewandelt werden kann. Falls ja: True und Wert wird in wRegUpdated gespeichert. ansonsten false.
            {
                var.wReg = (byte)wRegUpdated;
            }
            else if (txt.Length == 2 && int.TryParse(txt, System.Globalization.NumberStyles.HexNumber, null, out wRegUpdated))
            {
                var.wReg = (byte)wRegUpdated;
            }
            else
            {
                MessageBox.Show("Bitte einen gültigen Wert (0-FF) für das W-Register eingeben.");
            }
        }
    }
}
