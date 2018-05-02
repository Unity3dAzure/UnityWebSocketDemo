# Web Socket Unity demos

### Azure Web Socket services for Unity

* [Bing Speech](https://portal.azure.com) web socket demo (requires Key)
* [Bing Speech](https://portal.azure.com) and [LUIS app](https://www.luis.ai/applications) for using natural language to target and control Unity GameObject properties.
* [Bot Framework](https://portal.azure.com) [Direct Line](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-channel-connect-directline) web socket demo (requires Direct Line key)

### Localhost Web Socket samples for Unity

* Echo message demo
* Transform JSON demo

## :octocat: Download instructions

This project contains git submodule dependencies so use:  
`git clone --recursive https://github.com/Unity3dAzure/UnityWebSocketDemo.git`

Or if you've already done a git clone then use:  
`git submodule update --init --recursive`

## Setup instructions

### How to run the Bing Speech and LUIS Unity demo

1.  Create Bing Speech service in [Azure](https://portal.azure.com) and get a key for the Unity _BingSpeechService_ script.

2.  Sign-in to [LUIS](https://www.luis.ai) and go to [My apps](https://www.luis.ai/applications) section.

3.  Select **'Import new app'** and import the sample [UnityLUISApp.json](https://gist.github.com/deadlyfingers/66a4511163e399a02836bc2b420cb79b) file.

4.  Under 'Publish > Resources and Keys' section select **'Add Key'**

5.  Copy the key's **Endpoint** ending in 'q=' into the Unity _LUISApp_ script.

### How to run the Bot Framework demo

1. Create an [Azure Bot Service](https://portal.azure.com).

2. In the Azure Bot Service select **Channels** and add a [Direct Line](https://docs.microsoft.com/en-us/azure/bot-service/bot-service-channel-connect-directline) channel.

3. Copy the Direct Line secret key into the Unity _BotService_ script.

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
  * [WebSocket-Sharp* forked for supporting Unity and custom headers](https://github.com/deadlyfingers/websocket-sharp)

Refer to the download instructions above to install these submodules.

Questions or tweet #Azure #GameDev @deadlyfingers
