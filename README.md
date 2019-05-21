# XeroThrottleAndRetry

This project implements two key things for accessing Xero API. The code is kind of complex so its lathered with UnitTests.

## Throttle

Xero has a limit of 60 calls within a rolling 60 second period. The Throttle class can be used to keep your application under that limit. 

## Retry

This method will retry a failed call after sleeping for abit.

## ThrottleAndRetry

Implements  both Throttling and Retrying.

How to use:

await ThrottleAndRetry.DoAsync(() => _xeroCoreApi.Something.DoSomething(123));

or 

var result = await ThrottleAndRetry.DoAsync<int>(() => _xeroCoreApi.Something.GetList("abc"));
