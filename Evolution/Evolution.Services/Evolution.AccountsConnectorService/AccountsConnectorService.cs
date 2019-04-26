using Evolution.DAL;
using Evolution.CommonService;

namespace Evolution.AccountsConnectorService {
    public partial class AccountsConnectorService : CommonService.CommonService {

        #region Construction

        public AccountsConnectorService(EvolutionEntities dbEntities) : base(dbEntities) { }

        #endregion
    }
}
