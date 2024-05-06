using PIC_Controller;

namespace PIC_Test
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestPushStack()
        {
            //arrange
            Variablen var = new Variablen();
            StackPush stackPush = new StackPush(var);

            int stackPosition = var.stackPointer = 0;
            int addressForStack = 1;

            int expectedStackpointer = 1;
            string expectedStackContent = "0000000000001";
            //act
            stackPush.PushStack(addressForStack);
            string actualStackContent = var.stack1[stackPosition];
            //assert
            Assert.AreEqual(expectedStackContent, actualStackContent);
            Assert.AreEqual(expectedStackpointer, var.stackPointer);
        }

        [TestMethod]
        public void TestUpdateWReg1()
        {
            //arrange
            Variablen var = new Variablen();
            UIAccess uiAccess = new UIAccess(var);

            string updateWReg = "0FF";
            var.wReg = 0;

            int expectedWReg = 0;
            //act
            uiAccess.wRegUpdate(updateWReg);
            //assert
            Assert.AreEqual(expectedWReg, var.wReg);
        }

        [TestMethod]
        public void TestUpdateWReg2()
        {
            //arrange
            Variablen var = new Variablen();
            UIAccess uiAccess = new UIAccess(var);

            string updateWReg = "FF";
            var.wReg = 0;

            int expectedWReg = 255;
            //act
            uiAccess.wRegUpdate(updateWReg);
            //assert
            Assert.AreEqual(expectedWReg, var.wReg);
        }
    }
}