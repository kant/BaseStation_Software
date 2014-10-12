﻿using Caliburn.Micro;
using RED.Interfaces;
using RED.Models.ControlCenter;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RED.ViewModels.ControlCenter
{
    public class TCPConnectionVM : PropertyChangedBase, ISubscribe
    {
        private TCPConnectionModel Model;

        public const string LocalMachineName = "RED Master";
        public const string LocalSoftwareName = "RED";

        private DataRouterVM router { get; set; }
        private NetworkStream Stream { get; set; }
        public TcpClient Client
        {
            get
            {
                return Model.Client;
            }
            set
            {
                Model.Client = value;
                NotifyOfPropertyChange(() => Client);
            }
        }
        public IPAddress RemoteIP
        {
            get
            {
                return ((IPEndPoint)(Client.Client.RemoteEndPoint)).Address;
            }
        }
        public string RemoteName
        {
            get
            {
                return Model.RemoteName;
            }
            set
            {
                Model.RemoteName = value;
                NotifyOfPropertyChange(() => RemoteName);
            }
        }
        public string RemoteSoftware
        {
            get
            {
                return Model.RemoteSoftware;
            }
            set
            {
                Model.RemoteSoftware = value;
                NotifyOfPropertyChange(() => RemoteSoftware);
            }
        }

        public TCPConnectionVM(TcpClient client)
        {
            Model = new TCPConnectionModel();

            this.Client = client;
            Stream = Client.GetStream();

            InitializeConnection();

            //Start Listening
            ReceiveNetworkData();
        }

        private async void InitializeConnection()
        {
            Encoding ascii = Encoding.ASCII;
            byte[] buffer;

            //Send Local Name
            buffer = ascii.GetBytes(LocalMachineName);
            await Stream.WriteAsync(buffer, 0, buffer.Length);

            //Get and Save Remote Name
            buffer = new byte[256];
            int remoteNameLength = await Stream.ReadAsync(buffer, 0, buffer.Length);
            RemoteName = ascii.GetString(buffer, 0, remoteNameLength);

            //Send Local Software
            buffer = ascii.GetBytes(LocalSoftwareName);
            await Stream.WriteAsync(buffer, 0, buffer.Length);

            //Get and Save Remote Software
            buffer = new byte[256];
            int remoteSoftwareLength = await Stream.ReadAsync(buffer, 0, buffer.Length);
            RemoteSoftware = ascii.GetString(buffer, 0, remoteSoftwareLength);
        }

        private async void ReceiveNetworkData()
        {
            byte[] buffer = new byte[1024];
            while (true)//TODO: have this stop if we close
            {
                await Stream.ReadAsync(buffer, 0, buffer.Length);
                using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
                {
                    int dataId = br.ReadInt32();
                    Int16 dataLength = br.ReadInt16();
                    byte[] data = br.ReadBytes(dataLength);

                    switch (dataId)
                    {
                        case 1: router.Subscribe(this, dataId); break;//Subscribe Request
                        case 2: router.UnSubscribe(this, dataId); break;//Unsubscribe Request
                        default: router.Send(dataId, data); break;//Normal Packet
                    }
                }
            }
        }

        public void Close()
        {
            Client.Close();
        }

        public void Receive(int dataId, byte[] data)
        {
            using (BinaryWriter bw = new BinaryWriter(Stream))
            {
                bw.Write(dataId);
                bw.Write((Int16)(data.Length));
                bw.Write(data);
            }
        }
    }
}
