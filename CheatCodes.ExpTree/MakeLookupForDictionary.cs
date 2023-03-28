using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static CheatCodes.ExpTree.MakeLookupForDictionary;

namespace CheatCodes.ExpTree
{
    /// <summary>
    /// 룩업 테이블을 미리 만들어서 일반적인 Dictionary보다 빠르게 작동한다.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class MakeLookupForDictionary
    {
        /// <summary>
        /// 룩업테이블  지원하는 사전
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        public class CompiledDictionary<TKey, TValue>
        {
            private readonly IDictionary<TKey, TValue> _inner = new Dictionary<TKey, TValue>();

            private Func<TKey, TValue> _lookup;

            public CompiledDictionary() => UpdateLookup();

            public void UpdateLookup()
            {
                // Parameter for lookup key
                var keyParameter = Expression.Parameter(typeof(TKey));

                // Expression that gets the key's hash code
                var keyGetHashCodeCall = Expression.Call(
                    keyParameter,
                    typeof(object).GetMethod(nameof(GetHashCode))
                );

                // Expression that converts the key to string
                var keyToStringCall = Expression.Call(
                    keyParameter,
                    typeof(object).GetMethod(nameof(ToString))
                );

                // Expression that throws 'not found' exception in case of failure
                var exceptionCtor = typeof(KeyNotFoundException)
                    .GetConstructor(new[] { typeof(string) });

                var throwException = Expression.Throw(
                    Expression.New(exceptionCtor, keyToStringCall),
                    typeof(TValue)
                );

                // Switch expression with cases for every hash code
                var body = Expression.Switch(
                    typeof(TValue), // expression type
                    keyGetHashCodeCall, // switch condition
                    throwException, // default case
                    null, // use default comparer
                    _inner // switch cases
                        .GroupBy(p => p.Key.GetHashCode())
                        .Select(g =>
                        {
                            // No collision, construct constant expression
                            if (g.Count() == 1)
                            {
                                return Expression.SwitchCase(
                                    Expression.Constant(g.Single().Value), // body
                                    Expression.Constant(g.Key) // test value
                                );
                            }

                            // Collision, construct inner switch for the key's value
                            return Expression.SwitchCase(
                                Expression.Switch(
                                    typeof(TValue),
                                    keyParameter, // switch on the actual key
                                    throwException,
                                    null,
                                    g.Select(p => Expression.SwitchCase(
                                        Expression.Constant(p.Value),
                                        Expression.Constant(p.Key)
                                    ))
                                ),
                                Expression.Constant(g.Key)
                            );
                        })
                );

                var lambda = Expression.Lambda<Func<TKey, TValue>>(body, keyParameter);

                _lookup = lambda.Compile();
            }

            public TValue this[TKey key]
            {
                get => _lookup(key);
                set => _inner[key] = value;
            }

            // The rest of the interface implementation is omitted for brevity
        }

    }

    public class Benchmarks
    {
        private readonly Dictionary<string, int> _normalDictionary =
            new Dictionary<string, int>();

        private readonly CompiledDictionary<string, int> _compiledDictionary =
            new CompiledDictionary<string, int>();

        [Params(1000, 10000, 100000)]
        public int Count { get; set; }

        public string TargetKey { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            // Seed the dictionaries with values
            foreach (var i in Enumerable.Range(0, Count))
            {
                var key = $"key_{i}";

                _normalDictionary[key] = i;
                _compiledDictionary[key] = i;
            }

            // Recompile lookup
            _compiledDictionary.UpdateLookup();

            // Try to get the middle element
            TargetKey = $"key_{Count / 2}";
        }

        [Benchmark(Description = "Standard dictionary", Baseline = true)]
        public int Normal()
        {
            return _normalDictionary[TargetKey];
        }

        [Benchmark(Description = "Compiled dictionary")]
        public int Compiled()
        {

            return _compiledDictionary[TargetKey];
        }

        public static void Run() => BenchmarkRunner.Run<Benchmarks>();
    }
}
