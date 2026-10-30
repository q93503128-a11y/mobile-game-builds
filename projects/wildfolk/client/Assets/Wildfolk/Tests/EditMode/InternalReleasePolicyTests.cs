using System;
using System.Collections.Generic;
using NUnit.Framework;
namespace Wildfolk.Tests
{
    public sealed class InternalReleasePolicyTests
    {
        private static BackendSettings Config()=>new BackendSettings{Enabled=false,Environments=new[]{new BackendEnvironment{Channel="Alpha",Name="alpha",Id="a9eee8ca-e2a8-4fa7-9530-6df9b230e943"},new BackendEnvironment{Channel="Production",Name="production",Id="9da27a5f-4c79-44bc-b96b-1e490f83619d"}}};
        private static List<string> Issues(BackendSettings config,BuildChannel channel,bool development=false)=>(List<string>)Type.GetType("Wildfolk.Editor.BackendBuildValidator, Assembly-CSharp-Editor",true).GetMethod("Issues").Invoke(null,new object[]{config,channel,development});
        [Test]public void DisabledAlphaCanPackageWithoutInventedAccountConfiguration()=>Assert.That(Issues(Config(),BuildChannel.Alpha),Is.Empty);
        [Test]public void OfflineAlphaStillRejectsMissingOrSharedEnvironment()
        {var c=Config();c.Environments=Array.FindAll(c.Environments,e=>e.Channel!="Alpha");Assert.That(Issues(c,BuildChannel.Alpha),Is.Not.Empty);c=Config();c.Environments[0].Id=c.Environments[1].Id;Assert.That(Issues(c,BuildChannel.Alpha),Is.Not.Empty);}
        [Test]public void AlphaCannotEnableFixturesOrDevelopmentOptions()
        {var c=Config();c.AllowDevelopmentIdentity=true;Assert.That(Issues(c,BuildChannel.Alpha),Is.Not.Empty);c.AllowDevelopmentIdentity=false;Assert.That(Issues(c,BuildChannel.Alpha,true),Is.Not.Empty);}
        [Test]public void ProductionAndEnabledAlphaStillRequireActualExternalSetup()
        {var c=Config();Assert.That(Issues(c,BuildChannel.Production),Is.Not.Empty);c.Enabled=true;Assert.That(Issues(c,BuildChannel.Alpha),Is.Not.Empty);}
    }
}
