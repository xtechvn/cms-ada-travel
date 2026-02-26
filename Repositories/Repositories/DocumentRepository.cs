using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly DocumentDAL _DocumentDAL;

        public DocumentRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _DocumentDAL = new DocumentDAL(dataBaseConfig.Value.SqlServer.ConnectionString);
        }

        public async Task<List<Document>> GetList(string name, int status, int pageIndex, int pageSize)
        {
            return await _DocumentDAL.GetList(name, status, pageIndex, pageSize);
        }

        public async Task<int> GetCount(string name, int status)
        {
            return await _DocumentDAL.GetCount(name, status);
        }

        public async Task<Document> GetById(int id)
        {
            return await _DocumentDAL.GetById(id);
        }

        public async Task<int> Insert(Document model)
        {
            return await _DocumentDAL.Insert(model);
        }

        public async Task<int> Update(Document model)
        {
            return await _DocumentDAL.Update(model);
        }

        
    }
}
