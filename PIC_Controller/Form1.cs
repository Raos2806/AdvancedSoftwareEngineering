using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Windows.Forms;



namespace PIC_Controller
{
    public partial class Form1 : Form
    {
        private List<List<int>> code;
        int[,] register = new int[2, 128 + 16];
        public int pc = 0;

        int hexBefehl = 0;
        int befehl;
        string[] zeilen;
        List<int> programmcode;
        int stack = 0;

        public int wReg = 0x00;
        int fsr = 0;
        int pcl = 0;
        int pclath = 0;
        int status = 18;

        int cflag = 0;
        int dcflag = 0;
        int zflag = 0;

        int stackPointer = 0;
        string[] stack1 = new string[8];

        bool T0_interrupt = false;
        bool RB0_flag = false;
        bool RB4_7_changed = false;


        bool WDTimer_aktiv = false;
        float WDTimer = 0;
        int vorteiler = 0x00;
        int vorteiler_max = 0xff;

        //Optionregister:
        int option = 0xFF;

        int intcon = 0;

        int temp;
        int temp2;
        int temp3;
        int wert1;

        bool autorun = false;
        int BP_reached = -1;
        bool BP_skip = false;

        float quarzFrequenz = 4.0f;
        float laufzeit;
        float zykluszeit;

        public Form1()
        {
            InitializeComponent();
            quarzFreq_comboBox.SelectedIndex = 6; //Dropdown auf 4MHz stellen
            PopulateDataGridView();
            zurücksetzen();
        }

        private void PopulateDataGridView()
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    register[i, j] = 0;
                }
            }
            register[0, 6] = 0x80;
            register[0, 8] = 0x80;
            register[0, 9] = 0x80;
            register[0, 134] = 0xFF;
            register[0, 135] = 0xFF;

            int regseite = 0;
            int regadresse = 0;
            for (int row = 0; row < 32; row++)
            {
                dataGridView1.Rows.Add();

                for (int col = 0; col < 8; col++)
                {
                    if (((row + 1) * (col + 1)) > 128)
                    {
                        regseite = 1;
                    }
                    else
                    {
                        regseite = 0;
                    }
                    regadresse = ((row * 10) + col) - (2 * row);
                    if (regadresse > 127)
                    {
                        regadresse = 0;
                    }
                    dataGridView1.Rows[row].Cells[0].Value = (row * 8).ToString("X2");
                    dataGridView1.Rows[row].Cells[col + 1].Value = register[regseite, regadresse + 1];
                }
            }

        }
        private void updateDataGridView()
        {
            int regseite = 0;
            int regadresse = 0;
            for (int row = 0; row < 32; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (((row + 1) * (col + 1)) > 128)
                    {
                        regseite = 1;
                    }
                    else
                    {
                        regseite = 0;
                    }
                    regadresse = ((row * 10) + col) - (2 * row);
                    if (regadresse > 127)
                    {
                        regadresse = 0;
                    }
                    dataGridView1.Rows[row].Cells[0].Value = (row * 8).ToString("X2");
                    dataGridView1.Rows[row].Cells[col + 1].Value = (register[regseite, regadresse] & 0xFF);
                }
            }
        }




        private List<string> Einlesen()
        {
            if (zeilen?.Length > 0)
            {
                Array.Clear(zeilen, 0, zeilen.Length);
            }
            var fileContent = string.Empty;
            //var filePath = string.Empty;

            List<string> lines = new List<string>();

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Reset();
                openFileDialog.Filter = "MASM Listing (*.LST)|*.LST";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {

                    var filePath = openFileDialog.FileName;

                    var fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream, Encoding.Default))
                    {
                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();

                            if (string.IsNullOrWhiteSpace(line.Substring(0, 1)))
                            {
                                //line = "\t" + line;
                                lines.Add("          " + line);
                            }
                            else
                            {
                                lines.Add(line);
                            }
                        }
                    }

                    zeilen = File.ReadAllLines(filePath);

                    programmcode = new List<int>();

                    foreach (string zeile in zeilen)
                    {
                        if (zeile.Length >= 8 && !string.IsNullOrWhiteSpace(zeile.Substring(0, 1)))
                        {
                            string extractedChars = zeile.Substring(5, 4);
                            int intValue = Convert.ToInt32(extractedChars, 16);
                            programmcode.Add(intValue);
                        }
                    }
                }
                else
                {
                    return null;
                }
            }

            return lines;
        }

        private void Einlesen_Btn_Click(object sender, EventArgs e)
        {
            Size laenge = new Size(0, 0);
            Font font = new Font("Arial", 10, FontStyle.Regular);

            List<string> ausgabe = Einlesen();

            if (ausgabe != null)
            {
                Ausgabe_LV.Items.Clear();
                for (int i = 0; i < ausgabe.Count; i++)
                {
                    if (laenge.Width < TextRenderer.MeasureText(ausgabe[i], font).Width)
                    {
                        laenge.Width = TextRenderer.MeasureText(ausgabe[i], font).Width;
                        Ausgabe_LV.Columns[0].Width = laenge.Width;
                    }

                    Ausgabe_LV.Items.Add(ausgabe[i]);
                }

                Aufsplitten(ausgabe);
            }
        }

        private void Aufsplitten(List<string> lines)
        {
            bool start = true;
            string codezeile;
            code = new List<List<int>>();

            for (int i = 0; i < lines.Count; i++)
            {
                if (!lines[i].StartsWith("\t") && !lines[i].StartsWith(" "))
                {
                    List<int> zeile = new List<int>();
                    codezeile = lines[i];
                    zeile.Add(i);
                    zeile.Add(Convert.ToInt32(codezeile.Remove(4, codezeile.Length - 4), 16));  //ersten 4 Zeichen
                    codezeile = codezeile.Remove(0, 5);                                         //ohne Leerzeichen
                    zeile.Add(Convert.ToInt32(codezeile.Remove(4, codezeile.Length - 4), 16));  //nächsten 4 Zeichen


                    code.Add(zeile);
                }
            }

            zurücksetzen();

            return;
        }

        private bool Einzelschritt()
        {
            if (pc < code.Count && BP_reached != pc)
            {
                for (int i = 0; i < Ausgabe_LV.Items.Count; i++)
                {
                    Ausgabe_LV.Items[i].BackColor = Color.White;
                    Ausgabe_LV.Items[i].ForeColor = Color.Black;

                    if (Ausgabe_LV.Items[i].Checked && Ausgabe_LV.Items[i].Index == code[pc][0] && BP_reached != pc)
                    {
                        BP_reached = pc;
                        Ausgabe_LV.Items[i].BackColor = Color.Red;
                        Ausgabe_LV.Items[i].ForeColor = Color.White;
                    }
                }

                if (BP_reached == pc)
                {
                    return true;
                }
                else
                {
                    Ausgabe_LV.Items[code[pc][0]].BackColor = Color.Green;
                }
            }
            else if (pc < code.Count && BP_reached == pc)
            {
                Ausgabe_LV.Items[code[pc][0]].BackColor = Color.Green;
                Ausgabe_LV.Items[code[pc][0]].ForeColor = Color.Black;

                if (BP_skip)
                {
                    Ausgabe_LV.Items[code[pc][0]].BackColor = Color.White;
                    pc += 1;
                    Ausgabe_LV.Items[code[pc][0]].BackColor = Color.DarkOrange;
                    BP_reached = -1;
                    return false;
                }
                else
                {
                    BP_reached = -1;
                }
            }
            else
            {
                return true;
            }

            hexBefehl = programmcode[pc];
            befehlsSwitch(hexBefehl);

            pc += 1;

            update();

            return false;
        }

        private void AutoRun()
        {
            if (BP_reached != -1 && autorun)
            {
                Einzelschritt();
            }

            while (autorun)
            {
                if (Einzelschritt()) //stoppen, wenn Breakpoint oder Ende erreicht wurde
                {
                    autorun = false;

                    update();

                    break;
                }

                //timer zum kurz warten wegen UI
            }

            return;
        }

        private void Automatic_Btn_MouseClick(object sender, MouseEventArgs e)
        {
            //Einzelschritt_Btn.Enabled = false;
            //Automatic_Btn.Enabled = false;

            autorun = !autorun;

            AutoRun();

            return;
        }

        private void Ausgabe_LV_MouseClick(object sender, MouseEventArgs e)
        {
            /*
            Ausgabe_LV.Items[Ausgabe_LV.SelectedIndices[0]].Checked = !Ausgabe_LV.Items[Ausgabe_LV.SelectedIndices[0]].Checked;
            if (Ausgabe_LV.Items[Ausgabe_LV.SelectedIndices[0]].Checked)
            {
                Ausgabe_LV.Items[Ausgabe_LV.SelectedIndices[0]].BackColor = Color.Red;
            }
            else
            {
                Ausgabe_LV.Items[Ausgabe_LV.SelectedIndices[0]].BackColor = Color.White;
            }
            */

            return;
        }


        private void Einzelschritt_Btn_Click(object sender, EventArgs e)
        {
            Einzelschritt();
        }

        private void update()
        {
            if (pc >= code?.Count)
            {
                Einzelschritt_Btn.Enabled = false;
                Automatic_Btn.Enabled = false;
            }
            else if (code?.Count > 0)
            {
                Einzelschritt_Btn.Enabled = true;
                Automatic_Btn.Enabled = true;
            }

            updateWReg();
            updateFlags();
            updatePC();
            Update_RA();
            Update_RB();
            updateDataGridView();

            laufzeit_TB.Text = laufzeit.ToString() + " µs";
            zykluszeit_TB.Text = zykluszeit.ToString() + " µs";
            dataGridView1.AutoResizeRows();

            if (autorun)
            {
                Automatic_Btn.ForeColor = Color.Green;
            }
            else
            {
                Automatic_Btn.ForeColor = Color.Red;
            }
        }
        private void updateWReg()
        {
            textBox1.Text = (wReg & 0xFF).ToString("X2");
        }
        private void updateFlags()
        {
            textBox2.Text = cflag.ToString("X");
            textBox3.Text = dcflag.ToString("X");
            textBox4.Text = zflag.ToString("X");
            textBox5.Text = status.ToString("X");
        }
        private void updatePC()
        {
            pcl = register[0, 0x2];
            pclath = register[0, 0xA];
            register[0, 2] = pcl;
            register[0, 3] = status;
            register[0, 130] = option;
            option_out.Text = register[0, 130].ToString("X2");
            intcon_out.Text = register[0, 0xB].ToString("X2");
            //register[0, 6] = 0x80;
            //register[0, 8] = 0x80;
            //register[0, 9] = 0x80;
            textBox7.Text = pcl.ToString("X");
            textBox8.Text = pclath.ToString("X");
            textBox9.Text = stack.ToString("X");
            textBox10.Text = pc.ToString();
        }
        private void Update_RA()
        {
            if ((register[0, 134] & 0x80) == 128) TrisA7_label.Text = "i";
            else
            {
                TrisA7_label.Text = "o";
                PinA7_button.Text = ((register[0, 5] & 0x80) / 0x80).ToString("X");
            }
            if ((register[0, 134] & 0x40) == 64) TrisA6_label.Text = "i";
            else
            {
                TrisA6_label.Text = "o";
                PinA6_button.Text = ((register[0, 5] & 0x40) / 0x40).ToString("X");
            }
            if ((register[0, 134] & 0x20) == 32) TrisA5_label.Text = "i";
            else
            {
                TrisA5_label.Text = "o";
                PinA5_button.Text = ((register[0, 5] & 0x20) / 0x20).ToString("X");
            }
            if ((register[0, 134] & 0x10) == 16) TrisA4_label.Text = "i";
            else
            {
                TrisA4_label.Text = "o";
                PinA4_button.Text = ((register[0, 5] & 0x10) / 0x10).ToString("X");
            }
            if ((register[0, 134] & 0x08) == 8) TrisA3_label.Text = "i";
            else
            {
                TrisA3_label.Text = "o";
                PinA3_button.Text = ((register[0, 5] & 0x08) / 0x08).ToString("X");
            }
            if ((register[0, 134] & 0x04) == 4) TrisA2_label.Text = "i";
            else
            {
                TrisA2_label.Text = "o";
                PinA2_button.Text = ((register[0, 5] & 0x04) / 0x04).ToString("X");
            }
            if ((register[0, 134] & 0x02) == 2) TrisA1_label.Text = "i";
            else
            {
                TrisA1_label.Text = "o";
                PinA1_button.Text = ((register[0, 5] & 0x02) / 0x02).ToString("X");
            }
            if ((register[0, 134] & 0x01) == 1) TrisA0_label.Text = "i";
            else
            {
                TrisA0_label.Text = "o";
                PinA0_button.Text = ((register[0, 5] & 0x01) / 0x01).ToString("X");
            }
        }

        private void Update_RB()
        {
            if ((register[0, 135] & 0x80) == 128) TrisB7_label.Text = "i";
            else
            {
                TrisB7_label.Text = "o";
                PinB7_button.Text = ((register[0, 6] & 0x80) / 0x80).ToString("X");
            }
            if ((register[0, 135] & 0x40) == 64) TrisB6_label.Text = "i";
            else
            {
                TrisB6_label.Text = "o";
                PinB6_button.Text = ((register[0, 6] & 0x40) / 0x40).ToString("X");
            }
            if ((register[0, 135] & 0x20) == 32) TrisB5_label.Text = "i";
            else
            {
                TrisB5_label.Text = "o";
                PinB5_button.Text = ((register[0, 6] & 0x20) / 0x20).ToString("X");
            }
            if ((register[0, 135] & 0x10) == 16) TrisB4_label.Text = "i";
            else
            {
                TrisB4_label.Text = "o";
                PinB4_button.Text = ((register[0, 6] & 0x10) / 0x10).ToString("X");
            }
            if ((register[0, 135] & 0x08) == 8) TrisB3_label.Text = "i";
            else
            {
                TrisB3_label.Text = "o";
                PinB3_button.Text = ((register[0, 6] & 0x08) / 0x08).ToString("X");
            }
            if ((register[0, 135] & 0x04) == 4) TrisB2_label.Text = "i";
            else
            {
                TrisB2_label.Text = "o";
                PinB2_button.Text = ((register[0, 6] & 0x04) / 0x04).ToString("X");
            }
            if ((register[0, 135] & 0x02) == 2) TrisB1_label.Text = "i";
            else
            {
                TrisB1_label.Text = "o";
                PinB1_button.Text = ((register[0, 6] & 0x02) / 0x02).ToString("X");
            }
            if ((register[0, 135] & 0x01) == 1) TrisB0_label.Text = "i";
            else
            {
                TrisB0_label.Text = "o";
                PinB0_button.Text = ((register[0, 6] & 0x01) / 0x01).ToString("X");
            }
        }

        private int SetVorteiler()
        {
            string value = register[1, 1].ToString();
            if (register[1, 1].ToString() == "0") // PSA 0 für TMR0

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

        private void updateLaufzeit(int cycles)
        {
            zykluszeit = (float)Math.Round(cycles * 4 / quarzFrequenz, 1);

            laufzeit += zykluszeit;

            return;
        }

        private void befehlsSwitch(int hexBefehl)
        {
            if((hexBefehl & 0x7F) == 0)
            {
                hexBefehl += register[0, 4];
            }
            if (hexBefehl == 0x0008) //Return
            {
                pc = stack;

                if (pc >= -1 && pc + 1 < code.Count)
                {
                    Ausgabe_LV.Items[code[pc + 1][0]].BackColor = Color.DarkOrange;
                }

                updateLaufzeit(2);
            }
            befehl = hexBefehl & 0x3C00;
            if (befehl == 0x1000) //BCF
            {
                temp2 = 0;
                temp = hexBefehl & 0x0380;
                temp = temp >> 7;
                temp2 = ~(1 << temp);
                register[0, (hexBefehl & 0x007F)] &= temp2;

                updateLaufzeit(1);
            }
            if (befehl == 0x1400) //BSF
            {
                temp2 = 0;
                temp = hexBefehl & 0x0380;
                temp = temp >> 7;
                temp2 = 1 << temp;
                register[0, (hexBefehl & 0x007F)] |= temp2;

                updateLaufzeit(1);
            }
            if (befehl == 0x1800) //BTFSC
            {
                temp2 = 0;
                temp = hexBefehl & 0x0380;
                temp = temp >> 7;
                temp2 = 1 << temp;
                temp2 = register[0, (hexBefehl & 0x007F)] & temp;
                if (temp2 == 0)
                {
                    pc++;
                }

                updateLaufzeit(1);
            }
            if (befehl == 0x1C00) //BTFSS
            {
                temp2 = 0;
                temp = hexBefehl & 0x0380;
                temp = temp >> 7;
                temp2 = 1 << temp;
                temp2 = register[0, (hexBefehl & 0x007F)] & temp;
                if (temp2 == 1)
                {
                    pc++;
                }

                updateLaufzeit(1);
            }
            befehl = hexBefehl & 0x3F00;
            if (befehl == 0x3E00 || befehl == 0x3F00) //ADDLW
            {
                wReg += (hexBefehl & 0x00FF);
                temp = wReg + (hexBefehl & 0x00FF);
                if (temp > 0x00FF)
                {
                    cflag = 1;
                }
                else
                {
                    cflag = 0;
                }
                int dcwReg = (wReg & 0x000F) + (hexBefehl & 0x000F);
                if (dcwReg > 0x000F)
                {
                    dcflag = 1;
                }
                else
                {
                    dcflag = 0;
                }
                if (wReg == 0)
                {
                    zflag = 1;
                }
                else
                {
                    zflag = 0;
                }

                updateLaufzeit(1);
            }
            befehl = hexBefehl & 0x3800;
            if (befehl == 0x2000) //CALL
            {
                
                stack = pc + 1;
                pc = (hexBefehl & 0x07FF) - 1;
                if (register[0, 0xA] > 0)
                {
                    pc = (hexBefehl & 0x07FF)-1;
                }
                else
                {
                    pc = pc + ((register[0, 0xA] & 0x18) << 8);
                }

                if (pc >= -1 && pc + 1 < code.Count)
                {
                    Ausgabe_LV.Items[code[pc + 1][0]].BackColor = Color.DarkOrange;
                }

                updateLaufzeit(2);
            }
            befehl = hexBefehl & 0x3800;
            if (befehl == 0x2800) //GOTO
            {
                pc = (hexBefehl & 0x07FF) - 1;

                if (pc >= -1 && pc + 1 < code.Count)
                {
                    Ausgabe_LV.Items[code[pc + 1][0]].BackColor = Color.DarkOrange;
                }

                updateLaufzeit(2);
            }
            befehl = hexBefehl & 0x3F00;
            if (befehl == 0x3400 || befehl == 0x3500 || befehl == 0x3600 || befehl == 0x3700) //RETLW bearbeiten
            {
                wReg = hexBefehl & 0x00FF;
                pc = stack - 1;

                if (wReg == 0)
                {
                    zflag = 1;
                }
                else
                {
                    zflag = 0;
                }

                if (pc >= -1 && pc + 1 < code.Count)
                {
                    Ausgabe_LV.Items[code[pc + 1][0]].BackColor = Color.DarkOrange;
                }

                updateLaufzeit(2);
            }
            else if (false) //RETFIE
            {
                if (pc >= -1 && pc + 1 < code.Count)
                {
                    Ausgabe_LV.Items[code[pc + 1][0]].BackColor = Color.DarkOrange;
                }

                updateLaufzeit(2);
            }
            else
            {
                if (pc + 1 < code.Count)
                {
                    Ausgabe_LV.Items[code[pc + 1][0]].BackColor = Color.DarkOrange;
                }
            }
            befehl = hexBefehl & 0x3F00;
            if (befehl == 0x3900) //ANDLW
            {
                int speicher = hexBefehl & 0x00FF;
                int mehrspeicher = wReg & speicher;
                wReg = mehrspeicher;

                updateLaufzeit(1);
            }

            if (befehl == 0x3800) //IORLW
            {
                wReg |= (hexBefehl & 0x00FF);
                if (wReg == 0)
                {
                    zflag = 1;
                }
                else
                {
                    zflag = 0;
                }

                updateLaufzeit(1);
            }
            if (befehl == 0x3000 || befehl == 0x3100 || befehl == 0x3200 || befehl == 0x3300) // MOVLW
            {
                wReg = (hexBefehl & 0x00FF);
                if (wReg == 0)
                {
                    zflag = 1;
                }
                else
                {
                    zflag = 0;
                }

                updateLaufzeit(1);
            }
            if (befehl == 0x3C00 || befehl == 0x3D00) //SUBLW
            {
                wReg = (hexBefehl & 0x00FF) - wReg;
                temp = (hexBefehl & 0x00FF) - wReg;
                if (temp > 0)
                {
                    cflag = 1;
                }
                else
                {
                    cflag = 0;
                }
                int dcwReg = (hexBefehl & 0x000F) - (wReg & 0x000F);
                if (dcwReg > 0)
                {
                    dcflag = 1;
                }
                else
                {
                    dcflag = 0;
                }
                if (wReg == 0)
                {
                    zflag = 1;
                }
                else
                {
                    zflag = 0;
                }

                updateLaufzeit(1);
            }
            if (befehl == 0x3A00) //XORLW
            {
                wReg ^= (hexBefehl & 0x00FF);
                if (wReg == 0)
                {
                    zflag = 1;
                }
                else
                {
                    zflag = 0;
                }

                updateLaufzeit(1);
            }
            if (befehl == 0x0700) //ADDWF
            {
                if ((hexBefehl & 0x0080) == 0x0000)
                {
                    wReg += register[0, (hexBefehl & 0x007F)];
                    temp = wReg + (register[0, (hexBefehl & 0x007F)] & 0x00FF);
                    if (temp > 0x00FF)
                    {
                        cflag = 1;
                    }
                    else
                    {
                        cflag = 0;
                    }
                    int dcwReg = (wReg & 0x000F) + (register[0, (hexBefehl & 0x007F)] & 0x000F);
                    if (dcwReg > 0x000F)
                    {
                        dcflag = 1;
                    }
                    else
                    {
                        dcflag = 0;
                    }
                    if (wReg == 0)
                    {
                        zflag = 1;
                    }
                    else
                    {
                        zflag = 0;
                    }
                }
                if ((hexBefehl & 0x0080) == 0x0080)
                {
                    register[0, (hexBefehl & 0x007F)] += wReg;

                    temp = wReg + (register[0, (hexBefehl & 0x007F)] & 0x00FF);
                    if (temp > 0x00FF)
                    {
                        cflag = 1;
                    }
                    else
                    {
                        cflag = 0;
                    }
                    int dcwReg = (wReg & 0x000F) + (register[0, (hexBefehl & 0x007F)] & 0x000F);
                    if (dcwReg > 0x000F)
                    {
                        dcflag = 1;
                    }
                    else
                    {
                        dcflag = 0;
                    }
                    if (register[0, (hexBefehl & 0x007F)] == 0)
                    {
                        zflag = 1;
                    }
                    else
                    {
                        zflag = 0;
                    }
                }

                updateLaufzeit(1);
            }
            if (befehl == 0x0500) //ANDWF
            {
                if ((hexBefehl & 0x0080) == 0x0000)
                {
                    wReg &= register[0, (hexBefehl & 0x007F)];
                }
                if ((hexBefehl & 0x0080) == 0x0080)
                {
                    register[0, (hexBefehl & 0x007F)] &= wReg;
                }

                updateLaufzeit(1);
            }
            befehl = hexBefehl & 0x3F80;
            if (befehl == 0x0180) //CLRF
            {
                register[0, (hexBefehl & 0x007F)] = 0;
                zflag = 1;

                updateLaufzeit(1);
            }
            if (befehl == 0x0100) //CLRW
            {
                wReg = 0;
                zflag = 1;

                updateLaufzeit(1);
            }
            befehl = hexBefehl & 0x3F00;
            if (befehl == 0x0900) //COMF
            {
                if ((hexBefehl & 0x0080) == 0x0000)
                {
                    wReg = ~register[0, (hexBefehl & 0x007F)];
                    if (wReg == 0)
                    {
                        zflag = 1;
                    }
                    else
                    {
                        zflag = 0;
                    }
                }
                if ((hexBefehl & 0x0080) == 0x0080)
                {
                    register[0, (hexBefehl & 0x007F)] = ~register[0, (hexBefehl & 0x007F)];
                    if (register[0, (hexBefehl & 0x007F)] == 0)
                    {
                        zflag = 1;
                    }
                    else
                    {
                        zflag = 0;
                    }
                }

                updateLaufzeit(1);
            }
            if (befehl == 0x0300) //DECF
            {
                if ((hexBefehl & 0x0080) == 0x0000)
                {
                    wReg = register[0, (hexBefehl & 0x007F)] - 1;
                }
                if ((hexBefehl & 0x0080) == 0x0080)
                {
                    register[0, (hexBefehl & 0x007F)] -= 1;
                }

                updateLaufzeit(1);
            }
            if (befehl == 0x0B00) //DECFSZ
            {
                if ((register[0, (hexBefehl & 0x007F)]) != 0)
                {

                    if ((hexBefehl & 0x0080) == 0x0000)
                    {
                        temp = wReg;
                        temp = register[0, (hexBefehl & 0x007F)] - 1;
                        if (temp != 0)
                        {
                            wReg = temp;
                        }
                        else
                        {
                            
                            pc++;
                        }
                    }
                    if ((hexBefehl & 0x0080) == 0x0080)
                    {
                        temp = register[0, (hexBefehl & 0x007F)];
                        temp--;
                        if (temp != 0)
                        {
                            register[0, (hexBefehl & 0x007F)] -= 1;
                        }
                        else
                        {
                            
                            pc++;
                        }
                    }
                }

                // 1 oder 2 - updateLaufzeit(2);
            }
            if (befehl == 0x0A00) //INCF
            {
                if ((hexBefehl & 0x0080) == 0x0000)
                {
                    wReg = (register[0, (hexBefehl & 0x007F)] + 1);
                }
                if ((hexBefehl & 0x0080) == 0x0080)
                {
                    register[0, (hexBefehl & 0x007F)] += 1;
                }

                updateLaufzeit(1);
            }
            if (befehl == 0x0F00) //INCFSZ
            {

                if ((hexBefehl & 0x0080) == 0x0000)
                {
                    temp = register[0, (hexBefehl & 0x007F)] + 1;
                    if (temp > 0xFF)
                    {
                        temp = 0;
                    }
                    if (temp != 0)
                    {
                        wReg = temp;
                    }
                    else
                    {
                        wReg = temp;
                        pc++;
                    }
                }
                if ((hexBefehl & 0x0080) == 0x0080)
                {
                    temp = register[0, (hexBefehl & 0x007F)] + 1;
                    if (temp > 0xFF)
                    {
                        temp = 0;
                    }
                    if (temp != 0)
                    {
                        register[0, (hexBefehl & 0x007F)] = temp;
                    }
                    else
                    {
                        register[0, (hexBefehl & 0x007F)] = temp;
                        pc++;
                    }
                }

                // 1 oder 2 - updateLaufzeit(2);
            }
            if (befehl == 0x0400) //IORWF
            {
                if ((hexBefehl & 0x0080) == 0x0000)
                {
                    wReg |= register[0, (hexBefehl & 0x007F)];
                    if (wReg == 0)
                    {
                        zflag = 1;
                    }
                    else
                    {
                        zflag = 0;
                    }
                }
                if ((hexBefehl & 0x0080) == 0x0080)
                {
                    register[0, (hexBefehl & 0x007F)] = wReg | register[0, (hexBefehl & 0x007F)];
                    if (register[0, (hexBefehl & 0x007F)] == 0)
                    {
                        zflag = 1;
                    }
                    else
                    {
                        zflag = 0;
                    }
                }

                updateLaufzeit(1);
            }
            if (befehl == 0x0800) //MOVF
            {
                if ((hexBefehl & 0x0080) == 0x0000)
                {
                    wReg = register[0, (hexBefehl & 0x007F)];
                    if (wReg == 0)
                    {
                        zflag = 1;
                    }
                    else
                    {
                        zflag = 0;
                    }
                }
                if ((hexBefehl & 0x0080) == 0x0080)
                {
                    if (register[0, (hexBefehl & 0x007F)] == 0)
                    {
                        zflag = 1;
                    }
                    else
                    {
                        zflag = 0;
                    }
                }

                updateLaufzeit(1);
            }
            befehl = hexBefehl & 0x3F80;
            if (befehl == 0x0080) //MOVWF
            {
                temp = hexBefehl & 0x007F;
                register[0, temp] = wReg;
                if (temp == 0x1)
                {
                    option = register[0, temp];

                }
                
                updateLaufzeit(1);
            }
            befehl = hexBefehl & 0x3F00;
            if (befehl == 0x0000) //nop
            {
                //passiert halt nix

                updateLaufzeit(1);
            }
            if (befehl == 0x0D00)//RLF, bearbeiten
            {
                temp = register[0, (hexBefehl & 0x007F)];
                temp2 = cflag;
                cflag = ((temp & 0x0080) >> 7) & 0x1;

                if ((hexBefehl & 0x0080) == 0x0000)
                {
                    wReg = ((temp << 1) & 0x00FF) + temp2;

                }
                if ((hexBefehl & 0x0080) == 0x0080)
                {
                    register[0, (hexBefehl & 0x007F)] = ((temp << 1) & 0x00FF) + temp2;
                }

                updateLaufzeit(1);
            }
            if (befehl == 0x0C00)//RRF, bearbeiten
            {
                temp = register[0, (hexBefehl & 0x007F)];
                temp2 = cflag;
                cflag = (temp & 0x0001);

                if ((hexBefehl & 0x0080) == 0x0000)
                {
                    wReg = (temp2 << 7) + ((temp & 0xFE) >> 1);
                }
                if ((hexBefehl & 0x0080) == 0x0080)
                {
                    register[0, (hexBefehl & 0x007F)] = (temp2 << 7) + ((temp & 0xFE) >> 1);
                }

                updateLaufzeit(1);
            }
            if (befehl == 0x0200) //SUBWF
            {
                if ((hexBefehl & 0x0080) == 0x0000)
                {
                    temp = wReg & 0xFF;
                    wReg = register[0, (hexBefehl & 0x007F)] - wReg;

                    if (temp > (register[0, (hexBefehl & 0x007F)] & 0xFF))
                    {
                        cflag = 0;
                        dcflag = 0;
                    }
                    else
                    {
                        cflag = 1;
                        dcflag = 1;
                    }

                    if (wReg == 0)
                    {
                        zflag = 1;
                    }
                    else
                    {
                        zflag = 0;
                    }
                }
                if ((hexBefehl & 0x0080) == 0x0080)
                {
                    temp = register[0, (hexBefehl & 0x007F)] & 0xFF;
                    register[0, (hexBefehl & 0x007F)] = register[0, (hexBefehl & 0x007F)] - wReg;
                    if ((wReg & 0xFF) > temp)
                    {
                        cflag = 0;
                        dcflag = 0;
                    }
                    else
                    {
                        cflag = 1;
                        dcflag = 1;
                    }

                    if (register[0, (hexBefehl & 0x007F)] == 0)
                    {
                        zflag = 1;
                    }
                    else
                    {
                        zflag = 0;
                    }
                }

                updateLaufzeit(1);
            }
            if (befehl == 0x0E00) //SWAPF
            {

                int firstHalf = 0x000F & register[0, (hexBefehl & 0x007F)];
                int secondHalf = (0x0070 & register[0, (hexBefehl & 0x007F)]) >> 4;
                int swappedValue = (firstHalf << 4) | secondHalf;
                if ((hexBefehl & 0x0080) == 0x0000)
                {
                    wReg = swappedValue;

                }
                if ((hexBefehl & 0x0080) == 0x0080)
                {
                    register[0, (hexBefehl & 0x007F)] = swappedValue;
                }

                updateLaufzeit(1);
            }
            if (befehl == 0x0600) //XORWF
            {
                if ((hexBefehl & 0x0080) == 0x0000)
                {
                    wReg ^= register[0, (hexBefehl & 0x007F)];
                }
                if ((hexBefehl & 0x0080) == 0x0080)
                {
                    register[0, (hexBefehl & 0x007F)] = wReg ^ register[0, (hexBefehl & 0x007F)];
                }

                updateLaufzeit(1);
            }

            /*
            Fehlt noch:
            -BCF
            -BSF
            -BTFSC
            -BTFSSü

            -RETURN
            -SLEEP

            -REFIE

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
        private void zurücksetzen()
        {
            if (zeilen?.Length > 0)
            {
                Array.Clear(zeilen, 0, zeilen.Length);
            }

            option = 0xFF;
            PS0_Toggle.Text = "1";
            PS1_Toggle.Text = "1";
            PS2_Toggle.Text = "1";
            PSA_Toggle.Text = "1";
            T0SE_Toggle.Text = "1";
            T0CS_Toggle.Text = "1";
            INTEDG_Toggle.Text = "1";
            RPBU_Toggle.Text = "1";

            GIE_Toggle.Text = "0";
            PIE_Toggle.Text = "0";
            T0IE_Toggle.Text = "0";
            INTE_Toggle.Text = "0";
            RBIE_Toggle.Text = "0";
            T0IF_Toggle.Text = "0";
            INTF_Toggle.Text = "0";
            RBIF_Toggle.Text = "0";

            PinB7_button.Text = "1";
            PinB6_button.Text = "0";
            PinB5_button.Text = "0";
            PinB4_button.Text = "0";
            PinB3_button.Text = "0";
            PinB2_button.Text = "0";
            PinB1_button.Text = "0";
            PinB0_button.Text = "0";

            PinA7_button.Text = "0";
            PinA6_button.Text = "0";
            PinA5_button.Text = "0";
            PinA4_button.Text = "0";
            PinA3_button.Text = "0";
            PinA2_button.Text = "0";
            PinA1_button.Text = "0";
            PinA0_button.Text = "0";

            autorun = false;
            BP_reached = -1;
            Automatic_Btn.ForeColor = Color.Red;

            pc = 0;

            laufzeit = 0;
            zykluszeit = 0; //evtl. nicht notwendig

            for (int i = 0; i < Ausgabe_LV.Items.Count; i++)
            {
                //Ausgabe_LV.Items[i].Checked = false;
                Ausgabe_LV.Items[i].BackColor = Color.White;
                Ausgabe_LV.Items[i].ForeColor = Color.Black;
            }

            if (code?.Count > 0)
            {
                Einzelschritt_Btn.Enabled = true;
                Automatic_Btn.Enabled = true;
            }
            else
            {
                Einzelschritt_Btn.Enabled = false;
                Automatic_Btn.Enabled = false;
            }

            wReg = 0;
            cflag = 0;
            dcflag = 0;
            zflag = 0;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    register[i, j] = 0;
                }
            }

            if (pc < code?.Count)
            {
                Ausgabe_LV.Items[code[pc][0]].BackColor = Color.DarkOrange;
            }

            /*if (programmcode?.Count > 0)
            {
                befehlsSwitch(programmcode[0]);
            }*/
            update();
        }

        private void Zurücksetzen_Btn_Click(object sender, EventArgs e)
        {
            zurücksetzen();
            return;
        }

        private void RPBU_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(RPBU_Toggle.Text) == 0)
            {
                RPBU_Toggle.Text = "1";
                option += 128;
            }
            else if (Convert.ToInt32(RPBU_Toggle.Text) == 1)
            {
                RPBU_Toggle.Text = "0";
                option -= 128;
            }
            option_out.Text = option.ToString("X2");

            return;
        }

        private void INTEDG_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(INTEDG_Toggle.Text) == 0)
            {
                INTEDG_Toggle.Text = "1";
                option += 64;
            }
            else if (Convert.ToInt32(INTEDG_Toggle.Text) == 1)
            {
                INTEDG_Toggle.Text = "0";
                option -= 64;
            }
            option_out.Text = option.ToString("X2");

            return;
        }

        private void T0CS_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(T0CS_Toggle.Text) == 0)
            {
                T0CS_Toggle.Text = "1";
                option += 32;
            }
            else if (Convert.ToInt32(T0CS_Toggle.Text) == 1)
            {
                T0CS_Toggle.Text = "0";
                option -= 32;
            }
            option_out.Text = option.ToString("X2");

            return;
        }

        private void T0SE_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(T0SE_Toggle.Text) == 0)
            {
                T0SE_Toggle.Text = "1";
                option += 16;
            }
            else if (Convert.ToInt32(T0SE_Toggle.Text) == 1)
            {
                T0SE_Toggle.Text = "0";
                option -= 16;
            }
            option_out.Text = option.ToString("X2");

            return;
        }

        private void PSA_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(PSA_Toggle.Text) == 0)
            {
                PSA_Toggle.Text = "1";
                option += 8;
            }
            else if (Convert.ToInt32(PSA_Toggle.Text) == 1)
            {
                PSA_Toggle.Text = "0";
                option -= 8;
            }
            option_out.Text = option.ToString("X2");

            return;
        }

        private void PS2_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(PS2_Toggle.Text) == 0)
            {
                PS2_Toggle.Text = "1";
                option += 4;
            }
            else if (Convert.ToInt32(PS2_Toggle.Text) == 1)
            {
                PS2_Toggle.Text = "0";
                option -= 4;
            }
            option_out.Text = option.ToString("X2");

            return;
        }

        private void PS1_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(PS1_Toggle.Text) == 0)
            {
                PS1_Toggle.Text = "1";
                option += 2;
            }
            else if (Convert.ToInt32(PS1_Toggle.Text) == 1)
            {
                PS1_Toggle.Text = "0";
                option -= 2;
            }
            option_out.Text = option.ToString("X2");

            return;
        }

        private void PS0_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(PS0_Toggle.Text) == 0)
            {
                PS0_Toggle.Text = "1";
                option += 1;
            }
            else if (Convert.ToInt32(PS0_Toggle.Text) == 1)
            {
                PS0_Toggle.Text = "0";
                option -= 1;
            }
            option_out.Text = option.ToString("X2");

            return;
        }

        private void GIE_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(GIE_Toggle.Text) == 0)
            {
                GIE_Toggle.Text = "1";
                register[0, 0xB] += 128;
            }
            else if (Convert.ToInt32(GIE_Toggle.Text) == 1)
            {
                GIE_Toggle.Text = "0";
                register[0, 0xB] -= 128;
            }
            intcon_out.Text = register[0, 0xB].ToString("X2");

            return;
        }
        private void PIE_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(PIE_Toggle.Text) == 0)
            {
                PIE_Toggle.Text = "1";
                register[0, 0xB] += 64;
            }
            else if (Convert.ToInt32(PIE_Toggle.Text) == 1)
            {
                PIE_Toggle.Text = "0";
                register[0, 0xB] -= 64;
            }
            intcon_out.Text = register[0, 0xB].ToString("X2");

            return;
        }

        private void T0IE_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(T0IE_Toggle.Text) == 0)
            {
                T0IE_Toggle.Text = "1";
                register[0, 0xB] += 32;
            }
            else if (Convert.ToInt32(T0IE_Toggle.Text) == 1)
            {
                T0IE_Toggle.Text = "0";
                register[0, 0xB] -= 32;
            }
            intcon_out.Text = register[0, 0xB].ToString("X2");

            return;
        }

        private void INTE_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(INTE_Toggle.Text) == 0)
            {
                INTE_Toggle.Text = "1";
                register[0, 0xB] += 16;
            }
            else if (Convert.ToInt32(INTE_Toggle.Text) == 1)
            {
                INTE_Toggle.Text = "0";
                register[0, 0xB] -= 16;
            }
            intcon_out.Text = register[0, 0xB].ToString("X2");

            return;
        }

        private void RBIE_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(RBIE_Toggle.Text) == 0)
            {
                RBIE_Toggle.Text = "1";
                register[0, 0xB] += 8;
            }
            else if (Convert.ToInt32(RBIE_Toggle.Text) == 1)
            {
                RBIE_Toggle.Text = "0";
                register[0, 0xB] -= 8;
            }
            intcon_out.Text = register[0, 0xB].ToString("X2");

            return;
        }

        private void T0IF_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(T0IF_Toggle.Text) == 0)
            {
                T0IF_Toggle.Text = "1";
                register[0, 0xB] += 4;
            }
            else if (Convert.ToInt32(T0IF_Toggle.Text) == 1)
            {
                T0IF_Toggle.Text = "0";
                register[0, 0xB] -= 4;
            }
            intcon_out.Text = register[0, 0xB].ToString("X2");

            return;
        }

        private void INTF_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(INTF_Toggle.Text) == 0)
            {
                INTF_Toggle.Text = "1";
                register[0, 0xB] += 2;
            }
            else if (Convert.ToInt32(INTF_Toggle.Text) == 1)
            {
                INTF_Toggle.Text = "0";
                register[0, 0xB] -= 2;
            }
            intcon_out.Text = register[0, 0xB].ToString("X2");

            return;
        }

        private void RBIF_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(RBIF_Toggle.Text) == 0)
            {
                RBIF_Toggle.Text = "1";
                register[0, 0xB] += 1;
            }
            else if (Convert.ToInt32(RBIF_Toggle.Text) == 1)
            {
                RBIF_Toggle.Text = "0";
                register[0, 0xB] -= 1;
            }
            intcon_out.Text = register[0, 0xB].ToString("X2");

            return;
        }

        private void BP_skip_CB_CheckedChanged(object sender, EventArgs e)
        {
            BP_skip = BP_skip_CB.Checked;

            return;
        }

        private void PinA7_button_Click(object sender, EventArgs e)
        {
            if ((register[0, 5] & 0x80) != 128) //Prüfung, ob es NICHT auf 1 steht
            {
                PinA7_button.Text = "1";
                register[0, 5] |= 0x80; //1 
            }
            else
            {
                PinA7_button.Text = "0";
                register[0, 5] &= 0x7F; //0
            }
            update();
        }

        private void PinA6_button_Click(object sender, EventArgs e)
        {
            if ((register[0, 5] & 0x40) != 64) //Prüfung, ob es NICHT auf 1 steht
            {
                PinA6_button.Text = "1";
                register[0, 5] |= 0x40; //1
            }
            else
            {
                PinA6_button.Text = "0";
                register[0, 5] &= 0xBF; //0
            }
            update();
        }

        private void PinA5_button_Click(object sender, EventArgs e)
        {
            if ((register[0, 5] & 0x20) != 32) //Prüfung, ob es NICHT auf 1 steht
            {
                PinA5_button.Text = "1";
                register[0, 5] |= 0x20; //1
            }
            else
            {
                PinA5_button.Text = "0";
                register[0, 5] &= 0xDF; //0
            }
            update();
        }

        private void PinA4_button_Click(object sender, EventArgs e)
        {
            if ((register[0, 5] & 0x10) != 16) //Prüfung, ob es NICHT auf 1 steht
            {
                PinA4_button.Text = "1";
                register[0, 5] |= 0x10; //1
            }
            else
            {
                PinA4_button.Text = "0";
                register[0, 5] &= 0xEF; //0
            }
            if (register[1, 1].ToString() == "1") // T0CS-Bit
            {
                if (register[1, 1].ToString() == "0") // T0SE
                {
                    if (register[0, 5].ToString() == "0") // RA4
                    {
                        if (register[1, 1].ToString() == "1") // PSA-Bit
                        {
                            register[0, 1]++;
                        }
                        else
                        {
                            vorteiler++;

                            if (vorteiler == vorteiler_max)    //Prüfung, ob Wert von TMR0 nun erhöht werden darf
                            {
                                vorteiler = 0;
                                register[0, 1]++;
                                vorteiler_max = SetVorteiler();
                            }

                            if (register[0, 1] > 0xff)
                            {
                                register[0, 11] |= 0b00000100;
                                register[0, 1] = 0x00;
                                T0_interrupt = true;
                            }
                        }
                    }
                }
                else
                {
                    if (register[0, 5].ToString() == "1") // RA4
                    {
                        if (register[1, 1].ToString() == "1") // PSA-Bit
                        {
                            register[0, 1]++;
                        }
                        else
                        {
                            vorteiler++;

                            if (vorteiler == vorteiler_max)    //Prüfung, ob Wert von TMR0 nun erhöht werden darf
                            {
                                vorteiler = 0;
                                register[0, 1]++;
                                vorteiler_max = SetVorteiler();
                            }

                            if (register[0, 1] > 0xff)
                            {
                                register[0, 11] |= 0b00000100;
                                register[0, 1] = 0x00;
                                T0_interrupt = true;
                            }
                        }
                    }
                }
            }
            update();

        }

        private void PinA3_button_Click(object sender, EventArgs e)
        {
            if ((register[0, 5] & 0x8) != 8) //Prüfung, ob es NICHT auf 1 steht
            {
                PinA3_button.Text = "1";
                register[0, 5] |= 0x8; //1
            }
            else
            {
                PinA3_button.Text = "0";
                register[0, 5] &= 0xF7; //0
            }
            update();
        }

        private void PinA2_button_Click(object sender, EventArgs e)
        {
            if ((register[0, 5] & 0x4) != 4) //Prüfung, ob es NICHT auf 1 steht
            {
                PinA2_button.Text = "1";
                register[0, 5] |= 0x4; //1
            }
            else
            {
                PinA2_button.Text = "0";
                register[0, 5] &= 0xFB; //0
            }
            update();
        }

        private void PinA1_button_Click(object sender, EventArgs e)
        {
            if ((register[0, 5] & 0x2) != 2) //Prüfung, ob es NICHT auf 1 steht
            {
                PinA1_button.Text = "1";
                register[0, 5] |= 0x2; //1
            }
            else
            {
                PinA1_button.Text = "0";
                register[0, 5] &= 0xFD; //0
            }
            update();
        }

        private void PinA0_button_Click(object sender, EventArgs e)
        {
            if ((register[0, 5] & 0x1) != 1) //Prüfung, ob es NICHT auf 1 steht
            {
                PinA0_button.Text = "1";
                register[0, 5] |= 0x1; //1
            }
            else
            {
                PinA0_button.Text = "0";
                register[0, 5] &= 0xFE; //0
            }
            update();
        }

        private void PinB7_button_Click(object sender, EventArgs e)
        {
            if ((register[0, 6] & 0x80) != 128) //Prüfung, ob es NICHT auf 1 steht
            {
                PinB7_button.Text = "1";
                register[0, 6] |= 0x80; //1
            }
            else
            {
                PinB7_button.Text = "0";
                register[0, 6] &= 0x7F; //0
            }

            if ((register[1, 6] & 0x80) == 128) //Pin gerade Input?!
            {
                RB4_7_changed = true;
            }

            update();
        }

        private void PinB6_button_Click(object sender, EventArgs e)
        {
            if ((register[0, 6] & 0x40) != 64) //Prüfung, ob es NICHT auf 1 steht
            {
                PinB6_button.Text = "1";
                register[0, 6] |= 0x40; //1
            }
            else
            {
                PinB6_button.Text = "0";
                register[0, 6] &= 0xBF; //0
            }

            if ((register[1, 6] & 0x40) == 64) //Pin gerade Input?!
            {
                RB4_7_changed = true;
            }

            update();
        }

        private void PinB5_button_Click(object sender, EventArgs e)
        {
            if ((register[0, 6] & 0x20) != 32) //Prüfung, ob es NICHT auf 1 steht
            {
                PinB5_button.Text = "1";
                register[0, 6] |= 0x20; //1
            }
            else
            {
                PinB5_button.Text = "0";
                register[0, 6] &= 0xDF; //0
            }

            if ((register[1, 6] & 0x20) == 32) //Pin gerade Input?!
            {
                RB4_7_changed = true;
            }

            update();
        }

        private void PinB4_button_Click(object sender, EventArgs e)
        {
            if ((register[0, 6] & 0x10) != 16) //Prüfung, ob es NICHT auf 1 steht
            {
                PinB4_button.Text = "1";
                register[0, 6] |= 0x10; //1
            }
            else
            {
                PinB4_button.Text = "0";
                register[0, 6] &= 0xEF; //0
            }

            if ((register[1, 6] & 0x10) == 16) //Pin gerade Input?!
            {
                RB4_7_changed = true;
            }

            update();
        }

        private void PinB3_button_Click(object sender, EventArgs e)
        {
            if ((register[0, 6] & 0x8) != 8) //Prüfung, ob es NICHT auf 1 steht
            {
                PinB3_button.Text = "1";
                register[0, 6] |= 0x8; //1
            }
            else
            {
                PinB3_button.Text = "0";
                register[0, 6] &= 0xF7; //0
            }
            update();
        }

        private void PinB2_button_Click(object sender, EventArgs e)
        {
            if ((register[0, 6] & 0x4) != 4) //Prüfung, ob es NICHT auf 1 steht
            {
                PinB2_button.Text = "1";
                register[0, 6] |= 0x4; //1
            }
            else
            {
                PinB2_button.Text = "0";
                register[0, 6] &= 0xFB; //0
            }
            update();
        }

        private void PinB1_button_Click(object sender, EventArgs e)
        {
            if ((register[0, 6] & 0x2) != 2) //Prüfung, ob es NICHT auf 1 steht
            {
                PinB1_button.Text = "1";
                register[0, 6] |= 0x2; //1
            }
            else
            {
                PinB1_button.Text = "0";
                register[0, 6] &= 0xFD; //0
            }
            update();
        }

        private void PinB0_button_Click(object sender, EventArgs e)
        {
            if ((register[0, 6] & 0x1) != 1) //Prüfung, ob es NICHT auf 1 steht => steigende Taktflanke
            {
                PinB0_button.Text = "1";

                register[0, 6] |= 0x1; // RB0-Bit auf 1
                if ((register[1, 1] & 0x40) == 64)  //Prüfung, ob "rising edge" (steigende Taktflanke) im Option Pin 6 (INTEDG) gesetzt ist
                {
                    RB0_flag = true;
                }
            }
            else
            {
                PinB0_button.Text = "0";

                register[0, 6] &= 0xFE; // RB0-Bit auf 0 => fallende Taktflanke
                if ((register[1, 1] & 0x40) == 0)   //Prüfung, ob "falling edge" (fallende Taktflanke) im Option Pin 6 (INTEDG) gesetzt ist
                {
                    RB0_flag = true;
                }
            }
            update();
        }

        private void getStackPointer()
        {
            //textBox5.Text = stackPointer.ToString();
        }

        private void pushStack(int toPush)
        {
            stack1[stackPointer] = Convert.ToString(toPush, 2).PadLeft(13, '0');
            stackPointer++;
            if (stackPointer > 7)
            {
                stackPointer = 0;
            }
        }
        private int popStack()
        {
            if (stackPointer > 0)
            {
                stackPointer--;
            }
            else if ((stackPointer == 0) && (stack1[7] != null))
            {
                stackPointer = 7;
            }

            int retval = Convert.ToInt16(stack1[stackPointer], 2);
            return retval;
        }

        private void InterruptFlag()
        {
            if (T0_interrupt)
            {
                register[0, 11] |= 0x04; //Timer-Interrupt-Flag setzen
                T0_interrupt = false;   //deaktivieren, damit nicht dauerhaft auslöst
            }

            if (RB0_flag)
            {
                register[0, 11] |= 0x02; //INTF-Bit setzen (RB0-Interrupt)
                RB0_flag = false;     //deaktivieren, damit nicht dauerhaft auslöst
            }

            if (RB4_7_changed)
            {
                register[0, 11] |= 0x01; //RBIF-Bit setzen
                RB4_7_changed = false;  //deaktivieren, damit nicht dauerhaft auslöst
            }
        }

        private void InterruptMaker()
        {
            InterruptFlag();    //muss hier auch stehen, da InterruptFlag() erst in UpdateUI() sonst aufgerufen werden würde, was idR nach timerIncrease() steht.
                                //Somit würde ohne diese Zeile ggf. ein Interrupt nicht ausgelöst werden.

            if (((register[0, 11] & 0x20) == 32) && ((register[0, 11] & 0x04) == 4)) //T0IE? Timer-Interrupt-Flag (T0-Interrupt)?
            {
                //kann NICHT aus SLEEP wecken!

                if ((register[0, 11] & 0x80) == 0x80)    //Prüfen, ob GIE (INTCON Bit 7) gesetzt ist, um Interrupts allgemein überhaupt an CPU weiterzuleiten
                {
                    pushStack(pc); //Rückkehradresse auf Stack pushen
                    pc = 0x04;    //Sprung zu Zeile 4, in der die ISRs stehen müssen

                    register[0, 11] &= 0b0111_1111;  //GIE Null setzen, damit nicht in Endlosschleife Interrupts ausgelöst werden

                    updateLaufzeit(2);   //Schreiben auf Stack + zu 04h springen -> 2 cycles
                }
            }

            if (((register[0, 11] & 0x10) == 16) && ((register[0, 11] & 0x02) == 2)) //INTE aktiviert? INTF (RB0-Interrupt)?
            {



                if ((register[0, 11] & 0x80) == 0x80)    //Prüfen, ob GIE (INTCON Bit 7) gesetzt ist, um Interrupts allgemein überhaupt an CPU weiterzuleiten
                {
                    pushStack(pc); //Rückkehradresse auf Stack pushen
                    pc = 0x04;    //Sprung zu Zeile 4, in der die ISRs stehen müssen

                    register[0, 11] &= 0b0111_1111;  //GIE Null setzen, damit nicht in Endlosschleife Interrupts ausgelöst werden

                    updateLaufzeit(2);   //Schreiben auf Stack + zu 04h springen -> 2 cycles
                }
            }

            if (((register[0, 11] & 0x08) == 8) && ((register[0, 11] & 0x01) == 1))  //RBIE? RBIF (RB7:4-Interrupt)?
            {

                if ((register[0, 11] & 0x80) == 0x80)    //Prüfen, ob GIE (INTCON Bit 7) gesetzt ist, um Interrupts allgemein überhaupt an CPU weiterzuleiten
                {
                    pushStack(pc); //Rückkehradresse auf Stack pushen
                    pc = 0x04;    //Sprung zu Zeile 4, in der die ISRs stehen müssen

                    register[0, 11] &= 0b0111_1111;  //GIE Null setzen, damit nicht in Endlosschleife Interrupts ausgelöst werden

                    updateLaufzeit(2);   //Schreiben auf Stack + zu 04h springen -> 2 cycles
                }
            }
        }

        private void quarzFreq_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (quarzFreq_comboBox.SelectedIndex)
            {
                case 0: //1 kHz
                    quarzFrequenz = 0.001f;
                    break;
                case 1: //25 kHz
                    quarzFrequenz = 0.025f;
                    break;
                case 2: //100 kHz
                    quarzFrequenz = 0.1f;
                    break;
                case 3: //200 kHz
                    quarzFrequenz = 0.2f;
                    break;
                case 4: //455 kHz
                    quarzFrequenz = 0.455f;
                    break;
                case 5: //2 MHz
                    quarzFrequenz = 2.0f;
                    break;
                case 6: //4 MHz
                    quarzFrequenz = 4.0f;
                    break;
                case 7: //10 MHz
                    quarzFrequenz = 10.0f;
                    break;

                default:
                    MessageBox.Show("Falsche Eingabe!");
                    break;
            }

            return;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            /*if (Convert.ToInt32(textBox2.Text) == 0)
            {
                cflag = 1;
                textBox2.Text = cflag.ToString("X");
                status += 1;
            }
            else if (Convert.ToInt32(textBox2.Text) == 1)
            {
                cflag = 0;
                textBox2.Text = cflag.ToString("X");
                status -= 1;
            } else
            {
                cflag = 0;
            }
            textBox9.Text = status.ToString("X2");
            */
            return;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            /*if (Convert.ToInt32(textBox3.Text) == 0)
            {
                dcflag = 1;
                textBox3.Text = "1";
                status += 2;
            }
            else if (Convert.ToInt32(textBox3.Text) == 1)
            {
                dcflag = 0;
                textBox3.Text = "0";
                status -= 2;
            }
            textBox9.Text = status.ToString("X2");
            */
            return;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            /*if (Convert.ToInt32(textBox4.Text) == 0)
            {
                zflag = 1;
                textBox4.Text = "1";
                status += 4;
            }
            else if (Convert.ToInt32(textBox4.Text) == 1)
            {
                zflag = 0;
                textBox4.Text = "0";
                status -= 4;
            }
            textBox9.Text = status.ToString("X2");
            */
            return;
        }
    }

}