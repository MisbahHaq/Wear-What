using MediatR;

namespace MultiVendor.Application.Common;

public interface IBaseCommand : IRequest<Result<object>> { }
public interface IBaseCommand<TResponse> : IRequest<Result<TResponse>> { }
public interface IBaseQuery<TResponse> : IRequest<Result<TResponse>> { }
