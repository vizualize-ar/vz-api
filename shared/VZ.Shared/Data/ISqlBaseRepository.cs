using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VZ.Shared.Models;

namespace VZ.Shared.Data
{
    public interface ISqlBaseRepository<TEntity> where TEntity : SqlBaseModel
    {
        Task<long> AddAsync(TEntity entity);
        Task<DeleteDocumentResult> DeleteAsync(long id);
        Task<TEntity> GetAsync(long id);
        List<TAnything> GetAll<TAnything>(string fields = "c.*");
        Task<List<TAnything>> GetAllAsync<TAnything>(string fields = "c.*");
        Task<TAnything> GetFirstAsync<TAnything>(string fields = "*", string predicate = null, (string, object)[] parameters = null);
        /// <summary>
        /// Query for a select list of fields
        /// Example:
        /// <code>
        /// var parameters = new ValueTuple<string, object>[]
        /// {
        ///     ("@businessId", businessId)
        /// };
        /// return await _businessProductRepository.GetSomeAsync<BusinessProduct>("*", $"{nameof(BusinessProduct.businessId)} = @businessId", parameters);
        /// </code>
        /// </summary>
        /// <typeparam name="TAnything"></typeparam>
        /// <param name="fields"></param>
        /// <param name="predicate">Eg., $"{nameof(BusinessProduct.businessId)} = @businessId"</param>
        /// <param name="parameters">
        /// Example:
        /// <code>
        /// var parameters = new ValueTuple<string, object>[]
        /// {
        ///     ("@businessId", businessId)
        /// };
        /// </code
        /// </param>
        /// <param name="take"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        Task<List<TAnything>> GetSomeAsync<TAnything>(string fields = "*", string predicate = null, (string, object)[] parameters = null, int? take = null, int? skip = null);
        Task<List<TAnything>> GetSomeRawAsync<TAnything>(string sql = "select * from c", params ValueTuple<string, object>[] parameters);
        Task<UpdateDocumentResult> UpdateAsync(TEntity entity);
    }
}
