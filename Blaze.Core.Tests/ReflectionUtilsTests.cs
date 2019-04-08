using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Blaze.Core.Tests
{
    [TestClass]
    public class ReflectionUtilsTests
    {
        class ReflectionClassOne
        {
            //Non-public ctor
            public ReflectionClassOne(int i) { }
            public int NormalRwProp { get; set; }
            public int ReadOnlyProp { get; }

            public int PrivateWriteAutoProp
            {
                get;
                private set;
            }
            private int _PropBackingField = 0;
            public int PropBackingField
            {
                get { return _PropBackingField; }
            }
            public int MethodProp { get { return ReadOnlyProp; } }
        }

        [TestMethod]
        public void TestConstruct()
        {
            var instance = ReflectionUtils.CreateInstance<ReflectionClassOne>();
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void TestSetNormalProp()
        {
            var instance = new ReflectionClassOne(1);
            bool res = ReflectionUtils.UnsafeSetProperty(
                instance,
                nameof(ReflectionClassOne.NormalRwProp),
                1);
            Assert.AreEqual(1, instance.NormalRwProp);
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void TestSetReadOnlyProp()
        {
            var instance = new ReflectionClassOne(1);
            bool res = ReflectionUtils.UnsafeSetProperty(
                instance,
                nameof(ReflectionClassOne.ReadOnlyProp),
                1);
            Assert.AreEqual(1, instance.ReadOnlyProp);
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void TestSetPrivateWriteAutoProp()
        {
            var instance = new ReflectionClassOne(1);
            bool res = ReflectionUtils.UnsafeSetProperty(
                instance,
                nameof(ReflectionClassOne.PrivateWriteAutoProp),
                1);
            Assert.AreEqual(1, instance.PrivateWriteAutoProp);
            Assert.IsTrue(res);
        }

        [TestMethod]
        public void TestSetPropBackingField()
        {
            var instance = new ReflectionClassOne(1);
            bool res = ReflectionUtils.UnsafeSetProperty(
                instance,
                nameof(ReflectionClassOne.PropBackingField),
                1);
            Assert.AreEqual(0, instance.PropBackingField);
            Assert.IsFalse(res);
        }

        [TestMethod]
        public void TestSetMethodProp()
        {
            var instance = new ReflectionClassOne(1);
            bool res = ReflectionUtils.UnsafeSetProperty(
                instance,
                nameof(ReflectionClassOne.MethodProp),
                1);
            Assert.AreEqual(0, instance.MethodProp);
            Assert.IsFalse(res);
        }
    }
}
