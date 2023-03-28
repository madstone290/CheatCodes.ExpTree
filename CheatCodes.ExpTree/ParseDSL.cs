using Flee.PublicTypes;
using Sprache;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CheatCodes.ExpTree
{
    /// <summary>
    /// DSL(Domain specific language) 파싱하기
    /// </summary>
    public static class ParseDSL
    {
        public static void Run()
        {
            var a = SimpleCalculator.Run("2 + 2");        // 4
            var b = SimpleCalculator.Run("3.15 * 5 + 2"); // 17.75
            var c = SimpleCalculator.Run("1 / 2 * 3");    // 1.5
            var d = DecentCalculator.Run("a / (b * c)", new Dictionary<string, double>()
            {
                { "a", 100 },
                { "b", 4 },
                { "c", 5 },
            }); // 5
        }

        /// <summary>
        /// Sprache 라이브러리를 이용한 간단한 수식 파싱
        /// </summary>
        public static class SimpleCalculator
        {
            private static readonly Parser<Expression> Constant =
                Parse.DecimalInvariant
                    .Select(n => double.Parse(n, CultureInfo.InvariantCulture))
                    .Select(n => Expression.Constant(n, typeof(double)))
                    .Token();

            private static readonly Parser<ExpressionType> Operator =
                Parse.Char('+').Return(ExpressionType.Add)
                    .Or(Parse.Char('-').Return(ExpressionType.Subtract))
                    .Or(Parse.Char('*').Return(ExpressionType.Multiply))
                    .Or(Parse.Char('/').Return(ExpressionType.Divide));

            private static readonly Parser<Expression> Operation =
                Parse.ChainOperator(Operator, Constant, Expression.MakeBinary);

            private static readonly Parser<Expression> FullExpression =
                Operation.Or(Constant).End();

            public static double Run(string expression)
            {
                var operation = FullExpression.Parse(expression);
                var func = Expression.Lambda<Func<double>>(operation).Compile();

                return func();
            }
        }

        /// <summary>
        /// Flee 라이브러리를 이용한 계산기
        /// </summary>
        public static class DecentCalculator
        {
            public static double Run(string expression, Dictionary<string, double> values) 
            {
                var context = new ExpressionContext();
                context.Imports.AddType(typeof(Math));

                foreach (var value in values)
                    context.Variables[value.Key] = value.Value;

                
                var compiledExp = context.CompileGeneric<double>(expression);
                return compiledExp.Evaluate();
            }

            
        }
    }
}
