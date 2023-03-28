using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CheatCodes.ExpTree
{
    /// <summary>
    /// ExpressionTree를 이용하여 제네릭 연산을 구현할 수 있다.
    /// </summary>
    public class ImplementGenericOperators
    {
        public static int ThreeFourths(int x) => 3 * x / 4;

        public static dynamic ThreeFourths(dynamic d) => 3 * d / 4;

        public static class ExpTree
        {
            private static class Impl<T>
            {
                public static Func<T, T> Of { get; }

                static Impl()
                {
                    var param = Expression.Parameter(typeof(T));

                    var three = Expression.Convert(Expression.Constant(3), typeof(T));
                    var four = Expression.Convert(Expression.Constant(4), typeof(T));

                    var operation = Expression.Divide(Expression.Multiply(param, three), four);

                    var lambda = Expression.Lambda<Func<T, T>>(operation, param);

                    Of = lambda.Compile();
                }
            }

            public static T Of<T>(T x) => Impl<T>.Of(x);
        }

        public class Benchmarks
        {
            [Benchmark(Description = "Static", Baseline = true)]
            [Arguments(13.37)]
            public double Static(double x) => 3 * x / 4;

            [Benchmark(Description = "Expressions")]
            [Arguments(13.37)]
            public double Expressions(double x) => ExpTree.Of(x);

            [Benchmark(Description = "Dynamic")]
            [Arguments(13.37)]
            public dynamic Dynamic(dynamic x) => 3 * x / 4;

            public static void Run() => BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
