using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using PillarAPI.Enums;
using PillarAPI.IdentifyResponses;
using PillarAPI.Interfaces;
using PillarAPI.RequestResponses;

namespace PillarAPI
{
    public class IOC
    {
        private readonly IWindsorContainer _container = new WindsorContainer();

        public IOC(PillarTypeEnum pillarType)
        {
            BootstrapContainer(pillarType);
        }

        private void BootstrapContainer(PillarTypeEnum pillarType)
        {
            _container.Register(Component.For<IdentifyPillarsGeneralTopicListener>());
            _container.Register(Component.For<PillarQueueListener>());
            _container.Register(Component.For<IPillarWrapper>().ImplementedBy<PillarWrapper>());

            _container.Register(Component.For<IGetStatus>().ImplementedBy<GetStatus>());
            _container.Register(Component.For<IGetAuditTrail>().ImplementedBy<GetAuditTrail>());
            _container.Register(Component.For<IGetFileId>().ImplementedBy<GetFileId>());

            switch (pillarType)
            {
                case PillarTypeEnum.Pillar:

                    _container.Register(Component.For<IPutFile>().ImplementedBy<PutFileWithWebDav>());
                    _container.Register(Component.For<IGetChecksum>().ImplementedBy<GetChecksum>());
                    _container.Register(Component.For<IGetFile>().ImplementedBy<GetFileWithWebDav>());
                    _container.Register(
                        Component.For<IResponseBuilderForIdentifyPillarsForGetFileRequest>()
                                 .ImplementedBy<ResponseBuilderForIdentifyPillarsForGetFileRequest>());


                    break;
                case PillarTypeEnum.ChecksumPillar:
                    _container.Register(Component.For<IGetFile>().ImplementedBy<ChecksumGetFile>());
                    _container.Register(Component.For<IGetChecksum>().ImplementedBy<ChecksumGetChecksum>());
                    _container.Register(Component.For<IPutFile>().ImplementedBy<ChecksumPutFile>());
                    _container.Register(
                        Component.For<IResponseBuilderForIdentifyPillarsForGetFileRequest>()
                                 .ImplementedBy<ChecksumResponseBuilderForIdentifyPillarsForGetFileRequest>());
                    break;
                default:
                    throw new ArgumentOutOfRangeException("pillarType");
            }
        }

        public PillarQueueListener GetPillarQueueListener()
        {
            return _container.Resolve<PillarQueueListener>();
        }

        public IdentifyPillarsGeneralTopicListener GetIdentifyPillarsGeneralTopicListener()
        {
            return _container.Resolve<IdentifyPillarsGeneralTopicListener>();
        }
    }
}