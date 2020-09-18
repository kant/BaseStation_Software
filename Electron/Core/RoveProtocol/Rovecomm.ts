import { DATAID } from "./RovecommManifest"
/* eslint-disable @typescript-eslint/no-var-requires */
const dgram = require("dgram")
const net = require("net")

function TCPListen(socket: any) {
  socket.on("data", (data: any) => {
    console.log(data, data.toString())
  })
}

class Rovecomm {
  UDPSocket: any

  TCPServer: any

  constructor() {
    this.UDPSocket = dgram.createSocket("udp4")
    this.TCPServer = net.createServer((TCPSocket: any) => TCPListen(TCPSocket))

    this.UDPListen()
    this.TCPServer.listen(8080)
  }

  UDPListen() {
    this.UDPSocket.on(
      "message",
      (msg: string, rinfo: { address: string; port: number }) => {
        console.log(`server got: ${msg} from ${rinfo.address}:${rinfo.port}`)
      }
    )
    this.UDPSocket.bind(8081)
  }
}

export function parse(part: string, packet: Uint8Array): any {
  const VersionNumber = 2

  enum DataTypes {
    INT8_T = 0,
    UINT8_T = 1,
    INT16_T = 2,
    UINT16_T = 3,
    INT32_T = 4,
    UINT32_T = 5,
    FLOAT_T = 6,
  }

  const sizes = [1, 1, 2, 2, 4, 4, 4]

  const version = packet[0]
  // eslint-disable-next-line no-bitwise
  const dataId = (packet[1] << 8) | packet[2]
  const dataType = packet[3]
  const dataLength = packet[4]

  const rawdata = packet.slice(5, packet.length - 5)
  const data: any = []

  if (version === VersionNumber) {
    console.log(dataId)
    switch (part) {
      case "dataId":
        return DATAID[dataId]
      case "dataType":
        return DataTypes[dataType]
      case "dataLength":
        return dataLength
      case "data":
        return data
      default:
        return "null"
    }
  } else {
    return "null"
  }
}

export const rovecomm = new Rovecomm()
