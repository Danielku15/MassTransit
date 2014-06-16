// Copyright 2007-2014 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.Pipeline.Sinks
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Policies;
    using Util;


    public class InstanceMessageFilter<TConsumer, TMessage> :
        IConsumeFilter<TMessage>
        where TConsumer : class
        where TMessage : class
    {
        readonly TConsumer _instance;
        readonly IPipe<ConsumeContext<TConsumer, TMessage>> _messageAdapter;
        readonly IRetryPolicy _retryPolicy;

        public InstanceMessageFilter(TConsumer instance,
            IConsumerMessageAdapter<TConsumer, TMessage> messageAdapter, IRetryPolicy retryPolicy)
        {
            _instance = instance;
            _messageAdapter = new LastPipe<ConsumeContext<TConsumer, TMessage>>(messageAdapter);
            _retryPolicy = retryPolicy;
        }

        public async Task Send(ConsumeContext<TMessage> context, IPipe<ConsumeContext<TMessage>> next)
        {
            Stopwatch timer = Stopwatch.StartNew();
            try
            {
                await _retryPolicy.Retry(context, x => _messageAdapter.Send(x.Push(_instance)));

                context.NotifyConsumed(timer.Elapsed, TypeMetadataCache<TConsumer>.ShortName);

                await next.Send(context);
            }
            catch (Exception ex)
            {
                context.NotifyFaulted(TypeMetadataCache<TConsumer>.ShortName, ex);
                throw;
            }
        }

        public bool Inspect(IPipeInspector inspector)
        {
            return inspector.Inspect(this);
        }
    }
}