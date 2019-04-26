using System;
using System.Security.Permissions;
using System.Security;
using System.Threading;
using System.Configuration;
using System.Collections.Generic;
using System.Security.Claims;
using Evolution.DAL;
using Evolution.Resources;
using Evolution.Enumerations;
using Evolution.Extensions;
using System.IO;

namespace Evolution.Security {
    public class CustomPrincipalPermission : CodeAccessSecurityAttribute {
        /*  AD/domain configuration:
           
            Domain name: {DOMAINNAME}
           
            User groups:
                Domain Users
                Everyone
                BUILTIN\Administrators
                Performance Log Users
                BUILTIN\Users
                NT AUTHORITY\INTERACTIVE
                CONSOLE LOGON
                NT AUTHORITY\Authenticated Users
                This Organization
                LOCAL
                Terminal Server Users
                System SQL
                General
                Web Workplace Users
                High Mandatory Level
        */

        public CustomPrincipalPermission(SecurityAction action)
            : base(action) { }

        public string Role { get; set; }

        public override IPermission CreatePermission() {
            return new RolesPrincipalPermission(Role);
        }

        public static string ChangeToSettingsString(string rl) {
            // Use web.config to map aaplication role names to AD role names.
            // This is a temporary measure until all roles are created in AD.
            string  rc = "";
            string[] roles = rl.Trim().Split(',');

            foreach (string roleName in roles) {
                if (!string.IsNullOrEmpty(rc)) rc += ",";
                rc += ConfigurationManager.AppSettings["Role" + roleName];
            }
            return rc;
        }
    }

    public class RolesPrincipalPermission : IPermission {

        private string _role;

        public RolesPrincipalPermission(string role) {
            _role = CustomPrincipalPermission.ChangeToSettingsString(role);
        }

        #region IPermission Members

        public IPermission Copy() {
            return new RolesPrincipalPermission(_role);
        }

        public void Demand() {
            bool        bFound = false;
            string[]    roles = _role.Split(',');

            var currentUser = System.Web.HttpContext.Current.User;

            foreach (string role in roles) {
                if(currentUser.IsInRole(role.Trim())) {
                    //if (Thread.CurrentPrincipal.IsInRole(role.Trim())) {
                    bFound = true;
                    break;
                }
            }

            if (!bFound) {
                using(EvolutionEntities db = new EvolutionEntities()) {
                    db.WriteLog(LogSection.Security,
                                LogSeverity.Severe,
                                "",
                                EvolutionResources.errUserIsNotInRole
                                                  //.DoSubstitutions(Thread.CurrentPrincipal.Identity.Name, _role),
                                                  .DoSubstitutions(currentUser.Identity.Name, _role),
                                "");
                }

                throw new SecurityException(EvolutionResources.errUserIsNotInRole
                                                              //.DoSubstitutions(Thread.CurrentPrincipal.Identity.Name, _role));
                                                              .DoSubstitutions(currentUser.Identity.Name, _role));
            }
        }

        public IPermission Intersect(IPermission target) {
            return this;
        }

        public bool IsSubsetOf(IPermission target) {
            return false;
        }

        public IPermission Union(IPermission target) {
            return this;
        }

        #endregion

        #region ISecurityEncodable Members

        public void FromXml(SecurityElement e) {
            throw new NotImplementedException();
        }

        public SecurityElement ToXml() {
            throw new NotImplementedException();
        }

        #endregion
    }
}
