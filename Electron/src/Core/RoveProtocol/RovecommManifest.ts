/* The manifest follows the following format:
 * DATAID = {
 *   Name: {
 *     Ip: "192.168.1.___",
 *     Port: 110__,
 *     Commands: {
 *       DataIdString: {
 *         dataId: ____, (number)
 *         dataType: ____, (Enumeration declared below)
 *         comments: "General description of command",
 *       },
 *     }
 *     Telemetry: {
 *        *follows same format as commands*
 *     },
 *   },
 * }
 */

// Enumeration of all datatypes supported by Basestation Rovecomm
export enum DataTypes {
  INT8_T = 0,
  UINT8_T = 1,
  INT16_T = 2,
  UINT16_T = 3,
  INT32_T = 4,
  UINT32_T = 5,
  FLOAT_T = 6,
  DOUBLE_T = 7,
  CHAR = 8,
}

export enum SystemPackets {
  PING = 1,
  PING_REPLY = 2,
  SUBSCRIBE = 3,
  UNSUBSCRIBE = 4,
  INVALID_VERSION = 5,
  NO_DATA = 6,
}

// The header length is currently 5 Bytes, stored here for better code in Rovecomm.ts
export const headerLength = 5

// Data sizes of the corresponding datatype enumeration
export const dataSizes = [1, 1, 2, 2, 4, 4, 4]

export const RovecommManifest = {
  Drive: {
    Ip: "192.168.1.134",
    Port: 11004,
    Commands: {
      DriveLeftRight: {
        dataId: 1000,
        dataType: DataTypes.INT16_T,
        dataCount: 2,
        comments: "[LeftSpeed, RightSpeed] (-1000, 1000)-> (-100%, 100%)",
      },
      DriveIndividual: {
        dataId: 1001,
        dataType: DataTypes.INT16_T,
        dataCount: 4,
        comments: "[LF, LR, RF, RR] (-1000, 1000)-> (-100%, 100%)",
      },
      SetSteeringAngle: {
        dataId: 1002,
        dataType: DataTypes.INT16_T,
        dataCount: 4,
        comments: "[LF, LR, RF, RR] (0, 359)",
      },
      PointTurn: {
        dataId: 1003,
        dataType: DataTypes.INT16_T,
        dataCount: 1,
        comments: "[PointTurnSpeed] (-1000,1000) (Full speed CCW, full speed CW)",
      },
      WatchdogOverride: {
        dataId: 1004,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[0-Turn off Watchdog Override, 1-Turn on Watchdog Override]",
      },
    },
    Telemetry: {
      DriveSpeeds: {
        dataId: 1100,
        dataType: DataTypes.INT16_T,
        dataCount: 4,
        comments: "[LF, LR, RF, RR] (-1000, 1000)-> (-100%, 100%)",
      },
      DriveAngles: {
        dataId: 1101,
        dataType: DataTypes.INT16_T,
        dataCount: 4,
        comments: "[LF, LR, RF, RR] -> (0, 360)",
      },
      SteeringMotorCurrents: {
        dataId: 1102,
        dataType: DataTypes.FLOAT_T,
        dataCount: 4,
        comments: "[M1, M2, M3, M4] (A)",
      },
    },
    Error: {
      SteeringMotorOverCurrent: {
        dataId: 1200,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[(0-undermaxcurrent, 1-overcurrent)] [LF, LR, RF, RL (Bitmask)]",
      },
    },
  },
  BMS: {
    Ip: "192.168.1.133",
    Port: 11003,
    Commands: {
      BMSStop: {
        dataId: 2000,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[delay] (s) -> a delay of 0 will shutdown, not restart and cannot be reversed",
      },
    },
    Telemetry: {
      PackI_Meas: {
        dataId: 2100,
        dataType: DataTypes.FLOAT_T,
        dataCount: 1,
        comments: "[Main] (A)",
      },
      PackV_Meas: {
        dataId: 2101,
        dataType: DataTypes.FLOAT_T,
        dataCount: 1,
        comments: "[Pack_Out] (V)",
      },
      CellV_Meas: {
        dataId: 2102,
        dataType: DataTypes.FLOAT_T,
        dataCount: 8,
        comments: "[C1-G, C2-1, C3-2, C4-3, C5-4, C6-5, C7-6, C8-7] (V)",
      },
      Temp_Meas: {
        dataId: 2103,
        dataType: DataTypes.FLOAT_T,
        dataCount: 1,
        comments: "[Temp] (degC)",
      },
      Error: {
        PackOverCurrent: {
          dataId: 2200,
          dataType: DataTypes.UINT8_T,
          dataCount: 1,
          comments: "",
        },
        CellUnderVoltage: {
          dataId: 2201,
          dataType: DataTypes.UINT8_T,
          dataCount: 1,
          comments: "(bitmasked)",
        },
        PackUnderVoltage: {
          dataId: 2202,
          dataType: DataTypes.UINT8_T,
          dataCount: 1,
          comments: "",
        },
        PackSuperHot: {
          dataId: 2203,
          dataType: DataTypes.UINT8_T,
          dataCount: 1,
          comments: "",
        },
      },
    },
  },
  Power: {
    Ip: "192.168.1.132",
    Port: 11002,
    Commands: {
      MotorBusEnable: {
        dataId: 3000,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[(0-Disable, 1-Enable)], [M1, M2, M3, M4, Spare (Bitmask)]",
      },
      "12VActBusEnable": {
        dataId: 3001,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[(0-Disable, 1-Enable)], [Gimbal, Multi, Aux (Bitmask)]",
      },
      "12VLogicBusEnable": {
        dataId: 3002,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[(0-Disable, 1-Enable)], [Gimbal, Multi, Aux, Drive, Nav, Cam, Extra (Bitmask)]",
      },
      "30VBusEnabled": {
        dataId: 3003,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[(0-Disable, 1-Enable)], [12V, Rockets, Aux, Drive (Bitmask)]",
      },
      VacuumEnabled: {
        dataId: 3004,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[(0-Disable, 1-Enable)]",
      },
    },
    Telemetry: {
      MotorBusEnabled: {
        dataId: 3100,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[(0-Disabled, 1-Enabled)],[M1, M2, M3, M4, Spare(Bitmask)]",
      },
      "12VActBusEnabled": {
        dataId: 3101,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[(0-Disable, 1-Enable)], [Gimbal, Multi, Aux (Bitmask)]",
      },
      "12VLogicBusEnabled": {
        dataId: 3102,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[(0-Disable, 1-Enable)], [Gimbal, Multi, Aux, Drive, Nav, Cam, Extra (Bitmask)]",
      },
      ThirtyVEnabled: {
        dataId: 3103,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[(0-Disable, 1-Enable)], [12V, Rockets, Aux, Drive (Bitmask)]",
      },
      VacuumEnabled: {
        dataId: 3104,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[(0-Disabled, 1-Enabled)]",
      },
      MotorBusCurrent: {
        dataId: 3105,
        dataType: DataTypes.FLOAT_T,
        dataCount: 5,
        comments: "[M1, M2, M3, M4, Spare] (A)",
      },
      "12VBusCurrent": {
        dataId: 3106,
        dataType: DataTypes.FLOAT_T,
        dataCount: 4,
        comments: "[Gimbal, Multi, Aux, Logic] (A)",
      },
      "30VBusCurrent": {
        dataId: 3107,
        dataType: DataTypes.FLOAT_T,
        dataCount: 4,
        comments: "[12V Board, Rockets, Aux, Drive] (A)",
      },
      VacuumCurrent: {
        dataId: 3108,
        dataType: DataTypes.FLOAT_T,
        dataCount: 1,
        comments: "[Vacuum] (A)",
      },
    },
    Error: {
      MotorBusOverCurrent: {
        dataId: 3200,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[(0-undermaxcurrent, 1-overcurrent)] [M1, M2, M3, M4, Spare (Bitmask)]",
      },
      "12VBusOverCurrent": {
        dataId: 3201,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[(0-undermaxcurrent, 1-overcurrent)] [Gimbal, Multi, Aux, Logic (Bitmask)]",
      },
      "30VBusOverCurrent": {
        dataId: 3202,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[(0-undermaxcurrent, 1-overcurrent)] [12V Board, Rockets, Aux, Drive (Bitmask)]",
      },
      VaccuumOverCurrent: {
        dataId: 3203,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[(0-undermaxcurrent, 1-overcurrent)] [Vacuum]",
      },
    },
  },
  Nav: {
    Ip: "192.168.1.136",
    Port: 11006,
    Commands: {},
    Telemetry: {
      GPSLatLon: {
        dataId: 5100,
        dataType: DataTypes.DOUBLE_T,
        dataCount: 2,
        comments: "[Lat, Long] [(-90, 90), (-180, 180)] (deg)",
      },
      IMUData: {
        dataId: 5101,
        dataType: DataTypes.FLOAT_T,
        dataCount: 3,
        comments: "[Pitch, Yaw, Roll] [(-90, 90), (0, 360), (-90, 90)] (deg)",
      },
      LidarData: {
        dataId: 5101,
        dataType: DataTypes.FLOAT_T,
        dataCount: 2,
        comments: "[Distance, Quality]",
      },
    },
    Error: {
      dataId: 5200,
      dataType: DataTypes.UINT8_T,
      dataCount: 1,
      comments: "",
    },
  },
  Gimbal: {
    Ip: "192.168.1.135",
    Port: 11005,
    Commands: {
      LeftDriveGimbalIncrement: {
        dataId: 6000,
        dataType: DataTypes.INT16_T,
        dataCount: 2,
        comments: "[Pan, Tilt](degrees 0-270)",
      },
      RightDriveGimbalIncrement: {
        dataId: 6001,
        dataType: DataTypes.INT16_T,
        dataCount: 2,
        comments: "[Pan, Tilt](degrees 0-270)",
      },
      LeftMainGimbalIncrement: {
        dataId: 6002,
        dataType: DataTypes.INT16_T,
        dataCount: 2,
        comments: "[Pan, Tilt](degrees 0-270)",
      },
      RightMainGimbalIncrement: {
        dataId: 6003,
        dataType: DataTypes.INT16_T,
        dataCount: 2,
        comments: "[Pan, Tilt](degrees 0-270)",
      },
      LeftDriveGimbalAbsolute: {
        dataId: 6004,
        dataType: DataTypes.INT16_T,
        dataCount: 2,
        comments: "[Pan, Tilt](degrees 0-270)",
      },
      RightDriveGimbalAbsolute: {
        dataId: 6005,
        dataType: DataTypes.INT16_T,
        dataCount: 2,
        comments: "[Pan, Tilt](degrees 0-270)",
      },
      LeftMainGimbalAbsolute: {
        dataId: 6006,
        dataType: DataTypes.INT16_T,
        dataCount: 2,
        comments: "[Pan, Tilt](degrees 0-270)",
      },
      RightMainGimbalAbsolute: {
        dataId: 6007,
        dataType: DataTypes.INT16_T,
        dataCount: 2,
        comments: "[Pan, Tilt](degrees 0-270)",
      },
      InitiateTestRoutine: {
        dataId: 6008,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "",
      },
    },
    Telemetry: {
      ServoPosition: {
        dataId: 6100,
        dataType: DataTypes.INT16_T,
        dataCount: 8,
        comments: "Array of 8 servo positions",
      },
    },
  },
  Multimedia: {
    Ip: "192.168.1.140",
    Port: 11010,
    Commands: {
      HeadlightIntensity: {
        dataId: 7000,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "Headlight intensity for the front of rover",
      },
      LEDRGB: {
        dataId: 7001,
        dataType: DataTypes.UINT8_T,
        dataCount: 3,
        comments: "[R, G, B] (0, 255)",
      },
      LEDPatterns: {
        dataId: 7002,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[Pattern] (Enum)",
      },
      StateDisplay: {
        dataId: 7003,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[Teleop, Autonomy, Reached Goal] (enum)",
      },
    },
    Telemetry: {},
    Error: {},
  },
  Arm: {
    Ip: "192.168.1.131",
    Port: 11001,
    Commands: {
      ArmVelocityControl: {
        dataId: 8000,
        dataType: DataTypes.INT16_T,
        dataCount: 6,
        comments: "[J1, J2, J3, J4, J5, J6] (rpm)",
      },
      ArmMoveToPosition: {
        dataId: 8001,
        dataType: DataTypes.FLOAT_T,
        dataCount: 6,
        comments: "[J1, J2, J3, J4, J5, J6] (Degrees)",
      },
      ArmIncrementPosition: {
        dataId: 8002,
        dataType: DataTypes.FLOAT_T,
        dataCount: 6,
        comments: "[J1, J2, J3, J4, J5, J6] (Degrees)",
      },
      ArmMoveIK: {
        dataId: 8003,
        dataType: DataTypes.FLOAT_T,
        dataCount: 6,
        comments: "[X, Y, Z, Y, P, R] (in)",
      },
      ArmIncrementIKRover: {
        dataId: 8004,
        dataType: DataTypes.FLOAT_T,
        dataCount: 6,
        comments: "[X, Y, Z, Y, P, R] (in)",
      },
      ArmIncrementIKWrist: {
        dataId: 8005,
        dataType: DataTypes.FLOAT_T,
        dataCount: 6,
        comments: "[X, Y, Z, Y, P, R] (in)",
      },
      SetClosedLoopState: {
        dataId: 8006,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "0-Disable Closed Loop, 1-Enable Closed Loop",
      },
      GripperMove: {
        dataId: 8010,
        dataType: DataTypes.INT16_T,
        dataCount: 1,
        comments: "[Power] (-1000, 1000) (m%)",
      },
      WatchdogOverride: {
        dataId: 8011,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[0-Turn off Watchdog Override, 1-Turn on Watchdog Override]",
      },
      LimitSwitchOverride: {
        dataId: 8012,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments:
          "[Base Tilt Up, Base Tilt Down, Base Twist CW, Base Twist CCW, Elbow Tilt Up, Elbow Tilt Down, Elbow  Twist CW, Elbow  Twist CCW] (0-Turn off Limit Switch Override, 1-Turn on Limit Switch Override) (bitmasked)",
      },
      RebootODrive: {
        dataId: 8013,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[1-Base, 2-Elbow, 3-Wrist]",
      },
      RequestJointPositions: {
        dataId: 8014,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "Prompt arm for J1-6 positions",
      },
      TogglePositionTelem: {
        dataId: 8015,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "Start auto pushing arm J1-6 positions",
      },
      RequestAxesPositions: {
        dataId: 8016,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "Prompt arm for XYZPYR Data",
      },
    },
    Telemetry: {
      MotorCurrents: {
        dataId: 8100,
        dataType: DataTypes.FLOAT_T,
        dataCount: 6,
        comments: "[M1, M2, M3, M4, M5, M6] (0, A)",
      },
      JointAngles: {
        dataId: 8101,
        dataType: DataTypes.FLOAT_T,
        dataCount: 6,
        comments: "[J1, J2, J3, J4, J5, J6] (0, Deg)",
      },
      MotorVelocities: {
        dataId: 8102,
        dataType: DataTypes.FLOAT_T,
        dataCount: 6,
        comments: "[J1, J2, J3, J4, J5, J6] (0, rpm)",
      },
      IKCoordinates: {
        dataId: 8103,
        dataType: DataTypes.FLOAT_T,
        dataCount: 6,
        comments: "[X, Y, Z, Y, P, R]",
      },
    },
    Error: {
      WatchDogStatus: {
        dataId: 8200,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[WatchDogStatus] (0-WD Not Triggered, 1-WD Triggered) ",
      },
      EncoderStatus: {
        dataId: 8201,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[E1, E2, E3, E4, E5, E6] (0-Good, 1-Failure)",
      },
      ODriveError: {
        dataId: 8202,
        dataType: DataTypes.UINT8_T,
        dataCount: 3,
        comments: "[Motor][Error Type][Error Specific]",
      },
    },
  },
  ScienceActuation: {
    Ip: "192.168.1.137",
    Port: 11007,
    Commands: {
      ZAxis: {
        dataId: 9000,
        dataType: DataTypes.INT16_T,
        dataCount: 1,
        comments: "[Power] (-1000, 1000) (m%)",
      },
      GenevaOpenLoop: {
        dataId: 9001,
        dataType: DataTypes.INT16_T,
        dataCount: 1,
        comments: "[Power] (-1000, 1000) (m%)",
      },
      Chemicals: {
        dataId: 9002,
        dataType: DataTypes.UINT16_T,
        dataCount: 3,
        comments: "[Chemical 1, Chemical 2, Chemical 3] (0, 1000) (m%)",
      },
      GenevaToPosition: {
        dataId: 9003,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[absolute position]",
      },
      GenevaIncrementPosition: {
        dataId: 9004,
        dataType: DataTypes.INT8_T,
        dataCount: 1,
        comments: "[relative position]",
      },
      LimitSwitchOverride: {
        dataId: 9005,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments:
          "[Z-axis Top, Z-axis Bottom, Geneva Set, Geneva Home] (0-Turn off Limit Switch Override, 1-Turn on Limit Switch Override) (bitmasked)",
      },
      MixerVelocity: {
        dataId: 9006,
        dataType: DataTypes.INT16_T,
        dataCount: 4,
        comments: "[Power] (-1000, 1000) (m%)",
      },
    },
    Telemetry: {
      GenevaCurrentPosition: {
        dataId: 9100,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[absolute position]",
      },
      LimitSwitchTriggered: {
        dataId: 9101,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[Z-axis Top, Z-axis Bottom, Geneva Set, Geneva Home] (bitmasked)",
      },
    },
    Error: {},
  },
  ScienceSensors: {
    Ip: "192.168.1.138",
    Port: 11008,
    Commands: {
      UVLedControl: {
        dataId: 10000,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "[(LED On = 1/LED Off = 0)]",
      },
      RunSpectrometer: {
        dataId: 10001,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "",
      },
    },
    Telemetry: {
      SpectrometerData: {
        dataId: 10100,
        dataType: DataTypes.UINT16_T,
        dataCount: 144,
        comments: "Sends half of the spectrum read",
      },
      Methane: {
        dataId: 10101,
        dataType: DataTypes.FLOAT_T,
        dataCount: 2,
        comments: "[Gass concentration %, Temperature (C)]",
      },
      CO2: {
        dataId: 10102,
        dataType: DataTypes.UINT16_T,
        dataCount: 1,
        comments: "[CO2 Concentration (ppm)]",
      },
      O2: {
        dataId: 10103,
        dataType: DataTypes.FLOAT_T,
        dataCount: 4,
        comments: "[partial pressure, (mBar), temperature (C), concentration (ppm), barometric pressue (mBar)]",
      },
      NO: {
        dataId: 10104,
        dataType: DataTypes.FLOAT_T,
        dataCount: 1,
        comments: "",
      },
      N20: {
        dataId: 10105,
        dataType: DataTypes.UINT16_T,
        dataCount: 1,
        comments: "[ N2O volume (ppm)]",
      },
    },
    Error: {},
  },
  Autonomy: {
    Ip: "192.168.1.139",
    Port: 11009,
    Commands: {
      StartAutonomy: {
        dataId: 11000,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "",
      },
      DisableAutonomy: {
        dataId: 11001,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "",
      },
      AddWaypoints: {
        dataId: 11002,
        dataType: DataTypes.DOUBLE_T,
        dataCount: 2,
        comments: "[Lat, Lon]",
      },
      ClearWaypoints: {
        dataId: 11003,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "",
      },
    },
    Telemetry: {
      CurrentState: {
        dataId: 11100,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "Enum (Idle, Navigating, SearchPattern, Approaching Marker)",
      },
      ReachedMarker: {
        dataId: 11101,
        dataType: DataTypes.UINT8_T,
        dataCount: 1,
        comments: "",
      },
    },
    Error: {
      CurrentLog: {
        dataId: 11200,
        dataType: DataTypes.CHAR,
        dataCount: 255,
        comments: "String version of most current error log",
      },
    },
  },
  Camera1: {
    Ip: "192.168.1.141",
    Port: 11011,
    Commands: {},
    Telemetry: {},
    Error: {},
  },
  Camera2: {
    Ip: "192.168.1.142",
    Port: 11012,
    Commands: {},
    Telemetry: {},
    Error: {},
  },
}
export const NetworkDevices = {
  BasestationSwitch: { Ip: "192.168.1.80" },
  Rover900MHzRocket: { Ip: "192.168.1.82" },
  Basestation900MHzRocket: { Ip: "192.168.1.83" },
  Rover5GHzRocket: { Ip: "192.168.1.84" },
  Basestation5GHzRocket: { Ip: "192.168.1.85" },
  Rover2_4GHzRocket: { Ip: "192.168.1.86" },
  GrandStream: { Ip: "192.168.1.226" },
}
