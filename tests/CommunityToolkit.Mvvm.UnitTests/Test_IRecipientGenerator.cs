// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma warning disable CS0618

using System;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Mvvm
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601", Justification = "Type only used for testing")]
    [TestClass]
    public partial class Test_IRecipientGenerator
    {
        [TestCategory("Mvvm")]
        [TestMethod]
        public void Test_IRecipientGenerator_GeneratedRegistration()
        {
            StrongReferenceMessenger? messenger = new();
            RecipientWithSomeMessages? recipient = new();

            MessageA? messageA = new();
            MessageB? messageB = new();

            Action<IMessenger, object, int> registrator = CommunityToolkit.Mvvm.Messaging.__Internals.__IMessengerExtensions.CreateAllMessagesRegistratorWithToken<int>(recipient);

            registrator(messenger, recipient, 42);

            Assert.IsTrue(messenger.IsRegistered<MessageA, int>(recipient, 42));
            Assert.IsTrue(messenger.IsRegistered<MessageB, int>(recipient, 42));

            Assert.IsNull(recipient.A);
            Assert.IsNull(recipient.B);

            _ = messenger.Send(messageA, 42);

            Assert.AreSame(recipient.A, messageA);
            Assert.IsNull(recipient.B);

            _ = messenger.Send(messageB, 42);

            Assert.AreSame(recipient.A, messageA);
            Assert.AreSame(recipient.B, messageB);
        }

        public sealed class RecipientWithSomeMessages :
            IRecipient<MessageA>,
            IRecipient<MessageB>
        {
            public MessageA? A { get; private set; }

            public MessageB? B { get; private set; }

            public void Receive(MessageA message)
            {
                A = message;
            }

            public void Receive(MessageB message)
            {
                B = message;
            }
        }

        public sealed class MessageA
        {
        }

        public sealed class MessageB
        {
        }
    }
}
