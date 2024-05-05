using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PIC_Controller
{
    public interface IfileHandler
    {
        void Einlesen(ListView listview);
        //void Aufsplitten(List<string> lines);
    }

    public class FileHandler: IfileHandler
    {
        Variablen var;

        public FileHandler(Variablen var)
        {
            this.var = var;
        }

        public void Einlesen(ListView listview)
        {
            Size laenge = new Size(0, 0);
            Font font = new Font("Arial", 10, FontStyle.Regular);

            if (var.zeilen?.Length > 0)
            {
                Array.Clear(var.zeilen, 0, var.zeilen.Length);
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

                    var.zeilen = File.ReadAllLines(filePath);

                    var.programmcode = new List<int>();

                    foreach (string zeile in var.zeilen)
                    {
                        if (zeile.Length >= 8 && !string.IsNullOrWhiteSpace(zeile.Substring(0, 1)))
                        {
                            string extractedChars = zeile.Substring(5, 4);
                            int intValue = Convert.ToInt32(extractedChars, 16);
                            var.programmcode.Add(intValue);
                        }
                    }
                }
                else
                {
                    lines = null;
                }
            }

            if (lines != null)
            {
                listview.Items.Clear();
                for (int i = 0; i < lines.Count; i++)
                {
                    if (laenge.Width < TextRenderer.MeasureText(lines[i], font).Width)
                    {
                        laenge.Width = TextRenderer.MeasureText(lines[i], font).Width;
                        listview.Columns[0].Width = laenge.Width;
                    }

                    listview.Items.Add(lines[i]);
                }

                Aufsplitten(lines);
            }
        }

        private void Aufsplitten(List<string> lines)
        {
            string codezeile;
            var.code = new List<List<int>>();

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

                    var.code.Add(zeile);
                }
            }

            return;
        }
    }
}
