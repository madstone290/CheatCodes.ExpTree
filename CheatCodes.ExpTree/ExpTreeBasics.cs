using AgileObjects.ReadableExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CheatCodes.ExpTree
{
    /// <summary>
    /// Exp Tree를 만들어본다
    /// </summary>
    public class ExpTreeBasics
    {
        public static void Run()
        {
            var getGreetings = ConstructGreetingFunction();

            var greetingsForJohn = getGreetings("John"); // "Greetings, John"
            var greetingsForNobody = getGreetings(" ");  // <null>

            Console.WriteLine("greetingsForJohn: " + greetingsForJohn);
            Console.WriteLine("greetingsForNobody: " + greetingsForNobody);

            var exp = CreateStatementBlock("C#");
            var lamda = Expression.Lambda<Action>(exp).Compile();
            lamda();

            var s1 = Expression.Constant(42).ToString(); // 42

            var s2 = Expression.Multiply(
                Expression.Constant(5),
                Expression.Constant(11)
            ).ToString(); // (5 * 11)

            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(exp.ToReadableString());
        }

        static Func<string, string> ConstructGreetingFunction()
        {
            // condition
            var personNameParameter = Expression.Parameter(typeof(string), "personName");
            var isNullOrWhiteSpaceMethod = typeof(string).GetMethod(nameof(string.IsNullOrWhiteSpace));
            var test = Expression.Not(
                Expression.Call(isNullOrWhiteSpaceMethod, personNameParameter));

            // true clause
            var concatMethod = typeof(string)
           .GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });

            var ifTrue = Expression.Call(
                concatMethod,
                Expression.Constant("Greetings, "),
                personNameParameter);

            // flase clause
            var ifFalse = Expression.Constant(null, typeof(string));

            // ternary condition
            var conditional = Expression.Condition(test, ifTrue, ifFalse);

            var lambda = Expression.Lambda<Func<string, string>>(conditional, personNameParameter);

            return lambda.Compile();
        }

        static Expression CreateStatementBlock(string word)
        {
            var consoleWriteMethod = typeof(Console)
                .GetMethod(nameof(Console.WriteLine), new[] { typeof(string) });

            var variableA = Expression.Variable(typeof(string), "a");
            var variableB = Expression.Variable(typeof(string), "b");

            return Expression.Block(
                // Declare variables in scope
                new[] { variableA, variableB },

                // Assign values to variables
                Expression.Assign(variableA, Expression.Constant("Foo ")),
                Expression.Assign(variableB, Expression.Constant("bar")),

                // Call methods
                Expression.Call(consoleWriteMethod, variableA),
                Expression.Call(consoleWriteMethod, variableB)
            );
        }
    }
}
