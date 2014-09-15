﻿using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using KafkaNet.Model;
using KafkaNet.Protocol;

namespace KafkaNet
{
    public class DefaultKafkaConnectionFactory : IKafkaConnectionFactory
    {
        public IKafkaConnection Create(Uri kafkaAddress, int responseTimeoutMs, IKafkaLog log)
        {
            //TODO kafkaAddress should probably be a KafkaEndpoint.  Needs review.
            return new KafkaConnection(new KafkaTcpSocket(log, Resolve(kafkaAddress, log)), responseTimeoutMs, log);
        }

        public KafkaEndpoint Resolve(Uri kafkaAddress, IKafkaLog log)
        {
            var ipAddress = GetFirstAddress(kafkaAddress.Host, log);
            var ipEndpoint = new IPEndPoint(ipAddress, kafkaAddress.Port);

            var kafkaEndpoint = new KafkaEndpoint()
            {
                ServeUri = kafkaAddress,
                Endpoint = ipEndpoint
            };

            return kafkaEndpoint;
        }


        private static IPAddress GetFirstAddress(string hostname, IKafkaLog log)
        {
            //lookup the IP address from the provided host name
            var addresses = Dns.GetHostAddresses(hostname);

            if (addresses.Length > 0)
            {
                Array.ForEach(addresses, address => log.DebugFormat("Found address {0} for {1}", address, hostname));

                var selectedAddress = addresses.FirstOrDefault(item => item.AddressFamily == AddressFamily.InterNetwork) ?? addresses.First();

                log.DebugFormat("Using address {0} for {1}", selectedAddress, hostname);

                return selectedAddress;
            }

            throw new UnresolvedHostnameException("Could not resolve the following hostname: {0}", hostname);
        }
    }
}
