﻿using Caliburn.Micro;
using RED.Interfaces;
using RED.Interfaces.Input;
using RED.Models.Modules;
using RED.Models.Network;
using RED.ViewModels.Input;
using System;
using System.Collections.Generic;

namespace RED.ViewModels.Modules
{
    public class GimbalViewModel : PropertyChangedBase, IInputMode
    {
        private const float MaxZoomSpeed = 1000;
        private const int SpeedLimit = 1000;
        private readonly GimbalModel _model;
        private readonly IRovecomm _rovecomm;
        private readonly IDataIdResolver _idResolver;
        private readonly ILogger _log;

        private GimbalStates controlState = GimbalStates.MainGimbal;

        public string Name { get; }
        public string ModeType { get; }
        
        public int PanIncrement
        {
            get
            {
                return _model.PanIncrement;
            }
            set
            {
                _model.PanIncrement = value;
                NotifyOfPropertyChange(() => PanIncrement);
            }
        }

        public int RollIncrement
        {
            get
            {
                return _model.RollIncrement;
            }
            set
            {
                _model.RollIncrement = value;
                NotifyOfPropertyChange(() => RollIncrement);
            }
        }

        public int TiltIncrement
        {
            get
            {
                return _model.TiltIncrement;
            }
            set
            {
                _model.TiltIncrement = value;
                NotifyOfPropertyChange(() => TiltIncrement);
            }
        }
        public string ControlState
        {
            get
            {
                return _model.ControlState;
            }
            set
            {
                _model.ControlState = value;
                NotifyOfPropertyChange(() => ControlState);
            }
        }

        public GimbalViewModel(IRovecomm networkMessenger, IDataIdResolver idResolver, ILogger log)
        {
            _model = new GimbalModel();
            _rovecomm = networkMessenger;
            _idResolver = idResolver;
            _log = log;
            Name = "Main Gimbal";
            ModeType = "Gimbal";
        }

        public void StartMode()
        {
            controlState = GimbalStates.MainGimbal;
            ControlState = "Main Gimbal";
        }

        public void SetValues(Dictionary<string, float> values)
        {
            UpdateControlState(values);

            if (controlState == GimbalStates.SubGimbal)
            {
                short pan, tilt;
                pan = (Int16)(values["Pan"] * PanIncrement);
                tilt = (Int16)(values["Tilt"] * TiltIncrement);
                _rovecomm.SendCommand(new Packet("PanServo", pan));
                _rovecomm.SendCommand(new Packet("TiltServo", tilt));
            }
            else
            {
                short pan, tilt, mast, zoom, roll;

                switch (ControllerBase.JoystickDirection(values["Tilt"], values["Pan"]))
                {
                    case ControllerBase.JoystickDirections.Right:
                    case ControllerBase.JoystickDirections.Left:
                        pan = (short)(values["Pan"] * SpeedLimit);
                        tilt = 0;
                        break;
                    case ControllerBase.JoystickDirections.Up:
                    case ControllerBase.JoystickDirections.Down:
                        tilt = (short)(values["Tilt"] * SpeedLimit);
                        pan = 0;
                        break;

                    default:
                        tilt = 0;
                        pan = 0;
                        break;
                }

                zoom = (Int16)(values["Zoom"] * MaxZoomSpeed);
                roll = (Int16)(values["Roll"] * RollIncrement);
                mast = (Int16)(ControllerBase.TwoButtonToggleDirection(values["GimbalMastTiltDirection"] != 0, (values["GimbalMastTiltMagnitude"])) * SpeedLimit);

                short[] openVals = { pan, tilt, roll, mast, zoom };
                byte[] data = new byte[openVals.Length * sizeof(Int16)];
                Buffer.BlockCopy(openVals, 0, data, 0, data.Length);
                _rovecomm.SendCommand(new Packet("GimbalOpenValues", data, 0, null));
            }
        }

        private void UpdateControlState(Dictionary<string, float> values)
        {
            if(values["MainGimbalSwitch"] == 1)
            {
                controlState = GimbalStates.MainGimbal;
                ControlState = "Main Gimbal";
            }
            else if(values["SubGimbalSwitch"] == 1)
            {
                controlState = GimbalStates.SubGimbal;
                ControlState = "Sub Gimbal";

                short[] openVals = { 0, 0, 0, 0, 0 };
                byte[] data = new byte[openVals.Length * sizeof(Int16)];
                Buffer.BlockCopy(openVals, 0, data, 0, data.Length);
                _rovecomm.SendCommand(new Packet("GimbalOpenValues", data, 0, null));
            }
        }

        public void StopMode()
        {
            _rovecomm.SendCommand(new Packet("GimbalOpenValues", new byte[]{ 0, 0, 0, 0, 0 }, 0, null), true);
        }

        public void Snapshot()
        {
            _rovecomm.SendCommand(new Packet("GimbalRecord", (byte)GimbalRecordValues.Snapshot), true);
        }

        public void RecordStart()
        {
            _rovecomm.SendCommand(new Packet("GimbalRecord", (byte)GimbalRecordValues.Start), true);
        }

        public void RecordStop()
        {
            _rovecomm.SendCommand(new Packet("GimbalRecord", (byte)GimbalRecordValues.Stop), true);
        }

        private enum GimbalRecordValues
        {
            Stop = 0,
            Start = 1,
            Snapshot = 2
        }

        private enum GimbalStates
        {
            MainGimbal,
            SubGimbal
        }
    }
}