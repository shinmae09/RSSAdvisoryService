namespace RSSAdvisoryAlert.Domain.Abstractions
{
    public interface IBaseRepository<T> where T : class
    {
        /// <summary>
        /// Get all Entities of Type T
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Get Entity of Type T by Id
        /// </summary>
        /// <returns></returns>
        Task<T> GetByIdAsync(int id);

        /// <summary>
        /// Add Entity of Type T
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Delete Entity of Type T by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteByIdAsync(int id);

        /// <summary>
        /// Saves all tracked changes for all entities.
        /// </summary>
        /// <returns></returns>
        Task SaveChangesAsync();
    }
}
