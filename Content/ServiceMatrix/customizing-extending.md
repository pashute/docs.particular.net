---
title: Using ServiceMatrix Generated Code
summary: Using ServiceMatrix and extending it with your own code. 
tags:
- ServiceMatrix
- Code Generation

---
ServiceMatrix accelerates the creation of a distributed NServiceBus solution.  As the canvas and visual elements are used to design the system the Visual Studio solution is updated with generated code and content.   

This article explores how to customize the ServiceMatrix solution and add your own code.

1. [Introduction](#introduction)
2. [Message Definitions](#message-definitions)
3. [NServiceBus Components](#customizing-components)
4. [Partial Classes and Methods](#the-dynamic-partial-class)
5. [Reviewing Generated Code](#reviewing-the-generated-code)
6. [Sagas](#understanding-saga-code)
7. [Saga Data](#saga-data)
8. [Saga Message Handlers](#saga-message-handlers)
9. [Custom Saga Finding](#custom-saga-finding-logic)
10. [Summary](#summary)

#Introduction
ServiceMatrix is a visual design tool that accelerates the design and coding of an NServiceBus system.  It generates code and automates the creation of classes, projects and configurations that would take much longer if done manually.  The generated code is designed in a way that allows for user modification and extension. This article reviews this high level design and demonstrates the extension points.    

##Viewing The Code
**NOTE**: In ServiceMatrix, to view the code for any of the components, use the drop-down within that component on the design canvas and choose `View Code`. To view related partial class definitions, put your cursor in the class name, right mouse-click and select `Go to Definition` or hit `F12`.

#Message Definitions
NServiceBus messages are plain old CLR classes in C#. As you send commands or publish events in ServiceMatrix, you are prompted only for a name. When you build your solution, the messages are generated.  The generated message classes don't contain any properties but you can easily add them. To modify a message class, [view the code](#viewing-the-code) and add whatever properties you wish.
```C#
namespace OnlineSales.InternalMessages.Commands.Sales
{
    public class SubmitOrder
    {
        public string CustomerName { get; set; }
        public string AccountNumber { get; set; }
        public string ShippingChoice { get; set; }
    }
}
``` 
If you build the solution again, the new message properties will be available in the components, handlers and sagas.  Since the message classes are only generated one time by ServiceMatrix, they are safe to edit.

#Customizing Components
Mixing dynamic code and user created code can be challenging.  This is especially true in an environment like ServiceMatrix where as you continue to design your system in the visual environment code is regenerated. This must be done without disturbing the users code that has been added along the way.   

##Partial Classes and Partial Methods
To solve this, the design of the generated code uses partial classes.  The dynamic files that are subject to regeneration by ServiceMatrix are created with a partial class definition in one file.  The same partial class is also defined in a separate file that is for user customization. Generated code in one file invokes virtual methods that are further defined in the user modified partial class. 

The dynamic partial class includes straight forward message handlers and partial methods.  This file should not be edited and warns you of this in the comments at the top of the file.  It will be regenerated every time the ServiceMatrix solution is built and will change as the visual design or settings are modified. 

```C#
//------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by ServiceMatrix.
//
// Changes to this file may cause incorrect behavior and will be lost if
// the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
```
##Reviewing The Generated Code 
The design of the generated message handlers use partial methods to provide opportunities for the modification of messages and the integration of your own code and services.  Lets look at the code for a sample component. 

##NServiceBus Component Code 
The code below was generated by ServiceMatrix for component that handles a `SubmitOrder` message and publishes a `Orderaccepted` event.  It looks like this on the canvas:

![SubmitOrderProcessing Component](images/servicematrix-orderprocessing.png)

###The Dynamic Partial Class
The dynamic code generator has created a handler for the `SubmitOrder` method.  In the handler a  `HandleImplementation` partial method for the message is invoked.  As a partial method, it is stubbed out in one class but implemented separately in the other partial class file.  The `Bus` property  provides the code with access to the `Bus` object.

```C#
namespace OnlineSales.Sales
{
    public partial class SubmitOrderProcessor : IHandleMessages<SubmitOrder>
    {
		public void Handle(SubmitOrder message)
		{
			// Handle message on partial class
			this.HandleImplementation(message);
		}

		partial void HandleImplementation(SubmitOrder message);

        public IBus Bus { get; set; }
    }
}
```
###The Customizable Partial Class 
The customizable partial class is generated by ServiceMatrix and is intended to be modified with the partial method implementations and custom logic.  This class is generated only one time.  After adding elements to your design in ServiceMatrix, this code is generated the first time you build your solution.  It will contain a partial method for each the message types it handles and a publish or send method for any outbound messages.    

```C#
namespace OnlineSales.Sales
{
    public partial class SubmitOrderHandler
    {
		
        partial void HandleImplementation(SubmitOrder message)
        {
            // TODO: SubmitOrderHandler: Add code to handle the SubmitOrder message.
            Console.WriteLine("Sales received " + message.GetType().Name);
            
          
  			var orderAccepted = new OnlineSales.Contracts.Sales.OrderAccepted();
            Bus.Publish(orderAccepted);
        }

    }
}

```
Code is generated for both incoming and outbound messages in your design.
 
**Inbound Message Handlers**
In the example above, the code generator has stubbed out a simple implementation of the partial `HandleImplementation` method for the  `SubmitOrder` command message.  This is the place to put any custom handler code.  

**Outbound Messages**
In the example above the code generator has also created the code to instantiate a new `OrderAccepted` event and publish it using the bus automatically when the the `SubmitOrder` message is handled. The comments indicate this code was generated by ServiceMatrix.  This code can be completedly customized.  If the design doesn't require the event to be published in response to the inbound message, it can be altered. 
##Adding Messages to the Design
The code generator will create code for handling and sending any messages that are in your design *the first time you build the solution*.  In the sample above, the code for handling the `SubmitOrder` message and publishing the `OrderAccepted` event was created because both of them were present in the ServiceMatrix design at initial build. 
 
As you continue to added to your design the code generator after the initial build, ServiceMatrix cannot safely edit the partial class definition or custom code could be impacted.  When you add additional messages to a component, ServiceMatrix will recognize that new code is needed and warn you as shown:

![User Code Changes Required](images/servicematrix-codechangerequired.png) 

Adding a new `HightPriorityOrderAccepted` event to the example above causes ServiceMatrix to prompt with the code required to create and publish the message.  ServiceMatrix can assist you by opening the partial class file and placing the code on the clipboard so you can easily add it.  

#Understanding Saga Code
ServiceMatrix supports the design of [NServiceBus sagas](images/sagas-in-nservicebus.md "Sagas in NserviceBus") and generates the necessary code to get you started.  The saga is a specialized stateful version of a ServiceMatrix component.  As we saw with the handlers, the Saga is generated with the dynamic code and user modified code in separate partial class files.  For sagas, the design has been extended to to include definitions for saga data, custom finding logic, and some convenience methods to make saga completion easier.  

The code below is based on a saga designed in ServiceMatrix that correlates the request and response with another endpoint as shown.  This saga handles the `OrderAccepted` event then publishes the `SubmitPayment` request to a payment processing service.  The saga handles and correlates the `SubmitPaymentResponse`.

![Billing Saga](images/servicematrix-billingsagaandpaymntcanvas.png)

##Designating the Startup Messages
Depending on the design, one or more messages can start a saga.  If the saga is handling multiple messages, you will be [prompted in ServiceMatrix](images/servicematrix-sagastarter.png "Designated Saga Starters") to indicate which messages will start the saga.  The partial class that contains the component definition will be generated to extent the NServiceBus.Saga base class and will use the marker interfaces to indicate the start-up message types and the message types that the saga will handle.  

The code below is generated for example saga class above. This is regenerated with each build and is not safe to edit.
```C#
namespace OnlineSales.Billing
{
    public partial class OrderAcceptedProcessor : Saga<OrderAcceptedProcessorSagaData>, IOrderAcceptedProcessor, ServiceMatrix.Shared.INServiceBusComponent, IAmStartedByMessages<OrderAccepted>, IHandleMessages<SubmitPaymentResponse>
    {
		public void Handle(OrderAccepted message)
		{
			......
```
Once again this is a partial class.  In the other partial class definition it is possible to manually add additional message types that the saga will handle. 
##Saga Data
The Saga data class defines the persisted data that is available inside handlers each time the saga is activated.  The Saga data is implemented as a partial class in the same dynamic file as the saga handler class itself. The generated saga data design includes default properties for a unique ID and the storage of any message that is handled.  It also stores information about the `Originator` of the message that started the saga and the Id of that message. 
```C#

public partial class OrderAcceptedProcessorSagaData : IContainSagaData
     {
           public virtual Guid Id { get; set; }
           public virtual string Originator { get; set; }
           public virtual string OriginalMessageId { get; set; }
           public virtual OrderAccepted OrderAccepted { get; set; }
           public virtual SubmitPaymentResponse SubmitPaymentResponse { get; set; }
    }
```
This generated file is dynamic so there is a separate file containing a place to modify the partial class.  Custom properties are easily added here.  Notice how in the example the OrderID has been added.  It is a unique identifier for the saga data and was marked with the `[Unique]` attribute.

```C#
namespace OnlineSales.Billing
{
    public partial class OrderAcceptedProcessorSagaData
    {
        [Unique]
        public Guid OrderID { get; set; }
        public string PaymentAuthorizationCode { get; set; }
    }
}
```
##Saga Message Handlers
The message handling code of the Saga is implemented very much like the [handler component code above](#nservicebus-component-code).  The code will implement a message handler in the saga for each of the messages handled in the ServiceMatrix design.   Similarly, for each handled message a virtual method is created in the form `HandleImplementation(<messagetype>)`.   These virtual methods are invoked and should be implememented and customized in the custom code partial class.   View the Saga code by select the `View Code` option on the component in the canvas. 

###Example Saga Code
The following sample customizes the saga code.  It handles an `OrderAccepted` event, stores the `OrderID` then creates and sends the `SubmitPayment` command.  The saga then handles the `SubmitPaymentResponse` message and stores the `PaymentAuthorizationCode` in the saga data.

```C#
namespace OnlineSales.Billing
{
    public partial class OrderAcceptedHandler
    {
		
        partial void HandleImplementation(OrderAccepted message)
        {
            // TODO: OrderAcceptedHandler: Add code to handle the OrderAccepted message.
            Console.WriteLine("Billing received {0} for order id {1}",message.GetType().Name, message.OrderID);

            //set the saga order id.  This will be accessable in any future handler.
            Data.OrderID = message.OrderID;

            Console.WriteLine("Submitting Order {0} for payment", Data.OrderID);


            //Pasting the code from the user code pop up here so we send the payment request when the OrderAccepted Event arrives.
            var submitPayment = new OnlineSales.Internal.Commands.Billing.SubmitPayment();
            Bus.Send(submitPayment);
            

        }

        partial void HandleImplementation(Internal.Messages.Billing.SubmitPaymentResponse message)
        {
            Data.PaymentAuthorizationCode = message.AuthorizationCode;
            
            System.Console.WriteLine("Payment response received for order {0} with auth code {1}", message.AuthorizationCode, Data.OrderID);

            Data.PaymentAuthorizationCode = message.AuthorizationCode;
                                    
        }

        partial void AllMessagesReceived()
        {
            //Publish the BillingCompleted event.  Assign event values from the saga data values. 
            var billingCompleted = new OnlineSales.Contracts.Billing.BillingCompleted();
            billingCompleted.AuthorizationCode = Data.PaymentAuthorizationCode;
            billingCompleted.OrderID = Data.OrderID;
            Bus.Publish(billingCompleted);
            
            //Mark this saga as complete and free up the persistence resources.
            System.Console.WriteLine("Marking Saga complete for order {0}", Data.OrderID);
            MarkAsComplete();
        }

    }
```
 
The `AllMessagesReceived` is a convenient partial method.  As part of the dynamically generated handler code, every inbound message is stored in saga data. After each message is handled the saga data is checked to see if all the messages have been received.  If so, the virtual method `AllMessagesReceived` is called.  The example uses it to trigger the publishing of a `BillingCompleted` event and then mark the saga as complete.

##Custom Saga Finding Logic
When a saga handles a message it must be able to correlate the message to a unique saga instance in order to retrieve the correct saga data. It does this by mapping designated properties of the message to properties of the saga data.  ServiceMatrix generates default code for this mapping that can be modified.  In the [drop-down menu ](images/servicematrix-configuresaga.png)for the saga component is an option for `Configure Saga`. The provided partial class overrides the `ConfigureHowToFindSaga` method.  The comments indicate how it can be modified for a specific situation. 

When a saga uses a Bus.Send to send a request message and later handles a reply, a custom mapping is not required.  This is because the framework will put a unique saga ID in the header of the quest and it will also be in put the reply message to be used for correlation.   This automatic correlation is a nice convenience. 

Events received by a saga must have a map so they will correlate correctly.  In the example below, an `OrderAccepted` and `BillingCompleted` are mapped to saga data by their `OrderID` property. 

```C#
public partial class OrderAcceptedHandler
    {
        public override void ConfigureHowToFindSaga()
        {
			//m represents the message and s represents the saga data in this mapping method. 
            
           ConfigureMapping<OrderAccepted>(m => m.OrderID).ToSaga(s => s.OrderID );
           ConfigureMapping<BillingCompleted>(m => m.OrderID).ToSaga(s => s.OrderID );

            
            // If you add new messages to be handled by your saga, you will need to manually add a call to ConfigureMapping for them.
        }
    }
```

#Summary
The visual design environment of ServiceMatrix generates code designed to be extensible.  This article reviewed the partial classes and methods that need to be customized.  

To learn more about using sagas within ServiceMatrix refer to [this article](getting-started-sagasfullduplex-2.0.md "Using Sagas in Full Duplex").

As you design your solution, learn to use ServiceInsight.  [This article](servicematrix-serviceinsight.md "ServiceMatrix and ServiceInsight") reviews how both products work together and add efficiency to your design process. 

You can monitor your sagas and bus endpoints in production using [ServicePulse](../ServicePulse/Index.md "ServicePulse")
     