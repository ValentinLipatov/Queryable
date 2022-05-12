using System.Linq.Expressions;
using System.Reflection;

namespace Queryable
{
    public class QueryProvider : IQueryProvider
    {
        IQueryable<T> IQueryProvider.CreateQuery<T>(Expression expression)
        {
            return new Query<T>(this, expression);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            Type elementType = Helper.GetElementType(expression.Type);

            try
            {
                var type = typeof(Query<>);
                var genericType = type.MakeGenericType(elementType);

                return (IQueryable)Activator.CreateInstance(genericType, new object[] { this, expression });
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
        }

        T IQueryProvider.Execute<T>(Expression expression)
        {
            return (T)Execute(expression);
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return Execute(expression);
        }

        public object Execute(Expression expression)
        {
            var comandText = new QueryTranslator().Translate(expression);
            var elementType = Helper.GetElementType(expression.Type);

            // TODO: Выполнить запрос и вернуть ответ

            return null;
        }
    }
}