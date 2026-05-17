namespace Loupedeck.OBSStudioForLogiPlugin.Tests
{
    using System;
    using Xunit;

    public class StateImageDataTests
    {
        [Fact]
        public void Equals_WhenAllPropertiesMatch_ReturnsTrue()
        {
            // Arrange
            StateImageData data1 = new StateImageData
            {
                Id = "test",
                IsActive = true,
                ActiveIconPath = "active.svg",
                InactiveIconPath = "inactive.svg"
            };
            StateImageData data2 = new StateImageData
            {
                Id = "test",
                IsActive = true,
                ActiveIconPath = "active.svg",
                InactiveIconPath = "inactive.svg"
            };

            // Act & Assert
            Assert.True(data1.Equals(data2));
            Assert.True(data1 == data2);
        }

        [Fact]
        public void Equals_WhenIdDiffers_ReturnsFalse()
        {
            // Arrange
            StateImageData data1 = new StateImageData
            {
                Id = "test1",
                IsActive = true,
                ActiveIconPath = "active.svg",
                InactiveIconPath = "inactive.svg"
            };
            StateImageData data2 = new StateImageData
            {
                Id = "test2",
                IsActive = true,
                ActiveIconPath = "active.svg",
                InactiveIconPath = "inactive.svg"
            };

            // Act & Assert
            Assert.False(data1.Equals(data2));
            Assert.True(data1 != data2);
        }

        [Fact]
        public void Equals_WhenIsActiveDiffers_ReturnsFalse()
        {
            // Arrange
            StateImageData data1 = new StateImageData
            {
                Id = "test",
                IsActive = true,
                ActiveIconPath = "active.svg",
                InactiveIconPath = "inactive.svg"
            };
            StateImageData data2 = new StateImageData
            {
                Id = "test",
                IsActive = false,
                ActiveIconPath = "active.svg",
                InactiveIconPath = "inactive.svg"
            };

            // Act & Assert
            Assert.False(data1.Equals(data2));
        }

        [Fact]
        public void Equals_WhenActiveIconPathDiffers_ReturnsFalse()
        {
            // Arrange
            StateImageData data1 = new StateImageData
            {
                Id = "test",
                IsActive = true,
                ActiveIconPath = "active1.svg",
                InactiveIconPath = "inactive.svg"
            };
            StateImageData data2 = new StateImageData
            {
                Id = "test",
                IsActive = true,
                ActiveIconPath = "active2.svg",
                InactiveIconPath = "inactive.svg"
            };

            // Act & Assert
            Assert.False(data1.Equals(data2));
        }

        [Fact]
        public void Equals_WhenInactiveIconPathDiffers_ReturnsFalse()
        {
            // Arrange
            StateImageData data1 = new StateImageData
            {
                Id = "test",
                IsActive = true,
                ActiveIconPath = "active.svg",
                InactiveIconPath = "inactive1.svg"
            };
            StateImageData data2 = new StateImageData
            {
                Id = "test",
                IsActive = true,
                ActiveIconPath = "active.svg",
                InactiveIconPath = "inactive2.svg"
            };

            // Act & Assert
            Assert.False(data1.Equals(data2));
        }

        [Fact]
        public void Equals_WithNull_ReturnsFalse()
        {
            // Arrange
            StateImageData data = new StateImageData
            {
                Id = "test",
                IsActive = true,
                ActiveIconPath = "active.svg",
                InactiveIconPath = "inactive.svg"
            };

            // Act & Assert
            Assert.False(data.Equals(null));
            Assert.True(data != null);
        }

        [Fact]
        public void GetHashCode_WhenEqual_ReturnsSameHash()
        {
            // Arrange
            StateImageData data1 = new StateImageData
            {
                Id = "test",
                IsActive = true,
                ActiveIconPath = "active.svg",
                InactiveIconPath = "inactive.svg"
            };
            StateImageData data2 = new StateImageData
            {
                Id = "test",
                IsActive = true,
                ActiveIconPath = "active.svg",
                InactiveIconPath = "inactive.svg"
            };

            // Act & Assert
            Assert.Equal(data1.GetHashCode(), data2.GetHashCode());
        }
    }
}
