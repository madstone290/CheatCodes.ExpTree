using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CheatCodes.ExpTree
{
    /// <summary>
    /// Exp tree gives us better performance than reflection
    /// </summary>
    public class OptimizeReflectionCalls
    {
        public static void Run()
        {
            int r = Reflection.CallExecute(new Command());
            Console.WriteLine(r); // 45

            r = ReflectionCached.CallExecute(new Command());
            Console.WriteLine(r); // 45

            r = ReflectionDelegate.CallExecute(new Command());
            Console.WriteLine(r); // 45

            r = ExpressionTrees.CallExecute(new Command());
            Console.WriteLine(r); // 45
        }

        public class Command
        {
            private int Execute() => 45;
        }

        public static class Reflection
        {
            public static int CallExecute(Command command)
            {
                return (int)typeof(Command)
                    .GetMethod("Execute", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(command, null);
            }
        }

        /// <summary>
        /// MethodInfo 캐시
        /// </summary>
        public static class ReflectionCached
        {
            private static MethodInfo ExecuteMethod { get; } = typeof(Command)
                .GetMethod("Execute", BindingFlags.NonPublic | BindingFlags.Instance);

            public static int CallExecute(Command command)
            {
                return (int)ExecuteMethod.Invoke(command, null);
            }
        }

        /// <summary>
        /// MethodInfo로 delegate 생성
        /// </summary>
        public static class ReflectionDelegate
        {
            private static MethodInfo ExecuteMethod { get; } = typeof(Command)
                .GetMethod("Execute", BindingFlags.NonPublic | BindingFlags.Instance);

            private static Func<Command, int> Impl { get; } =
                (Func<Command, int>)Delegate.CreateDelegate(typeof(Func<Command, int>), ExecuteMethod);

            public static int CallExecute(Command command)
            {
                return Impl(command);
            }
        }

        /// <summary>
        /// expression tree로 delegate 생성
        /// </summary>
        public static class ExpressionTrees
        {
            private static MethodInfo ExecuteMethod { get; } = typeof(Command)
                .GetMethod("Execute", BindingFlags.NonPublic | BindingFlags.Instance);

            private static Func<Command, int> Impl { get; }

            static ExpressionTrees()
            {
                var instance = Expression.Parameter(typeof(Command));
                var call = Expression.Call(instance, ExecuteMethod);
                Impl = Expression.Lambda<Func<Command, int>>(call, instance).Compile();
            }

            public static int CallExecute(Command command)
            {
                return Impl(command);
            }
        }

        public class Benchmarks
        {
            [Benchmark(Description = "Reflection", Baseline = true)]
            public int Reflection() => (int)typeof(Command)
                .GetMethod("Execute", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(new Command(), null);

            [Benchmark(Description = "Reflection (cached)")]
            public int Cached() => ReflectionCached.CallExecute(new Command());

            [Benchmark(Description = "Reflection (delegate)")]
            public int Delegate() => ReflectionDelegate.CallExecute(new Command());

            [Benchmark(Description = "Expressions")]
            public int Expressions() => ExpressionTrees.CallExecute(new Command());

            public static void Run() => BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
