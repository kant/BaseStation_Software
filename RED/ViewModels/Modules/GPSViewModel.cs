﻿using Caliburn.Micro;
using RED.Addons.Navigation;
using Core.Interfaces;
using RED.Models.Modules;
using System;
using System.IO;
using Core.Models;

namespace RED.ViewModels.Modules
{
    public class GPSViewModel : PropertyChangedBase, IRovecommReceiver
    {
        private readonly GPSModel _model;
        private readonly IDataIdResolver _idResolver;
        private readonly IRovecomm _rovecomm;

        public bool FixObtained
        {
            get
            {
                return _model.FixObtained;
            }
            set
            {
                _model.FixObtained = value;
                NotifyOfPropertyChange(() => FixObtained);
            }
        }
        public byte FixQuality
        {
            get
            {
                return _model.FixQuality;
            }
            set
            {
                _model.FixQuality = value;
                NotifyOfPropertyChange(() => FixQuality);
            }
        }
        public byte NumberOfSatellites
        {
            get
            {
                return _model.NumberOfSatellites;
            }
            set
            {
                _model.NumberOfSatellites = value;
                NotifyOfPropertyChange(() => NumberOfSatellites);
            }
        }
        public GPSCoordinate CurrentLocation
        {
            get
            {
                return _model.CurrentLocation;
            }
            set
            {
                _model.CurrentLocation = value;
                NotifyOfPropertyChange(() => CurrentLocation);
            }
        }
        public float CurrentAltitude
        {
            get
            {
                return _model.CurrentAltitude;
            }
            set
            {
                _model.CurrentAltitude = value;
                NotifyOfPropertyChange(() => CurrentAltitude);
            }
        }
        public float Speed
        {
            get
            {
                return _model.Speed;
            }
            set
            {
                _model.Speed = value;
                NotifyOfPropertyChange(() => Speed);
            }
        }
        public float SpeedAngle
        {
            get
            {
                return _model.SpeedAngle;
            }
            set
            {
                _model.SpeedAngle = value;
                NotifyOfPropertyChange(() => SpeedAngle);
            }
        }
        public GPSCoordinate BaseStationLocation
        {
            get
            {
                return _model.BaseStationLocation;
            }
            set
            {
                _model.BaseStationLocation = value;
                NotifyOfPropertyChange(() => BaseStationLocation);
                RecalculateAntennaDirection();
            }
        }
        public double AntennaDirectionDeg
        {
            get
            {
                return _model.AntennaDirectionDeg;
            }
            set
            {
                _model.AntennaDirectionDeg = value;
                NotifyOfPropertyChange(() => AntennaDirectionDeg);
            }
        }

        public float Heading
        {
            get
            {
                return _model.Heading;
            }
            set
            {
                _model.Heading = value;
                NotifyOfPropertyChange(() => Heading);
                NotifyOfPropertyChange(() => HeadingDeg);
            }
        }
        public float HeadingDeg
        {
            get
            {
                return (float)(Heading * 180d / Math.PI);
            }
        }

        public GPSViewModel(IRovecomm networkMessenger, IDataIdResolver idResolver)
        {
            _model = new GPSModel();
            _rovecomm = networkMessenger;
            _idResolver = idResolver;

            _rovecomm.NotifyWhenMessageReceived(this, "GPSQuality");
            _rovecomm.NotifyWhenMessageReceived(this, "GPSPosition");
            _rovecomm.NotifyWhenMessageReceived(this, "GPSSpeed");
            _rovecomm.NotifyWhenMessageReceived(this, "GPSSpeedAngle");
            _rovecomm.NotifyWhenMessageReceived(this, "GPSAltitude");
            _rovecomm.NotifyWhenMessageReceived(this, "GPSSatellites");
        }

        public void ReceivedRovecommMessageCallback(Packet packet, bool reliable)
        {
            switch (packet.Name)
            {
                case "GPSData":
                    var ms = new MemoryStream(packet.Data);
                    using (var br = new BinaryReader(ms))
                    {
                        FixObtained = br.ReadByte() != 0;
                        FixQuality = br.ReadByte();
                        NumberOfSatellites = br.ReadByte();
                        CurrentLocation = new GPSCoordinate()
                        {
                            Latitude = br.ReadInt32() / 10000000f,
                            Longitude = br.ReadInt32() / 10000000f
                        };
                        CurrentAltitude = br.ReadSingle();
                        Speed = br.ReadSingle();
                        SpeedAngle = br.ReadSingle();
                    }
                    break;
                case "Heading":
                    Heading = BitConverter.ToSingle(packet.Data, 0);
                    break;
                case "GPSQuality":
                    FixObtained = packet.Data[0] != 0;
                    FixQuality = packet.Data[0];
                    break;
                case "GPSPosition":
                    CurrentLocation = new GPSCoordinate()
                    {
                        Latitude = BitConverter.ToInt32(packet.Data, 1 * sizeof(Int32)) / 10000000f,
                        Longitude = -BitConverter.ToInt32(packet.Data, 0 * sizeof(Int32)) / 10000000f
                    };
                    break;
                case "GPSSpeed":
                    Speed = BitConverter.ToSingle(packet.Data, 0);
                    break;
                case "GPSSpeedAngle":
                    SpeedAngle = BitConverter.ToSingle(packet.Data, 0);
                    break;
                case "GPSAltitude":
                    CurrentAltitude = BitConverter.ToSingle(packet.Data, 0);
                    break;
                case "GPSSatellites":
                    NumberOfSatellites = packet.Data[0];
                    break;
            }
        }

		public void ReceivedRovecommMessageCallback(int index, bool reliable) {
			ReceivedRovecommMessageCallback(_rovecomm.GetPacketByID(index), false);
		}

		private void RecalculateAntennaDirection()
        {
            var thetaRad = Math.Atan2(CurrentLocation.Latitude - BaseStationLocation.Latitude, CurrentLocation.Longitude - BaseStationLocation.Longitude);
            AntennaDirectionDeg = thetaRad / Math.PI * 180;
        }
    }
}