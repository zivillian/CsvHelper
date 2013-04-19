﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CsvHelper.Tests
{
	[TestClass]
	public class InvalidateRecordsCacheTests
	{
		[TestMethod]
		public void InvalidateReaderTest()
		{
			using( var stream = new MemoryStream() )
			using( var reader = new StreamReader( stream ) )
			using( var writer = new StreamWriter( stream ) )
			using( var csv = new CsvReader( reader ) )
			{
				writer.WriteLine( "Id,Name" );
				writer.WriteLine( "1,one" );
				writer.WriteLine( "2,two" );
				writer.Flush();
				stream.Position = 0;

				csv.Configuration.ClassMapping<TestMap1>();
				csv.Read();
				var record = csv.GetRecord<Test>();

				Assert.IsNotNull( record );
				Assert.AreEqual( 1, record.Id );
				Assert.AreEqual( null, record.Name );

				stream.Position = 0;
				csv.InvalidateRecordCache<Test>();

				csv.Configuration.ClassMapping<TestMap2>();
				csv.Read();
				record = csv.GetRecord<Test>();

				Assert.IsNotNull( record );
				Assert.AreEqual( 0, record.Id );
				Assert.AreEqual( "two", record.Name );
			}
		}

		[TestMethod]
		public void InvalidateWriterTest()
		{
			using( var stream = new MemoryStream() )
			using( var reader = new StreamReader( stream ) )
			using( var writer = new StreamWriter( stream ) )
			using( var csv = new CsvWriter( writer ) )
			{
				csv.Configuration.ClassMapping<TestMap1>();
				var record = new Test { Id = 1, Name = "one" };
				csv.WriteRecord( record );

				csv.InvalidateRecordCache<Test>();
				csv.Configuration.ClassMapping<TestMap2>();
				record = new Test { Id = 2, Name = "two" };
				csv.WriteRecord( record );

				writer.Flush();
				stream.Position = 0;

				var data = reader.ReadToEnd();

				var expected = new StringBuilder();
				expected.AppendLine( "1" );
				expected.AppendLine( "two" );

				Assert.AreEqual( expected.ToString(), data );
			}
		}

		private class Test
		{
			public int Id { get; set; }

			public string Name { get; set; }
		}

		private sealed class TestMap1 : CsvClassMap<Test>
		{
			public TestMap1()
			{
				Map( m => m.Id );
			}
		}

		private sealed class TestMap2 : CsvClassMap<Test>
		{
			public TestMap2()
			{
				Map( m => m.Name );
			}
		}
	}
}
