// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Common.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommunityToolkit.Common.UnitTests.Collections;

[TestClass]
public class ObservableGroupTests
{
    [TestCategory("Collections")]
    [TestMethod]
    public void Ctor_ShouldHaveExpectedState()
    {
        ObservableGroup<string, int>? group = new("key");

        Assert.AreEqual(group.Key, "key");
        Assert.AreEqual(group.Count, 0);
    }

    [TestCategory("Collections")]
    [TestMethod]
    public void Ctor_WithGrouping_ShouldHaveExpectedState()
    {
        IntGroup? source = new("key", new[] { 1, 2, 3 });
        ObservableGroup<string, int>? group = new(source);

        Assert.AreEqual(group.Key, "key");
        CollectionAssert.AreEqual(group, new[] { 1, 2, 3 });
    }

    [TestCategory("Collections")]
    [TestMethod]
    public void Ctor_WithCollection_ShouldHaveExpectedState()
    {
        int[]? source = new[] { 1, 2, 3 };
        ObservableGroup<string, int>? group = new("key", source);

        Assert.AreEqual(group.Key, "key");
        CollectionAssert.AreEqual(group, new[] { 1, 2, 3 });
    }

    [TestCategory("Collections")]
    [TestMethod]
    public void Add_ShouldRaiseEvent()
    {
        bool collectionChangedEventRaised = false;
        int[]? source = new[] { 1, 2, 3 };
        ObservableGroup<string, int>? group = new("key", source);
        ((INotifyCollectionChanged)group).CollectionChanged += (s, e) => collectionChangedEventRaised = true;

        group.Add(4);

        Assert.AreEqual(group.Key, "key");
        CollectionAssert.AreEqual(group, new[] { 1, 2, 3, 4 });
        Assert.IsTrue(collectionChangedEventRaised);
    }

    [TestCategory("Collections")]
    [TestMethod]
    public void Update_ShouldRaiseEvent()
    {
        bool collectionChangedEventRaised = false;
        int[]? source = new[] { 1, 2, 3 };
        ObservableGroup<string, int>? group = new("key", source);
        ((INotifyCollectionChanged)group).CollectionChanged += (s, e) => collectionChangedEventRaised = true;

        group[1] = 4;

        Assert.AreEqual(group.Key, "key");
        CollectionAssert.AreEqual(group, new[] { 1, 4, 3 });
        Assert.IsTrue(collectionChangedEventRaised);
    }

    [TestCategory("Collections")]
    [TestMethod]
    public void Remove_ShouldRaiseEvent()
    {
        bool collectionChangedEventRaised = false;
        int[]? source = new[] { 1, 2, 3 };
        ObservableGroup<string, int>? group = new("key", source);
        ((INotifyCollectionChanged)group).CollectionChanged += (s, e) => collectionChangedEventRaised = true;

        _ = group.Remove(1);

        Assert.AreEqual(group.Key, "key");
        CollectionAssert.AreEqual(group, new[] { 2, 3 });
        Assert.IsTrue(collectionChangedEventRaised);
    }

    [TestCategory("Collections")]
    [TestMethod]
    public void Clear_ShouldRaiseEvent()
    {
        bool collectionChangedEventRaised = false;
        int[]? source = new[] { 1, 2, 3 };
        ObservableGroup<string, int>? group = new("key", source);
        ((INotifyCollectionChanged)group).CollectionChanged += (s, e) => collectionChangedEventRaised = true;

        group.Clear();

        Assert.AreEqual(group.Key, "key");
        Assert.AreEqual(group.Count, 0);
        Assert.IsTrue(collectionChangedEventRaised);
    }

    [TestCategory("Collections")]
    [DataTestMethod]
    [DataRow(0)]
    [DataRow(3)]
    public void IReadOnlyObservableGroup_ShouldReturnExpectedValues(int count)
    {
        ObservableGroup<string, int>? group = new("key", Enumerable.Range(0, count));
        IReadOnlyObservableGroup? iReadOnlyObservableGroup = (IReadOnlyObservableGroup)group;

        Assert.AreEqual(iReadOnlyObservableGroup.Key, "key");
        Assert.AreEqual(iReadOnlyObservableGroup.Count, count);
    }
}
