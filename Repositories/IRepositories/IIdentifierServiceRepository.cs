using System.Threading.Tasks;
namespace Repositories.IRepositories
{
    public interface IIdentifierServiceRepository
    {
        public  Task<string> buildServiceNo(int service_type, int? tenant_id = null);
        public Task<string> buildOrderManual(int? tenant_id = null);
        public bool IsOrderManual(string orderNo, int? tenant_id = null);
        public  Task<string> BuildPaymentRequest(int? tenant_id = null);
        public  Task<string> buildContractPay(int? tenant_id = null);
        public  Task<string> BuildPaymentVoucher(int? tenant_id = null);
        public Task<string> buildContractNo(int? tenant_id = null);
        public Task<string> buildClientNo(int client_type, int? tenant_id = null);
    }
}
