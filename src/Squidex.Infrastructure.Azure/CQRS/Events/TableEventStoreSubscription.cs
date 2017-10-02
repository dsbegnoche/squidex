using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Squidex.Infrastructure.Actors;
using Squidex.Infrastructure.CQRS.Events.Actors.Messages;
using Squidex.Infrastructure.Tasks;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Infrastructure.CQRS.Events
{
    public class TableEventStoreSubscription : Actor, IEventSubscription
    {
        private const int ReconnectWindowMax = 5;
        private const int ReconnectWaitMs = 1000;
        private static readonly TimeSpan TimeBetweenReconnects = TimeSpan.FromMinutes(5);
        private static readonly ConcurrentDictionary<string, bool> SubscriptionsCreated = new ConcurrentDictionary<string, bool>();
        private readonly CloudTable table;
        private readonly CloudStorageAccount cloudStorageAccount;
        private readonly CloudTableClient tableClient;
        private readonly string prefix;
        private readonly Queue<DateTime> reconnectTimes = new Queue<DateTime>();
        private string streamFilter;
        private string streamName;
        private long? position;
        private IActor parent;

        private sealed class ConnectMessage : IMessage
        {
        }

        private sealed class ConnectionFailedMessage : IMessage
        {
            public Exception Exception;
        }

        private sealed class ReceiveESEventMessage : IMessage
        {
            public Streamstone.EventData Event;
        }

        public TableEventStoreSubscription(CloudStorageAccount cloudStorageAccount, CloudTableClient tableClient, CloudTable table, string prefix)
        {
            this.prefix = prefix;
            this.table = table;
            this.cloudStorageAccount = cloudStorageAccount;
            this.tableClient = tableClient;
        }

        /*protected override Task OnStop()
        {
            return TaskHelper.Done;
        }

        protected override async Task OnError(Exception exception)
        {
            if (parent != null)
            {
                await parent.SendAsync(exception);
            }

            await StopAsync();
        }*/

        protected override async Task OnMessage(IMessage message)
        {
            switch (message)
            {
                case SubscribeMessage subscribe when parent == null:
                    {
                        parent = subscribe.Parent;
                        position = ParsePosition(subscribe.Position);

                        streamFilter = subscribe.StreamFilter;
                        streamName = $"by-{prefix.Simplify()}-{streamFilter.Simplify()}";

                        // await CreateProjectionAsync();
                        SendAsync(new ConnectMessage()).Forget();

                        break;
                    }

                case ConnectionFailedMessage connectionFailed when parent != null:
                    {
                        // subscription.Stop();
                        // subscription = null;

                        if (CanReconnect(DateTime.UtcNow))
                        {
                            Task.Delay(ReconnectWaitMs).ContinueWith(t => SendAsync(new ConnectMessage())).Forget();
                        }
                        else
                        {
                            await SendAsync(connectionFailed.Exception);
                        }

                        break;
                    }

                case ConnectMessage connect when parent != null:
                    {
                        // subscription = connection.SubscribeToStreamFrom(streamName, position, CatchUpSubscriptionSettings.Default, HandleEvent, null, HandleError);

                        break;
                    }/*
                case ReceiveESEventMessage receiveEvent when parent != null:
                    {
                        if (receiveEvent.Subscription == subscription)
                        {
                            var storedEvent = Formatter.Read(receiveEvent.Event);

                            await parent.SendAsync(new ReceiveEventMessage { Event = storedEvent, Source = this });

                            position = receiveEvent.Event.OriginalEventNumber;
                        }

                        break;
                    }
                    */
            }
        }

        private void HandleEvent(Streamstone.EventData resolved)
        {
            SendAsync(new ReceiveESEventMessage { Event = resolved }).Forget();
        }

        /*private void HandleError(EventStoreCatchUpSubscription s, SubscriptionDropReason reason, Exception ex)
        {
            if (reason == SubscriptionDropReason.ConnectionClosed && subscription == s)
            {
                SendAsync(new ConnectionFailedMessage { Exception = ex });
            }
            else if (reason != SubscriptionDropReason.UserInitiated && subscription == s)
            {
                var exception = ex ?? new ConnectionClosedException($"Subscription closed with reason {reason}.");

                SendAsync(ex).Forget();
            }
        }*/

        private static long? ParsePosition(string position)
        {
            return long.TryParse(position, out var parsedPosition) ? (long?)parsedPosition : null;
        }

        private bool CanReconnect(DateTime utcNow)
        {
            reconnectTimes.Enqueue(utcNow);

            while (reconnectTimes.Count >= ReconnectWindowMax)
            {
                reconnectTimes.Dequeue();
            }

            return reconnectTimes.Count < ReconnectWindowMax && (reconnectTimes.Count == 0 || (utcNow - reconnectTimes.Peek()) > TimeBetweenReconnects);
        }

        /*private async Task CreateProjectionAsync()
        {
            if (SubscriptionsCreated.TryAdd(streamName, true))
            {
                var projectsManager = await ConnectToProjections();

                var projectionConfig =
                    $@"fromAll()
                        .when({{
                            $any: function (s, e) {{
                                if (e.streamId.indexOf('{prefix}') === 0 && /{streamFilter}/.test(e.streamId.substring({prefix.Length + 1}))) {{
                                    linkTo('{streamName}', e);
                                }}
                            }}
                        }});";

                try
                {
                    await projectsManager.CreateContinuousAsync($"${streamName}", projectionConfig,
                        connection.Settings.DefaultUserCredentials);
                }
                catch (ProjectionCommandConflictException)
                {
                    // ignore
                }
            }
        }

        private async Task<ProjectionsManager> ConnectToProjections()
        {
            var addressParts = projectionHost.Split(':');

            if (addressParts.Length < 2 || !int.TryParse(addressParts[1], out var port))
            {
                port = 2113;
            }

            var endpoints = await Dns.GetHostAddressesAsync(addressParts[0]);
            var endpoint = new IPEndPoint(endpoints.First(x => x.AddressFamily == AddressFamily.InterNetwork), port);

            var projectionsManager =
                new ProjectionsManager(
                    connection.Settings.Log, endpoint,
                    connection.Settings.OperationTimeout);

            return projectionsManager;
        }*/
    }
}