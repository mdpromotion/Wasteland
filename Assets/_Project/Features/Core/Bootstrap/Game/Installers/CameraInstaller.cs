using _Project.Features.Camera.Application;
using _Project.Features.Camera.Infrastructure;
using VContainer;
using VContainer.Unity;

namespace _Project.Features.Core.Bootstrap.Game.Installers
{
    public static class CameraInstaller
    {
        public static void Install(
            IContainerBuilder builder,
            PlayerCameraConfig playerCameraConfig)
        {
            builder.RegisterInstance(playerCameraConfig);

            builder.RegisterComponentInHierarchy<CameraMotor>()
                .As<ICameraMotor>();

            builder.Register<CameraController>(Lifetime.Singleton)
                .As<ILateTickable>()
                .As<ICameraController>();

            builder.Register<CameraWorldRebaseSync>(Lifetime.Singleton)
                .AsImplementedInterfaces();
        }
    }
}