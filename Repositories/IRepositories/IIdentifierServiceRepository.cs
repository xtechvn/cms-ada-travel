using System.Threading.Tasks;
namespace Repositories.IRepositories
{
    public interface IIdentifierServiceRepository
    {
        public  Task<string> buildServiceNo(int service_type);
        public Task<string> buildOrderManual(int? tenant_id = null);
        public bool IsOrderManual(string orderNo);
        public  Task<string> BuildPaymentRequest();
        public  Task<string> buildContractPay();
        public  Task<string> BuildPaymentVoucher();
        public Task<string> buildContractNo();
        public Task<string> buildClientNo(int client_type);
    }
}
