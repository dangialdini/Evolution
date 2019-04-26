using Evolution.DAL;
using Evolution.CommonService;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.AccountsConnectorService {
    public partial class AccountsConnectorService {

        public ModelError SendCompletedPurchaseOrderToAccounts(PurchaseOrderHeaderModel poh) {
            ModelError error = new ModelError();
            error.SetInfo(EvolutionResources.infOrderCompleted);
            return error;
        }
    }
}
