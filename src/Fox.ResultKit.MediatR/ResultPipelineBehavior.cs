//==================================================================================================
// MediatR pipeline behavior for handling Result-typed responses.
// Automatic ThrowIfFailure execution in MediatR request pipeline.
//==================================================================================================
using System.Reflection;
using MediatR;

namespace Fox.ResultKit.MediatR;

//==================================================================================================
/// <summary>
/// MediatR pipeline behavior that automatically handles Result-typed responses.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type (Result or Result&lt;T&gt;).</typeparam>
//==================================================================================================
public class ResultPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    //==============================================================================================
    /// <summary>
    /// Executes the pipeline behavior.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    //==============================================================================================
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(next);

#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(ex.Message);
            }

            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var resultType = typeof(TResponse);

                var failureMethod = resultType.GetMethod(nameof(Result<>.Failure), BindingFlags.Public | BindingFlags.Static);

                if (failureMethod != null)
                {
                    var result = failureMethod.Invoke(null, [ex.Message]);
                    return (TResponse)result!;
                }
            }

            throw;
        }
#pragma warning restore CA1031 // Do not catch general exception types
    }
}
