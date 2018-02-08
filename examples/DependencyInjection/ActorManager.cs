using Microsoft.Extensions.Logging;
using Proto;

namespace DependencyInjection
{
    public class ActorManager : IActorManager
    {
        private readonly IActorFactory actorFactory;

        public ActorManager(IActorFactory actorFactory, ILogger<ActorManager> logger)
        {
            this.actorFactory = actorFactory;
            EventStream.Instance.Subscribe<DIActor.Ping>(x => logger.LogInformation($"EventStream reply: {x.Name}"));
        }

        public void Activate()
        {
            actorFactory.GetActor<DIActor>().Tell(new DIActor.Ping("no-name"));
            actorFactory.GetActor<DIActor>("named").Tell(new DIActor.Ping("named"));
        }
    }
}