﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using AppmetrS2S.Actions;
using AppmetrS2S.Persister;
using AppmetrS2S.Serializations;
using Xunit;
using Xunit.Abstractions;

namespace AppmetrS2S.Tests
{
    public class AppMetrJsonSerializationTests
    {
        private readonly ITestOutputHelper _output;

        public AppMetrJsonSerializationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SerializersShouldReturnsEqualsValues()
        {
            var batch = CreateBatch(50000);

            var defaultSerializer = new JavaScriptJsonSerializer();
            var cacheSerializer = new JavaScriptJsonSerializerWithCache();

            var defaultJson = defaultSerializer.Serialize(batch);
            var newtonsoftJson = cacheSerializer.Serialize(batch);

            Assert.Equal(defaultJson, newtonsoftJson);
        }

        [Fact]
        public void SerializeServerInstall()
        {
            var defaultSerializer = new JavaScriptJsonSerializer();
            var install = Events.ServerInstall("test").SetProperties(new Dictionary<string, object>
            {
                {"key", "value"} 
            });
            var events = new List<AppMetrAction> {install};
            var batch = new Batch(Guid.NewGuid().ToString(), 1, events);
            var json = defaultSerializer.Serialize(batch);
            var receiveBatch = defaultSerializer.Deserialize<Batch>(json);
            
            Assert.Equal(1, receiveBatch.GetBatch().Count);
            var receivedAction = receiveBatch.GetBatch()[0];
            Assert.Equal("test", receivedAction.GetUserId());
            Assert.Equal(1, receivedAction.GetProperties().Count);
            Assert.Equal("value", receivedAction.GetProperties()["key"]);
        }

        [Fact]
        public void SerializersBench()
        {
            var batch = CreateBatch(10000);

            var defaultSerializer = new JavaScriptJsonSerializer();
            var cacheSerializer = new JavaScriptJsonSerializerWithCache();

            defaultSerializer.Serialize(batch);
            cacheSerializer.Serialize(batch);

            const int iterationsCount = 100;

            var cacheTime = Stopwatch.StartNew();
            for (var i = 0; i < iterationsCount; i++)
            {
                cacheSerializer.Serialize(batch);
            }
            cacheTime.Stop();

            var defaultTime = Stopwatch.StartNew();
            for (var i = 0; i < iterationsCount; i++)
            {
                defaultSerializer.Serialize(batch);
            }
            defaultTime.Stop();

            _output.WriteLine("Default: " + defaultTime.Elapsed);
            _output.WriteLine("Newtonsoft: " + cacheTime.Elapsed);
        }

        [Fact]
        public void SerializePayment()
        {
            var events = new List<AppMetrAction>
            {
                new Payment("order1", "trans1", "proc1", "USD", "123", isSandbox: true)
            };
            var batch = new Batch(Guid.NewGuid().ToString(), 1, events);

            var defaultSerializer = new JavaScriptJsonSerializer();

            var json = defaultSerializer.Serialize(batch);

            _output.WriteLine("Json: " + json);
        }

        [Fact]
        public void SerializeUserTime()
        {
            var e = new Event("test");
            e.SetTimestamp(1);
            Assert.Equal(1, e.GetTimestamp());

            var events = new List<AppMetrAction> { e };
            var batch = new Batch(Guid.NewGuid().ToString(), 1, events);

            var defaultSerializer = new JavaScriptJsonSerializer();

            var json = defaultSerializer.Serialize(batch);

            _output.WriteLine("Json: " + json);
        }

        [Fact]
        public void SerializeUserTime_not_specified()
        {
            var events = new List<AppMetrAction> { new Event("test") };
            var batch = new Batch(Guid.NewGuid().ToString(), 1, events);

            var defaultSerializer = new JavaScriptJsonSerializer();

            var json = defaultSerializer.Serialize(batch);

            _output.WriteLine("Json: " + json);
        }

        [Fact]
        public void SerializeAttachEntityAttributes()
        {
            var defaultSerializer = new JavaScriptJsonSerializer();
            var attach = new AttachEntityAttributes("$serverUserId", "testId");
            var events = new List<AppMetrAction> { attach };
            var batch = new Batch(Guid.NewGuid().ToString(), 1, events);
            var json = defaultSerializer.Serialize(batch);
            var receiveBatch = defaultSerializer.Deserialize<Batch>(json);

            _output.WriteLine("Json: " + json);

            Assert.Equal(1, receiveBatch.GetBatch().Count);
            var receivedAction = (AttachEntityAttributes) receiveBatch.GetBatch()[0];
            Assert.Equal("$serverUserId", receivedAction.GetEntityName());
            Assert.Equal("testId", receivedAction.GetEntityValue());
        }

        private static Batch CreateBatch(int size)
        {
            var events = new List<Event>();
            for (var i = 0; i < size; i++)
            {
                var e = new Event("Event #" + i);
                e.SetProperties(new Dictionary<string, object>
                {
                    {"index", i},
                    {"string", "string"},
                    {"int", 1000},
                    {"float", 9.99f},
                    {"double", 8.88d},
                    {"long", long.MaxValue}
                });
                events.Add(e);
            }

            var batch = new Batch(Guid.NewGuid().ToString(), 1, events);
            return batch;
        }
    }
}