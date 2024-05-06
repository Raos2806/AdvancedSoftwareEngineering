using System;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace PIC_Controller
{
    public partial class Form1 : Form
    {
        Variablen var;
        Ibefehle cm;
        IfileHandler file;
        IuIAccess uiAccess;
        Ipin pin;
        Iflag flag;

        public Form1(Variablen var, Ibefehle cm, IfileHandler file, IuIAccess uiAccess, Ipin pin, Iflag flag)
        {
            this.cm = cm;
            this.var = var;
            this.file = file;
            this.uiAccess = uiAccess;
            this.pin = pin;
            this.flag = flag;
            
            InitializeComponent();
            quarzFreq_comboBox.SelectedIndex = 6; //Dropdown auf 4MHz stellen
            PopulateDataGridView();
            Zurücksetzen();
        }

        //RunTime
        private bool Einzelschritt()
        {
            if (var.pc < var.code.Count && var.BP_reached != var.pc)
            {
                for (int i = 0; i < Ausgabe_LV.Items.Count; i++)
                {
                    Ausgabe_LV.Items[i].BackColor = Color.White;
                    Ausgabe_LV.Items[i].ForeColor = Color.Black;

                    if (Ausgabe_LV.Items[i].Checked && Ausgabe_LV.Items[i].Index == var.code[var.pc][0] && var.BP_reached != var.pc)
                    {
                        var.BP_reached = var.pc;
                        Ausgabe_LV.Items[i].BackColor = Color.Red;
                        Ausgabe_LV.Items[i].ForeColor = Color.White;
                    }
                }

                if (var.BP_reached == var.pc)
                {
                    return true;
                }
                else
                {
                    Ausgabe_LV.Items[var.code[var.pc][0]].BackColor = Color.Green;
                }
            }
            else if (var.pc < var.code.Count && var.BP_reached == var.pc)
            {
                Ausgabe_LV.Items[var.code[var.pc][0]].BackColor = Color.Green;
                Ausgabe_LV.Items[var.code[var.pc][0]].ForeColor = Color.Black;

                if (var.BP_skip)
                {
                    Ausgabe_LV.Items[var.code[var.pc][0]].BackColor = Color.White;
                    var.pc += 1;
                    Ausgabe_LV.Items[var.code[var.pc][0]].BackColor = Color.DarkOrange;
                    var.BP_reached = -1;
                    return false;
                }
                else
                {
                    var.BP_reached = -1;
                }
            }
            else
            {
                return true;
            }

            var.hexBefehl = var.programmcode[var.pc];
            cm.BefehlsSwitch(Ausgabe_LV, var.hexBefehl);

            var.pc += 1;

            UpdateAll();

            return false;
        }
        private void AutoRun()
        {
            if (var.BP_reached != -1 && var.autorun)
            {
                Einzelschritt();
            }

            while (var.autorun)
            {
                if (Einzelschritt()) //stoppen, wenn Breakpoint oder Ende erreicht wurde
                {
                    var.autorun = false;

                    UpdateAll();

                    break;
                }

                //timer zum kurz warten wegen UI
            }

            return;
        }
        //

        private void UpdateAll()
        {
            if (var.pc >= var.code?.Count)
            {
                Einzelschritt_Btn.Enabled = false;
                Automatic_Btn.Enabled = false;
            }
            else if (var.code?.Count > 0)
            {
                Einzelschritt_Btn.Enabled = true;
                Automatic_Btn.Enabled = true;
            }

            UpdateWReg();
            UpdateFlags();
            UpdatePC();
            Update_RA();
            Update_RB();
            UpdateDataGridView();

            laufzeit_TB.Text = var.laufzeit.ToString() + " µs";
            zykluszeit_TB.Text = var.zykluszeit.ToString() + " µs";
            dataGridView1.AutoResizeRows();

            if (var.autorun)
            {
                Automatic_Btn.ForeColor = Color.Green;
            }
            else
            {
                Automatic_Btn.ForeColor = Color.Red;
            }
        }
        private void UpdateWReg()
        {
            textBox1.Text = (var.wReg & 0xFF).ToString("X2");
        }
        private void UpdateFlags()
        {
            textBox2.Text = flag.WriteFlag(var.cflag);
            textBox3.Text = flag.WriteFlag(var.dcflag);
            textBox4.Text = flag.WriteFlag(var.zflag);
            textBox5.Text = flag.WriteFlag(var.status);
        }
        private void UpdatePC()
        {
            var.pcl = var.register[0, 0x2];
            var.pclath = var.register[0, 0xA];
            var.register[0, 2] = var.pcl;
            var.register[0, 3] = var.status;
            var.register[0, 130] = var.option;
            option_out.Text = var.register[0, 130].ToString("X2");
            intcon_out.Text = var.register[0, 0xB].ToString("X2");
            //var.register[0, 6] = 0x80;
            //var.register[0, 8] = 0x80;
            //var.register[0, 9] = 0x80;
            textBox7.Text = var.pcl.ToString("X");
            textBox8.Text = var.pclath.ToString("X");
            textBox9.Text = var.stack.ToString("X");
            textBox10.Text = var.pc.ToString();
        }
        private void Update_RA()
        {
            if ((var.register[0, 134] & 0x80) == 128) TrisA7_label.Text = "i";
            else
            {
                TrisA7_label.Text = "o";
                PinA7_button.Text = ((var.register[0, 5] & 0x80) / 0x80).ToString("X");
            }
            if ((var.register[0, 134] & 0x40) == 64) TrisA6_label.Text = "i";
            else
            {
                TrisA6_label.Text = "o";
                PinA6_button.Text = ((var.register[0, 5] & 0x40) / 0x40).ToString("X");
            }
            if ((var.register[0, 134] & 0x20) == 32) TrisA5_label.Text = "i";
            else
            {
                TrisA5_label.Text = "o";
                PinA5_button.Text = ((var.register[0, 5] & 0x20) / 0x20).ToString("X");
            }
            if ((var.register[0, 134] & 0x10) == 16) TrisA4_label.Text = "i";
            else
            {
                TrisA4_label.Text = "o";
                PinA4_button.Text = ((var.register[0, 5] & 0x10) / 0x10).ToString("X");
            }
            if ((var.register[0, 134] & 0x08) == 8) TrisA3_label.Text = "i";
            else
            {
                TrisA3_label.Text = "o";
                PinA3_button.Text = ((var.register[0, 5] & 0x08) / 0x08).ToString("X");
            }
            if ((var.register[0, 134] & 0x04) == 4) TrisA2_label.Text = "i";
            else
            {
                TrisA2_label.Text = "o";
                PinA2_button.Text = ((var.register[0, 5] & 0x04) / 0x04).ToString("X");
            }
            if ((var.register[0, 134] & 0x02) == 2) TrisA1_label.Text = "i";
            else
            {
                TrisA1_label.Text = "o";
                PinA1_button.Text = ((var.register[0, 5] & 0x02) / 0x02).ToString("X");
            }
            if ((var.register[0, 134] & 0x01) == 1) TrisA0_label.Text = "i";
            else
            {
                TrisA0_label.Text = "o";
                PinA0_button.Text = ((var.register[0, 5] & 0x01) / 0x01).ToString("X");
            }
        }
        private void Update_RB()
        {
            if ((var.register[0, 135] & 0x80) == 128) TrisB7_label.Text = "i";
            else
            {
                TrisB7_label.Text = "o";
                PinB7_button.Text = ((var.register[0, 6] & 0x80) / 0x80).ToString("X");
            }
            if ((var.register[0, 135] & 0x40) == 64) TrisB6_label.Text = "i";
            else
            {
                TrisB6_label.Text = "o";
                PinB6_button.Text = ((var.register[0, 6] & 0x40) / 0x40).ToString("X");
            }
            if ((var.register[0, 135] & 0x20) == 32) TrisB5_label.Text = "i";
            else
            {
                TrisB5_label.Text = "o";
                PinB5_button.Text = ((var.register[0, 6] & 0x20) / 0x20).ToString("X");
            }
            if ((var.register[0, 135] & 0x10) == 16) TrisB4_label.Text = "i";
            else
            {
                TrisB4_label.Text = "o";
                PinB4_button.Text = ((var.register[0, 6] & 0x10) / 0x10).ToString("X");
            }
            if ((var.register[0, 135] & 0x08) == 8) TrisB3_label.Text = "i";
            else
            {
                TrisB3_label.Text = "o";
                PinB3_button.Text = ((var.register[0, 6] & 0x08) / 0x08).ToString("X");
            }
            if ((var.register[0, 135] & 0x04) == 4) TrisB2_label.Text = "i";
            else
            {
                TrisB2_label.Text = "o";
                PinB2_button.Text = ((var.register[0, 6] & 0x04) / 0x04).ToString("X");
            }
            if ((var.register[0, 135] & 0x02) == 2) TrisB1_label.Text = "i";
            else
            {
                TrisB1_label.Text = "o";
                PinB1_button.Text = ((var.register[0, 6] & 0x02) / 0x02).ToString("X");
            }
            if ((var.register[0, 135] & 0x01) == 1) TrisB0_label.Text = "i";
            else
            {
                TrisB0_label.Text = "o";
                PinB0_button.Text = ((var.register[0, 6] & 0x01) / 0x01).ToString("X");
            }
        }
        private void Zurücksetzen()
        {
            if (var.zeilen?.Length > 0)
            {
                Array.Clear(var.zeilen, 0, var.zeilen.Length);
            }

            var.option = 0xFF;
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

            var.autorun = false;
            var.BP_reached = -1;
            Automatic_Btn.ForeColor = Color.Red;

            var.pc = 0;

            var.laufzeit = 0;
            var.zykluszeit = 0; //evtl. nicht notwendig

            for (int i = 0; i < Ausgabe_LV.Items.Count; i++)
            {
                //Ausgabe_LV.Items[i].Checked = false;
                Ausgabe_LV.Items[i].BackColor = Color.White;
                Ausgabe_LV.Items[i].ForeColor = Color.Black;
            }

            if (var.code?.Count > 0)
            {
                Einzelschritt_Btn.Enabled = true;
                Automatic_Btn.Enabled = true;
            }
            else
            {
                Einzelschritt_Btn.Enabled = false;
                Automatic_Btn.Enabled = false;
            }

            var.wReg = 0;
            var.cflag = 0;
            var.dcflag = 0;
            var.zflag = 0;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    var.register[i, j] = 0;
                }
            }

            if (var.pc < var.code?.Count)
            {
                Ausgabe_LV.Items[var.code[var.pc][0]].BackColor = Color.DarkOrange;
            }

            /*if (programmcode?.Count > 0)
            {
                befehlsSwitch(programmcode[0]);
            }*/
            UpdateAll();
        }
        private void PopulateDataGridView()
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    var.register[i, j] = 0;
                }
            }
            var.register[0, 6] = 0x80;
            var.register[0, 8] = 0x80;
            var.register[0, 9] = 0x80;
            var.register[0, 134] = 0xFF;
            var.register[0, 135] = 0xFF;

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
                    dataGridView1.Rows[row].Cells[col + 1].Value = var.register[regseite, regadresse + 1];
                }
            }

        }
        private void UpdateDataGridView()
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
                    dataGridView1.Rows[row].Cells[col + 1].Value = (var.register[regseite, regadresse] & 0xFF);
                }
            }
        }

        private void Einlesen_Btn_Click(object sender, EventArgs e)
        {
            file.Einlesen(Ausgabe_LV);
            Zurücksetzen();
        }

        private void Einzelschritt_Btn_Click(object sender, EventArgs e)
        {
            Einzelschritt();
        }
        
        private void Automatic_Btn_MouseClick(object sender, MouseEventArgs e)
        {
            //Einzelschritt_Btn.Enabled = false;
            //Automatic_Btn.Enabled = false;

            var.autorun = !var.autorun;

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

        private void Zurücksetzen_Btn_Click(object sender, EventArgs e)
        {
            Zurücksetzen();
            return;
        }

        private void RPBU_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(RPBU_Toggle.Text) == 0)
            {
                RPBU_Toggle.Text = "1";
                var.option += 128;
            }
            else if (Convert.ToInt32(RPBU_Toggle.Text) == 1)
            {
                RPBU_Toggle.Text = "0";
                var.option -= 128;
            }
            option_out.Text = var.option.ToString("X2");

            return;
        }

        private void INTEDG_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(INTEDG_Toggle.Text) == 0)
            {
                INTEDG_Toggle.Text = "1";
                var.option += 64;
            }
            else if (Convert.ToInt32(INTEDG_Toggle.Text) == 1)
            {
                INTEDG_Toggle.Text = "0";
                var.option -= 64;
            }
            option_out.Text = var.option.ToString("X2");

            return;
        }

        private void T0CS_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(T0CS_Toggle.Text) == 0)
            {
                T0CS_Toggle.Text = "1";
                var.option += 32;
            }
            else if (Convert.ToInt32(T0CS_Toggle.Text) == 1)
            {
                T0CS_Toggle.Text = "0";
                var.option -= 32;
            }
            option_out.Text = var.option.ToString("X2");

            return;
        }

        private void T0SE_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(T0SE_Toggle.Text) == 0)
            {
                T0SE_Toggle.Text = "1";
                var.option += 16;
            }
            else if (Convert.ToInt32(T0SE_Toggle.Text) == 1)
            {
                T0SE_Toggle.Text = "0";
                var.option -= 16;
            }
            option_out.Text = var.option.ToString("X2");

            return;
        }

        private void PSA_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(PSA_Toggle.Text) == 0)
            {
                PSA_Toggle.Text = "1";
                var.option += 8;
            }
            else if (Convert.ToInt32(PSA_Toggle.Text) == 1)
            {
                PSA_Toggle.Text = "0";
                var.option -= 8;
            }
            option_out.Text = var.option.ToString("X2");

            return;
        }

        private void PS2_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(PS2_Toggle.Text) == 0)
            {
                PS2_Toggle.Text = "1";
                var.option += 4;
            }
            else if (Convert.ToInt32(PS2_Toggle.Text) == 1)
            {
                PS2_Toggle.Text = "0";
                var.option -= 4;
            }
            option_out.Text = var.option.ToString("X2");

            return;
        }

        private void PS1_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(PS1_Toggle.Text) == 0)
            {
                PS1_Toggle.Text = "1";
                var.option += 2;
            }
            else if (Convert.ToInt32(PS1_Toggle.Text) == 1)
            {
                PS1_Toggle.Text = "0";
                var.option -= 2;
            }
            option_out.Text = var.option.ToString("X2");

            return;
        }

        private void PS0_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(PS0_Toggle.Text) == 0)
            {
                PS0_Toggle.Text = "1";
                var.option += 1;
            }
            else if (Convert.ToInt32(PS0_Toggle.Text) == 1)
            {
                PS0_Toggle.Text = "0";
                var.option -= 1;
            }
            option_out.Text = var.option.ToString("X2");

            return;
        }

        private void GIE_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(GIE_Toggle.Text) == 0)
            {
                GIE_Toggle.Text = "1";
                var.register[0, 0xB] += 128;
            }
            else if (Convert.ToInt32(GIE_Toggle.Text) == 1)
            {
                GIE_Toggle.Text = "0";
                var.register[0, 0xB] -= 128;
            }
            intcon_out.Text = var.register[0, 0xB].ToString("X2");

            return;
        }

        private void PIE_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(PIE_Toggle.Text) == 0)
            {
                PIE_Toggle.Text = "1";
                var.register[0, 0xB] += 64;
            }
            else if (Convert.ToInt32(PIE_Toggle.Text) == 1)
            {
                PIE_Toggle.Text = "0";
                var.register[0, 0xB] -= 64;
            }
            intcon_out.Text = var.register[0, 0xB].ToString("X2");

            return;
        }

        private void T0IE_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(T0IE_Toggle.Text) == 0)
            {
                T0IE_Toggle.Text = "1";
                var.register[0, 0xB] += 32;
            }
            else if (Convert.ToInt32(T0IE_Toggle.Text) == 1)
            {
                T0IE_Toggle.Text = "0";
                var.register[0, 0xB] -= 32;
            }
            intcon_out.Text = var.register[0, 0xB].ToString("X2");

            return;
        }

        private void INTE_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(INTE_Toggle.Text) == 0)
            {
                INTE_Toggle.Text = "1";
                var.register[0, 0xB] += 16;
            }
            else if (Convert.ToInt32(INTE_Toggle.Text) == 1)
            {
                INTE_Toggle.Text = "0";
                var.register[0, 0xB] -= 16;
            }
            intcon_out.Text = var.register[0, 0xB].ToString("X2");

            return;
        }

        private void RBIE_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(RBIE_Toggle.Text) == 0)
            {
                RBIE_Toggle.Text = "1";
                var.register[0, 0xB] += 8;
            }
            else if (Convert.ToInt32(RBIE_Toggle.Text) == 1)
            {
                RBIE_Toggle.Text = "0";
                var.register[0, 0xB] -= 8;
            }
            intcon_out.Text = var.register[0, 0xB].ToString("X2");

            return;
        }

        private void T0IF_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(T0IF_Toggle.Text) == 0)
            {
                T0IF_Toggle.Text = "1";
                var.register[0, 0xB] += 4;
            }
            else if (Convert.ToInt32(T0IF_Toggle.Text) == 1)
            {
                T0IF_Toggle.Text = "0";
                var.register[0, 0xB] -= 4;
            }
            intcon_out.Text = var.register[0, 0xB].ToString("X2");

            return;
        }

        private void INTF_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(INTF_Toggle.Text) == 0)
            {
                INTF_Toggle.Text = "1";
                var.register[0, 0xB] += 2;
            }
            else if (Convert.ToInt32(INTF_Toggle.Text) == 1)
            {
                INTF_Toggle.Text = "0";
                var.register[0, 0xB] -= 2;
            }
            intcon_out.Text = var.register[0, 0xB].ToString("X2");

            return;
        }

        private void RBIF_Toggle_MouseClick(object sender, MouseEventArgs e)
        {
            if (Convert.ToInt32(RBIF_Toggle.Text) == 0)
            {
                RBIF_Toggle.Text = "1";
                var.register[0, 0xB] += 1;
            }
            else if (Convert.ToInt32(RBIF_Toggle.Text) == 1)
            {
                RBIF_Toggle.Text = "0";
                var.register[0, 0xB] -= 1;
            }
            intcon_out.Text = var.register[0, 0xB].ToString("X2");

            return;
        }

        private void BP_skip_CB_CheckedChanged(object sender, EventArgs e)
        {
            var.BP_skip = BP_skip_CB.Checked;

            return;
        }
        
        private void PinA7_button_Click(object sender, EventArgs e)
        {
            PinA7_button.Text = pin.PinA7();
            UpdateAll();
        }

        private void PinA6_button_Click(object sender, EventArgs e)
        {
            PinA6_button.Text = pin.PinA6();
            UpdateAll();
        }

        private void PinA5_button_Click(object sender, EventArgs e)
        {
            PinA5_button.Text = pin.PinA5();
            UpdateAll();
        }

        private void PinA4_button_Click(object sender, EventArgs e)
        {
            PinA4_button.Text = pin.PinA4();
            UpdateAll();

        }

        private void PinA3_button_Click(object sender, EventArgs e)
        {
            PinA3_button.Text = pin.PinA3();
            UpdateAll();
        }

        private void PinA2_button_Click(object sender, EventArgs e)
        {
            PinA2_button.Text = pin.PinA2();
            UpdateAll();
        }

        private void PinA1_button_Click(object sender, EventArgs e)
        {
            PinA1_button.Text = pin.PinA1();
            UpdateAll();
        }

        private void PinA0_button_Click(object sender, EventArgs e)
        {
            PinA0_button.Text = pin.PinA0();
            UpdateAll();
        }

        private void PinB7_button_Click(object sender, EventArgs e)
        {
            PinB7_button.Text = pin.PinB7();
            UpdateAll();
        }

        private void PinB6_button_Click(object sender, EventArgs e)
        {
            PinB6_button.Text = pin.PinB6();
            UpdateAll();
        }

        private void PinB5_button_Click(object sender, EventArgs e)
        {
            PinB5_button.Text = pin.PinB5();
            UpdateAll();
        }

        private void PinB4_button_Click(object sender, EventArgs e)
        {
            PinB4_button.Text = pin.PinB4();
            UpdateAll();
        }

        private void PinB3_button_Click(object sender, EventArgs e)
        {
            PinB3_button.Text = pin.PinB3();
            UpdateAll();
        }

        private void PinB2_button_Click(object sender, EventArgs e)
        {
            PinB2_button.Text = pin.PinB2();
            UpdateAll();
        }

        private void PinB1_button_Click(object sender, EventArgs e)
        {
            PinB1_button.Text = pin.PinB1();
            UpdateAll();
        }

        private void PinB0_button_Click(object sender, EventArgs e)
        {
            PinB0_button.Text = pin.PinB0();
            UpdateAll();
        }

        private void QuarzFreq_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            uiAccess.UpdateQuarzFrequenz(quarzFreq_comboBox.SelectedIndex);
        }
    }

}