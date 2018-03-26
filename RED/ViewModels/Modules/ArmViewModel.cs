﻿using Caliburn.Micro;
using RED.Configurations.Modules;
using RED.Contexts.Modules;
using RED.Interfaces;
using RED.Interfaces.Input;
using RED.Models.Modules;
using RED.ViewModels.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RED.ViewModels.Modules
{
    /// <summary>
    /// The main class for handling all arm-based logic, and corresponds to the Arm dropbox in the gui 
    /// 
    /// Inhereting from PropertyBasedChange (caliburn micro for interacting with the view), IInputMode, and 
    /// ISubscribe, this class essentially has three responsibilities.
    /// First, it contains all the logic that tells the arm view what to fill itself with. Most of the fields below are for this,
    /// such as AngleJ1 and so on. These are bound together with certain elements in the view, and changing them here
    /// changes them in the view.
    /// 
    /// Second, as an IInputMode, the class is responsible for handling what happens when the user starts up arm control. 
    /// An instance of this class is called when that happens, and methods such as StartMode, SetValues, and such are for 
    /// this purpose. More information can be found in IInputmode.cs over in Interfaces folder.
    /// 
    /// Finally, as an ISubscribe, this class is responsible for receiving messages from the router whenever RED 
    /// gets telemetry messages from Rover. When that happens the router will call this class and ask it to see
    /// if any of the messages (rovecomm dataid's) belong to it.
    /// 
    /// This class is designed to be constructed in the main view model class (usually ControlCenterViewModel).
    /// </summary>
    public class ArmViewModel : PropertyChangedBase, IInputMode, IRovecommReceiver
    {
        private enum ArmControlState
        {
            OpenLoop,
            IKRoverPOV, //point of view
            IKWristPOV,
            GuiControl
        };

        ArmControlState myState;

        private const string PositionsConfigName = "ArmPositions";

        private const byte ArmDisableCommand = 0x00;
        private const byte ArmEnableCommand = 0x01;

        private const short MotorRangeFactor = 1000;

        private readonly ArmModel _model;
        private readonly IRovecomm _rovecomm;
        private readonly IDataIdResolver _idResolver;
        private readonly ILogger _log;
        private readonly IConfigurationManager _configManager;

        public string Name { get; }
        public string ModeType { get; }

        public float AngleJ1
        {
            get
            {
                return _model.AngleJ1;
            }
            set
            {
                _model.AngleJ1 = value;
                NotifyOfPropertyChange(() => AngleJ1);
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

        public float AngleJ2
        {
            get
            {
                return _model.AngleJ2;
            }
            set
            {
                _model.AngleJ2 = value;
                NotifyOfPropertyChange(() => AngleJ2);
            }
        }
        public float AngleJ3
        {
            get
            {
                return _model.AngleJ3;
            }
            set
            {
                _model.AngleJ3 = value;
                NotifyOfPropertyChange(() => AngleJ3);
            }
        }
        public float AngleJ4
        {
            get
            {
                return _model.AngleJ4;
            }
            set
            {
                _model.AngleJ4 = value;
                NotifyOfPropertyChange(() => AngleJ4);
            }
        }
        public float AngleJ5
        {
            get
            {
                return _model.AngleJ5;
            }
            set
            {
                _model.AngleJ5 = value;
                NotifyOfPropertyChange(() => AngleJ5);
            }
        }
        public float AngleJ6
        {
            get
            {
                return _model.AngleJ6;
            }
            set
            {
                _model.AngleJ6 = value;
                NotifyOfPropertyChange(() => AngleJ6);
            }
        }


        public float CoordinateX
        {
            get
            {
                return _model.CoordinateX;
            }
            set
            {
                _model.CoordinateX = value;
                NotifyOfPropertyChange(() => CoordinateX);
            }
        }

        public float CoordinateY
        {
            get
            {
                return _model.CoordinateY;
            }
            set
            {
                _model.CoordinateY = value;
                NotifyOfPropertyChange(() => CoordinateY);
            }
        }

        public float CoordinateZ
        {
            get
            {
                return _model.CoordinateZ;
            }
            set
            {
                _model.CoordinateZ = value;
                NotifyOfPropertyChange(() => CoordinateZ);
            }
        }

        public float Yaw
        {
            get
            {
                return _model.Yaw;
            }
            set
            {
                _model.Yaw = value;
                NotifyOfPropertyChange(() => Yaw);
            }
        }

        public float Pitch
        {
            get
            {
                return _model.Pitch;
            }
            set
            {
                _model.Pitch = value;
                NotifyOfPropertyChange(() => Pitch);
            }
        }

        public float Roll
        {
            get
            {
                return _model.Roll;
            }
            set
            {
                _model.Roll = value;
                NotifyOfPropertyChange(() => Roll);
            }
        }

        public float CurrentMain
        {
            get
            {
                return _model.CurrentMain;
            }
            set
            {
                _model.CurrentMain = value;
                NotifyOfPropertyChange(() => CurrentMain);
            }
        }

        public int EndeffectorSpeedLimit
        {
            get
            {
                return _model.EndeffectorSpeedLimit;
            }
            set
            {
                _model.EndeffectorSpeedLimit = value;
                NotifyOfPropertyChange(() => EndeffectorSpeedLimit);
            }
        }

        public ObservableCollection<ArmPositionViewModel> Positions
        {
            get
            {
                return _model.Positions;
            }
            private set
            {
                _model.Positions = value;
                NotifyOfPropertyChange(() => Positions);
            }
        }
        public ArmPositionViewModel SelectedPosition
        {
            get
            {
                return _model.SelectedPosition;
            }
            set
            {
                _model.SelectedPosition = value;
                NotifyOfPropertyChange(() => SelectedPosition);
            }
        }

        public ArmViewModel(IRovecomm networkMessenger, IDataIdResolver idResolver, ILogger log, IConfigurationManager configs)
        {
            _model = new ArmModel();
            _rovecomm = networkMessenger;
            _idResolver = idResolver;
            _log = log;
            _configManager = configs;
            Name = "Arm";
            ModeType = "Arm";
            myState = ArmControlState.GuiControl;
            ControlState = "GUI control";

            _configManager.AddRecord(PositionsConfigName, ArmConfig.DefaultArmPositions);
            InitializePositions(_configManager.GetConfig<ArmPositionsContext>(PositionsConfigName));

            _rovecomm.NotifyWhenMessageReceived(this, _idResolver.GetId("ArmCurrentPosition"));
            _rovecomm.NotifyWhenMessageReceived(this, _idResolver.GetId("ArmFault"));
            _rovecomm.NotifyWhenMessageReceived(this, _idResolver.GetId("ArmCurrentMain"));
        }

        public void ReceivedRovecommMessageCallback(ushort dataId, byte[] data, bool reliable)
        {
            switch (_idResolver.GetName(dataId))
            {
                case "ArmCurrentPosition":
                    AngleJ1 = BitConverter.ToSingle(data, 0 * sizeof(float));
                    AngleJ2 = BitConverter.ToSingle(data, 1 * sizeof(float));
                    AngleJ3 = BitConverter.ToSingle(data, 2 * sizeof(float));
                    AngleJ4 = BitConverter.ToSingle(data, 3 * sizeof(float));
                    AngleJ5 = BitConverter.ToSingle(data, 4 * sizeof(float));
                    AngleJ6 = BitConverter.ToSingle(data, 5 * sizeof(float));
                    break;
                case "ArmFault":
                    _log.Log($"Arm reported a fault code of {data[0]}");
                    break;
                case "ArmCurrentMain":
                    CurrentMain = BitConverter.ToSingle(data, 0);
                    break;
            }
        }

        public void StartMode()
        {
            myState = ArmControlState.OpenLoop;
            ControlState = "Open loop";
        }

        private void SetOpenLoopValues(Dictionary<string, float> values)
        {
            if (values["DebouncedArmReset"] != 0)
            {
                _rovecomm.SendCommand(_idResolver.GetId("ArmStop"), (Int16)(0));
                _log.Log("Robotic Arm Resetting...");
                return;
            }

            Int16 ArmWristBend = 0;
            Int16 ArmWristTwist = 0;
            Int16 ArmElbowTwist = 0;
            Int16 ArmElbowBend = 0;
            Int16 ArmBaseTwist = 0;
            Int16 ArmBaseBend = 0;
            Int16 Gripper = 0;
            Int16 Nipper = 0;

            switch (JoystickDirection(values["WristBend"], values["WristTwist"]))
            {
                case JoystickDirections.Right:
                case JoystickDirections.Left:
                case JoystickDirections.Up:
                case JoystickDirections.Down:
                    ArmWristBend = (Int16)(values["WristBend"] * MotorRangeFactor);
                    ArmWristTwist = (Int16)(values["WristTwist"] * MotorRangeFactor);
                    break;
                case JoystickDirections.None:
                    ArmWristTwist = 0;
                    ArmWristBend = 0;
                    break;
            }

            switch (JoystickDirection(values["ElbowBend"], values["ElbowTwist"]))
            {
                case JoystickDirections.Up:
                case JoystickDirections.Down:
                    ArmElbowTwist = 0;
                    ArmElbowBend = (Int16)(values["ElbowBend"] * MotorRangeFactor);
                    break;
                case JoystickDirections.Right:
                case JoystickDirections.Left:
                    ArmElbowTwist = (Int16)(values["ElbowTwist"] * MotorRangeFactor);
                    ArmElbowBend = 0;
                    break;
                case JoystickDirections.None:
                    ArmElbowTwist = 0;
                    ArmElbowBend = 0;
                    break;
            }


            ArmBaseTwist = (Int16)(ControllerBase.TwoButtonToggleDirection(values["BaseTwistDirection"] != 0, (values["BaseTwistMagnitude"])) * MotorRangeFactor);
            ArmBaseBend = (Int16)(ControllerBase.TwoButtonToggleDirection(values["BaseBendDirection"] != 0, (values["BaseBendMagnitude"])) * MotorRangeFactor);
            Gripper = (Int16)(ControllerBase.TwoButtonTransform(values["GripperClose"] > 0, values["GripperOpen"] > 0, values["GripperClose"], -values["GripperOpen"], 0) * EndeffectorSpeedLimit);
            Nipper = (Int16)(ControllerBase.TwoButtonTransform(values["NipperClose"] > 0, values["NipperOpen"] > 0, values["NipperClose"], -values["NipperOpen"], 0) * EndeffectorSpeedLimit);

            Int16[] sendValues = { ArmBaseTwist, ArmBaseBend, ArmElbowBend, ArmElbowTwist, ArmWristBend, ArmWristTwist, Gripper, Nipper };
            byte[] data = new byte[sendValues.Length * sizeof(Int16)];
            Buffer.BlockCopy(sendValues, 0, data, 0, data.Length);
            _rovecomm.SendCommand(_idResolver.GetId("ArmValues"), data);
        }

        private void SetIKValues(Dictionary<string, float> values, ArmControlState stateToUse)
        {
            if (values["DebouncedArmReset"] != 0)
            {
                _rovecomm.SendCommand(_idResolver.GetId("ArmStop"), (Int16)(0));
                _log.Log("Robotic Arm Resetting...");
                return;
            }

            Int16 X = 0;
            Int16 Y = 0;
            Int16 Z = 0;
            Int16 Yaw = 0;
            Int16 Pitch = 0;
            Int16 Roll = 0;
            Int16 Gripper = 0;
            Int16 Nipper = 0;

            switch (JoystickDirection(values["IKYawIncrement"], values["IKPitchIncrement"]))
            {
                case JoystickDirections.Right:
                case JoystickDirections.Left:
                case JoystickDirections.Up:
                case JoystickDirections.Down:
                    Yaw = (Int16)(values["IKYawIncrement"] * MotorRangeFactor);
                    Pitch = (Int16)(values["IKPitchIncrement"] * MotorRangeFactor);
                    break;
                case JoystickDirections.None:
                    Yaw = 0;
                    Pitch = 0;
                    break;
            }

            switch (JoystickDirection(values["IKXIncrement"], values["IKYIncrement"]))
            {
                case JoystickDirections.Up:
                case JoystickDirections.Down:
                    Y = 0;
                    X = (Int16)(values["IKXIncrement"] * MotorRangeFactor);
                    break;
                case JoystickDirections.Right:
                case JoystickDirections.Left:
                    Y = (Int16)(values["IKYIncrement"] * MotorRangeFactor);
                    X = 0;
                    break;
                case JoystickDirections.None:
                    Y = 0;
                    X = 0;
                    break;
            }


            Z = (Int16)(ControllerBase.TwoButtonToggleDirection(values["IKZDirection"] != 0, (values["IKZMagnitude"])) * MotorRangeFactor);
            Roll = (Int16)(ControllerBase.TwoButtonToggleDirection(values["IKRollDirection"] != 0, (values["IKRollMagnitude"])) * MotorRangeFactor);
            Gripper = (Int16)(ControllerBase.TwoButtonTransform(values["GripperClose"] > 0, values["GripperOpen"] > 0, values["GripperClose"], -values["GripperOpen"], 0) * MotorRangeFactor);
            Nipper = (Int16)(ControllerBase.TwoButtonTransform(values["NipperClose"] > 0, values["NipperOpen"] > 0, values["NipperClose"], -values["NipperOpen"], 0) * MotorRangeFactor);


            Int16[] sendValues = { X, Y, Z, Yaw, Pitch, Roll, Gripper, Nipper };
            byte[] data = new byte[sendValues.Length * sizeof(Int16)];
            Buffer.BlockCopy(sendValues, 0, data, 0, data.Length);

            if (stateToUse == ArmControlState.IKWristPOV)
            {
                _rovecomm.SendCommand(_idResolver.GetId("IKWristIncrement"), data);
            }
            else if (stateToUse == ArmControlState.IKRoverPOV)
            {
                _rovecomm.SendCommand(_idResolver.GetId("IKRoverIncrement"), data);
            }
        }

        private void UpdateControlState(Dictionary<string, float> values)
        {
            ArmControlState oldState = myState;
            string state = "";

            if (values["UseAngular"] != 0)
            {
                myState = ArmControlState.GuiControl;
                state = "GUI control";
            }
            else if (values["UseOpenLoop"] != 0)
            {
                myState = ArmControlState.OpenLoop;
                state = "Open loop";
            }
            else if (values["UseRoverPerspectiveIK"] != 0)
            {
                myState = ArmControlState.IKRoverPOV;
                state = "Rover perspective IK";
            }
            else if (values["UseWristPerspectiveIK"] != 0)
            {
                myState = ArmControlState.IKWristPOV;
                state = "Wrist perspective IK";
            }

            if (oldState != myState)
            {
                _rovecomm.SendCommand(_idResolver.GetId("ArmStop"), (Int16)(0));
                ControlState = state;
            }
        }

        public void SetValues(Dictionary<string, float> values)
        {
            UpdateControlState(values);

            if (myState == ArmControlState.OpenLoop)
            {
                SetOpenLoopValues(values);
            }
            else if (myState == ArmControlState.GuiControl)
            {
                //controller does nothing in this state besides checking to see if you wanna switch states; otherwise user just
                //uses the gui to input commands
            }
            else if (myState == ArmControlState.IKRoverPOV || myState == ArmControlState.IKWristPOV)
            {
                SetIKValues(values, myState);
            }
        }

        public void StopMode()
        {
            _rovecomm.SendCommand(_idResolver.GetId("ArmStop"), (Int16)(0), true);
            _rovecomm.SendCommand(_idResolver.GetId("Endeffector1"), (Int16)(0), true);
            _rovecomm.SendCommand(_idResolver.GetId("Endeffector2"), (Int16)(0), true);

            myState = ArmControlState.GuiControl;
            ControlState = "GUI control";
        }

        public void EnableCommand(string bus, bool enableState)
        {
            ushort id;
            switch (bus)
            {
                case "All": id = _idResolver.GetId("ArmEnableAll"); break;
                case "Main": id = _idResolver.GetId("ArmEnableMain"); break;
                case "J1": id = _idResolver.GetId("ArmEnableJ1"); break;
                case "J2": id = _idResolver.GetId("ArmEnableJ2"); break;
                case "J3": id = _idResolver.GetId("ArmEnableJ3"); break;
                case "J4": id = _idResolver.GetId("ArmEnableJ4"); break;
                case "J5": id = _idResolver.GetId("ArmEnableJ5"); break;
                case "J6": id = _idResolver.GetId("ArmEnableJ6"); break;
                case "Endeff1": id = _idResolver.GetId("ArmEnableEndeff1"); break;
                case "Endeff2": id = _idResolver.GetId("ArmEnableEndeff2"); break;
                default: return;
            }

            _rovecomm.SendCommand(id, (enableState) ? ArmEnableCommand : ArmDisableCommand, true);
        }

        public void GetPosition()
        {
            _rovecomm.SendCommand(_idResolver.GetId("ArmGetPosition"), new byte[0], true);
        }
        public void SetPosition()
        {
            float[] angles = { AngleJ1, AngleJ2, AngleJ3, AngleJ4, AngleJ5, AngleJ6 };
            byte[] data = new byte[angles.Length * sizeof(float)];
            Buffer.BlockCopy(angles, 0, data, 0, data.Length);
            _rovecomm.SendCommand(_idResolver.GetId("ArmAbsoluteAngle"), data, true);
        }

        public void SetXYZPosition()
        {
            float[] coordinates = { CoordinateX, CoordinateY, CoordinateZ, Yaw, Pitch, Roll };
            byte[] data = new byte[coordinates.Length * sizeof(float)];
            Buffer.BlockCopy(coordinates, 0, data, 0, data.Length);
            _rovecomm.SendCommand(_idResolver.GetId("ArmAbsoluteXYZ"), data, true);
        }

        public void LimitSwitchOverride(byte index)
        {
            _rovecomm.SendCommand(_idResolver.GetId("LimitSwitchOverride"), index, true);
        }
        public void LimitSwitchUnOverride(byte index)
        {
            _rovecomm.SendCommand(_idResolver.GetId("LimitSwitchUnOverride"), index, true);
        }

        public void RecallPosition()
        {
            AngleJ1 = SelectedPosition.J1;
            AngleJ2 = SelectedPosition.J2;
            AngleJ3 = SelectedPosition.J3;
            AngleJ4 = SelectedPosition.J4;
            AngleJ5 = SelectedPosition.J5;
            AngleJ6 = SelectedPosition.J6;
        }
        public void StorePosition()
        {
            Positions.Add(new ArmPositionViewModel()
            {
                Name = "Unnamed Position",
                J1 = AngleJ1,
                J2 = AngleJ2,
                J3 = AngleJ3,
                J4 = AngleJ4,
                J5 = AngleJ5,
                J6 = AngleJ6
            });
        }
        public void DeletePosition()
        {
            Positions.Remove(SelectedPosition);
        }
        public void SaveConfigurations()
        {
            _configManager.SetConfig(PositionsConfigName, new ArmPositionsContext(Positions.Select(x => x.GetContext()).ToArray()));
        }
        public void InitializePositions(ArmPositionsContext config)
        {
            foreach (var position in config.Positions)
                Positions.Add(new ArmPositionViewModel(position));
        }

        private JoystickDirections JoystickDirection(float y, float x)
        {
            if (x == 0.0f && y == 0.0f) return JoystickDirections.None;

            var angle = Math.Atan2(y, x);
            if (angle > -Math.PI / 6 && angle < Math.PI / 6)
                return JoystickDirections.Right;
            else if (angle > Math.PI / 3 && angle < 2 * Math.PI / 3)
                return JoystickDirections.Up;
            else if (angle > 5 * Math.PI / 6 || angle < -5 * Math.PI / 6)
                return JoystickDirections.Left;
            else if (angle > -2 * Math.PI / 3 && angle < Math.PI / 3)
                return JoystickDirections.Down;
            else
                return JoystickDirections.None;
        }

        private enum JoystickDirections
        {
            None = 0,
            Left,
            Right,
            Up,
            Down
        }

        public class ArmPositionViewModel : PropertyChangedBase
        {
            private readonly ArmModel.ArmPositionModel _model;

            public string Name
            {
                get
                {
                    return _model.Name;
                }
                set
                {
                    _model.Name = value; NotifyOfPropertyChange(() => Name);
                }

            }
            public float J1
            {
                get
                {
                    return _model.J1;
                }
                set
                {
                    _model.J1 = value; NotifyOfPropertyChange(() => J1);
                }

            }
            public float J2
            {
                get
                {
                    return _model.J2;
                }
                set
                {
                    _model.J2 = value; NotifyOfPropertyChange(() => J2);
                }

            }
            public float J3
            {
                get
                {
                    return _model.J3;
                }
                set
                {
                    _model.J3 = value; NotifyOfPropertyChange(() => J3);
                }

            }
            public float J4
            {
                get
                {
                    return _model.J4;
                }
                set
                {
                    _model.J4 = value; NotifyOfPropertyChange(() => J4);
                }

            }
            public float J5
            {
                get
                {
                    return _model.J5;
                }
                set
                {
                    _model.J5 = value; NotifyOfPropertyChange(() => J5);
                }

            }
            public float J6
            {
                get
                {
                    return _model.J6;
                }
                set
                {
                    _model.J6 = value; NotifyOfPropertyChange(() => J6);
                }

            }

            public ArmPositionViewModel()
            {
                _model = new ArmModel.ArmPositionModel();
            }

            public ArmPositionViewModel(ArmPositionContext ctx)
                : this()
            {
                Name = ctx.Name;
                J1 = ctx.J1;
                J2 = ctx.J2;
                J3 = ctx.J3;
                J4 = ctx.J4;
                J5 = ctx.J5;
                J6 = ctx.J6;
            }

            public ArmPositionContext GetContext()
            {
                return new ArmPositionContext(Name, J1, J2, J3, J4, J5, J6);
            }
        }
    }
}