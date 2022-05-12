using System.Collections;
using System.Linq.Expressions;

namespace Queryable
{
    public class Query<T> : IQueryable<T>, IQueryable, IEnumerable<T>, IEnumerable, IOrderedQueryable<T>, IOrderedQueryable
    {
        public Query(QueryProvider provider)
        {
            Throw.IfNull(provider, nameof(provider));

            _provider = provider;
            _expression = Expression.Constant(this);
        }

        public Query(QueryProvider provider, Expression expression)
        {
            Throw.IfNull(provider, nameof(provider));
            Throw.IfNull(expression, nameof(expression));
            Throw.IfNotAssignable(typeof(IQueryable<T>), expression.Type, nameof(expression));

            _provider = provider;
            _expression = expression;
        }

        Type IQueryable.ElementType => typeof(T);

        private Expression _expression;
        Expression IQueryable.Expression => _expression;

        private QueryProvider _provider;
        IQueryProvider IQueryable.Provider => _provider;

        public IEnumerator<T> GetEnumerator()
        {
            return Throw.IfNotIs<IEnumerable<T>>(_provider.Execute(_expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Throw.IfNotIs<IEnumerable>(_provider.Execute(_expression)).GetEnumerator();
        }
    }
}