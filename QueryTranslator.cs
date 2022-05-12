using System.Linq.Expressions;
using System.Text;

namespace Queryable
{
    public class QueryTranslator : CusttomExpressionVisitor // ExpressionVisitor
    {
        private StringBuilder _stringBuilder;

        public string Translate(Expression expression)
        {
            _stringBuilder = new StringBuilder();

            Visit(expression);

            return _stringBuilder.ToString();
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
                e = ((UnaryExpression)e).Operand;

            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(System.Linq.Queryable) && m.Method.Name == "Where")
            {
                _stringBuilder.Append("SELECT * FROM (");
                Visit(m.Arguments[0]);
                _stringBuilder.Append(") AS T WHERE ");
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                Visit(lambda.Body);

                return m;
            }
            throw new NotSupportedException($"The method '{m.Method.Name}' is not supported");
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    _stringBuilder.Append(" NOT ");
                    Visit(u.Operand);
                    break;

                default:
                    throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported");
            }
            return u;
        }

        protected override Expression VisitBinary(BinaryExpression b)
        {
            _stringBuilder.Append("(");
            Visit(b.Left);
            switch (b.NodeType)
            {
                case ExpressionType.And:
                    _stringBuilder.Append(" AND ");
                    break;

                case ExpressionType.Or:
                    _stringBuilder.Append(" OR");
                    break;

                case ExpressionType.Equal:
                    _stringBuilder.Append(" = ");
                    break;

                case ExpressionType.NotEqual:
                    _stringBuilder.Append(" <> ");
                    break;

                case ExpressionType.LessThan:
                    _stringBuilder.Append(" < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    _stringBuilder.Append(" <= ");
                    break;

                case ExpressionType.GreaterThan:
                    _stringBuilder.Append(" > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    _stringBuilder.Append(" >= ");
                    break;

                default:
                    throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported");
            }

            Visit(b.Right);
            _stringBuilder.Append(")");
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            IQueryable q = c.Value as IQueryable;
            if (q != null)
            {
                _stringBuilder.Append("SELECT * FROM ");
                _stringBuilder.Append(q.ElementType.Name);
            }
            else if (c.Value == null)
            {
                _stringBuilder.Append("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        _stringBuilder.Append(((bool)c.Value) ? 1 : 0);
                        break;

                    case TypeCode.String:
                        _stringBuilder.Append("'");
                        _stringBuilder.Append(c.Value);
                        _stringBuilder.Append("'");
                        break;

                    case TypeCode.Object:
                        throw new NotSupportedException($"The constant for '{c.Value}' is not supported");

                    default:
                        _stringBuilder.Append(c.Value);
                        break;
                }
            }

            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                _stringBuilder.Append(m.Member.Name);
                return m;
            }

            throw new NotSupportedException($"The member '{m.Member.Name}' is not supported");
        }
    }
}