using Blog;
using Grpc.Core;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Blog.BlogService;

namespace server
{
    public class BlogServiceImpl : BlogServiceBase
    {
        private static MongoClient mongoClient = new MongoClient("mongodb://localhost:27017");
        private static IMongoDatabase mongoDatabase = mongoClient.GetDatabase("mydb");
        private static IMongoCollection<BsonDocument> mongoCollection =
            mongoDatabase.GetCollection<BsonDocument>("blog");

        public override Task<CreateBlogResponse> CreateBlog(CreateBlogRequest request, ServerCallContext context)
        {
            var blog = request.Blog;
            BsonDocument doc = new BsonDocument("author_id", blog.AuthorId)
                                    .Add("title", blog.Title)
                                    .Add("content", blog.Content);

            mongoCollection.InsertOne(doc);

            string id = doc.GetValue("_id").ToString();
            blog.Id = id;

            return Task.FromResult(new CreateBlogResponse()
            {
                Blog = blog
            });
        }

        public override async Task<ReadBlogResponse> ReadBlog(ReadBlogRequest request, ServerCallContext context)
        {
            var blog_id = request.BlogId;

            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(blog_id));
            var result = await mongoCollection.Find(filter).FirstOrDefaultAsync();
            if(result == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "The blog id " + blog_id + "wasn't found"));
            }

            Blog.Blog blog = new Blog.Blog()
            {
                AuthorId = result.GetValue("author_id").AsString,
                Content = result.GetValue("content").AsString,
                Title = result.GetValue("title").AsString,
            };

            return new ReadBlogResponse() { Blog = blog };
        }

        public override async Task<UpdateBlogResponse> UpdateBlog(UpdateBlogRequest request, ServerCallContext context)
        {
            var blog_id = request.Blog.Id;

            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(blog_id));
            var result = await mongoCollection.Find(filter).FirstOrDefaultAsync();
            if (result == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "The blog id " + blog_id + "wasn't found"));
            }

            var doc = new BsonDocument("author_id", request.Blog.AuthorId)
                                    .Add("title", request.Blog.Title)
                                    .Add("content", request.Blog.Content);

            mongoCollection.ReplaceOne(filter, doc);

            var blog = new Blog.Blog()
            {
                AuthorId = doc.GetValue("author_id").AsString,
                Content = doc.GetValue("content").AsString,
                Title = doc.GetValue("title").AsString,
            };

            blog.Id = blog_id;

            return new UpdateBlogResponse()
            {
                Blog = blog
            };
        }
        public override async Task<DeleteBlogResponse> DeleteBlog(DeleteBlogRequest request, ServerCallContext context)
        {
            var blog_id = request.BlogId;

            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(blog_id));

            var result = mongoCollection.DeleteOne(filter);

            if(result.DeletedCount == 0)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "the blog id " + blog_id + "was not found"));
            }

            return new DeleteBlogResponse()
            {
                BlogId = blog_id,
            };

        }

        public override async Task ListBlog(ListBlogRequest request, IServerStreamWriter<ListBlogResponse> responseStream, ServerCallContext context)
        {
            var filter = new FilterDefinitionBuilder<BsonDocument>().Empty;

            var result = mongoCollection.Find(filter);

            foreach (var item in result.ToList())
            {
                await responseStream.WriteAsync(new ListBlogResponse()
                {
                    Blog = new Blog.Blog()
                    {
                        AuthorId = item.GetValue("author_id").AsString,
                        Content = item.GetValue("content").AsString,
                        Title = item.GetValue("title").AsString,
                        Id = item.GetValue("_id").ToString()
                    }
                });
            }
        }
    }
}
