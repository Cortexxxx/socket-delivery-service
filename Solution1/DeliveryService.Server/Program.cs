using DeliveryService.Protocol.Map;
using DeliveryService.Protocol.Messages;
using DeliveryService.Protocol.Network;
using DeliveryService.Server;

var serverSessions = new ServerSessions();
var ordersService = new OrdersService();
var mapper = new FileMap(ServerConfig.MapPath, ServerConfig.MapObstacleChar, ServerConfig.MapStorageChar);
var tryLoadMap = mapper.TryLoad(out var map, out var storagePoint);
var tryGetBitmask = MapMessage.TryGetBitmaskFromBoolMatrix(map, out var bitmaskMap);

if (!tryLoadMap || !tryGetBitmask || map == null || bitmaskMap == null || storagePoint == null)
{
    Console.WriteLine("Map was not loaded properly!");
    return;
}

var mapState = new MapState(map, bitmaskMap, storagePoint);

var node = NetworkNodeFactory.Create(NetworkConfig.ServerPort,
    sender => new ServerHandlers(serverSessions, sender, mapState, ordersService));

var serverApp = new ServerApp(node.Transport, node.Sender, serverSessions, node.ReceiveProcessor, node.ReliabilityLayer);

node.Transport.Start();
serverApp.Run();