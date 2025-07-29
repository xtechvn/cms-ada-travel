using System.Threading.Tasks;
namespace Repositories.IRepositories
{
    public interface IIdentifierServiceRepository
    {
        Task<string> buildOrderNoManual(int company_type = 0);// don thu cong
        Task<string> buildContractPay(); // sinh mã PHIEU THU
        Task<string> buildContractNo();//mã hợp đồng
        public int countServiceUse(int service_type);
        Task<string> buildClientNo(int client_type);
    }
}
