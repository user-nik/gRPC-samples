using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sqrt;
using static Sqrt.SqrtService;

namespace server
{
    public class SqrtServiceIml : SqrtServiceBase
    {
        public override async Task<SqrtResponse> sqrt(SqrtRequest request, ServerCallContext context)
        {
            int number = request.Number;

            if (number >= 0)
            {
                return new SqrtResponse() { SquareRoot = Math.Sqrt(number) };
            }
            else
                throw new RpcException(new Status(StatusCode.InvalidArgument, "number < 0"));
        }
    }
}
