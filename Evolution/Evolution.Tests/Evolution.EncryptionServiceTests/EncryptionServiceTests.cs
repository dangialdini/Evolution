using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.EncryptionService;

namespace Evolution.EncryptionServiceTests {
    [TestClass]
    public class RFC2898Tests : BaseTest {
        [TestMethod]
        public void EncryptTest() {
            // Tested by EncryptDecryptTest below
        }

        [TestMethod]
        public void DecryptTest() {
            // Tested by EncryptDecryptTest below
        }

        [TestMethod]
        public void EncryptDecryptTest() {
            string password = "password";
            string data = "This is some data to encrypt.";

            string encrypted = RFC2898.Encrypt(data, password);
            Assert.IsTrue(encrypted != data, $"Error: The Encrypt method returned {encrypted} when an encrypted string was expected");

            string decrypted = RFC2898.Decrypt(encrypted, password);
            Assert.IsTrue(decrypted != encrypted, $"Error: The Decrypt method returned {decrypted} when an unencrypted string was expected");
            Assert.IsTrue(decrypted == data, $"Error: The Decrypt method returned {decrypted} when {data} was expected");
        }
    }
}
