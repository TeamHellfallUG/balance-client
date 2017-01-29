# balance-client
- optimised for Unity3D builds with .NET 2.0 / Mono

# about
- Scalable Game Server Framework
- Interchangeable Protocols (currently UDP and TCP(WebSockets))
- Adds complexity via inheritance
- This lib also ships a HttpClient with an API similar to "request.js"

# multiple functional server layers
- Basis Layer (Scalable Instances via Redis Messages)
- Simple JSON Layer
- Group Layer
- Vector (3D/2D Position) Layer
- Room Layer + Matchmaking

# scale tools
- WebSocket Loadbalancer
- UDP Loadbalancer

# how to use
- git clone this repo
- open ./*.sln in MonoDevelop or Visual Studio
- build a Release
- copy all *.dlls (should be 5) from ./bin/Release into your Unity /Assets folder
- add "using Balance".. to your scripts (checkout Example Project for usage)
- checkout "balance-engine" to build a backend for your client

# other info
- some parts are still wip
- this is the client library, the server is available under "krystianity/balance-engine"

# used libraries
- lidgren network gen3 under MIT license (c) 2015 lidgren
- newtonsoft.json under MIT license (c) 2007 James Newton-King
- betterhttpclient (adapted) no license provided (c) 2016 Yozer
- websocket-sharp under MIT license (c) 2010-2017 sta.blockhead
