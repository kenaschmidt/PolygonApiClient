# (Unofficial) Polygon C# Client - WebSocket & RESTful APIs

This is an UNOFFICIAL C# client library for the Polygon REST and WebSocket API.  Please visit [Polygon.IO](https://polygon.io/) for more information.

This library targets .NET Framework 4.8.1 right now because I don't know any better.

## General Architecture

Functionality is provided by a top-level `PolygonClient` class which interfaces with both a `PolygonRestClient` and multiple `PolygonSocketClient`s (one for each asset class endpoint)

Return data is represented by PolygonResponseModels which mirror JSON return data for the most part.

### PolygonClient

-Manages both REST and Socket interaction.
-Exposes all Polygon REST and Socket endpoints in the form of async methods. Parameters mirror REST parameters and return POCOs with matching definitions - though some parameter names have been modified for clarity.

*Currently only implements endpoints for STOCKS and OPTIONS - Indices and Currencies TBD

A `PolygonClient` is instantiated with an API Key and `PolygonSubscriptionSettings` object, which contains values reflecting permissions/limitations based on the user's subscription plan.

### REST Calls

REST calls vary slightly between calls due to v1/v2/v3 endpoints.

REST calls have 1:1 matching async methods which return objects mirroring the JSON responses send by Polygon.

Most calls return a Rest_Response object which contains an array of Rest_Result (specific to the data type being returned). Older calls return results in the top-level object, and a few return 'special' formats which are handled by 'special'
 handlers... unfortunately.

Top-level client methods normally return a *Result or *Result[].

### Socket Calls

Socket calls return a `SocketHandler` object which provides a security-specific link to streaming socket events (Aggregates, Trades, Quotes). 

## Update Notes

### 1/3/24

-`PolygonClient` Base client functionality is mostly complete and tested. Can successfully pull down all REST requests for Stocks and Options (Haven't touched indices or currencies yet).
-Started working on an `ExtendedClient` that will provide a more intuitive interface with the return data. This introduces some new models (`Security`,`Tick`,`OptionChain` etc) and some data structures to store and manage/analyze. This namespace will be more experimental and obviously the base client will function independently.

### 1/5/24

-Have been working on an `ExtendedClient` which will implement a more user-friendly interface, with a set of models that more closely reflect real-work use cases. No set roadmap for this namespace right now and it's not required for base client functionality, though ome of the base client is being modified to better allow extended features (additional interfaces, etc).
