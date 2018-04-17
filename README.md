# Web Socket Unity demos

Web Socket samples for Unity:

* [Bing Speech API](https://portal.azure.com) web socket demo (requires Key)
* Echo message demo
* Transform JSON demo

## :octocat: Download instructions

This project contains git submodule dependencies so use:  
`git clone --recursive https://github.com/Unity3dAzure/UnityWebSocketDemo.git`

Or if you've already done a git clone then use:  
`git submodule update --init --recursive`

## Demos

### How to run the localhost Web Socket echo demo

1.  Clone the [Web Socket server repo](https://github.com/deadlyfingers/UnityWebSocket-server) and run the [echo server](https://github.com/deadlyfingers/UnityWebSocket-server/blob/master/server-echo.js).

`node ./server-echo.js`

2.  Open Unity project's _Echo Demo_ scene in Unity and hit play!

### How to run the localhost Web Socket 3d transform JSON demo

1.  Clone the [Web Socket server repo](https://github.com/deadlyfingers/UnityWebSocket-server) and run the [json server](https://github.com/deadlyfingers/UnityWebSocket-server/blob/master/server-json.js).

`node ./server-json.js`

2.  Open Unity project's _My Transform Demo_ scene in Unity and hit play!

## Dependencies

* [UnityWebSocket](https://github.com/Unity3DAzure/UnityWebSocket)
  * [WebSocket-Sharp](https://github.com/deadlyfingers/websocket-sharp)

Refer to the download instructions above to install these submodules.

Questions or tweet #Azure #GameDev @deadlyfingers
