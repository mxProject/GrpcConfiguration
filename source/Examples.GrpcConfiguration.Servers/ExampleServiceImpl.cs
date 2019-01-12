using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Examples.GrpcConfiguration.Models;

namespace Examples.GrpcConfiguration.Servers
{

    /// <summary>
    /// 
    /// </summary>
    internal class ExampleServiceImpl : ExampleService.ExampleServiceBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async override Task<Member> GetMember(MemberCondition request, ServerCallContext context)
        {
            await Task.Yield();
            return new Member()
            {
                ID = request.ID,
                Name = "member" + request.ID,
                Age = 20,
            };
        }
    }

}
