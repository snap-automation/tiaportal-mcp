
using TiaMcpServer.Siemens;
using Siemens.Engineering;
using System.Linq;

namespace TiaMcpServer.Test
{
    [TestClass]
    public class PortalTests
    {
        private bool IsTiaPortalRunning()
        {
            return TiaPortal.GetProcesses().Any();
        }

        [TestMethod]
        public void ConnectPortal_Dynamically_BasedOnTiaPortalStatus()
        {
            if (IsTiaPortalRunning())
            {
                ConnectPortal_TiaPortalRunning_AttachesToInstance();
            }
            else
            {
                ConnectPortal_NoTiaPortalRunning_StartsNewInstance();
            }
        }

        private void ConnectPortal_NoTiaPortalRunning_StartsNewInstance()
        {
            // Arrange
            var portal = new Portal();

            // Act
            var result = portal.ConnectPortal();

            // Assert
            Assert.IsTrue(result, "ConnectPortal should return true when starting a new instance.");
            Assert.IsNotNull(portal.GetState(), "The portal state should not be null after connecting.");
            Assert.IsTrue(portal.IsConnected(), "The portal should be connected.");

            // Cleanup
            portal.DisconnectPortal();
        }

        private void ConnectPortal_TiaPortalRunning_AttachesToInstance()
        {
            // Arrange
            var portal = new Portal();

            // Act
            var result = portal.ConnectPortal();

            // Assert
            Assert.IsTrue(result, "ConnectPortal should return true when attaching to an existing instance.");
            Assert.IsNotNull(portal.GetState(), "The portal state should not be null after connecting.");
            Assert.IsTrue(portal.IsConnected(), "The portal should be connected.");

            // Cleanup
            portal.DisconnectPortal();
        }
    }
}
