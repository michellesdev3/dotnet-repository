// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Internals;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommunityToolkit.Mvvm.UnitTests;

[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601", Justification = "Type only used for testing")]
[TestClass]
public partial class Test_Messenger
{
    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_UnregisterRecipientWithMessageType(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        object? recipient = new();

        messenger.Unregister<MessageA>(recipient);
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_UnregisterRecipientWithMessageTypeAndToken(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        object? recipient = new();

        messenger.Unregister<MessageA, string>(recipient, nameof(MessageA));
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_UnregisterRecipientWithToken(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        object? recipient = new();

        messenger.UnregisterAll(recipient, nameof(MessageA));
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_UnregisterRecipientWithRecipient(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        object? recipient = new();

        messenger.UnregisterAll(recipient);
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_RegisterAndUnregisterRecipientWithMessageType(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        object? recipient = new();

        messenger.Register<MessageA>(recipient, (r, m) => { });

        messenger.Unregister<MessageA>(recipient);

        Assert.IsFalse(messenger.IsRegistered<MessageA>(recipient));
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_RegisterAndUnregisterRecipientWithMessageTypeAndToken(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        object? recipient = new();

        messenger.Register<MessageA, string>(recipient, nameof(MessageA), (r, m) => { });

        messenger.Unregister<MessageA, string>(recipient, nameof(MessageA));

        Assert.IsFalse(messenger.IsRegistered<MessageA, string>(recipient, nameof(MessageA)));
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_RegisterAndUnregisterRecipientWithToken(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        object? recipient = new();

        messenger.Register<MessageA, string>(recipient, nameof(MessageA), (r, m) => { });

        messenger.UnregisterAll(recipient, nameof(MessageA));

        Assert.IsFalse(messenger.IsRegistered<MessageA, string>(recipient, nameof(MessageA)));
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_RegisterAndUnregisterRecipientWithRecipient(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        object? recipient = new();

        messenger.Register<MessageA, string>(recipient, nameof(MessageA), (r, m) => { });

        messenger.UnregisterAll(recipient);

        Assert.IsFalse(messenger.IsRegistered<MessageA, string>(recipient, nameof(MessageA)));
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_IsRegistered_Register_Send_UnregisterOfTMessage_WithNoToken(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        object a = new();

        Assert.IsFalse(messenger.IsRegistered<MessageA>(a));

        object? recipient = null;
        string? result = null;

        messenger.Register<MessageA>(a, (r, m) =>
        {
            recipient = r;
            result = m.Text;
        });

        Assert.IsTrue(messenger.IsRegistered<MessageA>(a));

        _ = messenger.Send(new MessageA { Text = nameof(MessageA) });

        Assert.AreSame(recipient, a);
        Assert.AreEqual(result, nameof(MessageA));

        messenger.Unregister<MessageA>(a);

        Assert.IsFalse(messenger.IsRegistered<MessageA>(a));

        recipient = null;
        result = null;

        _ = messenger.Send(new MessageA { Text = nameof(MessageA) });

        Assert.IsNull(recipient);
        Assert.IsNull(result);
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_IsRegistered_Register_Send_UnregisterRecipient_WithNoToken(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        object a = new();

        Assert.IsFalse(messenger.IsRegistered<MessageA>(a));

        string? result = null;
        messenger.Register<MessageA>(a, (r, m) => result = m.Text);

        Assert.IsTrue(messenger.IsRegistered<MessageA>(a));

        _ = messenger.Send(new MessageA { Text = nameof(MessageA) });

        Assert.AreEqual(result, nameof(MessageA));

        messenger.UnregisterAll(a);

        Assert.IsFalse(messenger.IsRegistered<MessageA>(a));

        result = null;
        _ = messenger.Send(new MessageA { Text = nameof(MessageA) });

        Assert.IsNull(result);
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_IsRegistered_Register_Send_UnregisterOfTMessage_WithToken(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        object a = new();

        Assert.IsFalse(messenger.IsRegistered<MessageA>(a));

        string? result = null;
        messenger.Register<MessageA, string>(a, nameof(MessageA), (r, m) => result = m.Text);

        Assert.IsTrue(messenger.IsRegistered<MessageA, string>(a, nameof(MessageA)));

        _ = messenger.Send(new MessageA { Text = nameof(MessageA) }, nameof(MessageA));

        Assert.AreEqual(result, nameof(MessageA));

        messenger.Unregister<MessageA, string>(a, nameof(MessageA));

        Assert.IsFalse(messenger.IsRegistered<MessageA, string>(a, nameof(MessageA)));

        result = null;
        _ = messenger.Send(new MessageA { Text = nameof(MessageA) }, nameof(MessageA));

        Assert.IsNull(result);
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_DuplicateRegistrationWithMessageType(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        object? recipient = new();

        messenger.Register<MessageA>(recipient, (r, m) => { });

        _ = Assert.ThrowsException<InvalidOperationException>(() =>
        {
            messenger.Register<MessageA>(recipient, (r, m) => { });
        });
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_DuplicateRegistrationWithMessageTypeAndToken(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        object? recipient = new();

        messenger.Register<MessageA, string>(recipient, nameof(MessageA), (r, m) => { });

        _ = Assert.ThrowsException<InvalidOperationException>(() =>
        {
            messenger.Register<MessageA, string>(recipient, nameof(MessageA), (r, m) => { });
        });
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_IRecipient_NoMessages(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        RecipientWithNoMessages? recipient = new();

        messenger.RegisterAll(recipient);

        // We just need to verify we got here with no errors, this
        // recipient has no declared handlers so there's nothing to do
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_IRecipient_SomeMessages_NoToken(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        RecipientWithSomeMessages? recipient = new();

        messenger.RegisterAll(recipient);

        Assert.IsTrue(messenger.IsRegistered<MessageA>(recipient));
        Assert.IsTrue(messenger.IsRegistered<MessageB>(recipient));

        Assert.AreEqual(recipient.As, 0);
        Assert.AreEqual(recipient.Bs, 0);

        _ = messenger.Send<MessageA>();

        Assert.AreEqual(recipient.As, 1);
        Assert.AreEqual(recipient.Bs, 0);

        _ = messenger.Send<MessageB>();

        Assert.AreEqual(recipient.As, 1);
        Assert.AreEqual(recipient.Bs, 1);

        messenger.UnregisterAll(recipient);

        Assert.IsFalse(messenger.IsRegistered<MessageA>(recipient));
        Assert.IsFalse(messenger.IsRegistered<MessageB>(recipient));
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_IRecipient_SomeMessages_WithToken(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        RecipientWithSomeMessages? recipient = new();
        string? token = nameof(Test_Messenger_IRecipient_SomeMessages_WithToken);

        messenger.RegisterAll(recipient, token);

        Assert.IsTrue(messenger.IsRegistered<MessageA, string>(recipient, token));
        Assert.IsTrue(messenger.IsRegistered<MessageB, string>(recipient, token));

        Assert.IsFalse(messenger.IsRegistered<MessageA>(recipient));
        Assert.IsFalse(messenger.IsRegistered<MessageB>(recipient));

        Assert.AreEqual(recipient.As, 0);
        Assert.AreEqual(recipient.Bs, 0);

        _ = messenger.Send<MessageB, string>(token);

        Assert.AreEqual(recipient.As, 0);
        Assert.AreEqual(recipient.Bs, 1);

        _ = messenger.Send<MessageA, string>(token);

        Assert.AreEqual(recipient.As, 1);
        Assert.AreEqual(recipient.Bs, 1);

        messenger.UnregisterAll(recipient, token);

        Assert.IsFalse(messenger.IsRegistered<MessageA>(recipient));
        Assert.IsFalse(messenger.IsRegistered<MessageB>(recipient));
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_RegisterWithTypeParameter(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        RecipientWithNoMessages? recipient = new() { Number = 42 };

        int number = 0;

        messenger.Register<RecipientWithNoMessages, MessageA>(recipient, (r, m) => number = r.Number);

        _ = messenger.Send<MessageA>();

        Assert.AreEqual(number, 42);
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger), false)]
    [DataRow(typeof(WeakReferenceMessenger), true)]
    public void Test_Messenger_Collect_Test(Type type, bool isWeak)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;

        WeakReference weakRecipient;

        void Test()
        {
            RecipientWithNoMessages? recipient = new() { Number = 42 };
            weakRecipient = new WeakReference(recipient);

            messenger.Register<MessageA>(recipient, (r, m) => { });

            Assert.IsTrue(messenger.IsRegistered<MessageA>(recipient));
            Assert.IsTrue(weakRecipient.IsAlive);

            GC.KeepAlive(recipient);
        }

        Test();

        GC.Collect();

        Assert.AreEqual(!isWeak, weakRecipient.IsAlive);

        GC.KeepAlive(messenger);
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_Reset(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        RecipientWithSomeMessages? recipient = new();

        messenger.RegisterAll(recipient);

        Assert.IsTrue(messenger.IsRegistered<MessageA>(recipient));
        Assert.IsTrue(messenger.IsRegistered<MessageB>(recipient));

        messenger.Reset();

        Assert.IsFalse(messenger.IsRegistered<MessageA>(recipient));
        Assert.IsFalse(messenger.IsRegistered<MessageB>(recipient));
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_Default(Type type)
    {
        PropertyInfo defaultInfo = type.GetProperty("Default")!;

        object? default1 = defaultInfo!.GetValue(null);
        object? default2 = defaultInfo!.GetValue(null);

        Assert.IsNotNull(default1);
        Assert.IsNotNull(default2);
        Assert.AreSame(default1, default2);
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_Cleanup(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;
        RecipientWithSomeMessages? recipient = new();

        messenger.Register<MessageA>(recipient);

        Assert.IsTrue(messenger.IsRegistered<MessageA>(recipient));

        void Test()
        {
            RecipientWithSomeMessages? recipient2 = new();

            messenger.Register<MessageB>(recipient2);

            Assert.IsTrue(messenger.IsRegistered<MessageB>(recipient2));

            GC.KeepAlive(recipient2);
        }

        Test();

        GC.Collect();

        // Here we just check that calling Cleanup doesn't alter the state
        // of the messenger. This method shouldn't really do anything visible
        // to consumers, it's just a way for messengers to compact their data.
        messenger.Cleanup();

        Assert.IsTrue(messenger.IsRegistered<MessageA>(recipient));
    }

#if NETCOREAPP // Auto-trimming is disabled on .NET Framework
    [TestMethod]
    public void Test_WeakReferenceMessenger_AutoCleanup()
    {
        IMessenger messenger = new WeakReferenceMessenger();

        static int GetRecipientsMapCount(IMessenger messenger)
        {
            object recipientsMap =
                typeof(WeakReferenceMessenger)
                .GetField("recipientsMap", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(messenger)!;

            return (int)recipientsMap.GetType().GetProperty("Count")!.GetValue(recipientsMap)!;
        }

        WeakReference weakRecipient;

        void Test()
        {
            RecipientWithSomeMessages? recipient = new();
            weakRecipient = new WeakReference(recipient);

            messenger.Register<MessageA>(recipient);

            Assert.IsTrue(messenger.IsRegistered<MessageA>(recipient));

            Assert.AreEqual(GetRecipientsMapCount(messenger), 1);

            GC.KeepAlive(recipient);
        }

        Test();

        GC.Collect();

        Assert.IsFalse(weakRecipient.IsAlive);

        // Now that the recipient is collected, trigger another full GC collection
        // to let the automatic cleanup callback run and trim the messenger data
        GC.Collect();
        GC.WaitForPendingFinalizers();

        Assert.AreEqual(GetRecipientsMapCount(messenger), 0);

        GC.KeepAlive(messenger);
    }
#endif

    [TestMethod]
    public void Test_StrongReferenceMessenger_AutoTrimming_UnregisterAll()
    {
        StrongReferenceMessenger messenger = new();
        IDictionary2 recipientsMap = (IDictionary2)typeof(StrongReferenceMessenger).GetField("recipientsMap", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(messenger)!;
        IDictionary2<Type2, object> typesMap = (IDictionary2<Type2, object>)typeof(StrongReferenceMessenger).GetField("typesMap", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(messenger)!;

        RecipientWithSomeMessages recipientA = new();
        RecipientWithSomeMessages recipientB = new();

        messenger.Register<MessageA>(recipientA);

        Assert.IsTrue(messenger.IsRegistered<MessageA>(recipientA));

        // There's one registered handler
        Assert.AreEqual(1, recipientsMap.Count);
        Assert.AreEqual(1, typesMap.Count);

        messenger.Register<MessageB>(recipientA);

        Assert.IsTrue(messenger.IsRegistered<MessageB>(recipientA));

        // There's two message types, for the same recipient
        Assert.AreEqual(1, recipientsMap.Count);
        Assert.AreEqual(2, typesMap.Count);

        messenger.Register<MessageA>(recipientB);

        Assert.IsTrue(messenger.IsRegistered<MessageA>(recipientB));

        // Now there's two recipients too
        Assert.AreEqual(2, recipientsMap.Count);
        Assert.AreEqual(2, typesMap.Count);

        messenger.UnregisterAll(recipientA);

        Assert.IsFalse(messenger.IsRegistered<MessageA>(recipientA));
        Assert.IsFalse(messenger.IsRegistered<MessageB>(recipientA));

        // Only the second recipient is left, which has a single message type
        Assert.AreEqual(1, recipientsMap.Count);
        Assert.AreEqual(1, typesMap.Count);

        messenger.UnregisterAll(recipientB);

        Assert.IsFalse(messenger.IsRegistered<MessageB>(recipientA));

        // Now both lists should be empty
        Assert.AreEqual(0, recipientsMap.Count);
        Assert.AreEqual(0, typesMap.Count);
    }

    [TestMethod]
    public void Test_StrongReferenceMessenger_AutoTrimming_UnregisterAllWithToken()
    {
        StrongReferenceMessenger messenger = new();
        IDictionary2 recipientsMap = (IDictionary2)typeof(StrongReferenceMessenger).GetField("recipientsMap", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(messenger)!;
        IDictionary2<Type2, object> typesMap = (IDictionary2<Type2, object>)typeof(StrongReferenceMessenger).GetField("typesMap", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(messenger)!;

        RecipientWithSomeMessages recipientA = new();
        RecipientWithSomeMessages recipientB = new();

        messenger.Register<MessageA, int>(recipientA, 42);
        messenger.Register<MessageB, int>(recipientA, 42);
        messenger.Register<MessageA, int>(recipientB, 42);

        Assert.IsTrue(messenger.IsRegistered<MessageA, int>(recipientA, 42));
        Assert.IsTrue(messenger.IsRegistered<MessageB, int>(recipientA, 42));
        Assert.IsTrue(messenger.IsRegistered<MessageA, int>(recipientB, 42));

        // There are two recipients, for two message types
        Assert.AreEqual(2, recipientsMap.Count);
        Assert.AreEqual(2, typesMap.Count);

        messenger.UnregisterAll(recipientA, 42);

        Assert.IsFalse(messenger.IsRegistered<MessageA, int>(recipientA, 42));
        Assert.IsFalse(messenger.IsRegistered<MessageB, int>(recipientA, 42));

        // Only the second recipient is left again
        Assert.AreEqual(1, recipientsMap.Count);
        Assert.AreEqual(1, typesMap.Count);

        messenger.UnregisterAll(recipientB, 42);

        Assert.IsFalse(messenger.IsRegistered<MessageA, int>(recipientB, 42));

        // Now both lists should be empty
        Assert.AreEqual(0, recipientsMap.Count);
        Assert.AreEqual(0, typesMap.Count);
    }

    [TestMethod]
    public void Test_StrongReferenceMessenger_AutoTrimming_UnregisterAllWithMessageTypeAndToken()
    {
        StrongReferenceMessenger messenger = new();
        IDictionary2 recipientsMap = (IDictionary2)typeof(StrongReferenceMessenger).GetField("recipientsMap", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(messenger)!;
        IDictionary2<Type2, object> typesMap = (IDictionary2<Type2, object>)typeof(StrongReferenceMessenger).GetField("typesMap", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(messenger)!;

        RecipientWithSomeMessages recipientA = new();
        RecipientWithSomeMessages recipientB = new();

        messenger.Register<MessageA, int>(recipientA, 42);
        messenger.Register<MessageB, int>(recipientA, 42);
        messenger.Register<MessageA, int>(recipientB, 42);

        messenger.Unregister<MessageA, int>(recipientA, 42);

        Assert.IsFalse(messenger.IsRegistered<MessageA, int>(recipientA, 42));
        Assert.IsTrue(messenger.IsRegistered<MessageB, int>(recipientA, 42));
        Assert.IsTrue(messenger.IsRegistered<MessageA, int>(recipientB, 42));

        // First recipient is still subscribed to MessageB.
        // The second one has a single subscription to MessageA.
        Assert.AreEqual(2, recipientsMap.Count);
        Assert.AreEqual(2, typesMap.Count);

        messenger.Unregister<MessageB, int>(recipientA, 42);

        Assert.IsFalse(messenger.IsRegistered<MessageA, int>(recipientA, 42));
        Assert.IsFalse(messenger.IsRegistered<MessageB, int>(recipientA, 42));
        Assert.IsTrue(messenger.IsRegistered<MessageA, int>(recipientB, 42));

        // Only the second recipient is left
        Assert.AreEqual(1, recipientsMap.Count);
        Assert.AreEqual(1, typesMap.Count);

        messenger.Unregister<MessageA, int>(recipientB, 42);

        Assert.IsFalse(messenger.IsRegistered<MessageA, int>(recipientA, 42));
        Assert.IsFalse(messenger.IsRegistered<MessageB, int>(recipientA, 42));
        Assert.IsFalse(messenger.IsRegistered<MessageA, int>(recipientB, 42));

        // Now both lists should be empty
        Assert.AreEqual(0, recipientsMap.Count);
        Assert.AreEqual(0, typesMap.Count);
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_ManyRecipients(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;

        void Test()
        {
            RecipientWithSomeMessages?[] recipients = Enumerable.Range(0, 512).Select(_ => new RecipientWithSomeMessages()).ToArray();

            foreach (RecipientWithSomeMessages? recipient in recipients)
            {
                messenger.RegisterAll(recipient!);
            }

            foreach (RecipientWithSomeMessages? recipient in recipients)
            {
                Assert.IsTrue(messenger.IsRegistered<MessageA>(recipient!));
                Assert.IsTrue(messenger.IsRegistered<MessageB>(recipient!));
            }

            _ = messenger.Send<MessageA>();
            _ = messenger.Send<MessageB>();
            _ = messenger.Send<MessageB>();

            foreach (RecipientWithSomeMessages? recipient in recipients)
            {
                Assert.AreEqual(recipient!.As, 1);
                Assert.AreEqual(recipient!.Bs, 2);
            }

            foreach (ref RecipientWithSomeMessages? recipient in recipients.AsSpan())
            {
                recipient = null;
            }
        }

        Test();

        GC.Collect();

        // Just invoke a final cleanup to improve coverage, this is unrelated to this test in particular
        messenger.Cleanup();
    }

    // See https://github.com/CommunityToolkit/WindowsCommunityToolkit/issues/4081
    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_RegisterMultiple_UnregisterSingle(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;

        object? recipient1 = new();
        object? recipient2 = new();

        int handler1CalledCount = 0;
        int handler2CalledCount = 0;

        messenger.Register<object, MessageA>(recipient1, (r, m) => { handler1CalledCount++; });
        messenger.Register<object, MessageA>(recipient2, (r, m) => { handler2CalledCount++; });

        messenger.UnregisterAll(recipient2);

        _ = messenger.Send(new MessageA());

        Assert.AreEqual(1, handler1CalledCount);
        Assert.AreEqual(0, handler2CalledCount);
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_ConcurrentOperations_DefaultChannel_SeparateRecipients(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;

        RecipientWithConcurrency[] recipients = Enumerable.Range(0, 1024).Select(static _ => new RecipientWithConcurrency()).ToArray();
        DateTime end = DateTime.Now.AddSeconds(5);

        _ = Parallel.For(0, recipients.Length, i =>
        {
            RecipientWithConcurrency r = recipients[i];
            Random random = new(i);

            while (DateTime.Now < end)
            {
                Assert.IsFalse(messenger.IsRegistered<MessageA>(r));
                Assert.IsFalse(messenger.IsRegistered<MessageB>(r));

                // Here we randomize the way messages are subscribed. This ensures that we're testing both the normal
                // registration with a delegate, as well as the fast path within WeakReferenceMessenger that is enabled
                // when IRecipient<TMessage> is used. Since broadcasts are done in parallel here, it will happen that
                // some messages are broadcast when multiple recipients are active through both types of registration.
                // This test verifies that the messengers work fine when this is the case (and not just when only one
                // of the two method is used, as mixing them up as necessary is a perfectly supported scenario as well).
                switch (random.Next(0, 3))
                {
                    case 0:
                        messenger.Register<RecipientWithConcurrency, MessageA>(r, static (r, m) => r.Receive(m));
                        messenger.Register<RecipientWithConcurrency, MessageB>(r, static (r, m) => r.Receive(m));
                        break;
                    case 1:
                        messenger.Register<MessageA>(r);
                        messenger.Register<MessageB>(r);
                        break;
                    default:
                        messenger.RegisterAll(r);
                        break;
                }

                Assert.IsTrue(messenger.IsRegistered<MessageA>(r));
                Assert.IsTrue(messenger.IsRegistered<MessageB>(r));

                int a = r.As;
                int b = r.Bs;

                _ = messenger.Send<MessageA>();

                // We can't just check that the count has been increased by 1, as there may be other concurrent broadcasts
                Assert.IsTrue(r.As > a);

                _ = messenger.Send<MessageB>();

                Assert.IsTrue(r.Bs > b);

                switch (random.Next(0, 2))
                {
                    case 0:
                        messenger.Unregister<MessageA>(r);
                        messenger.Unregister<MessageB>(r);
                        break;
                    default:
                        messenger.UnregisterAll(r);
                        break;
                }
            }
        });
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_ConcurrentOperations_DefaultChannel_SharedRecipients(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;

        RecipientWithConcurrency[] recipients = Enumerable.Range(0, 512).Select(static _ => new RecipientWithConcurrency()).ToArray();
        DateTime end = DateTime.Now.AddSeconds(5);

        _ = Parallel.For(0, recipients.Length * 2, i =>
        {
            RecipientWithConcurrency r = recipients[i / 2];
            Random random = new(i);

            while (DateTime.Now < end)
            {
                if (i % 2 == 0)
                {
                    Assert.IsFalse(messenger.IsRegistered<MessageA>(r));

                    switch (random.Next(0, 2))
                    {
                        case 0:
                            messenger.Register<RecipientWithConcurrency, MessageA>(r, static (r, m) => r.Receive(m));
                            break;
                        default:
                            messenger.Register<MessageA>(r);
                            break;
                    }

                    Assert.IsTrue(messenger.IsRegistered<MessageA>(r));

                    int a = r.As;

                    _ = messenger.Send<MessageA>();

                    Assert.IsTrue(r.As > a);

                    messenger.Unregister<MessageA>(r);
                }
                else
                {
                    Assert.IsFalse(messenger.IsRegistered<MessageB>(r));

                    switch (random.Next(0, 2))
                    {
                        case 0:
                            messenger.Register<RecipientWithConcurrency, MessageB>(r, static (r, m) => r.Receive(m));
                            break;
                        default:
                            messenger.Register<MessageB>(r);
                            break;
                    }

                    Assert.IsTrue(messenger.IsRegistered<MessageB>(r));

                    int b = r.Bs;

                    _ = messenger.Send<MessageB>();

                    Assert.IsTrue(r.Bs > b);

                    messenger.Unregister<MessageB>(r);
                }
            }
        });
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_ConcurrentOperations_WithToken_SeparateRecipients(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;

        RecipientWithConcurrency[] recipients = Enumerable.Range(0, 1024).Select(static _ => new RecipientWithConcurrency()).ToArray();
        string[] tokens = Enumerable.Range(0, 16).Select(static i => i.ToString()).ToArray();
        DateTime end = DateTime.Now.AddSeconds(5);

        _ = Parallel.For(0, recipients.Length, i =>
        {
            RecipientWithConcurrency r = recipients[i];
            string token = tokens[i % tokens.Length];
            Random random = new(i);

            while (DateTime.Now < end)
            {
                Assert.IsFalse(messenger.IsRegistered<MessageA, string>(r, token));
                Assert.IsFalse(messenger.IsRegistered<MessageB, string>(r, token));

                switch (random.Next(0, 3))
                {
                    case 0:
                        messenger.Register<RecipientWithConcurrency, MessageA, string>(r, token, static (r, m) => r.Receive(m));
                        messenger.Register<RecipientWithConcurrency, MessageB, string>(r, token, static (r, m) => r.Receive(m));
                        break;
                    case 1:
                        messenger.Register<MessageA, string>(r, token);
                        messenger.Register<MessageB, string>(r, token);
                        break;
                    default:
                        messenger.RegisterAll(r, token);
                        break;
                }

                Assert.IsTrue(messenger.IsRegistered<MessageA, string>(r, token));
                Assert.IsTrue(messenger.IsRegistered<MessageB, string>(r, token));

                int a = r.As;
                int b = r.Bs;

                _ = messenger.Send<MessageA, string>(token);

                Assert.IsTrue(r.As > a);

                _ = messenger.Send<MessageB, string>(token);

                Assert.IsTrue(r.Bs > b);

                switch (random.Next(0, 3))
                {
                    case 0:
                        messenger.Unregister<MessageA, string>(r, token);
                        messenger.Unregister<MessageB, string>(r, token);
                        break;
                    case 1:
                        messenger.UnregisterAll(r, token);
                        break;
                    default:
                        messenger.UnregisterAll(r);
                        break;
                }
            }
        });
    }

    [TestMethod]
    [DataRow(typeof(StrongReferenceMessenger))]
    [DataRow(typeof(WeakReferenceMessenger))]
    public void Test_Messenger_ConcurrentOperations_WithToken_SharedRecipients(Type type)
    {
        IMessenger? messenger = (IMessenger)Activator.CreateInstance(type)!;

        RecipientWithConcurrency[] recipients = Enumerable.Range(0, 512).Select(static _ => new RecipientWithConcurrency()).ToArray();
        string[] tokens = Enumerable.Range(0, 16).Select(static i => i.ToString()).ToArray();
        DateTime end = DateTime.Now.AddSeconds(5);

        _ = Parallel.For(0, recipients.Length * 2, i =>
        {
            RecipientWithConcurrency r = recipients[i / 2];
            string token = tokens[i % tokens.Length];
            Random random = new(i);

            while (DateTime.Now < end)
            {
                if (i % 2 == 0)
                {
                    Assert.IsFalse(messenger.IsRegistered<MessageA, string>(r, token));

                    switch (random.Next(0, 2))
                    {
                        case 0:
                            messenger.Register<RecipientWithConcurrency, MessageA, string>(r, token, static (r, m) => r.Receive(m));
                            break;
                        default:
                            messenger.Register<MessageA, string>(r, token);
                            break;
                    }

                    Assert.IsTrue(messenger.IsRegistered<MessageA, string>(r, token));

                    int a = r.As;

                    _ = messenger.Send<MessageA, string>(token);

                    Assert.IsTrue(r.As > a);

                    messenger.Unregister<MessageA, string>(r, token);
                }
                else
                {
                    Assert.IsFalse(messenger.IsRegistered<MessageB, string>(r, token));

                    switch (random.Next(0, 2))
                    {
                        case 0:
                            messenger.Register<RecipientWithConcurrency, MessageB, string>(r, token, static (r, m) => r.Receive(m));
                            break;
                        default:
                            messenger.Register<MessageB, string>(r, token);
                            break;
                    }

                    Assert.IsTrue(messenger.IsRegistered<MessageB, string>(r, token));

                    int b = r.Bs;

                    _ = messenger.Send<MessageB, string>(token);

                    Assert.IsTrue(r.Bs > b);

                    messenger.Unregister<MessageB, string>(r, token);
                }
            }
        });
    }

    public sealed class RecipientWithNoMessages
    {
        public int Number { get; set; }
    }

    public sealed class RecipientWithSomeMessages :
        IRecipient<MessageA>,
        IRecipient<MessageB>,
        ICloneable
    {
        public int As { get; private set; }

        public int Bs { get; private set; }

        public void Receive(MessageA message)
        {
            As++;
        }

        public void Receive(MessageB message)
        {
            Bs++;
        }

        // We also add the ICloneable interface to test that the message
        // interfaces are all handled correctly even when inteleaved
        // by other unrelated interfaces in the type declaration.
        public object Clone() => throw new NotImplementedException();
    }

    public sealed class RecipientWithConcurrency :
        IRecipient<MessageA>,
        IRecipient<MessageB>
    {
        private volatile int a;
        private volatile int b;

        public int As => this.a;

        public int Bs => this.b;

        public void Receive(MessageA message)
        {
            _ = Interlocked.Increment(ref this.a);
        }

        public void Receive(MessageB message)
        {
            _ = Interlocked.Increment(ref this.b);
        }
    }

    public sealed class MessageA
    {
        public string? Text { get; set; }
    }

    public sealed class MessageB
    {
    }
}
