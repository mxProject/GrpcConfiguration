using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Grpc.Core;
using mxProject.Helpers.Grpc.Configuration;
using Examples.GrpcConfiguration.Models;

namespace Examples.GrpcConfiguration.Clients
{

    /// <summary>
    /// 
    /// </summary>
    internal partial class Form1 : Form
    {

        #region ctor

        /// <summary>
        /// 
        /// </summary>
        internal Form1() : this(null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rpcContext"></param>
        internal Form1(RpcConfigurationContext rpcContext)
        {
            InitializeComponent();
            m_RpcContext = rpcContext;
        }

        #endregion

        #region gRPC

        private readonly RpcConfigurationContext m_RpcContext;

        #endregion

        private async void BtnCallRpc_Click(object sender, EventArgs e)
        {
            try
            {

                CallInvoker invoker = m_RpcContext.GetCallInvoker("invoker1");

                ExampleService.ExampleServiceClient client = new ExampleService.ExampleServiceClient(invoker);

                MemberCondition condition = new MemberCondition() { ID = "123" };

                Models.Member member = await client.GetMemberAsync(condition);

                MessageBox.Show(member.Name);

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show(ex.Message);
            }

        }
    }
}
