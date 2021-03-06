﻿using DecaTec.WebDav.Tools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecaTec.WebDav.UnitTest
{
    [TestClass]
    public class UnitTestTextHelper
    {
        [TestMethod]
        public void UT_TextHelper_StringContainsRawUnicode_WithRawUnicode()
        {
            string strUni = "\u03a0";
            var res = TextHelper.StringContainsRawUnicode(strUni);
            Assert.AreEqual(true, res);
        }

        [TestMethod]
        public void UT_TextHelper_StringContainsRawUnicode_WithOutRawUnicode()
        {
            string strUni = "Test";
            var res = TextHelper.StringContainsRawUnicode(strUni);
            Assert.AreEqual(false, res);
        }

        [TestMethod]
        public void UT_TextHelper_StringContainsRawUnicode_WithEmptyString()
        {
            string strUni = string.Empty; ;
            var res = TextHelper.StringContainsRawUnicode(strUni);
            Assert.AreEqual(false, res);
        }
    }
}
