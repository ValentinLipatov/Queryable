using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Queryable
{
    public abstract class CusttomExpressionVisitor
    {
        protected CusttomExpressionVisitor()
        {

        }

        protected virtual Expression Visit(Expression expression)
        {
            if (expression == null)
                return expression;

            switch (expression.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary((UnaryExpression)expression);

                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return VisitBinary((BinaryExpression)expression);

                case ExpressionType.TypeIs:
                    return VisitTypeIs((TypeBinaryExpression)expression);

                case ExpressionType.Conditional:
                    return VisitConditional((ConditionalExpression)expression);

                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)expression);

                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)expression);

                case ExpressionType.MemberAccess:
                    return VisitMember((MemberExpression)expression);

                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)expression);

                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)expression);

                case ExpressionType.New:
                    return VisitNew((NewExpression)expression);

                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray((NewArrayExpression)expression);

                case ExpressionType.Invoke:
                    return VisitInvocation((InvocationExpression)expression);

                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression)expression);

                case ExpressionType.ListInit:
                    return VisitListInit((ListInitExpression)expression);

                default:
                    throw new Exception($"Unhandled expression type: '{expression.NodeType}'");
            }
        }

        protected virtual MemberBinding VisitBinding(MemberBinding memberBinding)
        {
            switch (memberBinding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment)memberBinding);

                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding)memberBinding);

                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding)memberBinding);

                default:
                    throw new Exception("Unhandled binding type '{binding.BindingType}'");
            }
        }

        protected virtual ElementInit VisitElementInitializer(ElementInit elementInit)
        {
            ReadOnlyCollection<Expression> arguments = VisitExpressionList(elementInit.Arguments);

            if (arguments != elementInit.Arguments)
                return Expression.ElementInit(elementInit.AddMethod, arguments);

            return elementInit;
        }

        protected virtual Expression VisitUnary(UnaryExpression unaryExpression)
        {
            Expression operand = Visit(unaryExpression.Operand);

            if (operand != unaryExpression.Operand)
                return Expression.MakeUnary(unaryExpression.NodeType, operand, unaryExpression.Type, unaryExpression.Method);

            return unaryExpression;
        }

        protected virtual Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Expression left = Visit(binaryExpression.Left);
            Expression right = Visit(binaryExpression.Right);
            Expression conversion = Visit(binaryExpression.Conversion);

            if (left != binaryExpression.Left || right != binaryExpression.Right || conversion != binaryExpression.Conversion)
            {
                if (binaryExpression.NodeType == ExpressionType.Coalesce && binaryExpression.Conversion != null)
                    return Expression.Coalesce(left, right, conversion as LambdaExpression);
                else
                    return Expression.MakeBinary(binaryExpression.NodeType, left, right, binaryExpression.IsLiftedToNull, binaryExpression.Method);
            }

            return binaryExpression;
        }

        protected virtual Expression VisitTypeIs(TypeBinaryExpression typeBinaryExpression)
        {
            Expression expr = Visit(typeBinaryExpression.Expression);

            if (expr != typeBinaryExpression.Expression)
                return Expression.TypeIs(expr, typeBinaryExpression.TypeOperand);

            return typeBinaryExpression;
        }

        protected virtual Expression VisitConstant(ConstantExpression constantExpression)
        {
            return constantExpression;
        }

        protected virtual Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            Expression test = Visit(conditionalExpression.Test);
            Expression ifTrue = Visit(conditionalExpression.IfTrue);
            Expression ifFalse = Visit(conditionalExpression.IfFalse);

            if (test != conditionalExpression.Test || ifTrue != conditionalExpression.IfTrue || ifFalse != conditionalExpression.IfFalse)
                return Expression.Condition(test, ifTrue, ifFalse);

            return conditionalExpression;
        }

        protected virtual Expression VisitParameter(ParameterExpression parameterExpression)
        {
            return parameterExpression;
        }

        protected virtual Expression VisitMember(MemberExpression memberExpression)
        {
            Expression exp = Visit(memberExpression.Expression);

            if (exp != memberExpression.Expression)
                return Expression.MakeMemberAccess(exp, memberExpression.Member);

            return memberExpression;
        }

        protected virtual Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            Expression obj = Visit(methodCallExpression.Object);
            IEnumerable<Expression> args = VisitExpressionList(methodCallExpression.Arguments);

            if (obj != methodCallExpression.Object || args != methodCallExpression.Arguments)
                return Expression.Call(obj, methodCallExpression.Method, args);

            return methodCallExpression;
        }

        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> expressions)
        {
            List<Expression> list = null;

            for (int i = 0, n = expressions.Count; i < n; i++)
            {
                Expression p = Visit(expressions[i]);
                if (list != null)
                {
                    list.Add(p);
                }
                else if (p != expressions[i])
                {
                    list = new List<Expression>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(expressions[j]);
                    }
                    list.Add(p);
                }
            }
            if (list != null)
            {
                return list.AsReadOnly();
            }

            return expressions;
        }

        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment memberAssignment)
        {
            Expression e = Visit(memberAssignment.Expression);

            if (e != memberAssignment.Expression)
                return Expression.Bind(memberAssignment.Member, e);

            return memberAssignment;
        }

        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding memberMemberBinding)
        {
            IEnumerable<MemberBinding> bindings = VisitBindingList(memberMemberBinding.Bindings);

            if (bindings != memberMemberBinding.Bindings)
                return Expression.MemberBind(memberMemberBinding.Member, bindings);

            return memberMemberBinding;
        }

        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding memberListBinding)
        {
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(memberListBinding.Initializers);
            
            if (initializers != memberListBinding.Initializers)
                return Expression.ListBind(memberListBinding.Member, initializers);

            return memberListBinding;
        }

        protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> memberBindings)
        {
            List<MemberBinding> list = null;

            for (int i = 0, n = memberBindings.Count; i < n; i++)
            {
                MemberBinding b = VisitBinding(memberBindings[i]);
                if (list != null)
                {
                    list.Add(b);
                }
                else if (b != memberBindings[i])
                {
                    list = new List<MemberBinding>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(memberBindings[j]);
                    }
                    list.Add(b);
                }
            }

            if (list != null)
                return list;

            return memberBindings;
        }

        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> elementInits)
        {
            List<ElementInit> list = null;

            for (int i = 0, n = elementInits.Count; i < n; i++)
            {
                ElementInit init = VisitElementInitializer(elementInits[i]);
                if (list != null)
                {
                    list.Add(init);
                }
                else if (init != elementInits[i])
                {
                    list = new List<ElementInit>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(elementInits[j]);
                    }
                    list.Add(init);
                }
            }

            if (list != null)
                return list;

            return elementInits;
        }

        protected virtual Expression VisitLambda(LambdaExpression lambdaExpression)
        {
            Expression body = Visit(lambdaExpression.Body);

            if (body != lambdaExpression.Body)
                return Expression.Lambda(lambdaExpression.Type, body, lambdaExpression.Parameters);

            return lambdaExpression;
        }

        protected virtual NewExpression VisitNew(NewExpression newExpression)
        {
            IEnumerable<Expression> args = VisitExpressionList(newExpression.Arguments);
            if (args != newExpression.Arguments)
            {
                if (newExpression.Members != null)
                    return Expression.New(newExpression.Constructor, args, newExpression.Members);
                else
                    return Expression.New(newExpression.Constructor, args);
            }

            return newExpression;
        }

        protected virtual Expression VisitMemberInit(MemberInitExpression memberInitExpression)
        {
            NewExpression n = VisitNew(memberInitExpression.NewExpression);
            IEnumerable<MemberBinding> bindings = VisitBindingList(memberInitExpression.Bindings);
            
            if (n != memberInitExpression.NewExpression || bindings != memberInitExpression.Bindings)
                return Expression.MemberInit(n, bindings);

            return memberInitExpression;
        }

        protected virtual Expression VisitListInit(ListInitExpression listInitExpression)
        {
            NewExpression n = VisitNew(listInitExpression.NewExpression);
            IEnumerable<ElementInit> initializers = VisitElementInitializerList(listInitExpression.Initializers);

            if (n != listInitExpression.NewExpression || initializers != listInitExpression.Initializers)
                return Expression.ListInit(n, initializers);

            return listInitExpression;
        }

        protected virtual Expression VisitNewArray(NewArrayExpression newArrayExpression)
        {
            IEnumerable<Expression> exprs = VisitExpressionList(newArrayExpression.Expressions);
           
            if (exprs != newArrayExpression.Expressions)
            {
                if (newArrayExpression.NodeType == ExpressionType.NewArrayInit)
                    return Expression.NewArrayInit(newArrayExpression.Type.GetElementType(), exprs);
                else
                  
                    return Expression.NewArrayBounds(newArrayExpression.Type.GetElementType(), exprs);
            }

            return newArrayExpression;
        }

        protected virtual Expression VisitInvocation(InvocationExpression invocationExpression)
        {
            IEnumerable<Expression> args = VisitExpressionList(invocationExpression.Arguments);
            Expression expr = Visit(invocationExpression.Expression);
            
            if (args != invocationExpression.Arguments || expr != invocationExpression.Expression)
                return Expression.Invoke(expr, args);
            
            return invocationExpression;
        }
    }
}