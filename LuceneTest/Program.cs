using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using Lucene.Net;
using System.IO;

namespace LuceneTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var directory = LuceneIndex();
            var luceneIndexSearcher = new Lucene.Net.Search.IndexSearcher(directory);
            string input;
            Console.WriteLine(luceneIndexSearcher.MaxDoc);
            Console.WriteLine(luceneIndexSearcher.Doc(0));
            do
            {
                input = Console.ReadLine();
                //build a query object
                var searchTerm = new Lucene.Net.Index.Term("Data", input);
                var query = new Lucene.Net.Search.FuzzyQuery(searchTerm, 0.05f);

                //execute the query
                var hits = luceneIndexSearcher.Search(query, 10);
                //iterate over the results.
                for (int i = 0; i < Math.Min(10, hits.TotalHits); i++)
                {
                    var doc = hits.ScoreDocs[i];
                    string contentValue = luceneIndexSearcher.Doc(doc.Doc).Get("Data");

                    Console.WriteLine(contentValue);
                }
                if(input == "all")
                {
                    for(int i= 0; i< luceneIndexSearcher.MaxDoc; i++)
                    {
                        Console.WriteLine(luceneIndexSearcher.Doc(i));
                    }
                }

            }
            while (input != "exit");
            directory.Dispose();
        }

        static Lucene.Net.Store.FSDirectory LuceneIndex()
        {
            //var indexFileLocation = @"C:\Users\Kieran\Documents\Visual Studio 2015\Projects\LuceneTest\LuceneTest\LuceneIndex";
            var luceneDirectory = Lucene.Net.Store.FSDirectory.Open(indexFileLocation);
            var luceneAnalyser = new Lucene.Net.Analysis.Standard.StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
            var luceneIndexWriter = new Lucene.Net.Index.IndexWriter(luceneDirectory, luceneAnalyser, true, Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED);

            
            var data = Builder<TestData>.CreateListOfSize(50).All().With(x => x.Data = Faker.Name.FullName()).Build();
            foreach(var testdata in data)
            {
                var luceneDocument = new Lucene.Net.Documents.Document();
                luceneDocument.Add(new Lucene.Net.Documents.Field("Data", testdata.Data, Lucene.Net.Documents.Field.Store.YES,
                                        Lucene.Net.Documents.Field.Index.ANALYZED, Lucene.Net.Documents.Field.TermVector.YES));
                luceneIndexWriter.AddDocument(luceneDocument);
            }
            
            luceneIndexWriter.Optimize();
            luceneIndexWriter.Close();
            return luceneDirectory;
        }
        class TestData
        {
            public string Data { get; set; }
        }
    }
}
