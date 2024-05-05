using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIC_Controller
{
    public interface Ipin
    {
        string PinA7();
        string PinA6();
        string PinA5();
        string PinA4();
        string PinA3();
        string PinA2();
        string PinA1();
        string PinA0();
        string PinB7();
        string PinB6();
        string PinB5();
        string PinB4();
        string PinB3();
        string PinB2();
        string PinB1();
        string PinB0();
    }

    public class Pin : Ipin
    {
        Variablen var;
        Itimer timer;

        public Pin(Variablen var, Itimer timer)
        {
            this.var = var;
            this.timer = timer;
        }
        public string PinA7()
        {
            string temp = "";

            if ((var.register[0, 5] & 0x80) != 128) //Prüfung, ob es NICHT auf 1 steht
            {
                temp = "1";
                var.register[0, 5] |= 0x80; //1 
            }
            else
            {
                temp = "0";
                var.register[0, 5] &= 0x7F; //0
            }

            return temp;
        }
        public string PinA6()
        {
            string temp = "";

            if ((var.register[0, 5] & 0x40) != 64) //Prüfung, ob es NICHT auf 1 steht
            {
                temp = "1";
                var.register[0, 5] |= 0x40; //1
            }
            else
            {
                temp = "0";
                var.register[0, 5] &= 0xBF; //0
            }

            return temp;
        }
        public string PinA5()
        {
            string temp = "";

            if ((var.register[0, 5] & 0x20) != 32) //Prüfung, ob es NICHT auf 1 steht
            {
                temp = "1";
                var.register[0, 5] |= 0x20; //1
            }
            else
            {
                temp = "0";
                var.register[0, 5] &= 0xDF; //0
            }

            return temp;
        }
        public string PinA4()
        {
            string temp = "";

            if ((var.register[0, 5] & 0x10) != 16) //Prüfung, ob es NICHT auf 1 steht
            {
                temp = "1";
                var.register[0, 5] |= 0x10; //1
            }
            else
            {
                temp = "0";
                var.register[0, 5] &= 0xEF; //0
            }
            if (var.register[1, 1].ToString() == "1") // T0CS-Bit
            {
                if (var.register[1, 1].ToString() == "0") // T0SE
                {
                    if (var.register[0, 5].ToString() == "0") // RA4
                    {
                        if (var.register[1, 1].ToString() == "1") // PSA-Bit
                        {
                            var.register[0, 1]++;
                        }
                        else
                        {
                            var.vorteiler++;

                            if (var.vorteiler == var.vorteiler_max)    //Prüfung, ob Wert von TMR0 nun erhöht werden darf
                            {
                                var.vorteiler = 0;
                                var.register[0, 1]++;
                                var.vorteiler_max = timer.SetVorteiler();
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
                else
                {
                    if (var.register[0, 5].ToString() == "1") // RA4
                    {
                        if (var.register[1, 1].ToString() == "1") // PSA-Bit
                        {
                            var.register[0, 1]++;
                        }
                        else
                        {
                            var.vorteiler++;

                            if (var.vorteiler == var.vorteiler_max)    //Prüfung, ob Wert von TMR0 nun erhöht werden darf
                            {
                                var.vorteiler = 0;
                                var.register[0, 1]++;
                                var.vorteiler_max = timer.SetVorteiler();
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

            return temp;
        }
        public string PinA3()
        {
            string temp = "";

            if ((var.register[0, 5] & 0x8) != 8) //Prüfung, ob es NICHT auf 1 steht
            {
                temp = "1";
                var.register[0, 5] |= 0x8; //1
            }
            else
            {
                temp = "0";
                var.register[0, 5] &= 0xF7; //0
            }

            return temp;
        }
        public string PinA2()
        {
            string temp = "";

            if ((var.register[0, 5] & 0x4) != 4) //Prüfung, ob es NICHT auf 1 steht
            {
                temp = "1";
                var.register[0, 5] |= 0x4; //1
            }
            else
            {
                temp = "0";
                var.register[0, 5] &= 0xFB; //0
            }

            return temp;
        }
        public string PinA1()
        {
            string temp = "";

            if ((var.register[0, 5] & 0x2) != 2) //Prüfung, ob es NICHT auf 1 steht
            {
                temp = "1";
                var.register[0, 5] |= 0x2; //1
            }
            else
            {
                temp = "0";
                var.register[0, 5] &= 0xFD; //0
            }

            return temp;
        }
        public string PinA0()
        {
            string temp = "";

            if ((var.register[0, 5] & 0x1) != 1) //Prüfung, ob es NICHT auf 1 steht
            {
                temp = "1";
                var.register[0, 5] |= 0x1; //1
            }
            else
            {
                temp = "0";
                var.register[0, 5] &= 0xFE; //0
            }

            return temp;
        }
        public string PinB7()
        {
            string temp = "";

            if ((var.register[0, 6] & 0x80) != 128) //Prüfung, ob es NICHT auf 1 steht
            {
                temp = "1";
                var.register[0, 6] |= 0x80; //1
            }
            else
            {
                temp = "0";
                var.register[0, 6] &= 0x7F; //0
            }

            if ((var.register[1, 6] & 0x80) == 128) //Pin gerade Input?!
            {
                var.RB4_7_changed = true;
            }

            return temp;
        }
        public string PinB6()
        {
            string temp = "";

            if ((var.register[0, 6] & 0x40) != 64) //Prüfung, ob es NICHT auf 1 steht
            {
                temp = "1";
                var.register[0, 6] |= 0x40; //1
            }
            else
            {
                temp = "0";
                var.register[0, 6] &= 0xBF; //0
            }

            if ((var.register[1, 6] & 0x40) == 64) //Pin gerade Input?!
            {
                var.RB4_7_changed = true;
            }

            return temp;
        }
        public string PinB5()
        {
            string temp = "";

            if ((var.register[0, 6] & 0x20) != 32) //Prüfung, ob es NICHT auf 1 steht
            {
                temp = "1";
                var.register[0, 6] |= 0x20; //1
            }
            else
            {
                temp = "0";
                var.register[0, 6] &= 0xDF; //0
            }

            if ((var.register[1, 6] & 0x20) == 32) //Pin gerade Input?!
            {
                var.RB4_7_changed = true;
            }

            return temp;
        }
        public string PinB4()
        {
            string temp = "";

            if ((var.register[0, 6] & 0x10) != 16) //Prüfung, ob es NICHT auf 1 steht
            {
                temp = "1";
                var.register[0, 6] |= 0x10; //1
            }
            else
            {
                temp = "0";
                var.register[0, 6] &= 0xEF; //0
            }

            if ((var.register[1, 6] & 0x10) == 16) //Pin gerade Input?!
            {
                var.RB4_7_changed = true;
            }

            return temp;
        }
        public string PinB3()
        {
            string temp = "";

            if ((var.register[0, 6] & 0x8) != 8) //Prüfung, ob es NICHT auf 1 steht
            {
                temp = "1";
                var.register[0, 6] |= 0x8; //1
            }
            else
            {
                temp = "0";
                var.register[0, 6] &= 0xF7; //0
            }

            return temp;
        }
        public string PinB2()
        {
            string temp = "";

            if ((var.register[0, 6] & 0x4) != 4) //Prüfung, ob es NICHT auf 1 steht
            {
                temp = "1";
                var.register[0, 6] |= 0x4; //1
            }
            else
            {
                temp = "0";
                var.register[0, 6] &= 0xFB; //0
            }

            return temp;
        }
        public string PinB1()
        {
            string temp = "";

            if ((var.register[0, 6] & 0x2) != 2) //Prüfung, ob es NICHT auf 1 steht
            {
                temp = "1";
                var.register[0, 6] |= 0x2; //1
            }
            else
            {
                temp = "0";
                var.register[0, 6] &= 0xFD; //0
            }

            return temp;
        }
        public string PinB0()
        {
            string temp = "";

            if ((var.register[0, 6] & 0x1) != 1) //Prüfung, ob es NICHT auf 1 steht => steigende Taktflanke
            {
                temp = "1";

                var.register[0, 6] |= 0x1; // RB0-Bit auf 1
                if ((var.register[1, 1] & 0x40) == 64)  //Prüfung, ob "rising edge" (steigende Taktflanke) im Option Pin 6 (INTEDG) gesetzt ist
                {
                    var.RB0_flag = true;
                }
            }
            else
            {
                temp = "0";

                var.register[0, 6] &= 0xFE; // RB0-Bit auf 0 => fallende Taktflanke
                if ((var.register[1, 1] & 0x40) == 0)   //Prüfung, ob "falling edge" (fallende Taktflanke) im Option Pin 6 (INTEDG) gesetzt ist
                {
                    var.RB0_flag = true;
                }
            }

            return temp;
        }
    }
}
