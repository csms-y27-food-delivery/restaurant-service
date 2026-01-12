using Grpc.Core;
using Grpc.Core.Interceptors;
using RestaurantService.Infrastructure.Persistence.Exceptions;
using System.Globalization;

namespace RestaurantService.Presentation.Grpc.Interceptors;

public sealed class GrpcExceptionInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (RepositoryException ex)
        {
            throw ToRpcException(ex);
        }
        catch (Exception)
        {
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error."));
        }
    }

    private static RpcException ToRpcException(RepositoryException ex)
    {
        StatusCode statusCode = MapRepositoryExceptionToStatusCode(ex);
        string message = ex.Message;
        Metadata trailers = CreateMetadata(ex);

        return new RpcException(new Status(statusCode, message), trailers);
    }

    private static StatusCode MapRepositoryExceptionToStatusCode(RepositoryException ex)
    {
        return ex switch
        {
            DishNotFoundException => StatusCode.NotFound,
            RestaurantNotFoundException => StatusCode.NotFound,
            _ => StatusCode.Internal,
        };
    }

    private static Metadata CreateMetadata(RepositoryException ex)
    {
        var trailers = new Metadata();

        if (!string.IsNullOrWhiteSpace(ex.Code))
            trailers.Add("error-code", ex.Code);

        if (!string.IsNullOrWhiteSpace(ex.Operation))
            trailers.Add("error-operation", ex.Operation);

        if (ex.EntityId is not null)
            trailers.Add("error-entity-id", ex.EntityId.Value.ToString(CultureInfo.InvariantCulture));

        trailers.Add("exception-type", ex.GetType().Name);

        return trailers;
    }
}