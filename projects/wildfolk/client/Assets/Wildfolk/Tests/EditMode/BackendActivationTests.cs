using System;
using NUnit.Framework;
namespace Wildfolk.Tests
{
    public sealed class BackendActivationTests
    {
        private static BackendSettings Config()=>new BackendSettings{Enabled=false,Environments=new[]{new BackendEnvironment{Channel="Development",Name="development",Id="9aa02b53-8272-4975-a42a-f6a0b75a360f"},new BackendEnvironment{Channel="Alpha",Name="alpha",Id="a9eee8ca-e2a8-4fa7-9530-6df9b230e943"},new BackendEnvironment{Channel="Production",Name="production",Id="9da27a5f-4c79-44bc-b96b-1e490f83619d"}}};
        [Test]public void MissingAlphaNeverFallsBackToProductionEvenWhenBackendDisabled(){var c=Config();c.Environments=Array.FindAll(c.Environments,e=>e.Channel!="Alpha");Assert.Throws<BackendSetupException>(()=>BackendPolicy.RequireEnvironment(c,BuildChannel.Alpha));Assert.That(BackendPolicy.RequireEnvironment(c,BuildChannel.Production).Name,Is.EqualTo("production"));}
        [Test]public void EmptyOrDuplicateEnvironmentMappingsFailClosed(){var c=Config();c.Environments[1].Id="";Assert.Throws<BackendSetupException>(()=>BackendPolicy.RequireEnvironment(c,BuildChannel.Alpha));c=Config();c.Environments[1].Id=c.Environments[2].Id;Assert.Throws<BackendSetupException>(()=>BackendPolicy.RequireEnvironment(c,BuildChannel.Alpha));}
        [Test]public void ChannelsUseVerifiedUniqueNonProductionIds(){var c=Config();var d=BackendPolicy.RequireEnvironment(c,BuildChannel.Development);var a=BackendPolicy.RequireEnvironment(c,BuildChannel.Alpha);var p=BackendPolicy.RequireEnvironment(c,BuildChannel.Production);Assert.That(d.Id,Is.Not.EqualTo(p.Id));Assert.That(a.Id,Is.Not.EqualTo(p.Id));Assert.That(d.Id,Is.Not.EqualTo(a.Id));}
    }
}
