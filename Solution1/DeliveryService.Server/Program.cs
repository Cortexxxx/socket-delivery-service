using System.Reflection;
using DeliveryService.Protocol;
using DeliveryService.Protocol.Logger;
using DeliveryService.Protocol.Map;
using DeliveryService.Protocol.Messages;
using DeliveryService.Server;

var udpTransport = new UdpTransport(ServerConfig.ServerPort, 
    new PacketCodec(new MessagesRegistry(Assembly.GetAssembly(typeof(DriveMessage)))),
    new HexLogger(LoggerFilters.None));

var serverSender = new ServerSender(udpTransport);
var serverSessions = new ServerSessions();

var mapper = new FileMap(ServerConfig.MapPath, ServerConfig.MapObstacleChar, ServerConfig.MapStorageChar);
var tryLoadMap = mapper.TryLoad(out var map, out var storagePoint);
var tryGetBitmask = MapMessage.TryGetBitmaskFromBoolMatrix(map, out var bitmaskMap);

if (!tryLoadMap || !tryGetBitmask || map == null || bitmaskMap == null || storagePoint == null)
{
    Console.WriteLine("Map was not loaded properly!");
    return;
}

var mapMessage = new MapMessage
{
    Width = (byte)map.GetLength(1),
    Height = (byte)map.GetLength(0),
    Bitmask = bitmaskMap,
    StoragePoint = (Point<byte>)storagePoint
};

var messagesDispatcher = new MessagesDispatcher(new ServerHandlers(serverSessions, serverSender, mapMessage));



var serverApp = new ServerApp(udpTransport, serverSender, serverSessions, messagesDispatcher, map);

udpTransport.Start();
serverApp.Run();