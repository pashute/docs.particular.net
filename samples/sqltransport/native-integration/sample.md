---
title: SQL Server
summary: 'How to integrate SQL Server transport with NHibernate persistence without outbox'
tags:
- SQL Server
- NHibernate
---

 1. Make sure you have SQL Server Express installed and accessible as `.\SQLEXPRESS`. Create a databases `samples`
 2. Start the Receiver project.
 3. In the Sender's console you should see `Press any key to send a message. Press `q` to quit` text when the app is ready. 
 4. Hit any key.
 5. A message will be sent using ADO.net and be received by the app.
 6. Open SQL Server Management Studio and go to the `samples` database.
 7. Open the Scripts.sql included in the sample
 7. Run the `SendFromTSQL` statement
 8. Notice how the app shows that a new message has been processed
 9. Create the `Orders` table using the `CreateLegacyTable` statement
 10. Create the insert trigger using the `CreateTrigger` statement
 11. Right click the table you just created and do `Edit top X rows`
 12. Notice that a message is received by the app for each "order" you create
 

## Code walk-through

The first thing you need to do when doing native integration with the SqlServer transport is figure out where to insert your "message". The database and server names can easily be found by looking at the connection string and the and the table name is by default the same as your endpoint name. So with the following endpoint configuration


<!-- import EndpointConfiguration-->


The table would be `Samples.SqlServer.NativeIntegration` in the database `samples` on server `.\SQLEXPRESS` (localhost). Now that we know where to put the data lets see how we can make sure that the endpoint can parse our message.

### Serialization

In this sample we'll be using JSON to serialize the messages but XML would have worked equally well. First we'll setup the endpoint to user JSON using the following configuration:

<!-- import SerializerConfiguration-->

Not that our endpoint can understand JSON we define a message contract to use. In NServiceBus messages are just C# classes so in this case we create the following class

<!-- import MessageContract-->

The final piece of the puzzle to to tell the serializer what C# class our JSON payload belongs to. We do this using the JSON.net `$type` attribute. The message body will then look as follows:

<!-- import MessagePayload-->


### Sending the message
Not that we've done all the legwork send a message to our endpoint using plain ADO.net is as easy as

<!-- import SendingUsingAdoNet-->

Armed with this you will now be able to send messages from any app in your organization that supports ADO.net

## Sending from within the database

So far we've seen how to send from other .Net applications. While that is fine sometimes you'll integrate with old legacy apps where performing sends straight from within the database it self might be a better approach. Just execute the following TSQL statement and notice how the message is consumed by your NServiceBus endpoint.

<!-- import SendFromTSQL -->

### Using triggers to emit messages

Sometimes you're not allowed to touch the legacy systems you're dealing with and this is where triggers come in handy. Yes you read that right triggers! while, rightfully so,  considered evil by most sane people there is still use cases where a trigger might be just what we need. 

Lets create a fictive `Orders` using

<!-- import CreateLegacyTable -->

and create a on `inserted` trigger that will send a `LegacyOrderDetected` message for each new order that we add to the table. Here's the trigger:

<!-- import CreateTrigger -->

Notice how we generate a unique message id by hashing the identity column. NServiceBus requires each message to have a unique id in order to safely perform retries.

That's it, just add a few order to the table and notice how the app receives the messages. In a real life scenario you would likely use this to trigger a `bus.Publish` to push an `OrderAccepted` event out on the bus.

Happy inserting!







