using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIC_Controller
{
    public interface Iflag
    {
        string WriteFlag(int flag);
    }

    public class Flag: Iflag
    {
        public string WriteFlag(int flag)
        {
            return flag.ToString("X");
        }
    }
}
