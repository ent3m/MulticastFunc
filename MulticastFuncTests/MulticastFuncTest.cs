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

            // construct a MulticastFunc
            MulticastFunc<int>? multicast = null;
            multicast += func;  // add a single method
            multicast += () => 43;  // add a lambda expression
            multicast += combinedFunc;  // add a multicast delegate

            // should contain 4 functions
            Assert.AreEqual(4, multicast.Count);
        }

        [TestMethod]
        public void OperatorMinus_RemovesFunction()
        {
            static int func() => 42;

            // construct a multicast delegate with 2 functions
            Func<int>? combinedFunc = default;
            combinedFunc += func;
            combinedFunc += func;

            // construct a MulticastFunc
            MulticastFunc<int>? multicast = null;
            multicast += combinedFunc;  // add a multicast delegate
            multicast += func;  // add a single method
            multicast -= combinedFunc;  // remove the multicast delegate

            // MulticastFunc should not be null because it has 1 function remaining
            Assert.IsNotNull(multicast);
            Assert.AreEqual(1, multicast.Count);
        }

        [TestMethod]
        public void OperatorMinus_RemovesAll()
        {
            static int func() => 42;

            // construct a MulticastFunc
            MulticastFunc<int>? multicast = null;
            multicast += func;  // add a single method
            multicast -= func;  // remove the single method

            // MulticastFunc is null because all functions are removed
            Assert.IsNull(multicast);
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
            // construct a MulticastFunc with 1 function that return 42
            MulticastFunc<int>? multicast = (Func<int>)(() => 42);

            // explicity convert and assign it to a Func delegate
            Func<int>? resultFunc = (Func<int>?)multicast;

            Assert.IsNotNull(resultFunc);   // func delegate is not null because multicast is not null
            Assert.AreEqual(42, resultFunc());  // invoking the func delegate should return 42, similar to the MulticastFunc's function
        }

        [TestMethod]
        public void Invoke_CallsAllFunctions()
        {
            static int func1() => 41;
            static int func2() => 42;
            static int func3() => 43;
            static int func4() => 44;
            static int func5() => 45;

            // create a delegate with 4 functions
            Func<int>? func = null;
            func += func1;
            func += func2;
            func += func3;
            func += func4;

            // create a MulticastFunc
            MulticastFunc<int>? multicast = func;   // output: 41, 42, 43, 44
            multicast += func5; // output: 41, 42, 43, 44, 45
            multicast -= func;  // output: 45

            Assert.AreEqual(45, multicast!.Invoke()[0]);

            multicast += func;  // output: 45, 41, 42, 43, 44
            multicast -= func1; // output: 45, 42, 43, 44

            var results = multicast!.Invoke();

            // there should be 4 functions left, producing output: 45, 42, 43, 44
            CollectionAssert.AreEquivalent(expected, results);
        }
        private static readonly int[] expected = [45, 42, 43, 44];

        [TestMethod]
        public void Invoke_CallsAllFunctions1Arg()
        {
            static int func1(int arg) => 41;
            static int func2(int arg) => 42;
            static int func3(int arg) => 43;
            static int func4(int arg) => 44;
            static int func5(int arg) => 45;

            // create a delegate with 4 functions
            Func<int, int>? func = null;
            func += func1;
            func += func2;
            func += func3;
            func += func4;

            // create a MulticastFunc
            MulticastFunc<int, int>? multicast = func;   // output: 41, 42, 43, 44
            multicast += func5; // output: 41, 42, 43, 44, 45
            multicast -= func;  // output: 45

            Assert.AreEqual(45, multicast!.Invoke(default)[0]);

            multicast += func;  // output: 45, 41, 42, 43, 44
            multicast -= func1; // output: 45, 42, 43, 44

            var results = multicast!.Invoke(default);

            // there should be 4 functions left, producing output: 45, 42, 43, 44
            CollectionAssert.AreEquivalent(expected, results);
        }

        [TestMethod]
        public void Invoke_CallsAllFunctions2Arg()
        {
            static int func1(int arg1, int arg2) => 41;
            static int func2(int arg1, int arg2) => 42;
            static int func3(int arg1, int arg2) => 43;
            static int func4(int arg1, int arg2) => 44;
            static int func5(int arg1, int arg2) => 45;

            // create a delegate with 4 functions
            Func<int, int, int>? func = null;
            func += func1;
            func += func2;
            func += func3;
            func += func4;

            // create a MulticastFunc
            MulticastFunc<int, int, int>? multicast = func;   // output: 41, 42, 43, 44
            multicast += func5; // output: 41, 42, 43, 44, 45
            multicast -= func;  // output: 45

            Assert.AreEqual(45, multicast!.Invoke(default, default)[0]);

            multicast += func;  // output: 45, 41, 42, 43, 44
            multicast -= func1; // output: 45, 42, 43, 44

            var results = multicast!.Invoke(default, default);

            // there should be 4 functions left, producing output: 45, 42, 43, 44
            CollectionAssert.AreEquivalent(expected, results);
        }


        [TestMethod]
        public void Invoke_CallsAllFunctions3Arg()
        {
            static int func1(int arg1, int arg2, int arg3) => 41;
            static int func2(int arg1, int arg2, int arg3) => 42;
            static int func3(int arg1, int arg2, int arg3) => 43;
            static int func4(int arg1, int arg2, int arg3) => 44;
            static int func5(int arg1, int arg2, int arg3) => 45;

            // create a delegate with 4 functions
            Func<int, int, int, int>? func = null;
            func += func1;
            func += func2;
            func += func3;
            func += func4;

            // create a MulticastFunc
            MulticastFunc<int, int, int, int>? multicast = func;   // output: 41, 42, 43, 44
            multicast += func5; // output: 41, 42, 43, 44, 45
            multicast -= func;  // output: 45

            Assert.AreEqual(45, multicast!.Invoke(default, default, default)[0]);

            multicast += func;  // output: 45, 41, 42, 43, 44
            multicast -= func1; // output: 45, 42, 43, 44

            var results = multicast!.Invoke(default, default, default);

            // there should be 4 functions left, producing output: 45, 42, 43, 44
            CollectionAssert.AreEquivalent(expected, results);
        }


        [TestMethod]
        public void Invoke_CallsAllFunctions4Arg()
        {
            static int func1(int arg1, int arg2, int arg3, int arg4) => 41;
            static int func2(int arg1, int arg2, int arg3, int arg4) => 42;
            static int func3(int arg1, int arg2, int arg3, int arg4) => 43;
            static int func4(int arg1, int arg2, int arg3, int arg4) => 44;
            static int func5(int arg1, int arg2, int arg3, int arg4) => 45;

            // create a delegate with 4 functions
            Func<int, int, int, int, int>? func = null;
            func += func1;
            func += func2;
            func += func3;
            func += func4;

            // create a MulticastFunc
            MulticastFunc<int, int, int, int, int>? multicast = func;   // output: 41, 42, 43, 44
            multicast += func5; // output: 41, 42, 43, 44, 45
            multicast -= func;  // output: 45

            Assert.AreEqual(45, multicast!.Invoke(default, default, default, default)[0]);

            multicast += func;  // output: 45, 41, 42, 43, 44
            multicast -= func1; // output: 45, 42, 43, 44

            var results = multicast!.Invoke(default, default, default, default);

            // there should be 4 functions left, producing output: 45, 42, 43, 44
            CollectionAssert.AreEquivalent(expected, results);
        }


        [TestMethod]
        public void Invoke_CallsAllFunctions5Arg()
        {
            static int func1(int arg1, int arg2, int arg3, int arg4, int arg5) => 41;
            static int func2(int arg1, int arg2, int arg3, int arg4, int arg5) => 42;
            static int func3(int arg1, int arg2, int arg3, int arg4, int arg5) => 43;
            static int func4(int arg1, int arg2, int arg3, int arg4, int arg5) => 44;
            static int func5(int arg1, int arg2, int arg3, int arg4, int arg5) => 45;

            // create a delegate with 4 functions
            Func<int, int, int, int, int, int>? func = null;
            func += func1;
            func += func2;
            func += func3;
            func += func4;

            // create a MulticastFunc
            MulticastFunc<int, int, int, int, int, int>? multicast = func;   // output: 41, 42, 43, 44
            multicast += func5; // output: 41, 42, 43, 44, 45
            multicast -= func;  // output: 45

            Assert.AreEqual(45, multicast!.Invoke(default, default, default, default, default)[0]);

            multicast += func;  // output: 45, 41, 42, 43, 44
            multicast -= func1; // output: 45, 42, 43, 44

            var results = multicast!.Invoke(default, default, default, default, default);

            // there should be 4 functions left, producing output: 45, 42, 43, 44
            CollectionAssert.AreEquivalent(expected, results);
        }

        [TestMethod]
        public void Invoke_CallsAllFunctions6Arg()
        {
            static int func1(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6) => 41;
            static int func2(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6) => 42;
            static int func3(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6) => 43;
            static int func4(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6) => 44;
            static int func5(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6) => 45;

            // create a delegate with 4 functions
            Func<int, int, int, int, int, int, int>? func = null;
            func += func1;
            func += func2;
            func += func3;
            func += func4;

            // create a MulticastFunc
            MulticastFunc<int, int, int, int, int, int, int>? multicast = func;   // output: 41, 42, 43, 44
            multicast += func5; // output: 41, 42, 43, 44, 45
            multicast -= func;  // output: 45

            Assert.AreEqual(45, multicast!.Invoke(default, default, default, default, default, default)[0]);

            multicast += func;  // output: 45, 41, 42, 43, 44
            multicast -= func1; // output: 45, 42, 43, 44

            var results = multicast!.Invoke(default, default, default, default, default, default);

            // there should be 4 functions left, producing output: 45, 42, 43, 44
            CollectionAssert.AreEquivalent(expected, results);
        }

        [TestMethod]
        public void Invoke_CallsAllFunctions7Arg()
        {
            static int func1(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7) => 41;
            static int func2(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7) => 42;
            static int func3(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7) => 43;
            static int func4(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7) => 44;
            static int func5(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7) => 45;

            // create a delegate with 4 functions
            Func<int, int, int, int, int, int, int, int>? func = null;
            func += func1;
            func += func2;
            func += func3;
            func += func4;

            // create a MulticastFunc
            MulticastFunc<int, int, int, int, int, int, int, int>? multicast = func;   // output: 41, 42, 43, 44
            multicast += func5; // output: 41, 42, 43, 44, 45
            multicast -= func;  // output: 45

            Assert.AreEqual(45, multicast!.Invoke(default, default, default, default, default, default, default)[0]);

            multicast += func;  // output: 45, 41, 42, 43, 44
            multicast -= func1; // output: 45, 42, 43, 44

            var results = multicast!.Invoke(default, default, default, default, default, default, default);

            // there should be 4 functions left, producing output: 45, 42, 43, 44
            CollectionAssert.AreEquivalent(expected, results);
        }

        [TestMethod]
        public void Invoke_CallsAllFunctions8Arg()
        {
            static int func1(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8) => 41;
            static int func2(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8) => 42;
            static int func3(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8) => 43;
            static int func4(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8) => 44;
            static int func5(int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7, int arg8) => 45;

            // create a delegate with 4 functions
            Func<int, int, int, int, int, int, int, int, int>? func = null;
            func += func1;
            func += func2;
            func += func3;
            func += func4;

            // create a MulticastFunc
            MulticastFunc<int, int, int, int, int, int, int, int, int>? multicast = func;   // output: 41, 42, 43, 44
            multicast += func5; // output: 41, 42, 43, 44, 45
            multicast -= func;  // output: 45

            Assert.AreEqual(45, multicast!.Invoke(default, default, default, default, default, default, default, default)[0]);

            multicast += func;  // output: 45, 41, 42, 43, 44
            multicast -= func1; // output: 45, 42, 43, 44

            var results = multicast!.Invoke(default, default, default, default, default, default, default, default);

            // there should be 4 functions left, producing output: 45, 42, 43, 44
            CollectionAssert.AreEquivalent(expected, results);
        }

        [TestMethod]
        public void Invoke_SpanBuffer()
        {
            static int func() => 42;

            // create a MulticastFunc with 3 functions that return 42
            MulticastFunc<int>? multicast = null;
            multicast += func;
            multicast += func;
            multicast += func;

            // create a buffer large enough to store the results
            Span<int> buffer = stackalloc int[multicast.Count];
            var results = multicast.Invoke(buffer);

            // the buffer should contain 3 results: 42, 42, 42
            Assert.AreEqual(3, results.Length);
            Assert.AreEqual(42, buffer[0]);
            Assert.AreEqual(42, buffer[1]);
            Assert.AreEqual(42, buffer[2]);
        }

        [TestMethod]
        public void Invoke_Buffer()
        {
            static int func() => 42;

            // create a MulticastFunc with 3 functions that return 42
            MulticastFunc<int>? multicast = null;
            multicast += func;
            multicast += func;
            multicast += func;

            // create a buffer large enough to store the results
            int[] buffer = new int[multicast.Count];
            var written = multicast.Invoke(buffer);

            // the buffer should contain 3 results: 42, 42, 42
            Assert.AreEqual(3, written);
            Assert.AreEqual(42, buffer[0]);
            Assert.AreEqual(42, buffer[1]);
            Assert.AreEqual(42, buffer[2]);
        }

        [TestMethod]
        public void Equals_Override()
        {
            static int func1() => 42;
            static int func2() => 16;

            Func<int>? func = null;
            func += func1;

            MulticastFunc<int>? multicast1 = null;
            multicast1 += func1;

            MulticastFunc<int>? multicast2 = null;
            multicast2 += func1;

            MulticastFunc<int>? multicast3 = null;
            multicast3 += func1;
            multicast3 += func1;

            MulticastFunc<int>? multicast4 = null;
            multicast4 += func1;
            multicast4 += func1;

            MulticastFunc<int>? multicast5 = null;
            multicast5 += func1;
            multicast5 += func2;

            // true because it should equal itself
            Assert.IsTrue(multicast1.Equals(multicast1));
            // false because of type difference
            Assert.IsFalse(multicast1.Equals(func));
            // true because content are the same
            Assert.IsTrue(multicast1.Equals(multicast2));
            // false because of different length and content
            Assert.IsFalse(multicast2.Equals(multicast3));
            // true because content are the same
            Assert.IsTrue(multicast3.Equals(multicast4));
            // false because of different content
            Assert.IsFalse(multicast4.Equals(multicast5));
        }
    }
}
