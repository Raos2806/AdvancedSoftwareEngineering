using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIC_Controller
{

    public interface Iinterrupt
    {
        void InterruptFlag();
        void InterruptMaker();
    }

    public class Interrupt: Iinterrupt
    {
        Variablen var;
        IstackPush stackPush;
        Itimer timer;

        public Interrupt(Variablen var, IstackPush stackPush, Itimer timer)
        {
            this.var = var;
            this.stackPush = stackPush;
            this.timer = timer;
        }

        public void InterruptFlag()
        {
            if (var.T0_interrupt)
            {
                var.register[0, 11] |= 0x04; //Timer-Interrupt-Flag setzen
                var.T0_interrupt = false;   //deaktivieren, damit nicht dauerhaft auslöst
            }

            if (var.RB0_flag)
            {
                var.register[0, 11] |= 0x02; //INTF-Bit setzen (RB0-Interrupt)
                var.RB0_flag = false;     //deaktivieren, damit nicht dauerhaft auslöst
            }

            if (var.RB4_7_changed)
            {
                var.register[0, 11] |= 0x01; //RBIF-Bit setzen
                var.RB4_7_changed = false;  //deaktivieren, damit nicht dauerhaft auslöst
            }
        }

        public void InterruptMaker()
        {
            InterruptFlag();    //muss hier auch stehen, da InterruptFlag() erst in UpdateUI() sonst aufgerufen werden würde, was idR nach timerIncrease() steht.
                                //Somit würde ohne diese Zeile ggf. ein Interrupt nicht ausgelöst werden.

            if (((var.register[0, 11] & 0x20) == 32) && ((var.register[0, 11] & 0x04) == 4)) //T0IE? Timer-Interrupt-Flag (T0-Interrupt)?
            {
                //kann NICHT aus SLEEP wecken!

                if ((var.register[0, 11] & 0x80) == 0x80)    //Prüfen, ob GIE (INTCON Bit 7) gesetzt ist, um Interrupts allgemein überhaupt an CPU weiterzuleiten
                {
                    stackPush.PushStack(var.pc); //Rückkehradresse auf Stack pushen
                    var.pc = 0x04;    //Sprung zu Zeile 4, in der die ISRs stehen müssen

                    var.register[0, 11] &= 0b0111_1111;  //GIE Null setzen, damit nicht in Endlosschleife Interrupts ausgelöst werden

                    timer.UpdateLaufzeit(2);   //Schreiben auf Stack + zu 04h springen -> 2 cycles
                }
            }

            if (((var.register[0, 11] & 0x10) == 16) && ((var.register[0, 11] & 0x02) == 2)) //INTE aktiviert? INTF (RB0-Interrupt)?
            {



                if ((var.register[0, 11] & 0x80) == 0x80)    //Prüfen, ob GIE (INTCON Bit 7) gesetzt ist, um Interrupts allgemein überhaupt an CPU weiterzuleiten
                {
                    stackPush.PushStack(var.pc); //Rückkehradresse auf Stack pushen
                    var.pc = 0x04;    //Sprung zu Zeile 4, in der die ISRs stehen müssen

                    var.register[0, 11] &= 0b0111_1111;  //GIE Null setzen, damit nicht in Endlosschleife Interrupts ausgelöst werden

                    timer.UpdateLaufzeit(2);   //Schreiben auf Stack + zu 04h springen -> 2 cycles
                }
            }

            if (((var.register[0, 11] & 0x08) == 8) && ((var.register[0, 11] & 0x01) == 1))  //RBIE? RBIF (RB7:4-Interrupt)?
            {

                if ((var.register[0, 11] & 0x80) == 0x80)    //Prüfen, ob GIE (INTCON Bit 7) gesetzt ist, um Interrupts allgemein überhaupt an CPU weiterzuleiten
                {
                    stackPush.PushStack(var.pc); //Rückkehradresse auf Stack pushen
                    var.pc = 0x04;    //Sprung zu Zeile 4, in der die ISRs stehen müssen

                    var.register[0, 11] &= 0b0111_1111;  //GIE Null setzen, damit nicht in Endlosschleife Interrupts ausgelöst werden

                    timer.UpdateLaufzeit(2);   //Schreiben auf Stack + zu 04h springen -> 2 cycles
                }
            }
        }
    }
}
