using RSSAdvisoryAlert.Domain.Abstractions.Common;

namespace RSSAdvisoryAlert.Domain.Common.Mapper
{
    public abstract class BaseRequestMapper<TRequest, TEntity> : IRequestMapper<TRequest, TEntity> where TEntity : class
    {
        public virtual TEntity MapToEntity(TRequest request)
        {
            if (request == null) 
            { 
                throw new ArgumentNullException(nameof(request)); 
            }

            var entity = CreateEntity(request);

            Map(request, entity);

            return entity;
        }

        public virtual void MapToExistingEntity(TRequest request, TEntity entity)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            Map(request, entity);
        }
        
        protected abstract TEntity CreateEntity(TRequest request);

        protected abstract void Map(TRequest request, TEntity entity);
    }
}
