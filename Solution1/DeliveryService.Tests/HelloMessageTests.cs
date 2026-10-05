    using DeliveryService.Protocol.Enums;
    using DeliveryService.Protocol.Messages;

    namespace DeliveryService.Tests;

    public class HelloMessageTests
    {
        [Theory]
        [InlineData(Role.Operator)]
        [InlineData(Role.Customer)]
        [InlineData(Role.Robot)]
        public void Good_HelloMessageRoundTrip_ParsesSerializesAndReturnTrue(Role role)
        {
            // Arrange

            var message = new HelloMessage
            {
                Role = role
            };

            var buffer = new byte[100];
            var payload = new[] { (byte)role };

            // Act

            var isSuccessfullySerialized = message.TrySerialize(buffer, out var written);
            var isSuccessfullyParsed = HelloMessage.TryParse(payload, out var parsedMessage);

            // Assert

            Assert.True(isSuccessfullySerialized);
            Assert.True(isSuccessfullyParsed);
            Assert.Equal(1, written);
            Assert.Equal((byte)role, buffer[0]);
            Assert.Equal(role, parsedMessage!.Role);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        public void Bad_PayloadLength_ReturnFalse(int payloadLength)
        {
            // Arrange

            var payload = new byte[payloadLength];

            // Act

            var isSuccessfullyParsed = HelloMessage.TryParse(payload, out _);

            // Assert

            Assert.False(isSuccessfullyParsed);
        }

        [Theory]
        [InlineData(0x00)]
        [InlineData(0x04)]
        public void Bad_UndefinedRole_ReturnFalse(byte role)
        {
            // Arrange

            var payload = new[] { role };

            // Act

            var isSuccessfullyParsed = HelloMessage.TryParse(payload, out _);

            // Assert

            Assert.False(isSuccessfullyParsed);
        }
    }
