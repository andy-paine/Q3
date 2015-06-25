﻿using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Q3Server
{
    [Authorize]
    public class QHub : Hub
    {
        private IQManager queueManager;
        private IUserAccessor userAccessor;

        public QHub(IQManager queueManager, IUserAccessor userAccessor)
        {
            this.queueManager = queueManager;
            this.userAccessor = userAccessor;
        }

        public void StartQueue(string queueName, string restrictToGroup)
        {
            Trace.WriteLine("-> StartQueue: " + queueName + " restricted to " + restrictToGroup);
            queueManager.CreateQueue(queueName, restrictToGroup, userAccessor.User);
        }

        public void JoinQueue(int id)
        {
            Trace.WriteLine("-> JoinQueue: " + id);
            queueManager.GetQueue(id).AddUser(userAccessor.User);
        }

        public void LeaveQueue(int id)
        {
            Trace.WriteLine("-> LeaveQueue: " + id);
            queueManager.GetQueue(id).RemoveUser(userAccessor.User);
        }

        public void ActivateQueue(int id)
        {
            Trace.WriteLine("-> ActivateQueue: " + id);
            queueManager.GetQueue(id).Activate();
        }

        public void MessageQueue(int id, string message)
        {
            Trace.WriteLine("-> MessageQueue: " + id + " - " + message);
            queueManager.GetQueue(id).AddMessage(userAccessor.User, message);
        }

        public void CloseQueue(int id)
        {
            Trace.WriteLine("-> CloseQueue: " + id);
            queueManager.CloseQueue(id);
        }

        public IEnumerable<Queue> ListQueues()
        {
            return queueManager.ListQueues();
        }
    }
}