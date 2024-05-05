using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIC_Controller
{
    public class Variablen
    {
        public List<List<int>> code = new List<List<int>>();
        public int[,] register = new int[2, 128 + 16];
        public int pc = 0;

        public int hexBefehl = 0;
        public int befehl;
        public string[] zeilen; // = new string[1024];
        public List<int> programmcode = new List<int>();
        public int stack = 0;

        public int wReg = 0x00;
        public int fsr = 0;
        public int pcl = 0;
        public int pclath = 0;
        public int status = 18;

        public int cflag = 0;
        public int dcflag = 0;
        public int zflag = 0;

        public int stackPointer = 0;
        public string[] stack1 = new string[8];

        public bool T0_interrupt = false;
        public bool RB0_flag = false;
        public bool RB4_7_changed = false;

        public bool WDTimer_aktiv = false;
        public float WDTimer = 0;
        public int vorteiler = 0x00;
        public int vorteiler_max = 0xff;

        public bool autorun = false;
        public int BP_reached = -1;
        public bool BP_skip = false;

        public float quarzFrequenz = 4.0f;
        public float laufzeit = 0;
        public float zykluszeit = 0;

        //Optionregister:
        public int option = 0xFF;
        public int intcon = 0;

        //Für Testzwecke:
        public int temp = 0;
        public int temp2 = 0;
        public int temp3 = 0;
        public int wert1 = 0;
    }
}
