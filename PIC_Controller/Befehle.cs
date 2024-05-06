using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIC_Controller
{
    public interface Ibefehle
    {
        void BefehlsSwitch(ListView listview, int hexBefehl);
        void ADDWF(int hexBefehl);
        void ANDWF(int hexBefehl);
        void CLRF(int hexBefehl);
        void CLRW();
        void COMF(int hexBefehl);
        void DECF(int hexBefehl);
        void DECFSZ(int hexBefehl);
        void INCF(int hexBefehl);
        void INCFSZ(int hexBefehl);
        void IORWF(int hexBefehl);
        void MOVF(int hexBefehl);
        void MOVWF(int hexBefehl);
        void NOP();
        void RLF(int hexBefehl);
        void RRF(int hexBefehl);
        void SUBWF(int hexBefehl);
        void SWAPF(int hexBefehl);
        void XORWF(int hexBefehl);
        void BCF(int hexBefehl);
        void BSF(int hexBefehl);
        void BTFSC(int hexBefehl);
        void ANDLW(int hexBefehl);
        void CALL(ListView listview, int hexBefehl);
        void CLRWDT();
        void ADDLW(int hexBefehl);
        void GOTO(ListView listview, int hexBefehl);
        void IORLW(int hexBefehl);
        void MOVLW(int hexBefehl);
        void RETFIE(ListView listview);
        void RETLW(ListView listview, int hexBefehl);
        void RETURN(ListView listview);
        void SLEEP();
        void SUBLW(int hexBefehl);
        void XORLW(int hexBefehl);
    }

    public class Befehle: Ibefehle
    {
        Variablen var;
        Itimer timer;
        IstackPush stackPush;
        IstackPop stackPop;
        Iinterrupt interrupt;
        Iflag flag;

        public Befehle(Variablen var, Itimer timer, IstackPush stackPush, IstackPop stackPop, Iinterrupt interrupt, Iflag flag)
        {
            this.var = var;
            this.timer = timer;
            this.stackPush = stackPush;
            this.stackPop = stackPop;
            this.interrupt = interrupt;
            this.flag = flag;
        }

        public void BefehlsSwitch(ListView listview, int hexBefehl)
        {
            if ((hexBefehl & 0x7F) == 0)
            {
                hexBefehl += var.register[0, 4];
            }
            if (hexBefehl == 0x0008) //Return
            {
                RETURN(listview);
            }
            var.befehl = hexBefehl & 0x3C00;
            if (var.befehl == 0x1000) //BCF
            {
                BCF(hexBefehl);
            }
            if (var.befehl == 0x1400) //BSF
            {
                BSF(hexBefehl);
            }
            if (var.befehl == 0x1800) //BTFSC
            {
                BTFSC(hexBefehl);
            }
            if (var.befehl == 0x1C00) //BTFSS
            {
                BTFSS(hexBefehl);
            }
            var.befehl = hexBefehl & 0x3F00;
            if (var.befehl == 0x3E00 || var.befehl == 0x3F00) //ADDLW
            {
                ADDLW(hexBefehl);
            }
            var.befehl = hexBefehl & 0x3800;
            if (var.befehl == 0x2000) //CALL
            {
                CALL(listview, hexBefehl);
            }
            var.befehl = hexBefehl & 0x3800;
            if (var.befehl == 0x2800) //GOTO
            {
                GOTO(listview, hexBefehl);
            }
            var.befehl = hexBefehl & 0x3F00;
            if (var.befehl == 0x3400 || var.befehl == 0x3500 || var.befehl == 0x3600 || var.befehl == 0x3700) //RETLW bearbeiten
            {
                RETLW(listview, hexBefehl);
            }
            else if (false) //RETFIE
            {
                RETFIE(listview);
            }
            else
            {
                if (var.pc + 1 < var.code.Count)
                {
                    //ohoh
                    listview.Items[var.code[var.pc + 1][0]].BackColor = Color.DarkOrange;
                }
            }
            var.befehl = hexBefehl & 0x3F00;
            if (var.befehl == 0x3900) //ANDLW
            {
                ANDLW(hexBefehl);
            }

            if (var.befehl == 0x3800) //IORLW
            {
                IORLW(hexBefehl);
            }
            if (var.befehl == 0x3000 || var.befehl == 0x3100 || var.befehl == 0x3200 || var.befehl == 0x3300) // MOVLW
            {
                MOVLW(hexBefehl);
            }
            if (var.befehl == 0x3C00 || var.befehl == 0x3D00) //SUBLW
            {
                SUBLW(hexBefehl);
            }
            if (var.befehl == 0x3A00) //XORLW
            {
                XORLW(hexBefehl);
            }
            if (var.befehl == 0x0700) //ADDWF
            {
                ADDWF(hexBefehl);
            }
            if (var.befehl == 0x0500) //ANDWF
            {
                ANDWF(hexBefehl);
            }
            var.befehl = hexBefehl & 0x3F80;
            if (var.befehl == 0x0180) //CLRF
            {
                CLRF(hexBefehl);
            }
            if (var.befehl == 0x0100) //CLRW
            {
                CLRW();
            }
            var.befehl = hexBefehl & 0x3F00;
            if (var.befehl == 0x0900) //COMF
            {
                COMF(hexBefehl);
            }
            if (var.befehl == 0x0300) //DECF
            {
                DECF(hexBefehl);
            }
            if (var.befehl == 0x0B00) //DECFSZ
            {
                DECFSZ(hexBefehl);
            }
            if (var.befehl == 0x0F00) //INCFSZ
            {
                INCFSZ(hexBefehl);
            }
            if (var.befehl == 0x0400) //IORWF
            {
                IORWF(hexBefehl);
            }
            if (var.befehl == 0x0800) //MOVF
            {
                MOVF(hexBefehl);
            }
            var.befehl = hexBefehl & 0x3F80;
            if (var.befehl == 0x0080) //MOVWF
            {
                MOVWF(hexBefehl);
            }
            var.befehl = hexBefehl & 0x3F00;
            if (var.befehl == 0x0000) //NOP
            {
                NOP();
            }
            if (var.befehl == 0x0D00)//RLF, bearbeiten
            {
                RLF(hexBefehl);
            }
            if (var.befehl == 0x0C00)//RRF, bearbeiten
            {
                RRF(hexBefehl);
            }
            if (var.befehl == 0x0200) //SUBWF
            {
                SUBWF(hexBefehl);
            }
            if (var.befehl == 0x0E00) //SWAPF
            {
                SWAPF(hexBefehl);
            }
            if (var.befehl == 0x0600) //XORWF
            {
                XORWF(hexBefehl);
            }

            /*
            Fehlt noch:
            -SLEEP
            -CLRWDT

            Timer
            WatchDogTimer
            */
        }
        /*private void register_sync()
        {
            for(int i = 0; i < 256; i++)
            {
                if ((i != )) {
                    register[1][i] = register[0][i];
                }
            }
        }*/

        public void ADDWF(int hexBefehl)
        {
            if ((hexBefehl & 0x0080) == 0x0000)
            {
                var.wReg += var.register[0, (hexBefehl & 0x007F)];
                var.temp = var.wReg + (var.register[0, (hexBefehl & 0x007F)] & 0x00FF);
                if (var.temp > 0x00FF)
                {
                    var.cflag = 1;
                }
                else
                {
                    var.cflag = 0;
                }
                int dcwReg = (var.wReg & 0x000F) + (var.register[0, (hexBefehl & 0x007F)] & 0x000F);
                if (dcwReg > 0x000F)
                {
                    var.dcflag = 1;
                }
                else
                {
                    var.dcflag = 0;
                }
                if (var.wReg == 0)
                {
                    var.zflag = 1;
                }
                else
                {
                    var.zflag = 0;
                }
            }
            if ((hexBefehl & 0x0080) == 0x0080)
            {
                var.register[0, (hexBefehl & 0x007F)] += var.wReg;

                var.temp = var.wReg + (var.register[0, (hexBefehl & 0x007F)] & 0x00FF);
                if (var.temp > 0x00FF)
                {
                    var.cflag = 1;
                }
                else
                {
                    var.cflag = 0;
                }
                int dcwReg = (var.wReg & 0x000F) + (var.register[0, (hexBefehl & 0x007F)] & 0x000F);
                if (dcwReg > 0x000F)
                {
                    var.dcflag = 1;
                }
                else
                {
                    var.dcflag = 0;
                }
                if (var.register[0, (hexBefehl & 0x007F)] == 0)
                {
                    var.zflag = 1;
                }
                else
                {
                    var.zflag = 0;
                }
            }

            timer.UpdateLaufzeit(1);
        }

        public void ANDWF(int hexBefehl)
        {
            if ((hexBefehl & 0x0080) == 0x0000)
            {
                var.wReg &= var.register[0, (hexBefehl & 0x007F)];
            }
            if ((hexBefehl & 0x0080) == 0x0080)
            {
                var.register[0, (hexBefehl & 0x007F)] &= var.wReg;
            }

            timer.UpdateLaufzeit(1);
        }

        public void CLRF(int hexBefehl)
        {
            var.register[0, (hexBefehl & 0x007F)] = 0;
            var.zflag = 1;

            timer.UpdateLaufzeit(1);
        }

        public void CLRW()
        {
            var.wReg = 0;
            var.zflag = 1;

            timer.UpdateLaufzeit(1);
        }

        public void COMF(int hexBefehl)
        {
            if ((hexBefehl & 0x0080) == 0x0000)
            {
                var.wReg = ~var.register[0, (hexBefehl & 0x007F)];
                if (var.wReg == 0)
                {
                    var.zflag = 1;
                }
                else
                {
                    var.zflag = 0;
                }
            }
            if ((hexBefehl & 0x0080) == 0x0080)
            {
                var.register[0, (hexBefehl & 0x007F)] = ~var.register[0, (hexBefehl & 0x007F)];
                if (var.register[0, (hexBefehl & 0x007F)] == 0)
                {
                    var.zflag = 1;
                }
                else
                {
                    var.zflag = 0;
                }
            }

            timer.UpdateLaufzeit(1);
        }

        public void DECF(int hexBefehl)
        {
            if ((hexBefehl & 0x0080) == 0x0000)
            {
                var.wReg = var.register[0, (hexBefehl & 0x007F)] - 1;
            }
            if ((hexBefehl & 0x0080) == 0x0080)
            {
                var.register[0, (hexBefehl & 0x007F)] -= 1;
            }

            timer.UpdateLaufzeit(1);
        }

        public void DECFSZ(int hexBefehl)
        {
            if ((var.register[0, (hexBefehl & 0x007F)]) != 0)
            {

                if ((hexBefehl & 0x0080) == 0x0000)
                {
                    var.temp = var.wReg;
                    var.temp = var.register[0, (hexBefehl & 0x007F)] - 1;
                    if (var.temp != 0)
                    {
                        var.wReg = var.temp;
                    }
                    else
                    {

                        var.pc++;
                    }
                }
                if ((hexBefehl & 0x0080) == 0x0080)
                {
                    var.temp = var.register[0, (hexBefehl & 0x007F)];
                    var.temp--;
                    if (var.temp != 0)
                    {
                        var.register[0, (hexBefehl & 0x007F)] -= 1;
                    }
                    else
                    {

                        var.pc++;
                    }
                }
            }

            timer.UpdateLaufzeit(2);
        }

        public void INCF(int hexBefehl)
        {
            if ((hexBefehl & 0x0080) == 0x0000)
            {
                var.wReg = (var.register[0, (hexBefehl & 0x007F)] + 1);
            }
            if ((hexBefehl & 0x0080) == 0x0080)
            {
                var.register[0, (hexBefehl & 0x007F)] += 1;
            }

            timer.UpdateLaufzeit(1);
        }

        public void INCFSZ(int hexBefehl)
        {
            if ((hexBefehl & 0x0080) == 0x0000)
            {
                var.temp = var.register[0, (hexBefehl & 0x007F)] + 1;
                if (var.temp > 0xFF)
                {
                    var.temp = 0;
                }
                if (var.temp != 0)
                {
                    var.wReg = var.temp;
                }
                else
                {
                    var.wReg = var.temp;
                    var.pc++;
                }
            }
            if ((hexBefehl & 0x0080) == 0x0080)
            {
                var.temp = var.register[0, (hexBefehl & 0x007F)] + 1;
                if (var.temp > 0xFF)
                {
                    var.temp = 0;
                }
                if (var.temp != 0)
                {
                    var.register[0, (hexBefehl & 0x007F)] = var.temp;
                }
                else
                {
                    var.register[0, (hexBefehl & 0x007F)] = var.temp;
                    var.pc++;
                }
            }

            timer.UpdateLaufzeit(2);
        }

        public void IORWF(int hexBefehl)
        {
            if ((hexBefehl & 0x0080) == 0x0000)
            {
                var.wReg |= var.register[0, (hexBefehl & 0x007F)];
                if (var.wReg == 0)
                {
                    var.zflag = 1;
                }
                else
                {
                    var.zflag = 0;
                }
            }
            if ((hexBefehl & 0x0080) == 0x0080)
            {
                var.register[0, (hexBefehl & 0x007F)] = var.wReg | var.register[0, (hexBefehl & 0x007F)];
                if (var.register[0, (hexBefehl & 0x007F)] == 0)
                {
                    var.zflag = 1;
                }
                else
                {
                    var.zflag = 0;
                }
            }

            timer.UpdateLaufzeit(1);
        }

        public void MOVF(int hexBefehl)
        {
            if ((hexBefehl & 0x0080) == 0x0000)
            {
                var.wReg = var.register[0, (hexBefehl & 0x007F)];
                if (var.wReg == 0)
                {
                    var.zflag = 1;
                }
                else
                {
                    var.zflag = 0;
                }
            }
            if ((hexBefehl & 0x0080) == 0x0080)
            {
                if (var.register[0, (hexBefehl & 0x007F)] == 0)
                {
                    var.zflag = 1;
                }
                else
                {
                    var.zflag = 0;
                }
            }

            timer.UpdateLaufzeit(1);
        }

        public void MOVWF(int hexBefehl)
        {
            var.temp = hexBefehl & 0x007F;
            var.register[0, var.temp] = var.wReg;
            if (var.temp == 0x1)
            {
                var.option = var.register[0, var.temp];

            }

            timer.UpdateLaufzeit(1);
        }

        public void NOP()
        {
            timer.UpdateLaufzeit(1);
        }

        public void RLF(int hexBefehl)
        {
            var.temp = var.register[0, (hexBefehl & 0x007F)];
            var.temp2 = var.cflag;
            var.cflag = ((var.temp & 0x0080) >> 7) & 0x1;

            if ((hexBefehl & 0x0080) == 0x0000)
            {
                var.wReg = ((var.temp << 1) & 0x00FF) + var.temp2;

            }
            if ((hexBefehl & 0x0080) == 0x0080)
            {
                var.register[0, (hexBefehl & 0x007F)] = ((var.temp << 1) & 0x00FF) + var.temp2;
            }

            timer.UpdateLaufzeit(1);
        }

        public void RRF(int hexBefehl)
        {
            var.temp = var.register[0, (hexBefehl & 0x007F)];
            var.temp2 = var.cflag;
            var.cflag = (var.temp & 0x0001);

            if ((hexBefehl & 0x0080) == 0x0000)
            {
                var.wReg = (var.temp2 << 7) + ((var.temp & 0xFE) >> 1);
            }
            if ((hexBefehl & 0x0080) == 0x0080)
            {
                var.register[0, (hexBefehl & 0x007F)] = (var.temp2 << 7) + ((var.temp & 0xFE) >> 1);
            }

            timer.UpdateLaufzeit(1);
        }

        public void SUBWF(int hexBefehl)
        {
            if ((hexBefehl & 0x0080) == 0x0000)
            {
                var.temp = var.wReg & 0xFF;
                var.wReg = var.register[0, (hexBefehl & 0x007F)] - var.wReg;

                if (var.temp > (var.register[0, (hexBefehl & 0x007F)] & 0xFF))
                {
                    var.cflag = 0;
                    var.dcflag = 0;
                }
                else
                {
                    var.cflag = 1;
                    var.dcflag = 1;
                }

                if (var.wReg == 0)
                {
                    var.zflag = 1;
                }
                else
                {
                    var.zflag = 0;
                }
            }
            if ((hexBefehl & 0x0080) == 0x0080)
            {
                var.temp = var.register[0, (hexBefehl & 0x007F)] & 0xFF;
                var.register[0, (hexBefehl & 0x007F)] = var.register[0, (hexBefehl & 0x007F)] - var.wReg;
                if ((var.wReg & 0xFF) > var.temp)
                {
                    var.cflag = 0;
                    var.dcflag = 0;
                }
                else
                {
                    var.cflag = 1;
                    var.dcflag = 1;
                }

                if (var.register[0, (hexBefehl & 0x007F)] == 0)
                {
                    var.zflag = 1;
                }
                else
                {
                    var.zflag = 0;
                }
            }

            timer.UpdateLaufzeit(1);
        }

        public void SWAPF(int hexBefehl)
        {
            int firstHalf = 0x000F & var.register[0, (hexBefehl & 0x007F)];
            int secondHalf = (0x0070 & var.register[0, (hexBefehl & 0x007F)]) >> 4;
            int swappedValue = (firstHalf << 4) | secondHalf;
            if ((hexBefehl & 0x0080) == 0x0000)
            {
                var.wReg = swappedValue;

            }
            if ((hexBefehl & 0x0080) == 0x0080)
            {
                var.register[0, (hexBefehl & 0x007F)] = swappedValue;
            }

            timer.UpdateLaufzeit(1);
        }

        public void XORWF(int hexBefehl)
        {
            if ((hexBefehl & 0x0080) == 0x0000)
            {
                var.wReg ^= var.register[0, (hexBefehl & 0x007F)];
            }
            if ((hexBefehl & 0x0080) == 0x0080)
            {
                var.register[0, (hexBefehl & 0x007F)] = var.wReg ^ var.register[0, (hexBefehl & 0x007F)];
            }

            timer.UpdateLaufzeit(1);
        }

        public void BCF(int hexBefehl)
        {
            var.temp2 = 0;
            var.temp = hexBefehl & 0x0380;
            var.temp = var.temp >> 7;
            var.temp2 = ~(1 << var.temp);
            var.register[0, (hexBefehl & 0x007F)] &= var.temp2;

            timer.UpdateLaufzeit(1);
        }

        public void BSF(int hexBefehl)
        {
            var.temp2 = 0;
            var.temp = hexBefehl & 0x0380;
            var.temp = var.temp >> 7;
            var.temp2 = 1 << var.temp;
            var.register[0, (hexBefehl & 0x007F)] |= var.temp2;

            timer.UpdateLaufzeit(1);
        }

        public void BTFSC(int hexBefehl)
        {
            var.temp2 = 0;
            var.temp = hexBefehl & 0x0380;
            var.temp = var.temp >> 7;
            var.temp2 = 1 << var.temp;
            var.temp2 = var.register[0, (hexBefehl & 0x007F)] & var.temp;
            if (var.temp2 == 0)
            {
                var.pc++;
            }

            timer.UpdateLaufzeit(1);
        }

        public void BTFSS(int hexBefehl)
        {
            var.temp2 = 0;
            var.temp = hexBefehl & 0x0380;
            var.temp = var.temp >> 7;
            var.temp2 = 1 << var.temp;
            var.temp2 = var.register[0, (hexBefehl & 0x007F)] & var.temp;
            if (var.temp2 == 1)
            {
                var.pc++;
            }

            timer.UpdateLaufzeit(1);
        }

        public void ANDLW(int hexBefehl)
        {
            int speicher = hexBefehl & 0x00FF;
            int mehrspeicher = var.wReg & speicher;
            var.wReg = mehrspeicher;

            timer.UpdateLaufzeit(1);
        }

        public void CALL(ListView lv, int hexBefehl)
        {
            var.stack = var.pc + 1;
            var.pc = (hexBefehl & 0x07FF) - 1;
            if (var.register[0, 0xA] > 0)
            {
                var.pc = (hexBefehl & 0x07FF) - 1;
            }
            else
            {
                var.pc = var.pc + ((var.register[0, 0xA] & 0x18) << 8);
            }

            if (var.pc >= -1 && var.pc + 1 < var.code.Count)
            {
                lv.Items[var.code[var.pc + 1][0]].BackColor = Color.DarkOrange;
            }

            timer.UpdateLaufzeit(2);
        }

        public void CLRWDT()
        {
            
        }

        public void ADDLW(int hexBefehl)
        {
            var.wReg += (hexBefehl & 0x00FF);
            var.temp = var.wReg + (hexBefehl & 0x00FF);
            if (var.temp > 0x00FF)
            {
                var.cflag = 1;
            }
            else
            {
                var.cflag = 0;
            }
            int dcwReg = (var.wReg & 0x000F) + (hexBefehl & 0x000F);
            if (dcwReg > 0x000F)
            {
                var.dcflag = 1;
            }
            else
            {
                var.dcflag = 0;
            }
            if (var.wReg == 0)
            {
                var.zflag = 1;
            }
            else
            {
                var.zflag = 0;
            }

            timer.UpdateLaufzeit(1);
        }

        public void GOTO(ListView lv, int hexBefehl)
        {
            var.pc = (hexBefehl & 0x07FF) - 1;

            if (var.pc >= -1 && var.pc + 1 < var.code.Count)
            {
                lv.Items[var.code[var.pc + 1][0]].BackColor = Color.DarkOrange;
            }

            timer.UpdateLaufzeit(2);
        }

        public void IORLW(int hexBefehl)
        {
            var.wReg |= (hexBefehl & 0x00FF);
            if (var.wReg == 0)
            {
                var.zflag = 1;
            }
            else
            {
                var.zflag = 0;
            }

            timer.UpdateLaufzeit(1);
        }

        public void MOVLW(int hexBefehl)
        {
            var.wReg = (hexBefehl & 0x00FF);
            if (var.wReg == 0)
            {
                var.zflag = 1;
            }
            else
            {
                var.zflag = 0;
            }

            timer.UpdateLaufzeit(1);
        }

        public void RETFIE(ListView lv)
        {
            if (var.pc >= -1 && var.pc + 1 < var.code.Count)
            {
                lv.Items[var.code[var.pc + 1][0]].BackColor = Color.DarkOrange;
            }

            timer.UpdateLaufzeit(2);
        }

        public void RETLW(ListView lv, int hexBefehl)
        {
            var.wReg = hexBefehl & 0x00FF;
            var.pc = var.stack - 1;

            if (var.wReg == 0)
            {
                var.zflag = 1;
            }
            else
            {
                var.zflag = 0;
            }

            if (var.pc >= -1 && var.pc + 1 < var.code.Count)
            {
                lv.Items[var.code[var.pc + 1][0]].BackColor = Color.DarkOrange;
            }

            timer.UpdateLaufzeit(2);
        }

        public void RETURN(ListView lv)
        {
            var.pc = var.stack;

            if (var.pc >= -1 && var.pc + 1 < var.code.Count)
            {
                lv.Items[var.code[var.pc + 1][0]].BackColor = Color.DarkOrange;
            }

            timer.UpdateLaufzeit(2);
        }

        public void SLEEP()
        {
            //fehlt
        }

        public void SUBLW(int hexBefehl)
        {
            var.wReg = (hexBefehl & 0x00FF) - var.wReg;
            var.temp = (hexBefehl & 0x00FF) - var.wReg;
            if (var.temp > 0)
            {
                var.cflag = 1;
            }
            else
            {
                var.cflag = 0;
            }
            int dcwReg = (hexBefehl & 0x000F) - (var.wReg & 0x000F);
            if (dcwReg > 0)
            {
                var.dcflag = 1;
            }
            else
            {
                var.dcflag = 0;
            }
            if (var.wReg == 0)
            {
                var.zflag = 1;
            }
            else
            {
                var.zflag = 0;
            }

            timer.UpdateLaufzeit(1);
        }

        public void XORLW(int hexBefehl)
        {
            var.wReg ^= (hexBefehl & 0x00FF);
            if (var.wReg == 0)
            {
                var.zflag = 1;
            }
            else
            {
                var.zflag = 0;
            }

            timer.UpdateLaufzeit(1);
        }
    }
}
