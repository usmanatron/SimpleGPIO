﻿using System;
using NSubstitute;
using SimpleGPIO.GPIO;
using SimpleGPIO.OS;
using SimpleGPIO.Properties;
using Xunit;

namespace SimpleGPIO.Tests.GPIO
{
    public class LinuxPinInterfaceTests
    {
        [Fact]
        public void EnabledIfDirectoryExists()
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            fs.Exists("/sys/class/gpio/gpio123").Returns(true);
            var pinInterface = new LinuxPinInterface(123, fs);

            //act
            var enabled = pinInterface.Enabled;

            //assert
            Assert.True(enabled);
        }

        [Fact]
        public void NotEnabledIfDirectoryDoesNotExist()
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            fs.Exists("/sys/class/gpio/gpio123").Returns(false);
            var pinInterface = new LinuxPinInterface(123, fs);

            //act
            var enabled = pinInterface.Enabled;

            //assert
            Assert.False(enabled);
        }

        [Fact]
        public void EnabledUsesCachedValueOnSubsequentRequests()
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            var pinInterface = new LinuxPinInterface(123, fs);
            var enabled = pinInterface.Enabled;

            //act
            enabled = pinInterface.Enabled;

            //assert
            fs.Received(1).Exists(Arg.Any<string>());
        }

        [Fact]
        public void EnabledUsesCachedValueAfterWrite()
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            var pinInterface = new LinuxPinInterface(123, fs)
            {
                Enabled = true
            };

            //act
            var enabled = pinInterface.Enabled;

            //assert
            fs.Received(0).Exists(Arg.Any<string>());
        }

        [Theory]
        [InlineData("in", IOMode.Read)]
        [InlineData("out", IOMode.Write)]
        public void CanGetIOMode(string value, IOMode expected)
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            fs.Read("/sys/class/gpio/gpio123/direction").Returns(value);

            var pinInterface = new LinuxPinInterface(123, fs);

            //act
            var io = pinInterface.IOMode;

            //assert
            Assert.Equal(expected, io);
        }

        [Fact]
        public void GettingIOModeEnablesPinIfNotEnabled()
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            fs.Read("/sys/class/gpio/gpio123/direction").Returns("in");
            var pinInterface = new LinuxPinInterface(123, fs);

            //act
            var io = pinInterface.IOMode;

            //assert
            fs.Received().Write("/sys/class/gpio/export", "123");
        }

        [Theory]
        [InlineData(IOMode.Read, "in")]
        [InlineData(IOMode.Write, "out")]
        public void CanSetIOMode(IOMode io, string expected)
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();

            //act
            var pinInterface = new LinuxPinInterface(123, fs)
            {
                Enabled = true,
                IOMode = io
            };

            //assert
            fs.Received().Write("/sys/class/gpio/gpio123/direction", expected);
        }

        [Fact]
        public void SettingIOModeEnablesPinIfNotEnabled()
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();

            //act
            var pinInterface = new LinuxPinInterface(123, fs)
            {
                IOMode = IOMode.Write
            };

            //assert
            fs.Received().Write("/sys/class/gpio/export", "123");
        }

        [Theory]
        [InlineData("1", Power.Off)]
        [InlineData("0", Power.On)]
        public void CanGetPower(string value, Power expected)
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            fs.Read("/sys/class/gpio/gpio123/value").Returns(value);

            var pinInterface = new LinuxPinInterface(123, fs);

            //act
            var power = pinInterface.Power;

            //assert
            Assert.Equal(expected, power);
        }

        [Fact]
        public void GettingPowerEnablesPinIfNotEnabled()
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            fs.Read("/sys/class/gpio/gpio123/value").Returns("1");
            var pinInterface = new LinuxPinInterface(123, fs);

            //act
            var power = pinInterface.Power;

            //assert
            fs.Received().Write("/sys/class/gpio/export", "123");
        }

        [Theory]
        [InlineData(Power.Off, "1")]
        [InlineData(Power.On, "0")]
        public void CanSetPower(Power power, string expected)
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            fs.Read("/sys/class/gpio/gpio123/direction").Returns("out");

            //act
            var pinInterface = new LinuxPinInterface(123, fs)
            {
                Enabled = true,
                Power = power
            };

            //assert
            fs.Received().Write("/sys/class/gpio/gpio123/value", expected);
        }

        [Fact]
        public void SettingPowerSetsIOModeIfNotWrite()
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            fs.Read("/sys/class/gpio/gpio123/direction").Returns("in");

            //act
            var pinInterface = new LinuxPinInterface(123, fs)
            {
                Power = Power.On
            };

            //assert
            fs.Received().Write("/sys/class/gpio/gpio123/direction", "out");
        }

        [Fact]
        public void EnableSetsEnabledTrue()
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            var pinInterface = new LinuxPinInterface(123, fs)
            {
                Enabled = false
            };

            //act
            pinInterface.Enable();

            //assert
            Assert.True(pinInterface.Enabled);
        }

        [Fact]
        public void DisableSetsEnabledFalse()
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            var pinInterface = new LinuxPinInterface(123, fs)
            {
                Enabled = true
            };

            //act
            pinInterface.Disable();

            //assert
            Assert.False(pinInterface.Enabled);
        }

        [Fact]
        public void TurnOnSetsPowerOn()
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            fs.Read("/sys/class/gpio/gpio123/direction").Returns("out");
            var pinInterface = new LinuxPinInterface(123, fs)
            {
                Power = Power.Off
            };

            //act
            pinInterface.TurnOn();

            //assert
            Assert.Equal(Power.On, pinInterface.Power);
        }

        [Fact]
        public void TurnOffSetsPowerOff()
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            fs.Read("/sys/class/gpio/gpio123/direction").Returns("out");
            var pinInterface = new LinuxPinInterface(123, fs)
            {
                Power = Power.On
            };

            //act
            pinInterface.TurnOff();

            //assert
            Assert.Equal(Power.Off, pinInterface.Power);
        }

        [Fact]
        public void ToggleTurnsOnIfOff()
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            fs.Read("/sys/class/gpio/gpio123/direction").Returns("out");
            var pinInterface = new LinuxPinInterface(123, fs)
            {
                Power = Power.Off
            };

            //act
            pinInterface.Toggle();

            //assert
            Assert.Equal(Power.On, pinInterface.Power);
        }

        [Fact]
        public void ToggleTurnsOffIfOn()
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            fs.Read("/sys/class/gpio/gpio123/direction").Returns("out");
            var pinInterface = new LinuxPinInterface(123, fs)
            {
                Power = Power.On
            };

            //act
            pinInterface.Toggle();

            //assert
            Assert.Equal(Power.Off, pinInterface.Power);
        }

        [Fact]
        public void CanToggleForADuration()
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            fs.Read("/sys/class/gpio/gpio123/direction").Returns("out");
            fs.Read("/sys/class/gpio/gpio123/value").Returns("0");
            var pinInterface = new LinuxPinInterface(123, fs);

            //act
            pinInterface.Toggle(100, TimeSpan.FromMilliseconds(100));

            //assert
            fs.Received(20).Write("/sys/class/gpio/gpio123/value", Arg.Any<string>());
        }

        [Fact]
        public void CanToggleAtHighFrequency()
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            fs.Read("/sys/class/gpio/gpio123/direction").Returns("out");
            fs.Read("/sys/class/gpio/gpio123/value").Returns("0");
            var pinInterface = new LinuxPinInterface(123, fs);

            //act
            pinInterface.Toggle(TimeSpan.TicksPerMillisecond, TimeSpan.FromMilliseconds(1));

            //assert
            fs.Received(20).Write("/sys/class/gpio/gpio123/value", Arg.Any<string>());
        }

        [Fact]
        public void CanToggleForSetIterations()
        {
            //arrange
            var fs = Substitute.For<IFileSystem>();
            fs.Read("/sys/class/gpio/gpio123/direction").Returns("out");
            fs.Read("/sys/class/gpio/gpio123/value").Returns("0");
            var pinInterface = new LinuxPinInterface(123, fs);

            //act
            pinInterface.Toggle(TimeSpan.TicksPerMillisecond, 10);

            //assert
            fs.Received(20).Write("/sys/class/gpio/gpio123/value", Arg.Any<string>());
        }
    }
}