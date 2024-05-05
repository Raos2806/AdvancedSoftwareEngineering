using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIC_Controller
{
    public interface IstackPush
    {
        void PushStack(int toPush);
    }

    public interface IstackPop
    {
        int PopStack();
    }

    public class StackPush: IstackPush
    {
        Variablen var;

        public StackPush(Variablen var)
        {
            this.var = var;
        }

        public void PushStack(int toPush)
        {
            var.stack1[var.stackPointer] = Convert.ToString(toPush, 2).PadLeft(13, '0');
            var.stackPointer++;
            if (var.stackPointer > 7)
            {
                var.stackPointer = 0;
            }
        }
    }

    public class StackPop: IstackPop
    {
        Variablen var;

        public StackPop(Variablen var)
        {
            this.var = var;
        }

        public int PopStack()
        {
            if (var.stackPointer > 0)
            {
                var.stackPointer--;
            }
            else if ((var.stackPointer == 0) && (var.stack1[7] != null))
            {
                var.stackPointer = 7;
            }

            int retval = Convert.ToInt16(var.stack1[var.stackPointer], 2);
            return retval;
        }
    }
}