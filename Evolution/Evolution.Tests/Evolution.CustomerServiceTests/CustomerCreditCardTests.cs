using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Extensions;

namespace Evolution.CustomerServiceTests {
    public partial class CustomerServiceTests {
        [TestMethod]
        public void FindCreditCardsListItemModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var cardList = CustomerService.FindCreditCardsListItemModel(testCompany, testCustomer.Id);
            int expected = 0,
                actual = cardList.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Add cards
            int numCards = 3;
            List<CreditCardModel> cards = new List<CreditCardModel>();
            for (var i = 0; i < numCards; i++) {
                cards.Add(addCreditCard(testCompany, testCustomer));
            }

            db.RefreshCreditCards();

            cardList = CustomerService.FindCreditCardsListItemModel(testCompany, testCustomer.Id, false)
                                      .OrderBy(cl => cl.Id)
                                      .ToList();
            expected = numCards;
            actual = cardList.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Check that all the cards match
            for(int i = 0; i < numCards; i++) {
                AreEqual(cards[i], cardList[i]);
            }

            // Delete cards
            foreach(var card in cards) {
                CustomerService.DeleteCreditCard(card.Id);

                cardList = CustomerService.FindCreditCardsListItemModel(testCompany, testCustomer.Id);
                expected--;
                actual = cardList.Count();
                Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");
            }
        }

        [TestMethod]
        public void FindCreditCardsListModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var cardList = CustomerService.FindCreditCardsListModel(testCompany.Id, testCustomer.Id, 0, 1, 9999)
                                          .Items
                                          .OrderBy(cl => cl.Id)
                                          .ToList();
            int expected = 0,
                actual = cardList.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Add cards
            int numCards = 3;
            List<CreditCardModel> cards = new List<CreditCardModel>();
            for (var i = 0; i < numCards; i++) {
                cards.Add(addCreditCard(testCompany, testCustomer));
            }

            db.RefreshCreditCards();

            cardList = CustomerService.FindCreditCardsListModel(testCompany.Id, testCustomer.Id, 0, 1, 9999)
                                      .Items
                                      .OrderBy(cl => cl.Id)
                                      .ToList();
            expected = numCards;
            actual = cardList.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            // Check that all the cards match
            for (int i = 0; i < numCards; i++) {
                AreEqual(cards[i], cardList[i]);
            }

            // Delete cards
            foreach (var card in cards) {
                CustomerService.DeleteCreditCard(card.Id);

                cardList = CustomerService.FindCreditCardsListModel(testCompany.Id, testCustomer.Id, 0, 1, 9999)
                                          .Items
                                          .OrderBy(cl => cl.Id)
                                          .ToList();
                expected--;
                actual = cardList.Count();
                Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");
            }
        }

        [TestMethod]
        public void FindCreditCardModelTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            var card = addCreditCard(testCompany, testCustomer);

            db.RefreshCreditCards();    // Force EF to relink FK's

            // Find the card
            var testCard = CustomerService.FindCreditCardModel(card.Id, testCompany, testCustomer, false);
            testCard.CreditCardNo = testCard.CreditCardNo.ObscureString(true);
            Assert.IsTrue(testCard != null, $"Error: A NULL object was returned when a credit card object was expected");
            AreEqual(card, testCard);

            // Delete the card
            CustomerService.DeleteCreditCard(card.Id);

            // Find the card again
            testCard = CustomerService.FindCreditCardModel(card.Id, testCompany, testCustomer, false);
            Assert.IsTrue(testCard == null, $"Error: A credit card object was returned when NULL was expected. This indicates that the card failed to be deleted");
        }

        [TestMethod]
        public void InsertOrUpdateCreditCardTest() {
            // Tested in methods above
        }

        [TestMethod]
        public void DeleteCreditCardTest() {
            // Tested in methods above
        }

        [TestMethod]
        public void LockCreditCardTest() {
            // Get a test user and test company
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);
            var testCustomer = GetTestCustomer(testCompany, testUser);

            // Add card and try to retrieve it
            var card1 = addCreditCard(testCompany, testCustomer);

            // Get the current Lock
            string lockGuid = CustomerService.LockCreditCard(card1);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            var error = CustomerService.InsertOrUpdateCreditCard(card1, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = CustomerService.InsertOrUpdateCreditCard(card1, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = CustomerService.LockCreditCard(card1);
            error = CustomerService.InsertOrUpdateCreditCard(card1, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        private CreditCardModel addCreditCard(CompanyModel company, CustomerModel customer) {
            var now = DateTime.Now;
            var cardProvider = LookupService.FindCreditCardProviders().First();

            CreditCardModel model = new CreditCardModel {
                CompanyId = company.Id,
                CustomerId = customer.Id,
                CreditCardProviderId = Convert.ToInt32(cardProvider.Id),
                CardProviderName = cardProvider.Text,
                CardProviderLogo = "/Content/CreditCards/" + cardProvider.ImageURL,
                CreditCardNo = RandomString().Left(16),
                NameOnCard = RandomString().Left(42),
                Expiry = $"{now.Day}/18",
                CCV = RandomInt().ToString().PadLeft(3).Right(3),
                Notes = "",
                Enabled = true
            };
            var error = CustomerService.InsertOrUpdateCreditCard(model, "");
            Assert.IsTrue(!error.IsError, error.Message);

            model.CreditCardNo = model.CreditCardNo.ObscureString(true);

            return model;
        }
    }
}
