# mediator.lib

Decouple code by using a handler router based on message type

## Usage

```csharp
class ExampleController
{
    private readonly Mediator _mediator;

    public ExampleController(Mediator mediator) => _mediator = mediator;

    public string? PostData(PostDataMessage data)
    {
        if (data != null)
        {
            var result = _mediator.Handle<PostDataMessage,PostDataResponse>(data); // call indirection;
            return new string(result.ToString());
        }
        return null;
    }
}

record PostDataMessage(object data);
record PostDataResponse(int statusCode);
```

## Changes

- v1.0.0 - initial Mediator
- v1.1.0 - added MediatorThreadSafe and removed locking mechanism from original Mediator