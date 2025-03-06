using MulticastFunc;

namespace MulticastFuncTests
{
    [TestClass]
    public sealed class MulticastFuncTest
    {
        [TestMethod]
        public void OperatorPlus_AddsFunction()
        {
            static int func() => 42;

            // construct a multicast delegate with 2 functions
            Func<int>? combinedFunc = default;  
            combinedFunc += func;
            combinedFunc += func;

            MulticastFunc<int>? multicast = null;
            multicast += func;  // add a single method
            multicast += combinedFunc;  // add a multicast delegate

            // should contain 3 functions
            Assert.AreEqual(3, multicast.Count);
        }

        [TestMethod]
        public void OperatorMinus_RemovesFunction()
        {
            static int func() => 42;

            // construct a multicast delegate with 2 functions
            Func<int>? combinedFunc = default;
            combinedFunc += func;
            combinedFunc += func;

            MulticastFunc<int>? multicast = null;
            multicast += combinedFunc;  // add a multicast delegate
            multicast += func;  // add a single method
            multicast -= combinedFunc;  // remove the multicast delegate
            multicast -= func;  // remove the single method

            Assert.IsNotNull(multicast);    // MulticastFunc should not be null after removing all functions
            Assert.AreEqual(0, multicast.Count);    // should contain 0 functions
        }

        [TestMethod]
        public void ImplicitOperator_ConvertFuncDelegateToMulticastFunc()
        {
            Func<int> func = () => 42;
            // func is implicitly converted to MulticastFunc so it can be assigned to MulticastFunc
            MulticastFunc<int> multicast = func;

            Assert.AreEqual(1, multicast.Count);
        }

        [TestMethod]
        public void ExplicitOperator_ConvertMulticastFuncToFuncDelegate()
        {
            static int func() => 42;

            // construct a MulticastFunc with 1 function that return 42
            MulticastFunc<int>? multicast = null;
            multicast += func;

            // explicity convert and assign it to a Func delegate
            Func<int>? resultFunc = (Func<int>?)multicast;

            Assert.IsNotNull(resultFunc);   // func delegate is not null because multicast is not null
            Assert.AreEqual(42, resultFunc());  // invoking the func delegate should return 42, similar to the MulticastFunc's function
        }

        [TestMethod]
        public void Invoke_CallsAllFunctions()
        {
            Func<int> func1 = () => 42;
            Func<int> func2 = () => 43;
            Func<int> func3 = () => 44;
            Func<int> func4 = () => 45;

            // construct a multicast delegate with 4 functions
            MulticastFunc<int>? multicast = func1;
            multicast += func2;
            multicast += func3;
            multicast -= func2; // remove the 2nd function
            multicast += func4;

            var results = multicast.Invoke();

            // there should be 3 functions left, producing 3 results: 42, 44, 45
            Assert.AreEqual(3, results.Length);
            CollectionAssert.Contains(results, 42);
            CollectionAssert.DoesNotContain(results, 43);
            CollectionAssert.Contains(results, 44);
            CollectionAssert.Contains(results, 45);
        }
    }
}
