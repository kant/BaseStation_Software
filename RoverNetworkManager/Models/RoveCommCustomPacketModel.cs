﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoverNetworkManager.Networking;

namespace RoverNetworkManager.Models
{
    internal class RoveCommCustomPacketModel
    {
        internal Dictionary<string, List<MetadataRecordContext>> Commands;
    }
}
