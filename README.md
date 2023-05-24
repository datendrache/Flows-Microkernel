# Flows Microkernel Libaries

The Flow Microkernel that controls all the event streams

# Why Microkernel?

About 11 years of my career was based on working with different types of Complex Event Processing tools, and there were more than a few challenges in design that I thought was cringeworthy design.  However, because these CEP tools were special purposed for some industry or another, they started with a single message type and may expand into others that are often just recoded copies of the original.  The result becomes a hot mess over time.

# Define Microkernel, this ain't no operating system.

1) Data being passed through the Flow Microkernel is effectively anything, with as much abstracted away as possible.
2) As much as possible has been stripped out of the three main management systems (input, output, and processing)
3) Adapters to the Microkernel are defined externally, using only a basic common Interface to communicate with the Microkernel

In other words, the less code in the Kernel, the more reliable it becomes and easier to debug.  Side effects include faster performance, not having to reinvent wheels, and a complete set of event triggers that can be applied univerally.
