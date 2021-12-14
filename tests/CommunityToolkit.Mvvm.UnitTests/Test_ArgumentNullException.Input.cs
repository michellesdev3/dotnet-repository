// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommunityToolkit.Mvvm.UnitTests;

public partial class Test_ArgumentNullException
{
    [TestMethod]
    public void Test_RelayCommand()
    {
        Assert(() => new RelayCommand(execute: null!), "execute");
        Assert(() => new RelayCommand(execute: null!, () => true), "execute");
        Assert(() => new RelayCommand(() => { }, canExecute: null!), "canExecute");
    }

    [TestMethod]
    public void Test_RelayCommandOfT()
    {
        Assert(() => new RelayCommand<string>(execute: null!), "execute");
        Assert(() => new RelayCommand<string>(execute: null!, s => true), "execute");
        Assert(() => new RelayCommand<string>(s => { }, canExecute: null!), "canExecute");
    }

    [TestMethod]
    public void Test_AsyncRelayCommand()
    {
        Assert(() => new AsyncRelayCommand(execute: null!), "execute");
        Assert(() => new AsyncRelayCommand(execute: null!, true), "execute");
        Assert(() => new AsyncRelayCommand(cancelableExecute: null!), "cancelableExecute");
        Assert(() => new AsyncRelayCommand(cancelableExecute: null!, true), "cancelableExecute");
        Assert(() => new AsyncRelayCommand(execute: null!, () => true), "execute");
        Assert(() => new AsyncRelayCommand(() => Task.CompletedTask, canExecute: null!), "canExecute");
        Assert(() => new AsyncRelayCommand(execute: null!, () => true, true), "execute");
        Assert(() => new AsyncRelayCommand(() => Task.CompletedTask, canExecute: null!, true), "canExecute");
        Assert(() => new AsyncRelayCommand(cancelableExecute: null!, () => true), "cancelableExecute");
        Assert(() => new AsyncRelayCommand(t => Task.CompletedTask, canExecute: null!), "canExecute");
        Assert(() => new AsyncRelayCommand(cancelableExecute: null!, () => true, true), "cancelableExecute");
        Assert(() => new AsyncRelayCommand(t => Task.CompletedTask, canExecute: null!, true), "canExecute");
    }

    [TestMethod]
    public void Test_AsyncRelayCommandOfT()
    {
        Assert(() => new AsyncRelayCommand<string>(execute: null!), "execute");
        Assert(() => new AsyncRelayCommand<string>(execute: null!, true), "execute");
        Assert(() => new AsyncRelayCommand<string>(cancelableExecute: null!), "cancelableExecute");
        Assert(() => new AsyncRelayCommand<string>(cancelableExecute: null!, true), "cancelableExecute");
        Assert(() => new AsyncRelayCommand<string>(execute: null!, s => true), "execute");
        Assert(() => new AsyncRelayCommand<string>(s => Task.CompletedTask, canExecute: null!), "canExecute");
        Assert(() => new AsyncRelayCommand<string>(execute: null!, s => true, true), "execute");
        Assert(() => new AsyncRelayCommand<string>(s => Task.CompletedTask, canExecute: null!, true), "canExecute");
        Assert(() => new AsyncRelayCommand<string>(cancelableExecute: null!, s => true), "cancelableExecute");
        Assert(() => new AsyncRelayCommand<string>(t => Task.CompletedTask, canExecute: null!), "canExecute");
        Assert(() => new AsyncRelayCommand<string>(cancelableExecute: null!, s => true, true), "cancelableExecute");
        Assert(() => new AsyncRelayCommand<string>(t => Task.CompletedTask, canExecute: null!, true), "canExecute");
    }
}
