namespace MassTransit.Courier.Factories
{
    using System;
    using System.Threading.Tasks;
    using GreenPipes;


    public class FactoryMethodActivityFactory<TActivity, TArguments, TLog> :
        IActivityFactory<TActivity, TArguments, TLog>
        where TActivity : class, IExecuteActivity<TArguments>, ICompensateActivity<TLog>
        where TArguments : class
        where TLog : class
    {
        readonly ICompensateActivityFactory<TActivity, TLog> _compensateFactory;
        readonly IExecuteActivityFactory<TActivity, TArguments> _executeFactory;

        public FactoryMethodActivityFactory(Func<TArguments, TActivity> executeFactory,
            Func<TLog, TActivity> compensateFactory)
        {
            _executeFactory = new FactoryMethodExecuteActivityFactory<TActivity, TArguments>(executeFactory);
            _compensateFactory = new FactoryMethodCompensateActivityFactory<TActivity, TLog>(compensateFactory);
        }

        public Task<ResultContext<ExecutionResult>> Execute(ExecuteContext<TArguments> context,
            IRequestPipe<ExecuteActivityContext<TActivity, TArguments>, ExecutionResult> next)
        {
            return _executeFactory.Execute(context, next);
        }

        public Task<ResultContext<CompensationResult>> Compensate(CompensateContext<TLog> context,
            IRequestPipe<CompensateActivityContext<TActivity, TLog>, CompensationResult> next)
        {
            return _compensateFactory.Compensate(context, next);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("factoryMethod");
        }
    }
}
