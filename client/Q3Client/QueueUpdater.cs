﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Q3Client
{
    class QueueUpdater
    {
        private readonly Hub hub;
        private readonly ObservableCollection<Queue> queues = new ObservableCollection<Queue>();
        private Dictionary<int, Queue> queuesById = new Dictionary<int, Queue>();
        private User user;

        private QueueList queueList;
        private DisplayTimer alertDisplayTimer;


        public QueueUpdater(Hub hub, User user)
        {
            this.hub = hub;
            this.user = user;

            queueList = new QueueList(hub);
            queueList.Show();

            alertDisplayTimer = new DisplayTimer(queueList);
        }

        public ObservableCollection<Queue> Queues
        {
            get { return queues; }
        }

        public async Task RefreshALl()
        {
            var serverQueues = await hub.ListQueues();


            foreach (var q in serverQueues.OrderBy(q => q.Id))
            {
                if (queuesById.ContainsKey(q.Id))
                {
                    MergeChanges(queuesById[q.Id], q);
                }
                else
                {
                    AddQueue(q, false);
                }
            }

            foreach (var q in queues.ToList())
            {
                if (!serverQueues.Any(sq => sq.Id == q.Id))
                {
                    queues.Remove(q);
                    queuesById.Remove(q.Id);

                    queueList.Dispatcher.Invoke(() =>
                    {
                        var window =
                            queueList.QueuesPanel.Children.OfType<QueueNotification>()
                                .FirstOrDefault(w => w.Queue.Id == q.Id);
                        if (window != null)
                        {
                            queueList.QueuesPanel.Children.Remove(window);
                        }
                    });
                }
            }
            
        }

        private void MergeChanges(Queue clientQueue, Queue serverQueue)
        {
            clientQueue.Name = serverQueue.Name;
            clientQueue.Status = serverQueue.Status;
            clientQueue.Members = serverQueue.Members;
        }

        public void AddQueue(Queue queue)
        {
            AddQueue(queue, true);
        }

        private void AddQueue(Queue queue, bool isNew)
        {
            queueList.Dispatcher.Invoke(() =>
            {

                queue.User = user;
                queues.Add(queue);
                queuesById.Add(queue.Id, queue);

                var window = new QueueNotification(queue);
                window.JoinQueue += (s, e) => hub.JoinQueue(queue.Id);
                window.LeaveQueue += (s, e) => hub.LeaveQueue(queue.Id);
                window.ActivateQueue += (s, e) => hub.ActivateQueue(queue.Id);
                window.CloseQueue += (s, e) => hub.CloseQueue(queue.Id);
                queueList.QueuesPanel.Children.Insert(0, window);
            });

            if (isNew)
            {
                alertDisplayTimer.ShowAlert();
            }
        }

        public void UpdateQueue(Queue serverQueue)
        {
            MergeChanges(queuesById[serverQueue.Id], serverQueue);

        }
    }
}
