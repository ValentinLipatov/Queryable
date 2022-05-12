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

        private static Expression StripQuotes(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
                expression = ((UnaryExpression)expression).Operand;

            return expression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.DeclaringType == typeof(System.Linq.Queryable) && methodCallExpression.Method.Name == "Where")
            {
                _stringBuilder.Append("SELECT * FROM (");
                Visit(methodCallExpression.Arguments[0]);
                _stringBuilder.Append(") AS T WHERE ");
                LambdaExpression lambda = (LambdaExpression)StripQuotes(methodCallExpression.Arguments[1]);
                Visit(lambda.Body);

                return methodCallExpression;
            }
            throw new NotSupportedException($"The method '{methodCallExpression.Method.Name}' is not supported");
        }

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            switch (unaryExpression.NodeType)
            {
                case ExpressionType.Not:
                    _stringBuilder.Append(" NOT ");
                    Visit(unaryExpression.Operand);
                    break;

                default:
                    throw new NotSupportedException($"The unary operator '{unaryExpression.NodeType}' is not supported");
            }
            return unaryExpression;
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            _stringBuilder.Append("(");
            Visit(binaryExpression.Left);

            switch (binaryExpression.NodeType)
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
                    throw new NotSupportedException($"The binary operator '{binaryExpression.NodeType}' is not supported");
            }

            Visit(binaryExpression.Right);
            _stringBuilder.Append(")");

            return binaryExpression;
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            IQueryable q = constantExpression.Value as IQueryable;
            if (q != null)
            {
                _stringBuilder.Append("SELECT * FROM ");
                _stringBuilder.Append(q.ElementType.Name);
            }
            else if (constantExpression.Value == null)
            {
                _stringBuilder.Append("NULL");
            }
            else
            {
                switch (Type.GetTypeCode(constantExpression.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        _stringBuilder.Append(((bool)constantExpression.Value) ? 1 : 0);
                        break;

                    case TypeCode.String:
                        _stringBuilder.Append("'");
                        _stringBuilder.Append(constantExpression.Value);
                        _stringBuilder.Append("'");
                        break;

                    case TypeCode.Object:
                        throw new NotSupportedException($"The constant for '{constantExpression.Value}' is not supported");

                    default:
                        _stringBuilder.Append(constantExpression.Value);
                        break;
                }
            }

            return constantExpression;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
        {
            if (memberExpression.Expression != null && memberExpression.Expression.NodeType == ExpressionType.Parameter)
            {
                _stringBuilder.Append(memberExpression.Member.Name);
                return memberExpression;
            }

            throw new NotSupportedException($"The member '{memberExpression.Member.Name}' is not supported");
        }
    }
}