// <copyright file="SerializationTests.cs" company="Adrian Mos">
// Copyright (c) Adrian Mos with all rights reserved.
// </copyright>

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using IX.StandardExtensions;
using Xunit;

namespace IX.Observable.UnitTests
{
    /// <summary>
    /// Serialization tests.
    /// </summary>
    public class SerializationTests
    {
        /// <summary>
        /// Observables the list serialization test.
        /// </summary>
        [Fact(DisplayName = "ObservableList serialization")]
        public void ObservableListSerializationTest()
        {
            // ARRANGE
            // =======

            // A random generator (we'll test random values to avoid hard-codings)
            var r = new Random();

            // The data contract serializer we'll use to serialize and deserialize
            var dcs = new DataContractSerializer(typeof(ObservableList<DummyDataContract>));

            // The dummy data
            var ddc1 = new DummyDataContract { RandomValue = r.Next() };
            var ddc2 = new DummyDataContract { RandomValue = r.Next() };
            var ddc3 = new DummyDataContract { RandomValue = r.Next() };
            var ddc4 = new DummyDataContract { RandomValue = r.Next() };

            // The original observable list
            var l1 = new ObservableList<DummyDataContract>
            {
                ddc1,
                ddc2,
                ddc3,
                ddc4,
            };

            // The deserialized list
            ObservableList<DummyDataContract> l2;

            // The serialization content
            string content;

            // ACT
            // ===
            using (var ms = new MemoryStream())
            {
                dcs.WriteObject(ms, l1);

                ms.Seek(0, SeekOrigin.Begin);

                using (var textReader = new StreamReader(ms, Encoding.UTF8, false, 32768, true))
                {
                    content = textReader.ReadToEnd();
                }

                ms.Seek(0, SeekOrigin.Begin);

                l2 = dcs.ReadObject(ms) as ObservableList<DummyDataContract>;
            }

            // ASSERT
            // ======

            // Serialization content is OK
            Assert.False(string.IsNullOrWhiteSpace(content));
            Assert.Equal(
                $@"<ObservableDDCList xmlns=""http://schemas.datacontract.org/2004/07/IX.Observable"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:a=""http://schemas.datacontract.org/2004/07/IX.Observable.UnitTests""><DDC><a:RandomValue>{ddc1.RandomValue}</a:RandomValue></DDC><DDC><a:RandomValue>{ddc2.RandomValue}</a:RandomValue></DDC><DDC><a:RandomValue>{ddc3.RandomValue}</a:RandomValue></DDC><DDC><a:RandomValue>{ddc4.RandomValue}</a:RandomValue></DDC></ObservableDDCList>",
                content);

            // Deserialized object is OK
            Assert.NotNull(l2);
            Assert.Equal(l1.Count, l2.Count);
            Assert.True(l1.SequenceEquals(l2));
        }

        /// <summary>
        /// Observables the list serialization test.
        /// </summary>
        [Fact(DisplayName = "ConcurrentObservableList serialization")]
        public void ConcurrentObservableListSerializationTest()
        {
            // ARRANGE
            // =======

            // A random generator (we'll test random values to avoid hard-codings)
            var r = new Random();

            // The data contract serializer we'll use to serialize and deserialize
            var dcs = new DataContractSerializer(typeof(ConcurrentObservableList<DummyDataContract>));

            // The dummy data
            var ddc1 = new DummyDataContract { RandomValue = r.Next() };
            var ddc2 = new DummyDataContract { RandomValue = r.Next() };
            var ddc3 = new DummyDataContract { RandomValue = r.Next() };
            var ddc4 = new DummyDataContract { RandomValue = r.Next() };

            // The original observable list
            var l1 = new ConcurrentObservableList<DummyDataContract>
            {
                ddc1,
                ddc2,
                ddc3,
                ddc4,
            };

            // The deserialized list
            ConcurrentObservableList<DummyDataContract> l2;

            // The serialization content
            string content;

            // ACT
            // ===
            using (var ms = new MemoryStream())
            {
                dcs.WriteObject(ms, l1);

                ms.Seek(0, SeekOrigin.Begin);

                using (var textReader = new StreamReader(ms, Encoding.UTF8, false, 32768, true))
                {
                    content = textReader.ReadToEnd();
                }

                ms.Seek(0, SeekOrigin.Begin);

                l2 = dcs.ReadObject(ms) as ConcurrentObservableList<DummyDataContract>;
            }

            // ASSERT
            // ======

            // Serialization content is OK
            Assert.False(string.IsNullOrWhiteSpace(content));
            Assert.Equal(
                $@"<ObservableDDCList xmlns=""http://schemas.datacontract.org/2004/07/IX.Observable"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:a=""http://schemas.datacontract.org/2004/07/IX.Observable.UnitTests""><DDC><a:RandomValue>{ddc1.RandomValue}</a:RandomValue></DDC><DDC><a:RandomValue>{ddc2.RandomValue}</a:RandomValue></DDC><DDC><a:RandomValue>{ddc3.RandomValue}</a:RandomValue></DDC><DDC><a:RandomValue>{ddc4.RandomValue}</a:RandomValue></DDC></ObservableDDCList>",
                content);

            // Deserialized object is OK
            Assert.NotNull(l2);
            Assert.Equal(l1.Count, l2.Count);
            Assert.True(l1.SequenceEquals(l2));
        }

        /// <summary>
        /// Class DummyDataContract.
        /// </summary>
        /// <seealso cref="System.IEquatable{DummyDataContract}" />
        [DataContract(Name = "DDC")]
        private class DummyDataContract : IEquatable<DummyDataContract>
        {
            /// <summary>
            /// Gets or sets the random value.
            /// </summary>
            /// <value>The random value.</value>
            [DataMember]
            public int RandomValue { get; set; }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
            public bool Equals(DummyDataContract other) => this.RandomValue == other?.RandomValue;
        }
    }
}