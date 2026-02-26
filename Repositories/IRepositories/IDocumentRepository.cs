using Entities.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IDocumentRepository
    {
        Task<List<Document>> GetList(string name, int status, int pageIndex, int pageSize);
        Task<int> GetCount(string name, int status);
        Task<Document> GetById(int id);
        Task<int> Insert(Document model);
        Task<int> Update(Document model);

    }
}
