namespace Thetis.Mediation;
public interface IRequest<TResponse> { }

public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

public class ThetisMediator(IServiceProvider provider)
{
    public async Task<TResponse> HandleRequest<TRequest, TResponse>(TRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request), "Request cannot be null");
        }
        
        //TODO: Determine how to deal with situations where there are more than one handler registered for the same request type.
        
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TRequest));
        dynamic handler = provider.GetService(handlerType) ?? 
                          throw new InvalidOperationException($"No handler registered for request type {request.GetType()}");
        
        return await handler.Handle((dynamic)request, cancellationToken);
    }
}

// Usage example:

//services.AddTransient<IRequestHandler<MyRequest, MyResponse>, MyRequestHandler>();
//services.AddTransient<IRequestHandler<AnotherRequest, AnotherResponse>, AnotherRequestHandler>();