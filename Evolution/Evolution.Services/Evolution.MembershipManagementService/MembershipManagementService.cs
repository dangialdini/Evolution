using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.DirectoryServices.AccountManagement;
using Evolution.DAL;
using Evolution.CommonService;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Resources;
using Evolution.Enumerations;
using AutoMapper;

namespace Evolution.MembershipManagementService {
    public class MembershipManagementService : CommonService.CommonService {

        #region Private Members

        private UserModel _user = null;
        private List<UserGroup> _userGroups = null;

        #endregion

        #region Construction

        protected IMapper Mapper = null;

        public MembershipManagementService(EvolutionEntities dbEntities) : base(dbEntities) {
            // Setup Automapper mappings
            var config = new MapperConfiguration((cfg => {
                cfg.CreateMap<User, UserModel>();
                cfg.CreateMap<UserModel, User>();
                cfg.CreateMap<UserAlias, UserAliasModel>();
                cfg.CreateMap<UserAliasModel, UserAlias>();
            }));

            Mapper = config.CreateMapper();
        }

        #endregion

        #region Public Members - AD Integration

        public string UserId(bool includeDomainName = false) {
            string userId = Thread.CurrentPrincipal.Identity.Name;
            if(!includeDomainName) {
                int pos = userId.IndexOf("\\");
                if (pos != -1) userId = userId.Substring(pos + 1);
            }
            return userId;
        }

        public bool IsLoggedIn {
            get {
                string userId = null;
                try {
                    userId = Thread.CurrentPrincipal.Identity.Name;
                } catch { }
                return userId != null;
            }
        }

        public UserModel User {
            get {
                if(_user == null) {
                    string userId = UserId();

                    bool bNew = false;

                    User user = db.FindUser(userId);
                    if (user == null) {
                        // User not found, so create them
                        string firstName = userId,
                               lastName = "";
                        int pos = firstName.IndexOf(".");
                        if (pos != -1) {
                            lastName = firstName.Substring(pos + 1);
                            firstName = firstName.Substring(0, pos);
                        }

                        user = new User {
                            Name = userId,
                            FirstName = firstName,
                            LastName = lastName,
                            DateFormat = "dd/MM/yyyy",
                            Enabled = true
                        };
                        db.InsertOrUpdateUser(user);
                        bNew = true;
                    }

                    _user = MapToModel(user);

                    // Set a default company
                    if (bNew) {
                        var company = db.FindCompanies()
                                        .FirstOrDefault();
                        if(company != null) SaveProperty(MMSProperty.CurrentCompany, company.Id);
                    }
                }
                return _user;
            }
            set { _user = value; }
        }

        public List<UserGroup> UserGroups {
            get {
                if (_userGroups == null) {
                    _userGroups = new List<UserGroup>();

                    foreach (var userGroup in db.FindUserGroups()) {
                        var convertedName = convertAppRoleNameToADName(userGroup.GroupName);
                        if (Thread.CurrentPrincipal.IsInRole(convertedName)) {
                            _userGroups.Add(userGroup);
                        }
                    }
                }
                return _userGroups;
            }
        }

        public string FindUserRoles() {
            string rc = "";
            foreach (var group in UserGroups) {
                if (!string.IsNullOrEmpty(rc)) rc += ",";
                rc += group.GroupName;
            }
            return rc;
        }

        /*
        // Active Directory Version
        public List<string> FindGroupsForUser(UserModel salesPerson) {
            List<string> groups = new List<string>();

            using (PrincipalContext domain = new PrincipalContext(ContextType.Domain)) {
                using (UserPrincipal user = UserPrincipal.FindByIdentity(domain, salesPerson.LoginId)) {
                    if (user != null) {
                        groups = user.GetGroups()                   // Returns a collection of principal objects
                                     .Select(x => x.SamAccountName) // Name of the group
                                     .ToList();
                        //PrincipalSearchResult<Principal> userGroups = user.GetAuthorizationGroups();

                        // iterate over all groups
                        //foreach (Principal p in userGroups) {
                            // Make sure we only add group principals
                        //    if (p is GroupPrincipal) groups.Add(p.DisplayName);
                        //}
                    }
                }
            }

            return groups;
        }
        */

        public UserModel MapToModel(User item) {
            var newItem = Mapper.Map<User, UserModel>(item);
            return newItem;
        }

        // Test version until A/D available
        public List<UserGroup> FindGroupsForUser(UserModel salesPerson) {
            return db.FindUserGroupsForUser(salesPerson.Id)
                     .ToList();
        }

        /*
        // Active Directory Version
        public List<UserModel> FindUsersInGroup(string groupName) {
            List<UserModel> users = new List<UserModel>();

            using (PrincipalContext domain = new PrincipalContext(ContextType.Domain)) {
                using (var group = GroupPrincipal.FindByIdentity(domain, groupName)) {
                    if (group != null) {
                        foreach (UserPrincipal user in group.GetMembers(true)) {
                            var groupUser = FindUserModel(user.Name);
                            if(groupUser != null) users.Add(groupUser);
                        }
                    }
                }
            }

            return users;
        }
        */

        // Test versions until A/D available
        public List<ListItemModel> FindUsersInGroup(string groupName) {
            var users = new List<ListItemModel>();
            var userGroup = db.FindUserGroup(groupName);
            if (userGroup != null) {
                foreach (var groupUser in userGroup.UserGroupUsers) {
                    if (users.Where(ul => ul.Id == groupUser.UserId.ToString())
                             .FirstOrDefault() == null) {
                        users.Add(new ListItemModel { Id = groupUser.UserId.ToString(),
                            Text = (groupUser.User.FirstName + " " + groupUser.User.LastName).Trim()});
                    }
                }
            }
            return users;
        }

        public List<UserModel> FindUsersInGroup(UserGroup userGroup) {
            List<UserModel> users = new List<UserModel>();
            foreach (var user in db.FindUserGroupUsers(userGroup)) {
                if (users.Where(ul => ul.Id == user.Id)
                        .FirstOrDefault() == null) {
                    users.Add(MapToModel(user));
                }
            }
            return users;
        }

        // Test version until A/D available
        public void AddUserToGroup(string groupName, UserModel user) {
            var group = db.FindUserGroup(groupName);
            if(group == null) {
                group = new UserGroup { GroupName = groupName, Enabled = true };
                db.InsertOrUpdateUserGroup(group);
            }

            var groupUser = db.FindUserGroupUser(group.Id, user.Id);
            if(groupUser == null) {
                groupUser = new UserGroupUser {
                    UserGroup = group,
                    User = db.FindUser(user.Id)
                };
                db.InsertOrUpdateUserGroupUser(groupUser);
            }
        }

        public bool IsUserInRole(string requiredRoles) {
            bool bFound = false;
            string[] roles = requiredRoles.Split(',');

            foreach (string roleName in roles) {
                if(UserGroups.Any(ug => ug.GroupName == roleName)) {
                    bFound = true;
                    break;
                }
            }
            return bFound;
        }

        private string convertAppRoleNameToADName(string roleName) {
            return ConfigurationManager.AppSettings["Role" + roleName];
        }

        #endregion

        #region Public Members - User

        public List<ListItemModel> FindUserListItemModel() {
            List<ListItemModel> users = new List<ListItemModel>();

            foreach (var user in db.FindUsers()) {
                var item = new ListItemModel {
                    Id = user.Id.ToString(),
                    Text = db.MakeName(user),
                    ImageURL = ""
                };
                users.Add(item);
            }
            return users.OrderBy(m => m.Text)
                        .ToList();
        }

        public UserModel FindUserModel(int id) {
            UserModel model = null;
            var user = db.FindUser(id);
            if(user != null) model = MapToModel(user);
            return model;
        }

        public UserModel FindUserModel(string userName) {
            UserModel model = null;
            var user = db.FindUser(userName);
            if (user != null) model = MapToModel(user);
            return model;
        }

        public UserModel FindNoReplyMailSenderUser() {
            UserModel user = new UserModel {
                EMail = GetConfigurationSetting("NoReplySender", "")
            };
            return user;
        }

        public UserModel FindUserByAliasName(string userName, string defaultValue = "AutoGen") {
            var user = db.FindUser(userName);
            if(user == null) {
                foreach(var alias in db.FindUserAliases(userName).ToList()) {
                    if (db.FindUser(alias.User.Name) != null) {
                        user = alias.User;
                    } else {
                        user = db.FindUser(defaultValue);
                    }
                }
            }
            return MapToModel(user);
        }

        public Error InsertOrUpdateUser(UserModel user, UserModel userDoingUpdate, string lockGuid) {
            var error = validateUserModel(user);
            if (!error.IsError) {
                bool bCont = true;
                if(userDoingUpdate != null) {
                    // Check that the lock is still current
                    bCont = db.IsLockStillValid(typeof(User).ToString(), user.Id, lockGuid);
                    if(!bCont) error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "FirstName");
                }

                if(bCont) {
                    User temp = null;
                    if (user.Id != 0) temp = db.FindUser(user.Id);
                    if (temp == null) temp = new User();

                    Mapper.Map<UserModel, User>(user, temp);

                    db.InsertOrUpdateUser(temp);
                    user.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteUser(UserModel user) {
            DeleteUser(user.Id);
        }

        public void DeleteUser(int id) {
            db.DeleteUser(id);
        }

        public string LockUser(UserModel model) {
            return db.LockRecord(typeof(User).ToString(), model.Id);
        }

        private Error validateUserModel(UserModel model) {
            var error = isValidRequiredString(getFieldValue(model.Name), 52, "Name", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.FirstName), 20, "FirstName", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.LastName), 52, "LastName", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidRequiredEMail(getFieldValue(model.EMail), 255, "EMail", EvolutionResources.errEMailRequiredInField);

            return error;
        }

        #endregion

        #region Public Members - UserAlias

        public Error InsertOrUpdateUserAlias(UserAliasModel userAliasModel, UserModel user, string lockGuid) {
            var error = validateUserAliasModel(userAliasModel);
            if (!error.IsError) {
                // Check that the lock is still current
                if(!db.IsLockStillValid(typeof(UserAlias).ToString(), userAliasModel.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "");
                } else {
                    UserAlias temp = null;
                    if (userAliasModel.Id != 0) temp = db.FindUserAlias(userAliasModel.Id);
                    if (temp == null) temp = new UserAlias();

                    Mapper.Map<UserAliasModel, UserAlias>(userAliasModel, temp);

                    db.InsertOrUpdateUserAlias(temp);
                    userAliasModel.Id = temp.Id;
                }
            }
            return error;
        }

        private Error validateUserAliasModel(UserAliasModel model) {
            var error = isValidRequiredString(getFieldValue(model.Name), 100, "Name", EvolutionResources.errTextDataRequiredInField);
            return error;
        }

        #endregion

        #region Public Members - User Session

        public void SaveProperty(string propName, string propValue) {
            db.InsertOrUpdateUserSessionProperty(User.Id, propName, propValue);
        }

        public void SaveProperty(string propName, int propValue) {
            SaveProperty(propName, propValue.ToString());
        }

        public void SaveProperty(string propName, DateTime propValue) {
            SaveProperty(propName, propValue.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        public string GetProperty(string propName, string defaultValue) {
            var prop = db.FindUserSessionProperty(User.Id, propName);
            if(prop == null) {
                return defaultValue;
            } else {
                return prop.PropertyValue;
            }
        }

        public int GetProperty(string propName, int defaultValue) {
            int rc;
            string propValue = GetProperty(propName, defaultValue.ToString());
            try {
                rc = Convert.ToInt32(propValue);
            } catch {
                rc = defaultValue;
            }
            return rc;
        }

        public DateTime GetProperty(string propName, DateTime defaultValue) {
            DateTime rc;
            string propValue = GetProperty(propName, defaultValue.ToString("yyyy-MM-dd HH:mm:ss"));
            try {
                rc = Convert.ToDateTime(propValue);
            } catch {
                rc = defaultValue;
            }
            return rc;
        }

        #endregion
    }
}
