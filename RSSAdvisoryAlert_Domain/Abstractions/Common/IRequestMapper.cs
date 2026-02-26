namespace RSSAdvisoryAlert.Domain.Abstractions.Common
{
    public interface IRequestMapper<TRequest, TEntity>
    {
        TEntity MapToEntity(TRequest request);

        void MapToExistingEntity(TRequest request, TEntity entity);
    }
}
