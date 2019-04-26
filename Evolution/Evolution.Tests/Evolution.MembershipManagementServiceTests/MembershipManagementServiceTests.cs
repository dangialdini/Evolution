using System;
using System.Threading;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.CommonService;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;

namespace Evolution.MembershipManagementServiceTests {
    [TestClass]
    public class MembershipManagementServiceTests : BaseTest {

        [Ignore]
        [TestMethod]
        public void FindUserRolesTest() {
            Assert.Fail("Test to be implemented");
        }

        [TestMethod]
        public void UserIdTest() {
            // Test that the service returns the same value as that of a direct call
            // to the thread current principal
            string userId1 = Thread.CurrentPrincipal.Identity.Name;
            string userId2 = MembershipManagementService.UserId(true);

            Assert.IsTrue(userId1 == userId2, $"Error: {userId2} was returned when {userId1} was expected");
        }

        [Ignore]
        [TestMethod]
        public void IsUserInRoleTest() {
            // This will be superceded with AD
            bool result = MembershipManagementService.IsUserInRole("ABD,DEF,Dom");
            Assert.IsFalse(result, "Error: True was returned when False was expected (1)");

            result = MembershipManagementService.IsUserInRole(UserRole.Administrator);
            Assert.IsTrue(result, "Error: False was returned when True was expected (2)");

            result = MembershipManagementService.IsUserInRole(UserRole.SuperUser);
            Assert.IsTrue(result, "Error: False was returned when True was expected (3)");

            result = MembershipManagementService.IsUserInRole(UserRole.AllUsers);
            Assert.IsTrue(result, "Error: False was returned when True was expected (4)");
        }

        [TestMethod]
        public void FindUserListItemModelTest() {
            // Warning: This test can fail if another test is creating users at the same time
            //          This occurs because users are global and not specific to an organisation,
            //          otherwise we segment users off into a separate test organisation.
            var user = GetTestUser();
            var testCompany = GetTestCompany(user);
            var model = MembershipManagementService.FindUserListItemModel();
            var dbData = db.FindUsers();

            int expected = dbData.Count(),
                actual = model.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were found when {expected} were expected");

            // Check that all the items match
            foreach (var item in model) {
                var dbItem = dbData.Where(m => m.Id.ToString() == item.Id).FirstOrDefault();
                Assert.IsTrue(dbItem != null, "Error: Model item not found in db item list");

                var test = MembershipManagementService.MapToModel(dbItem);
                AreEqual(item, test);
            }

            // Add another item a make sure it is found
            var newItem = GetTestUser(true, false);
            var error = MembershipManagementService.InsertOrUpdateUser(newItem, user, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            model = MembershipManagementService.FindUserListItemModel();
            var testItem = model.Where(i => i.Id.ToString() == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem != null, "Error: A NULL value was returned when a non-NULL value was expected");

            // Delete it and make sure it disappears
            MembershipManagementService.DeleteUser(newItem.Id);

            model = MembershipManagementService.FindUserListItemModel();
            testItem = model.Where(i => i.Id == newItem.Id.ToString()).FirstOrDefault();
            Assert.IsTrue(testItem == null, "Error: A non-NULL value was returned when a NULL value was expected");
        }

        [TestMethod]
        public void FindUserModelTest() {
            // Tested in DeleteUserTest
        }

        [TestMethod]
        public void FindNoReplyMailSenderUserTest() {
            var user = MembershipManagementService.FindNoReplyMailSenderUser();
            Assert.IsTrue(user != null, "Error: A NULL value was returned when an object was expected");
            Assert.IsTrue(user.EMail.IsValidEMail(), $"Error: EMail '{user.EMail}' was returned when a valid EMail address was expected");
        }

        [TestMethod]
        public void FindUserByAliasNameTest() {
            var testUser = GetTestUser();

            // Create UserAlias & add it to the db (to be able to search by UserAlias.Name)
            UserAliasModel model = createUserAlias(testUser);
            MembershipManagementService.InsertOrUpdateUserAlias(model, testUser, "");
            UserModel user = MembershipManagementService.FindUserByAliasName(model.Name);
            AreEqual(testUser, user);
        }

        [TestMethod]
        public void InsertOrUpdateUserTest() {
            // Tested in all tests which use CommonTest.GetTestUser
        }

        [TestMethod]
        public void DeleteUserTest() {
            var testUser = GetTestUser();

            var dbUser = db.FindUser(testUser.Id);
            Assert.IsTrue(dbUser != null, "Error: A NULL value was returned when a non-NULL value was expected (user not found)");

            var checkUser = MembershipManagementService.FindUserModel(dbUser.Id);
            Assert.IsTrue(checkUser != null, "Error: A NULL value was returned when a non-NULL value was expected (user not found)");

            var dbUserModel = MembershipManagementService.MapToModel(dbUser);
            AreEqual(checkUser, dbUserModel);

            MembershipManagementService.DeleteUser(dbUser.Id);
            checkUser = MembershipManagementService.FindUserModel(dbUser.Id);
            Assert.IsTrue(checkUser == null, "Error: A non-NULL value was returned when a NULL value was expected (user not deleted)");
        }

        [TestMethod]
        public void LockUserTest() {
            var testUser = GetTestUser();
            var testCompany = GetTestCompany(testUser);

            // Create a record
            var model = GetTestUser(true, false);

            var error = MembershipManagementService.InsertOrUpdateUser(model, testUser, "");
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");

            // Get the current Lock
            string lockGuid = MembershipManagementService.LockUser(model);
            Assert.IsTrue(!string.IsNullOrEmpty(lockGuid), "Error: Lock record was not found");

            // Simulate another user updating the record
            var otherUser = GetTestUser();
            error = MembershipManagementService.InsertOrUpdateUser(model, otherUser, lockGuid);
            Assert.IsTrue(!error.IsError, error.Message);

            // Now get the first user to update the record
            error = MembershipManagementService.InsertOrUpdateUser(model, testUser, lockGuid);
            Assert.IsTrue(error.IsError, "Error: The lock should have caused an error as it has changed");

            // Try to update with the new lock
            lockGuid = MembershipManagementService.LockUser(model);
            error = MembershipManagementService.InsertOrUpdateUser(model, testUser, lockGuid);
            Assert.IsTrue(!error.IsError, $"Error: {error.Message}");
        }

        [TestMethod]
        public void InsertOrUpdateUserAliasTest() {
            // Tested in FindUserByAliasNameTest
        }

        [TestMethod]
        public void SavePropertyTest() {
            var user = GetTestUser();

            MembershipManagementService.User = user;    // So we save properties against this user

            for (int i = 1; i < 50; i++) {
                string propName = "Prop" + i.ToString();

                // Set a string value
                string propValue1 = RandomString();

                MembershipManagementService.SaveProperty(propName, propValue1);

                string stringValue = MembershipManagementService.GetProperty(propName, "");
                Assert.IsTrue(propValue1 == stringValue, $"Error: Property {propName} returned {stringValue} when {propValue1} was expected");

                // Set an int value
                int propValue2 = RandomInt();

                MembershipManagementService.SaveProperty(propName, propValue2);

                int intValue = MembershipManagementService.GetProperty(propName, 0);
                Assert.IsTrue(propValue2 == intValue, $"Error: Property {propName} returned {intValue} when {propValue2} was expected");

                // set a date-time value
                DateTime propValue3 = RandomDateTime();

                MembershipManagementService.SaveProperty(propName, propValue3);

                DateTime dateTimeValue = MembershipManagementService.GetProperty(propName, new DateTime(1900, 1, 1, 0, 0, 0));
                // We compare the strings because the date values are sub-millisecond different
                Assert.IsTrue(propValue3.ToString() == dateTimeValue.ToString(), $"Error: Property {propName} returned {dateTimeValue} when {propValue3} was expected");
            }
        }

        [TestMethod]
        public void GetPropertyTest() {
            // Tested in SavePropertyTest
        }

        [Ignore]
        [TestMethod]
        public void FindGroupsForUserTest() {
            // This test stub is only needed until Active Directory groups are used
        }

        [Ignore]
        [TestMethod]
        public void FindUsersInGroupTest() {
            // This test stub is only needed until Active Directory groups are used
        }

        [Ignore]
        [TestMethod]
        public void AddUserToGroupTest() {
            // This test stub is only needed until Active Directory groups are used
        }

        [TestMethod]
        public void MapToModelTest() {
            // Tested by all READ tests in this module
        }

        private UserAliasModel createUserAlias(UserModel user) {
            UserAliasModel userAlias = new UserAliasModel();
            userAlias.UserId = user.Id;
            userAlias.Name = user.Name + "Alias";
            return userAlias;
        }
    }
}
