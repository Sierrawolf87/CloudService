using Microsoft.VisualStudio.TestTools.UnitTesting;
using CloudService_API.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudService_API.Data.Tests
{
    [TestClass()]
    public class AuxiliaryTests
    {
        [TestMethod()]
        public void GenerateHashPasswordTest()
        {
            Assert.AreEqual("EZrd35Q7PKSIu2Ihp3/DDE9e3pQhrIuB3SvQhEEQ48I=", Auxiliary.GenerateHashPassword("testPassword", "CloudService"));
            Assert.AreEqual("E1YUVnlj56oCXLdACoK8uB3/voOAm+JvJ51q0/MFaag=", Auxiliary.GenerateHashPassword("12345qwerty", "CloudService"));
            Assert.AreEqual("/VO+1TZlAehTlZsjgJTIQ+ejDt2eCv0T51BdU/qoiOI=", Auxiliary.GenerateHashPassword("zxcvbwqwer", "CloudService"));
        }

        [TestMethod()]
        public void GetExtensionTest()
        {
            Assert.AreEqual(".png", Auxiliary.GetExtension("file.png"));
            Assert.AreEqual(".zip", Auxiliary.GetExtension("file.zip"));
            Assert.AreEqual(".sketch", Auxiliary.GetExtension("file.png.sketch"));
            Assert.AreEqual(".data", Auxiliary.GetExtension("data1.file.data"));
        }
    }
}